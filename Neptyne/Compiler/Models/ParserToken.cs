using System.Collections.Generic;

namespace Neptyne.Compiler.Models;

public class ParserToken
{
    public TokenType Type { get; }

    public string Value { get; }
    
    public List<ParserToken> Params { get; }

    public int Line { get; }

    public int LineIndex { get; }

    public string File { get; }

    public ParserToken(TokenType type, string value, int line, int lineIndex, string file)
    {
        Type = type;
        Value = value;
        Line = line;
        LineIndex = lineIndex;
        Params = new List<ParserToken>();
        File = file;
    }
}
