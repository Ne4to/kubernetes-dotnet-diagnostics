using System.Collections.Generic;
using System.Linq;
using KubernetesDotnetDiagnostics.Exceptions;
using KubernetesDotnetDiagnostics.Models;

namespace KubernetesDotnetDiagnostics
{
    internal class ArgumentsParser
    {
        private const string CommandSeparator = "--";
        private readonly string[] _args;

        public ArgumentsParser(string[] args)
        {
            _args = args;
        }

        public Pod ParsePod()
        {
            var ns = GetArgumentValue("-n", "--namespace");
            var podName = GetPodName();

            if (podName == null)
            {
                throw new ParserException("Please specify pod name");
            }

            return new Pod(podName, ns);
        }

        public Tool ParseTool()
        {
            foreach (var arg in _args.TakeWhile(a => a != CommandSeparator))
            {
                if (arg == "--trace")
                {
                    return Tool.Trace;
                }

                if (arg == "--counters")
                {
                    return Tool.Counters;
                }
            }

            throw new ParserException("Please specify tool using --trace of --counters");
        }

        private string? GetArgumentValue(string shortForm, string longForm)
        {
            for (var argIndex = 0; argIndex < _args.Length; argIndex++)
            {
                var arg = _args[argIndex];
                if (arg == CommandSeparator)
                {
                    break;
                }

                if ((arg == shortForm || arg == longForm) && argIndex != _args.Length - 1)
                {
                    var nextArg = _args[argIndex + 1];
                    return nextArg == CommandSeparator
                        ? null
                        : nextArg;
                }
            }

            return null;
        }

        private string? GetPodName()
        {
            return _args.TakeWhile(a => a != CommandSeparator)
               .FirstOrDefault(arg => !arg.StartsWith('-'));
        }

        public IReadOnlyList<string> GetCommandArguments()
        {
            return _args.SkipWhile(a => a != CommandSeparator)
               .Skip(1)
               .ToArray();
        }
    }
}