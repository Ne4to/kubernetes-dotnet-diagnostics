using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KubernetesDotnetDiagnostics.Exceptions;
using KubernetesDotnetDiagnostics.Models;

namespace KubernetesDotnetDiagnostics
{
    internal static class Kubectl
    {
        public static async Task<bool> Exec2(Pod pod, bool redirectOutput, params string[] arguments)
        {
            return await Exec(pod, redirectOutput, arguments.ToArray());
        }

        public static async Task<bool> Exec(Pod pod, bool redirectOutput, IReadOnlyList<string> arguments)
        {
            var allArguments = new List<string>()
            {
                "exec",
                "-it",
                pod.Name
            };

            if (pod.Namespace != null)
            {
                allArguments.Add("-n");
                allArguments.Add(pod.Namespace);
            }

            allArguments.Add("--");

            allArguments.AddRange(arguments);
            return await Run(redirectOutput, allArguments);
        }

        public static async Task<bool> ExecEmbeddedSingleLineShellScript(Pod pod, string scriptName)
        {
            var script = EmbeddedResourceReader.GetSingleListShellScript(scriptName);
            return await Exec2(pod, false, "bash", "-c", script);
        }

        public static async Task ExecEmbeddedShellScript(Pod pod, string scriptName)
        {
            var tempScriptPath = EmbeddedResourceReader.SaveShellScript(scriptName);
            try
            {
                await UploadFile(tempScriptPath, pod, $"/diagnostics/{scriptName}");
                await Exec2(pod, false, "chmod", "+x", $"/diagnostics/{scriptName}");
                await Exec2(pod, false, "bash", $"/diagnostics/{scriptName}");
            }
            finally
            {
                File.Delete(tempScriptPath);
            }
        }

        /// <summary>
        /// Use <c>kubectl cp /tmp/foo &lt;some-pod&gt;:/tmp/bar</c>
        /// </summary>
        public static async Task<bool> UploadFile(string localPath, Pod pod, string containerPath)
        {
            var arguments = new List<string>()
            {
                "cp",
                localPath,
                pod.Namespace == null
                    ? $"{pod.Name}:{containerPath}"
                    : $"{pod.Namespace}/{pod.Name}:{containerPath}"
            };

            return await Run(false, arguments);
        }

        /// <summary>
        /// <c>kubectl cp demo-global-admin-processor-58dd4cbcff-fmfgw:/app/processor/trace.speedscope.json /temp/trace.speedscope.json</c>
        /// </summary>
        public static async Task<string> DownloadFile(Pod pod, string containerPath)
        {
            var index = containerPath.LastIndexOf('/');
            var fileName = index == -1
                ? containerPath
                : containerPath.Substring(index + 1);

            var localPath = Path.Combine(Path.GetTempPath(), fileName);
            Console.WriteLine($"Downloading {containerPath} to {localPath}");

            var arguments = new List<string>()
            {
                "cp",
                pod.Namespace == null
                    ? $"{pod.Name}:{containerPath}"
                    : $"{pod.Namespace}/{pod.Name}:{containerPath}",
                fileName
            };

            await Run(false, arguments);

            return localPath;
        }

        private static async Task<bool> Run(bool redirectOutput, IReadOnlyCollection<string> arguments)
        {
            var processStartInfo = new ProcessStartInfo("kubectl")
            {
                UseShellExecute = false,
                WorkingDirectory = Path.GetTempPath(),
                RedirectStandardOutput = redirectOutput,
                RedirectStandardError = redirectOutput
            };

            foreach (var arg in arguments)
            {
                processStartInfo.ArgumentList.Add(arg);
            }

            StringBuilder argsLogBuilder = new StringBuilder();
            argsLogBuilder.AppendJoin(' ', arguments);
            Console.WriteLine(argsLogBuilder);

            using var process = new Process
            {
                StartInfo = processStartInfo
            };
            process.Start();

            if (redirectOutput)
            {
                await Task.WhenAll(
                    PipeProcessOutput(
                        process,
                        process.StandardOutput,
                        Console.Out),

                    PipeProcessOutput(
                        process,
                        process.StandardError,
                        Console.Error));
            }

            process.WaitForExit();
            var exitCode = process.ExitCode;

            if (exitCode != 0)
            {
                throw new KubectlException();
            }

            return true;
        }

        private static async Task PipeProcessOutput(
            Process process,
            StreamReader readStream,
            TextWriter writer)
        {
            while (!process.HasExited)
            {
                var memory = new Memory<char>(new char[1024]);
                var readCount = await readStream.ReadAsync(memory);
                if (readCount == 0)
                {
                    break;
                }

                writer.Write(((ReadOnlyMemory<char>) memory).Span);
            }
        }
    }
}