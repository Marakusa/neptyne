using System.Collections.Generic;

namespace Neptyne.Compiler.Models;

public class ParserToken
{
    public TokenType Type { get; }

    public string Value { get; }
    
    public List<ParserToken> Params { get; }

    public int Line { get; }

    public ParserToken(TokenType type, string value, int line)
    {
        Type = type;
        Value = value;
        Line = line;
        Params = new List<ParserToken>();
    }
}
