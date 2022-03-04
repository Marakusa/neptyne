using System;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public static class Parser
{
    private static int _index;
    private static Token[] _tokens;

    private static int CurrentLine => _tokens[_index].Line;
    private static int CurrentLineIndex => _tokens[_index].LineIndex;
    private static string CurrentFile => _tokens[_index].File;
    
    public static ParserToken ParseToSyntaxTree(Token[] inputTokens, string name)
    {
        _index = 0;
        _tokens = inputTokens;
        
        ParserToken root = new(TokenType.Root, name, 1, 0, name);
        
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
                    ParserToken node = new(TokenType.Expression, "", CurrentLine, CurrentLineIndex, CurrentFile);
                    
                    while (token.Type != TokenType.CloseParentheses)
                    {
                        node.Params.Add(Walk());
                        token = _tokens[_index];
                        if (_index + 1 >= _tokens.Length)
                            throw new CompilerException(") expected", CurrentFile, CurrentLine, CurrentLineIndex);
                    }

                    _index++;
                    return node;
                }
                else
                {
                    ParserToken node = new(TokenType.Expression, "", CurrentLine, CurrentLineIndex, CurrentFile);
                    _index++;
                    return node;
                }
            case TokenType.OpenBraces:
                _index++;
                token = _tokens[_index];
                if (token.Type != TokenType.CloseBraces)
                {
                    ParserToken blockNode = new(TokenType.StatementBody, "", CurrentLine, CurrentLineIndex, CurrentFile);
                    
                    while (token.Type != TokenType.CloseBraces)
                    {
                        blockNode.Params.Add(Walk());
                        token = _tokens[_index];
                        if (_index + 1 >= _tokens.Length && token.Type != TokenType.CloseBraces)
                            throw new CompilerException("} expected", CurrentFile, CurrentLine, CurrentLineIndex);
                    }
                
                    _index++;
                    return blockNode;
                }
                else
                {
                    ParserToken node = new(TokenType.StatementBody, "", CurrentLine, CurrentLineIndex, CurrentFile);
                    _index++;
                    return node;
                }
            case TokenType.OpenBrackets:
                _index++;
                token = _tokens[_index];
                if (token.Type != TokenType.CloseBrackets)
                {
                    ParserToken blockNode = new(TokenType.Brackets, "", CurrentLine, CurrentLineIndex, CurrentFile);
                    
                    while (token.Type != TokenType.CloseBrackets)
                    {
                        blockNode.Params.Add(Walk());
                        token = _tokens[_index];
                        if (_index + 1 >= _tokens.Length && token.Type != TokenType.CloseBrackets)
                            throw new CompilerException("] expected", CurrentFile, CurrentLine, CurrentLineIndex);
                    }
                
                    _index++;
                    return blockNode;
                }
                else
                {
                    ParserToken node = new(TokenType.Brackets, "", CurrentLine, CurrentLineIndex, CurrentFile);
                    _index++;
                    return node;
                }
            default:
                _index++;
                return new ParserToken(token.Type, token.Value, CurrentLine, CurrentLineIndex, CurrentFile);
        }
    }
}
