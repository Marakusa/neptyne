using System;

namespace Neptyne.Compiler.Exceptions
{
    public class CompilerException : Exception
    {
        public CompilerException(string error, string script, int line, int currentLineIndex) : base($"{error}\n\t{script}: (Ln {line}, Col {currentLineIndex})")
        {
        }
    }
}
