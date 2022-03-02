using System;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public static class Parser
{
    private static int _index;
    private static Token[] _tokens;

    public static int CurrentLine => _tokens[_index].Line;

    public static ParserToken ParseToSyntaxTree(Token[] inputTokens, string name)
    {
        _index = 0;
        _tokens = inputTokens;
        
        ParserToken root = new(TokenType.Root, name, 1);
        
        while (_index < _tokens.Length)
        {
            root.Params.Add(Walk());
        }
        
        return root;
    }
    
    private static ParserToken Walk()
    {
        var token = _tokens[_index];
        
        switch (token.Type)
        {
            case TokenType.OpenParentheses:
                _index++;
                token = _tokens[_index];
                if (token.Type != TokenType.CloseParentheses)
                {
                    ParserToken node = new(TokenType.Expression, "", CurrentLine);
                    
                    while (token.Type != TokenType.CloseParentheses)
                    {
                        node.Params.Add(Walk());
                        token = _tokens[_index];
                        if (_index + 1 >= _tokens.Length)
                            throw new CompilerException(") expected", CurrentLine);
                    }

                    _index++;
                    return node;
                }
                else
                {
                    ParserToken node = new(TokenType.Expression, "", CurrentLine);
                    _index++;
                    return node;
                }
            case TokenType.OpenBraces:
                _index++;
                token = _tokens[_index];
                if (token.Type != TokenType.CloseBraces)
                {
                    ParserToken blockNode = new(TokenType.StatementBody, "", CurrentLine);
                    
                    while (token.Type != TokenType.CloseBraces)
                    {
                        blockNode.Params.Add(Walk());
                        token = _tokens[_index];
                        if (_index + 1 >= _tokens.Length && token.Type != TokenType.CloseBraces)
                            throw new CompilerException("} expected", CurrentLine);
                    }
                
                    _index++;
                    return blockNode;
                }
                else
                {
                    ParserToken node = new(TokenType.StatementBody, "", CurrentLine);
                    _index++;
                    return node;
                }
            case TokenType.OpenBrakcets:
                throw new NotImplementedException("Open brackets not implemented yet");
            default:
                _index++;
                return new(token.Type, token.Value, token.Line);
        }
    }
}
