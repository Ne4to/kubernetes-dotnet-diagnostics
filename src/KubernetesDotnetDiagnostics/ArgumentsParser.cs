using System.Linq;

namespace KubernetesDotnetDiagnostics
{
    internal class ArgumentsParser
    {
        private readonly string[] _args;

        public string? Namespace { get; private set; }
        public string? PodName { get; private set; }

        public ArgumentsParser(string[] args)
        {
            _args = args;
        }

        // -n, --namespace=''
        public void Parse()
        {
            var kubernetesNamespace = GetArgumentValue("-n", "--namespace");
            var podName = GetPodName();

            Namespace = kubernetesNamespace;
            PodName = podName;
        }

        private string? GetArgumentValue(string shortForm, string longForm)
        {
            for (var argIndex = 0; argIndex < _args.Length; argIndex++)
            {
                var arg = _args[argIndex];

                if ((arg == shortForm || arg == longForm) && argIndex != _args.Length - 1)
                {
                    return _args[argIndex + 1];
                }

            }

            return null;
        }

        private string? GetPodName()
        {
            return _args.FirstOrDefault(arg => !arg.StartsWith('-'));
        }
    }
}