using System;

namespace Neptyne.Compiler.Exceptions
{
    public class CompilerException : Exception
    {
        public CompilerException(string error, int line) : base($"{error}: {line}")
        {
        }
    }
}
