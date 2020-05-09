namespace KubernetesDotnetDiagnostics.Models
{
    internal class Pod
    {
        public string Name { get; }
        public string? Namespace { get; }

        public Pod(string name, string? ns)
        {
            Name = name;
            Namespace = ns;
        }
    }
}