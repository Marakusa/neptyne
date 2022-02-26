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
    private static List<Function> _functions = new();
    private static List<ParserToken> _abstractSyntaxTree;

    private static int _current;

    private static Function _currentFunction = null;

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

        foreach (var function in _functions)
        {
            ParseFunctinon(function);
        }

        var main = _functions.Find(f => f.Name == "main");
        
        if (main == null)
        {
            throw new CompilerException("Program does not contain a 'main' function suitable for an entry point", 1);
        }
        
        assemblyCode.Variables = _variables;
        assemblyCode.ConstantVariables = _constVariables;
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
                        break;
                    }
                    
                    throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
                }
                else
                    throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);
            case ParserTokenType.ReturnType:
                switch (node.Value)
                {
                    case "void":
                        if (parent == null || parent.Type != ParserTokenType.CodeBlock && parent.Type == ParserTokenType.Keyword)
                            throw new CompilerException("Return statement not excepted", node.Line);
                        ParseFunction(node);
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

        ParserToken typeNode = node;
        int typeNodeIndex = _current;
        PrimitiveTypeObject type = PrimitiveVariables.Parse(node.Value);

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("Variable name expected", node.Line);
        node = _abstractSyntaxTree[_current];
        if (_variables.Find(f => f.Name == node.Value) != null)
            throw new CompilerException($"Variable named '{node.Value}' is already declared", node.Line);
        if (node.Type != ParserTokenType.Name)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);

        string variableName = node.Value;

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("; expected", node.Line);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.AssignmentOperator && node.Type != ParserTokenType.EndStatementToken && node.Type != ParserTokenType.CallExpression)
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
            _current = typeNodeIndex;
            ParseFunction(typeNode);
            return;
        }
        
        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("; expected", node.Line);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.EndStatementToken)
            throw new CompilerException("; expected", node.Line);
    }

    private static void ParseFunction(ParserToken node)
    {
        if (_statementKeywords.Contains(Keyword.Const))
            throw new CompilerException("Functions cannot be constant", node.Line);
        if (_statementKeywords.Contains(Keyword.Readonly))
            throw new CompilerException("Functions cannot have a readonly keyword", node.Line);
        
        string returnType = node.Value;

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("Function name expected", node.Line);
        node = _abstractSyntaxTree[_current];
        if (_functions.Find(f => f.Name == node.Value) != null)
            throw new CompilerException($"Function named '{node.Value}' is already defined", node.Line);
        if (node.Type != ParserTokenType.Name)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);

        string functionName = node.Value;

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("( expected", node.Line);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.CallExpression)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);

        List<ParserToken> functionParameters = node.Params;
        
        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("{ expected", node.Line);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.CodeBlock)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", node.Line);

        List<ParserToken> functionBlock = node.Params;

        Function function = new(functionName, returnType, functionParameters, functionBlock, node, _current);
        _functions.Add(function);
        
        if (_current + 1 < _abstractSyntaxTree.Count && _abstractSyntaxTree[_current + 1].Type == ParserTokenType.EndStatementToken)
            throw new CompilerException("Unexpected token", _abstractSyntaxTree[_current + 1].Line);
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
    
    private static void ParseFunctinon(Function function)
    {
        if (function.BlockTokens.Count == 0)
            return;
        _current = _abstractSyntaxTree.Count;
        _abstractSyntaxTree.AddRange(function.BlockTokens);
        _currentFunction = function;
        ParseLine(function.ParentBlockNode);
        _currentFunction = null;
    }
}
