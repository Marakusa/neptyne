namespace Neptyne.Compiler.Models;

public class Token
{
    public TokenType Type { get; }

    public string Value { get; }
    
    public int Line { get; }
    
    public int LineIndex { get; }

    public string File { get; }

    public Token(TokenType type, string value, int line, int lineIndex, string file)
    {
        Type = type;
        Value = value;
        Line = line;
        LineIndex = lineIndex;
        File = file;
    }
}

public enum TokenType
{
    Root,
    OpenParentheses,
    CloseParentheses,
    Name,
    IntegerLiteral,
    StringLiteral,
    BooleanLiteral,
    FloatLiteral,
    StatementTerminator,
    Point,
    OpenBraces,
    CloseBraces,
    StatementIdentifier,
    Keyword,
    ReturnStatement,
    AdditionOperator,
    OpenBrakcets,
    CloseBrakcets,
    EqualityOperator,
    AssignmentOperator,
    AdditionAssignmentOperator,
    IncrementOperator,
    SubtractionOperator,
    SubtractionAssignmentOperator,
    DecrementOperator,
    MultiplicationAssignmentOperator,
    MultiplicationOperator,
    DivisionAssignmentOperator,
    DivisionOperator,
    LogicalAndOperator,
    LogicalOrOperator,
    LogicalNotOperator,
    Colon,
    Comma,
    Identifier,
    
    Expression,
    StatementBody,
    
    CStatement
}