using Xunit;

namespace KubernetesDotnetDiagnostics.Tests
{
    public class ArgumentsParserGetCommandArguments
    {
        [Fact]
        public void ReturnsRemainingArguments()
        {
            var args = new[]
            {
                "--counters", "pod-name",
                "--",
                "monitor", "--process-id", "1"
            };
            ArgumentsParser parser = new ArgumentsParser(args);

            var commandArguments = parser.GetCommandArguments();

            Assert.Equal(3, commandArguments.Count);
            Assert.Equal("monitor", commandArguments[0]);
            Assert.Equal("--process-id", commandArguments[1]);
            Assert.Equal("1", commandArguments[2]);
        }
    }
}