using System;
using System.IO;
using System.Reflection;

namespace KubernetesDotnetDiagnostics
{
    internal static class EmbeddedResourceReader
    {
        public static void WriteResourceTo(string name, Stream outputStream)
        {
            var resourceName = $"KubernetesDotnetDiagnostics.{name}";

            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                throw new Exception($"Resource '{resourceName}' is not found");
            }

            try
            {
                resourceStream.CopyTo(outputStream);
            }
            finally
            {
                resourceStream.Dispose();
            }
        }

        public static string SaveShellScript(string name)
        {
            var fileName = Path.GetTempFileName();
            using var fileStream = File.OpenWrite(fileName);
            WriteResourceTo($"shell_scripts.{name}", fileStream);
            return fileName;
        }
    }
}