using System;

namespace KubernetesDotnetDiagnostics.Exceptions
{
    internal class InternalException : Exception
    {
        public InternalException()
        {
        }

        public InternalException(string message)
            : base(message)
        {
        }

        public InternalException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}