using System.Collections.Generic;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;
using Neptyne.Compiler.Models.Assembly;

namespace Neptyne.Compiler;

public static class ParseToAsm
{
    private static List<AsmDataVariable> _variables = new();
    private static List<AsmFunction> _functions = new();
    private static List<ParserToken> _abstractSyntaxTree;

    private static int _current;

    public static string ParseToAssembly(ParserToken abstractSyntaxTree)
    {
        _variables = new();
        _functions = new();
        _current = 0;
        
        AsmScript assemblyCode = new();
        
        _abstractSyntaxTree = abstractSyntaxTree.Params;
        while (_current < abstractSyntaxTree.Params.Count)
        {
            ParseRoot(abstractSyntaxTree.Params[_current]);
            _current++;
        }

        foreach (var function in _functions)
        {
            ParseFunction(function);
        }

        var main = _functions.Find(f => f.Name == "main");
        
        if (main == null)
        {
            throw new CompilerException("Program does not contain a 'main' function suitable for an entry point", 1);
        }
        
        assemblyCode.DataSection.Items.AddRange(_variables);
        assemblyCode.Functions = _functions;

        return assemblyCode.Build();
    }

    private static void ParseRoot(ParserToken node)
    {
        switch (node.Type)
        {
            case ParserTokenType.ValueType:
                switch (node.Value)
                {
                    case "string":
                        ParseVariable(ParserTokenType.StringLiteral);
                        break;
                    case "int":
                        ParseVariable(ParserTokenType.NumberLiteral);
                        break;
                    default:
                        throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
                }
                break;
            case ParserTokenType.ReturnType:
                switch (node.Value)
                {
                    case "string":
                        ParseVariable(ParserTokenType.StringLiteral);
                        break;
                    case "int":
                        ParseVariable(ParserTokenType.NumberLiteral);
                        break;
                    case "void":
                        ParseVariable(ParserTokenType.ReturnType);
                        break;
                    default:
                        throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
                }
                break;
        }
    }

    private static void ParseVariable(ParserTokenType variableType)
    {
        string type = _abstractSyntaxTree[_current].Value;
        _current++;
        ParserToken token = _abstractSyntaxTree[_current];
        if (token.Type == ParserTokenType.Name)
        {
            string variableName = token.Value;

            _current++;
            token = _abstractSyntaxTree[_current];
            if (token.Type == ParserTokenType.AssignmentOperator && token.Value == "=")
            {
                _current++;
                token = _abstractSyntaxTree[_current];
                if (token.Type == variableType)
                {
                    DeclareVariable(variableName, token);
                    
                    _current++;
                    token = _abstractSyntaxTree[_current];
                    if (token.Type != ParserTokenType.EndStatementToken)
                    {
                        throw new CompilerException("; expected", token.Line);
                    }
                }
                else
                {
                    throw new CompilerException(
                        $"Cannot convert expression of type '{GetVariableType(token.Type)}' to type '{GetVariableType(variableType)}'", token.Line);
                }
            }
            else if (variableType == ParserTokenType.ReturnType || GetVariableType(variableType) != null)
            {
                if (token.Type == ParserTokenType.CallExpression)
                {
                    if (_current + 1 < _abstractSyntaxTree.Count && _abstractSyntaxTree[_current + 1].Type == ParserTokenType.CodeBlock)
                    {
                        DeclareFunction(variableName, type, token, GetBlock());
                    }
                    else
                    {
                        throw new CompilerException("{ expected", token.Line);
                    }
                }
                else
                {
                    throw new CompilerException("Expression expected", token.Line);
                }
            }
            else
            {
                throw new CompilerException("Unexpected token", token.Line);
            }
        }
        else
        {
            throw new CompilerException("Unexpected token", token.Line);
        }
    }

    private static void DeclareVariable(string variableName, ParserToken token)
    {
        if (_variables.Find(f => f.Name == variableName) == null)
        {
            AsmDataVariable variable = new($"{variableName}", token.Value, GetVariableType(token.Type));
            _variables.Add(variable);
        }
        else
            throw new CompilerException("Member with the same signature is already declared", token.Line);
    }

    private static void DeclareFunction(string variableName, string type, ParserToken token, List<ParserToken> block)
    {
        AsmFunction function = new(variableName, type, token.Params, block);
                        
        if (_functions.Find(f => f.Name == function.Name) != null)
            throw new CompilerException("Member with the same signature is already declared", token.Line);
        
        _functions.Add(function);
    }

