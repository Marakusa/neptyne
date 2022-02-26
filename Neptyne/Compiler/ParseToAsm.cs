using System;
using System.Collections.Generic;
using System.Linq;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;
using Neptyne.Compiler.Models.Assembly;

namespace Neptyne.Compiler;

public static class ParseToAsm
{
    private static List<Variable> _variables = new();
    private static List<Variable> _constVariables = new();
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
        while (_current < _abstractSyntaxTree.Count)
        {
            ParseLine(null);
            _current++;
        }

        /*foreach (var function in _functions)
        {
            ParseFunction(function);
        }*/

        var main = _functions.Find(f => f.Name == "main");
        
        if (main == null)
        {
            throw new CompilerException("Program does not contain a 'main' function suitable for an entry point", 1);
        }
        
        assemblyCode.Variables.AddRange(_variables);
        assemblyCode.ConstantVariables.AddRange(_constVariables);
        assemblyCode.Functions = _functions;

        return assemblyCode.Build();
    }

    private static List<Keyword> _statementKeywords = new();

    private static void ParseLine(ParserToken parent)
    {
        if (_current >= _abstractSyntaxTree.Count) return;
        
        ParserToken node = _abstractSyntaxTree[_current];
        switch (node.Type)
        {
            case ParserTokenType.EndStatementToken:
                EndStatement();
                break;
            case ParserTokenType.Keyword:
                if (parent == null || parent.Type != ParserTokenType.Keyword)
                    throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
                
                switch (node.Value)
                {
                    case "const":
                        if (_statementKeywords.Contains(Keyword.Readonly))
                            throw new CompilerException("Readonly is not valid for constant", node.Line);
                        _statementKeywords.Add(Keyword.Const);
                        break;
                    case "readonly":
                        if (_statementKeywords.Contains(Keyword.Const))
                            throw new CompilerException("Readonly is not valid for constant", node.Line);
                        _statementKeywords.Add(Keyword.Readonly);
                        break;
                    default:
                        throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
                }
                _current++;
                ParseLine(node);
                break;
            case ParserTokenType.PrimitiveType:
                if (PrimitiveVariables.IsValidPrimitive(node.Value))
                {
                    if (parent != null && parent.Type == ParserTokenType.Keyword ||
                        parent == null ||
                        parent.Type == ParserTokenType.CodeBlock)
                    {
                        ParsePrimitive(node, parent != null && parent.Type == ParserTokenType.CodeBlock);
                        _current++;
                    }
                    else
                        throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
                    break;
                }
                else
                    throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
            case ParserTokenType.ReturnType:
                switch (node.Value)
                {
                    case "void":
                        ParseReturnType(node, parent != null && parent.Type == ParserTokenType.CodeBlock);
                        _current++;
                        break;
                    default:
                        throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
                }
                break;
            default:
                throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
        }
    }

    private static void ParsePrimitive(ParserToken node, bool isInBlock)
    {
        if (isInBlock && _statementKeywords.Count > 0)
            throw new CompilerException("Statements inside blocks can't have keywords", node.Line);

        PrimitiveTypeObject type = PrimitiveVariables.Parse(node.Value);

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("Variable name expected", node.Line);
        node = _abstractSyntaxTree[_current];
        if (_variables.Find(f => f.Name == node.Value) != null)
            throw new CompilerException($"Variable '{node.Value}' is already declared", node.Line);
        if (node.Type != ParserTokenType.Name)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);

        string variableName = node.Value;

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("; expected", node.Line);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.AssignmentOperator || node.Type != ParserTokenType.EndStatementToken || node.Type != ParserTokenType.CallExpression)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
        if (node.Type == ParserTokenType.EndStatementToken)
        {
            EndStatement();
            return;
        }
        if (node.Type == ParserTokenType.AssignmentOperator)
        {
            _current++;
            if (_current >= _abstractSyntaxTree.Count)
                throw new CompilerException("Value for variable expected", node.Line);
            node = _abstractSyntaxTree[_current];
            if (type.LiteralType == GetLiteralType(node.Type, node))
            {
                if (isInBlock)
                {
                    throw new CompilerException("NotImplementedException", node.Line);
                }
                else
                {
                    DeclareClassVariable(variableName, node, type, _statementKeywords.ToArray());
                }
            }
            else
                throw new CompilerException(
                    $"Cannot convert expression of type to '{type.Name}'", node.Line);
        }
        else if (node.Type == ParserTokenType.CallExpression)
        {
            throw new CompilerException("NotImplementedException", node.Line);
        }
        
        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("; expected", node.Line);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.EndStatementToken)
            throw new CompilerException("; expected", node.Line);
    }

    private static void ParseReturnType(ParserToken node, bool b)
    {
        throw new CompilerException("NotImplementedException", node.Line);
    }
    
    private static LiteralType GetLiteralType(ParserTokenType nodeType, ParserToken node)
    {
        switch (nodeType)
        {
            case ParserTokenType.StringLiteral:
                return LiteralType.String;
            case ParserTokenType.BooleanLiteral:
                return LiteralType.Boolean;
            case ParserTokenType.NumberLiteral:
                return LiteralType.Number;
            case ParserTokenType.CharacterLiteral:
                return LiteralType.Character;
            case ParserTokenType.FloatLiteral:
                return LiteralType.Float;
        }

        throw new CompilerException("Invalid literal type", node.Line);
    }

    private static void EndStatement()
    {
        _statementKeywords.Clear();
    }

    private static void DeclareClassVariable(string variableName, ParserToken valueToken, PrimitiveTypeObject variableType, Keyword[] keywords)
    {
        if (_variables.Find(f => f.Name == variableName) == null)
        {
            if (keywords.Contains(Keyword.Const))
            {
                Variable variable = new(false, variableType, variableName, valueToken);
                _constVariables.Add(variable);
            }
            else
            {
                Variable variable = new(keywords.Contains(Keyword.Readonly), variableType, variableName, valueToken);
                _constVariables.Add(variable);
            }
        }
        else
            throw new CompilerException("Member with the same signature is already declared", valueToken.Line);
    }
}
