using System;
using System.Threading.Tasks;
using KubernetesDotnetDiagnostics.Exceptions;
using KubernetesDotnetDiagnostics.Models;
using Microsoft.Extensions.Logging;

// TODO investigate IKubernetes.NamespacedPodExecAsync(null,null,null,null,true,new ExecAsyncCallback(), )
// TODO https://github.com/jlfwong/speedscope#usage

namespace KubernetesDotnetDiagnostics
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                var argumentsParser = new ArgumentsParser(args);
                var pod = argumentsParser.ParsePod();
                var tool = argumentsParser.ParseTool();

                switch (tool)
                {
                    case Tool.Trace:
                        await RunTrace(pod);
                        break;

                    case Tool.Counters:
                        await RunCounters(pod);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (ParserException e)
            {
                logger.LogError(e.Message);
                return 1;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unable to process requested operation");
                return 1;
            }

            return 0;
        }

        private static async Task RunTrace(Pod pod)
        {
            await Kubectl.ExecEmbeddedShellScript(pod, "install-dotnet-trace.sh");

            var filenamePrefix = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
            var nettraceFullPath = $"/diagnostics/trace-{filenamePrefix}.nettrace";
            var speedscopeFullPath = $"/diagnostics/trace-{filenamePrefix}.speedscope.json";

            var traceArguments = new[]
            {
                "dotnet", "/diagnostics/dotnet-trace/tools/netcoreapp2.1/any/dotnet-trace.dll", "collect",
                "--format", "speedscope",
                "--output", nettraceFullPath,
                "--process-id", "1"
            };

            await Kubectl.Exec(pod, true, traceArguments);
            await Kubectl.DownloadFile(pod, nettraceFullPath);
            await Kubectl.DownloadFile(pod, speedscopeFullPath);
        }

        private static async Task RunCounters(Pod pod)
        {
            await Kubectl.ExecEmbeddedShellScript(pod, "install-dotnet-counters.sh");

            var traceArguments = new[]
            {
                "dotnet", "/diagnostics/dotnet-counters/tools/netcoreapp2.1/any/dotnet-counters.dll",
                "monitor",
                "--process-id", "1"
            };

            await Kubectl.Exec(pod, true, traceArguments);
        }
    }
}