using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace KubernetesDotnetDiagnostics
{
    class Program
    {
        static int Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                var argumentsParser = new ArgumentsParser(args);
                argumentsParser.Parse();

                // TODO upload single setup.sh file and execute
                var result = Exec(argumentsParser, "command -v unzip || (apt-get update && apt-get install unzip)")
                    && Exec(argumentsParser, "cd /diagnostics || (mkdir /diagnostics && cd /diagnostics)")
                    && Exec(argumentsParser, "curl -L --output dotnet-counters.nupkg https://www.nuget.org/api/v2/package/dotnet-counters/3.1.120604")
                    && Exec(argumentsParser, "unzip dotnet-counters.nupkg -d dotnet-counters")
                    && Exec(argumentsParser, "dotnet /diagnostics/dotnet-counters/tools/netcoreapp2.1/any/dotnet-counters.dll ps");

                logger.LogInformation($"Result = {result}");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to process requested operation");
                return 1;
            }

            return 0;
        }

        private static bool Exec(ArgumentsParser argumentsParser, params string[] arguments)
        {
            var processStartInfo = new ProcessStartInfo("kubectl")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            processStartInfo.ArgumentList.Add("exec");
            processStartInfo.ArgumentList.Add("-it");
            processStartInfo.ArgumentList.Add(argumentsParser.PodName);
            processStartInfo.ArgumentList.Add("--");
            processStartInfo.ArgumentList.Add("bash");
            processStartInfo.ArgumentList.Add("-c");

            foreach (var arg in arguments)
            {
                processStartInfo.ArgumentList.Add(arg);
            }

            var process = Process.Start(processStartInfo);
            var stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine(stdout);
            return process.ExitCode == 0;
        }
    }
}

// TODO investigate IKubernetes.NamespacedPodExecAsync(null,null,null,null,true,new ExecAsyncCallback(), )