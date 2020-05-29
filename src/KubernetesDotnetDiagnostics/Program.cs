using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
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
            var countersCommand = new Command("--counters")
            {
                CreatePodArgument(),
                CreateNamespaceOption(),
                CreateArgsOption("counters")
            };

            countersCommand.Handler = CommandHandler.Create(
                async (string? n, string pod, string[]? args) =>
                {
                    await RunCounters(new Pod(pod, n), null, args);
                });

            Command monitorCommand = new Command("--monitor")
            {
                Handler = CommandHandler.Create(async (string? n, string pod, string[]? args) =>
                {
                    await RunCounters(new Pod(pod, n), CountersMode.Monitor, args);
                })
            };
            countersCommand.AddCommand(monitorCommand);

            Command collectCommand = new Command("--collect")
            {
                Handler = CommandHandler.Create(async (string? n, string pod, string[]? args) =>
                {
                    await RunCounters(new Pod(pod, n), CountersMode.Collect, args);
                })
            };
            countersCommand.AddCommand(collectCommand);

            var traceCommand = new Command("--trace")
            {
                CreatePodArgument(),
                CreateNamespaceOption(),
                CreateArgsOption("counters")
            };

            traceCommand.Handler = CommandHandler.Create(
                async (string? n, string pod) =>
                {
                    await RunTrace(new Pod(pod, n));
                });

            var parent = new RootCommand("parent")
            {
                traceCommand,
                countersCommand,                
            };
            parent.IsHidden = true;

            return await parent.InvokeAsync(args);
        }

        private static Argument<string> CreatePodArgument()
        {
            return new Argument<string>("pod")
            {
                Description = "Pod name",
                Arity = ArgumentArity.ExactlyOne
            };
        }

        private static Option CreateNamespaceOption()
        {
            return new Option(new[] { "-n", "--namespace" }, "The namespace scope")
            {
                Argument = new Argument<string>()
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };
        }

        private static Option<string> CreateArgsOption(string commandName)
        {
            return new Option<string>("--args")
            {
                Description = $"dotnet-{commandName} arguments",
                Argument = new Argument<string>()
                {
                    Arity = ArgumentArity.OneOrMore
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

                await Kubectl.Exec2(pod, true, traceArguments);
                await Kubectl.DownloadFile(pod, nettraceFullPath);
                await Kubectl.DownloadFile(pod, speedscopeFullPath);
            }
            catch (KubectlException)
            {
            }
        }

        private static async Task RunCounters(Pod pod, CountersMode? mode, string[]? extraArgs)
        {
            try
            {
                await Kubectl.ExecEmbeddedSingleLineShellScript(pod, "install-dotnet-counters.sh");

                var countersArguments = new List<string>()
                {
                    "dotnet", "/diagnostics/dotnet-counters/tools/netcoreapp2.1/any/dotnet-counters.dll"
                };

                if (mode != null)
                {
                    switch (mode.Value)
                    {
                        case CountersMode.Monitor:
                            countersArguments.AddRange(new[] { "monitor", "--process-id", "1" });
                            break;

                        case CountersMode.Collect:
                            countersArguments.AddRange(new[] { "collect", "--process-id", "1" });
                            break;
                    }
                }

                if (extraArgs != null)
                {
                    countersArguments.AddRange(extraArgs);
                }

                await Kubectl.Exec(pod, true, countersArguments);
            }
            catch (KubectlException)
            {
            }
        }
    }

    internal enum CountersMode
    {
        Monitor,
        Collect
    }
}