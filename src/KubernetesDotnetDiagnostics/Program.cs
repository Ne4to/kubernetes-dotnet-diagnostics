using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using KubernetesDotnetDiagnostics.Exceptions;
using KubernetesDotnetDiagnostics.Models;

// TODO investigate IKubernetes.NamespacedPodExecAsync(null,null,null,null,true,new ExecAsyncCallback(), )
// TODO https://github.com/jlfwong/speedscope#usage

namespace KubernetesDotnetDiagnostics
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var countersCommand = new Command("counters")
            {
                CreatePodArgument(),
                CreateNamespaceOption()
            };

            countersCommand.Handler = CommandHandler.Create(
                async (string? n, string pod) =>
                {
                    await RunCounters(new Pod(pod, n));
                });

            var traceCommand = new Command("trace")
            {
                CreatePodArgument(),
                CreateNamespaceOption()
            };

            traceCommand.Handler = CommandHandler.Create(
                async (string? n, string pod) =>
                {
                    await RunTrace(new Pod(pod, n));
                });

            var parent = new RootCommand("parent")
            {
                traceCommand,
                countersCommand
            };
            parent.IsHidden = true;

            return await parent.InvokeAsync(args);
        }

        private static Argument<string> CreatePodArgument()
        {
            return new Argument<string>("pod")
            {
                Description = "Pod name",
                Arity = new ArgumentArity(1, 1)
            };
        }

        private static Option CreateNamespaceOption()
        {
            return new Option(new[]{ "-n", "--namespace"}, "The namespace scope")
            {
                Argument = new Argument<string>()
                {
                    Arity = new ArgumentArity(0, 1)
                }
            };
        }

        private static async Task RunTrace(Pod pod)
        {
            try
            {
                await Kubectl.ExecEmbeddedSingleLineShellScript(pod, "install-dotnet-trace.sh");

                var filenamePrefix = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
                var nettraceFullPath = $"/diagnostics/trace-{filenamePrefix}.nettrace";
                var speedscopeFullPath = $"/diagnostics/trace-{filenamePrefix}.speedscope.json";

                var traceArguments = new[]
                {
                    "dotnet", "/diagnostics/dotnet-trace/tools/netcoreapp2.1/any/dotnet-trace.dll", "collect",
                    "--format", "speedscope",
                    "--output", nettraceFullPath,
                    "--process-id", "1",
                    "--profile", "gc-collect"
                };

                await Kubectl.Exec(pod, true, traceArguments);
                await Kubectl.DownloadFile(pod, nettraceFullPath);
                await Kubectl.DownloadFile(pod, speedscopeFullPath);
            }
            catch (KubectlException)
            {
            }
        }

        private static async Task RunCounters(Pod pod)
        {
            try
            {
                await Kubectl.ExecEmbeddedSingleLineShellScript(pod, "install-dotnet-counters.sh");

                var traceArguments = new[]
                {
                    "dotnet", "/diagnostics/dotnet-counters/tools/netcoreapp2.1/any/dotnet-counters.dll",
                    "monitor",
                    "--process-id", "1"
                };

                await Kubectl.Exec(pod, true, traceArguments);
            }
            catch (KubectlException)
            {
            }
        }
    }
}