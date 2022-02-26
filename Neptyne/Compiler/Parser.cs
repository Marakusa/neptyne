using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public static class Parser
{
    private static int _index;
    private static Token[] _tokens;
    
    public static ParserToken Parse(Token[] inputTokens, string name)
    {
        _index = 0;
        _tokens = inputTokens;
        
        var root = new ParserToken(ParserTokenType.Root, name, 1);
        
        while (_index < _tokens.Length)
        {
            root.Params.Add(Walk());
        }
        
        return root;
    }

    private static ParserToken Walk()
    {
        Token token = _tokens[_index];
        
        switch (token.Type)
        {
            case TokenType.Type:
                _index++;
                switch (token.Value)
                {
                    case "void":
                        return new(ParserTokenType.ReturnType, token.Value, token.Line);
                    default:
                        return new(ParserTokenType.PrimitiveType, token.Value, token.Line);
                }
            case TokenType.Name:
                _index++;
                return new(ParserTokenType.Name, token.Value, token.Line);
            case TokenType.Number:
                _index++;
                return new(ParserTokenType.NumberLiteral, token.Value, token.Line);
            case TokenType.String:
                _index++;
                return new(ParserTokenType.StringLiteral, token.Value, token.Line);
            case TokenType.Float:
                _index++;
                return new(ParserTokenType.FloatLiteral, token.Value, token.Line);
            case TokenType.Boolean:
                _index++;
                return new(ParserTokenType.BooleanLiteral, token.Value, token.Line);
            case TokenType.EqualsSign:
                _index++;
                return new(ParserTokenType.AssignmentOperator, token.Value, token.Line);
            case TokenType.Semicolon:
                _index++;
                return new(ParserTokenType.EndStatementToken, token.Value, token.Line);
            case TokenType.OpenParenthesis:
                _index++;
                token = _tokens[_index];
                if (token.Type != TokenType.CloseParenthesis)
                {
                    ParserToken node = new(ParserTokenType.CallExpression, "", token.Line);
                    
                    while (token.Type != TokenType.CloseParenthesis)
                    {
                        node.Params.Add(Walk());
                        token = _tokens[_index];
                        if (_index + 1 >= _tokens.Length)
                            throw new CompilerException(") expected", _tokens[_index].Line);
                    }

                    _index++;
                    return node;
                }
                else
                {
                    ParserToken node = new(ParserTokenType.CallExpression, "", token.Line);
                    _index++;
                    return node;
                }
            case TokenType.OpenCurlyBrackets:
                _index++;
                token = _tokens[_index];
                if (token.Type != TokenType.CloseCurlyBrackets)
                {
                    ParserToken blockNode = new(ParserTokenType.CodeBlock, "", token.Line);
                    
                    while (token.Type != TokenType.CloseCurlyBrackets)
                    {
                        blockNode.Params.Add(Walk());
                        token = _tokens[_index];
                        if (_index + 1 >= _tokens.Length && token.Type != TokenType.CloseCurlyBrackets)
                            throw new CompilerException("} expected", _tokens[_index].Line);
                    }
                
                    _index++;
                    return blockNode;
                }
                else
                {
                    ParserToken node = new(ParserTokenType.CodeBlock, "", token.Line);
                    _index++;
                    return node;
                }
            case TokenType.Statement:
                _index++;
                return new(ParserTokenType.Statement, token.Value, token.Line);
            case TokenType.Keyword:
                _index++;
                return new(ParserTokenType.Keyword, token.Value, token.Line);
            default:
                throw new CompilerException($"Syntax error near '{token.Value}'", token.Line);
        }
    }
}
