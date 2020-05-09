using System;

namespace KubernetesDotnetDiagnostics.Exceptions
{
    internal class ParserException : InternalException
    {
        public ParserException(string message)
            : base(message)
        {
        }

        public ParserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}