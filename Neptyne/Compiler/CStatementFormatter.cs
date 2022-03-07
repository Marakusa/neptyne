using System.Collections.Generic;
using Neptyne.Compiler.Models;

namespace Neptyne.Compiler
{
    public static class CStatementFormatter
    {
        public static string Format(IEnumerable<ParserToken> tokens)
        {
            return string.Join(' ', tokens);
        }
    }
}
