using System;
using System.Collections.Generic;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;
using Neptyne.Compiler.Models.Assembly;

namespace Neptyne.Compiler;

public static class ParseToAsm
{
    private static List<AsmDataSectionItem> _variables = new();

    private static int _current;

    public static string ParseToAssembly(ParserToken abstractSyntaxTree)
    {
        _variables = new();
        _current = 0;
        
        AsmScript assemblyCode = new();

        Traverse(abstractSyntaxTree, null);
        
        assemblyCode.DataSection.Items.AddRange(_variables);
        assemblyCode.TextSection.Items.Add(new("global", "_start"));

        return assemblyCode.Build();
    }

    private static void Traverse(ParserToken node, ParserToken parentNode)
    {
        switch (node.Type)
        {
            case ParserTokenType.Main:
                while (_current < node.Params.Count)
                {
                    Traverse(node.Params[_current], node);
                    _current++;
                }
                break;
            case ParserTokenType.ValueType:
                switch (node.Value)
                {
                    case "string":
                        ParseVariable(ParserTokenType.StringLiteral, parentNode.Params, _current);
                        break;
                    case "int":
                        ParseVariable(ParserTokenType.NumberLiteral, parentNode.Params, _current);
                        break;
                    default:
                        throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
                }
                break;
        }
    }

    private static void ParseVariable(ParserTokenType variableType, List<ParserToken> nodes, int index)
    {
        int current = index + 1;
        if (nodes[current].Type == ParserTokenType.VariableName)
        {
            string variableName = nodes[current].Value;

            current++;
            if (nodes[current].Type == ParserTokenType.AssignmentOperator && nodes[current].Value == "=")
            {
                current++;
                if (nodes[current].Type == variableType)
                {
                    string variableValue = variableType == ParserTokenType.StringLiteral
                        ? $"'{nodes[current].Value.Replace("\'", "\\'").Replace("\\n", "',10,'")}'"
                        : nodes[current].Value;
                    
                    if (variableValue.EndsWith("10,''")) variableValue = variableValue.Substring(0, variableValue.Length - 3);
                    
                    if (_variables.Find(f => f.Name == variableName) == null)
                    {
                        AsmDataSectionItem variable = new($"var_{variableName}", "db", variableValue);
                        _variables.Add(variable);
                        AsmDataSectionItem variableLength = new($"len_{variableName}", "equ", $"$-var_{variableName}");
                        _variables.Add(variableLength);
                    }
                    else
                        throw new CompilerException($"Variable named '{variableName}' is already defined", nodes[current].Line);
                    
                    current++;
                    if (nodes[current].Type != ParserTokenType.EndStatementToken)
                    {
                        throw new CompilerException("; expected", nodes[current].Line);
                    }
                }
                else
                {
                    throw new CompilerException(
                        $"Cannot convert expression of type '{GetVariableType(nodes[current].Type)}' to type '{GetVariableType(variableType)}'",
                        nodes[current].Line);
                }
            }
            else
            {
                throw new CompilerException($"Unexpected token", nodes[current].Line);
            }
        }
        else
        {
            throw new CompilerException($"Unexpected token", nodes[current].Line);
        }
                    
        _current = current;
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
                return "unknown";
        }
    }
}
