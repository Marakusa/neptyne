using System.Collections.Generic;

namespace Neptyne.Compiler.Models;

public class ParserToken
{
    public ParserTokenType Type { get; }

    public string Value { get; }
    
    public List<ParserToken> Params { get; }

    public int Line { get; }

    public ParserToken(ParserTokenType type, string value, int line)
    {
        Type = type;
        Value = value;
        Line = line;
        Params = new List<ParserToken>();
    }
}

public enum ParserTokenType
{
    Root,
    NumberLiteral,
    FunctionCall,
    CallExpression,
    PrimitiveType,
    Keyword,
    Name,
    StringLiteral,
    AssignmentOperator,
    EndStatementToken,
    Statement,
    ReturnType,
    CodeBlock,
    BooleanLiteral,
    FloatLiteral,
    CharacterLiteral
}