    private static List<ParserToken> GetBlock()
    {
        ParserToken c = _abstractSyntaxTree[_current];
        _current++;
        if (_current < _abstractSyntaxTree.Count)
        {
            c = _abstractSyntaxTree[_current];
            if (c.Type == ParserTokenType.CodeBlock)
            {
                return c.Params;
            }
            
            throw new CompilerException("Expression block expected", c.Line);
        }
        
        throw new CompilerException("Expression block expected", c.Line);
    }

    private static void ParseFunction(AsmFunction function)
    {
        if (function.ParamsTokens != null)
            function.Params = ParseParameters(function.ParamsTokens);
        if (function.BlockTokens != null)
            function.Block = ParseBlock(function.BlockTokens);
    }

    private static List<AsmFunctionParameter> ParseParameters(List<ParserToken> tokens)
    {
        List<AsmFunctionParameter> parameters = new();
        
        int i = 0;
        while (i < tokens.Count)
        {
            var token = tokens[i];
            if (token.Type == ParserTokenType.ValueType)
            {
                string type = token.Value;
                i++;
                token = tokens[i];
                if (token.Type == ParserTokenType.Name)
                {
                    if (GetVariableType(token.Type) == type)
                    {
                        string value = token.Value;
                        parameters.Add(new(type, value));
                    }
                    else
                    {
                        throw new CompilerException(
                            $"Cannot convert expression of type '{GetVariableType(token.Type)}' to type '{type}'", token.Line);
                    }
                }
                else
                {
                    throw new CompilerException("Value type expected", token.Line);
                }
            }
            else
            {
                throw new CompilerException("Value type expected", token.Line);
            }
        }

        return parameters;
    }

    private static List<AsmStatement> ParseBlock(List<ParserToken> tokens)
    {
        List<AsmStatement> statements = new();
        
        int i = 0;
        while (i < tokens.Count)
        {
            if (tokens[i].Type == ParserTokenType.CodeBlock)
            {
                statements.AddRange(ParseStatement(tokens, i));
            }
            else
                i++;
        }

        return statements;
    }

    private static string GetVariableType(ParserTokenType type)
    {
        switch (type)
        {
            case ParserTokenType.NumberLiteral:
                return "int";
            case ParserTokenType.StringLiteral:
                return "string";
            default:
                return null;
        }
    }

    private static AsmStatement[] ParseStatement(List<ParserToken> tokens, int i)
    {
        List<AsmStatement> statements = new();

        i++;
        ParserToken token = tokens[i];
        switch (token.Type)
        {
            case ParserTokenType.ValueType:
                switch (token.Value)
                {
                    case "string":
                        ParseVariable(ParserTokenType.StringLiteral);
                        break;
                    case "int":
                        ParseVariable(ParserTokenType.NumberLiteral);
                        break;
                    default:
                        throw new CompilerException($"Could not resolve symbol '{token.Value}'", token.Line);
                }
                break;
            case ParserTokenType.Name:
                switch (token.Value)
                {
                    // Built in functions
                    case "out":
                        i++;
                        token = tokens[i];
                        if (token.Type == ParserTokenType.CallExpression)
                        {
                            if (token.Params.Count != 1
                                && GetVariableType(token.Params[0].Type) == "string")
                            {
                                string[] parameters = new string[token.Params.Count];
                                int j = 0;
                                foreach (var param in token.Params)
                                {
                                    parameters[j] = param.Value;
                                    j++;
                                }
                                i++;
                                token = tokens[i];
                                if (token.Type == ParserTokenType.EndStatementToken)
                                {
                                    statements.Add(new AsmCallStatement("out", $"({(string.Join(",", parameters))})"));
                                }
                                else
                                {
                                    throw new CompilerException("; expected", token.Line);
                                }
                            }
                            else
                            {
                                throw new CompilerException($"Insufficient amount of parameters: 1 needed, {token.Params.Count} given", token.Line);
                            }
                        }
                        else
                        {
                            throw new CompilerException("( expected", token.Line);
                        }
                        break;
                    default:
                        throw new CompilerException($"Could not resolve symbol '{token.Value}'", token.Line);
                }
                break;
            case ParserTokenType.ReturnType:
                break;
            case ParserTokenType.EndStatementToken:
                return statements.ToArray();
            default:
                throw new CompilerException($"Could not resolve symbol '{token.Value}'", token.Line);
        }

        return statements.ToArray();
    }
}
