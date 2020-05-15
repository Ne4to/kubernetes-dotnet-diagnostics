using System;
using System.IO;
using System.Reflection;
using System.Text;

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

        public static string GetSingleListShellScript(string name)
        {
            var resourceName = $"KubernetesDotnetDiagnostics.shell_scripts.{name}";

            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                throw new Exception($"Resource '{resourceName}' is not found");
            }

            var reader = new StreamReader(resourceStream);
            var content = reader.ReadToEnd();

            var parts = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            StringBuilder commandBuild = new StringBuilder();
            for (var index = 0; index < parts.Length; index++)
            {
                var part = parts[index].Trim();

                if (part.StartsWith('#'))
                {
                    continue;
                }

                commandBuild.Append(part);
                if (!part.EndsWith("then") && !part.EndsWith(';'))
                {
                    commandBuild.Append(';');
                }

                commandBuild.Append(' ');
            }

            return commandBuild.ToString();
        }
    }
}