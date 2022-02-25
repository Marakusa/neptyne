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

        _functions.Add(new("_start", "void", null, null));
        assemblyCode.TextSection.Items.Add(new("global", "_start"));
        
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
        ParserToken c = _abstractSyntaxTree[_current];
        if (c.Type == ParserTokenType.Name)
        {
            string variableName = c.Value;

            _current++;
            c = _abstractSyntaxTree[_current];
            if (c.Type == ParserTokenType.AssignmentOperator && c.Value == "=")
            {
                _current++;
                c = _abstractSyntaxTree[_current];
                if (c.Type == variableType)
                {
                    if (_variables.Find(f => f.Name == variableName) == null)
                    {
                        AsmDataVariable variable = new($"{variableName}", c.Value, GetVariableType(c.Type));
                        _variables.Add(variable);
                    }
                    else
                        throw new CompilerException($"Variable named '{variableName}' is already defined", c.Line);
                    
                    _current++;
                    c = _abstractSyntaxTree[_current];
                    if (c.Type != ParserTokenType.EndStatementToken)
                    {
                        throw new CompilerException("; expected", c.Line);
                    }
                }
                else
                {
                    throw new CompilerException(
                        $"Cannot convert expression of type '{GetVariableType(c.Type)}' to type '{GetVariableType(variableType)}'", c.Line);
                }
            }
            else if (variableType == ParserTokenType.ReturnType || GetVariableType(variableType) != null)
            {
                if (c.Type == ParserTokenType.CallExpression)
                {
                    if (_current + 1 < _abstractSyntaxTree.Count && _abstractSyntaxTree[_current + 1].Type == ParserTokenType.CodeBlock)
                    {
                        AsmFunction function = new(variableName, type, c.Params, GetBlock());
                        _functions.Add(function);
                    }
                    else
                    {
                        throw new CompilerException("{ expected", c.Line);
                    }
                }
                else
                {
                    throw new CompilerException("Expression expected", c.Line);
                }
            }
            else
            {
                throw new CompilerException("Unexpected token", c.Line);
            }
        }
        else
        {
            throw new CompilerException("Unexpected token", c.Line);
        }
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
        
        int i = -1;
        while (i < tokens.Count)
        {
            statements.AddRange(ParseStatement(tokens, i));
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
        ParserToken token = _abstractSyntaxTree[i];
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
                        token = _abstractSyntaxTree[i];
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
                                token = _abstractSyntaxTree[i];
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
            case ParserTokenType.EndStatementToken:
                return statements.ToArray();
            default:
                throw new CompilerException($"Could not resolve symbol '{token.Value}'", token.Line);
        }

        return statements.ToArray();
    }
}
