using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public static class Parser
{
    private static int index;
    private static Token[] tokens;
    
    public static ParserToken Parse(Token[] inputTokens, string name)
    {
        index = 0;
        tokens = inputTokens;
        
        var main = new ParserToken(ParserTokenType.Main, name, 1);
        
        while (index < tokens.Length)
        {
            main.Params.Add(Walk());
        }
        
        return main;
    }

    private static ParserToken Walk()
    {
        Token token = tokens[index];
        
        switch (token.Type)
        {
            case TokenType.Type:
                index++;
                return new(ParserTokenType.ValueType, token.Value, token.Line);
            case TokenType.Name:
                index++;
                return new(ParserTokenType.VariableName, token.Value, token.Line);
            case TokenType.Number:
                index++;
                return new(ParserTokenType.NumberLiteral, token.Value, token.Line);
            case TokenType.String:
                index++;
                return new(ParserTokenType.StringLiteral, token.Value, token.Line);
            case TokenType.EqualsSign:
                index++;
                return new(ParserTokenType.AssignmentOperator, token.Value, token.Line);
            case TokenType.Semicolon:
                index++;
                return new(ParserTokenType.EndStatementToken, token.Value, token.Line);
            case TokenType.OpenParenthesis:
                index++;
                token = tokens[index];
                var node = new ParserToken(ParserTokenType.CallExpression, token.Value, token.Line);

                index++;
                token = tokens[index];
                while (token.Type != TokenType.CloseParenthesis)
                {
                    node.Params.Add(Walk());
                    token = tokens[index];
                }
                index++;
                return node;
            default:
                throw new CompilerException($"Syntax error near '{token.Value}'", token.Line);
        }
    }
}
