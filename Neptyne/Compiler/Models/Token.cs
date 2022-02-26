namespace Neptyne.Compiler.Models;

public class Token
{
    public TokenType Type { get; }

    public string Value { get; }
    
    public int Line { get; }

    public Token(TokenType type, string value, int line)
    {
        Type = type;
        Value = value;
        Line = line;
    }
}

public enum TokenType
{
    OpenParenthesis,
    CloseParenthesis,
    Type,
    Name,
    Number,
    String,
    Boolean,
    Float,
    Semicolon,
    Point,
    EqualsSign,
    OpenCurlyBrackets,
    CloseCurlyBrackets,
    Statement,
    Keyword,
    PrimitiveType,
    ReturnStatement
}