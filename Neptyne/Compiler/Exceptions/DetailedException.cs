using System;

namespace Neptyne.Compiler.Exceptions;

public class DetailedException : Exception
{
    public DetailedException(string message) : base(message)
    {
    }
}
