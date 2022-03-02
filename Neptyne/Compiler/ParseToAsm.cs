using System.Collections.Generic;
using System.Linq;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;
using Neptyne.Compiler.Models.Assembly;

namespace Neptyne.Compiler;

public class ParseToAsm
{
    private readonly List<Variable> _variables = new();
    private readonly List<Variable> _constVariables = new();
    private readonly List<Function> _functions = new();
    private List<ParserToken> _abstractSyntaxTree;

    private int CurrentLine => _abstractSyntaxTree[_current].Line;
    private int _current;

    private Function _currentFunction;
    private bool _functionHasReturnStatement = false;

    private readonly List<Keyword> _statementKeywords = new();
    private int _stringCounter;
    private int _lengthCounter;

    public string Start(ParserToken abstractSyntaxTree)
    {
        _abstractSyntaxTree = abstractSyntaxTree.Params;
        while (_current < _abstractSyntaxTree.Count)
        {
            Parse(null);
            _current++;
        }

        var main = _functions.Find(f => f.Name == "main");
        
        if (main == null)
        {
            throw new CompilerException("Program does not contain a 'main' function suitable for an entry point", 1);
        }

        foreach (var function in _functions)
        {
            ParseFunction(function);
        }

        AsmScript assemblyCode = new()
        {
            Variables = _variables,
            Functions = _functions
        };
        return assemblyCode.Build();
    }

    private void Parse(ParserToken parent)
    {
        if (_current >= _abstractSyntaxTree.Count)
            return;

        var node = _abstractSyntaxTree[_current];
        switch (node.Type)
        {
            case TokenType.StatementTerminator:
                EndStatement();
                break;
            case TokenType.Keyword:
                if (parent != null || parent != null && parent.Type != TokenType.Keyword)
                    throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);

                switch (node.Value)
                {
                    case "const":
                        if (_statementKeywords.Contains(Keyword.Readonly))
                            throw new CompilerException("Readonly is not valid for constant", CurrentLine);
                        _statementKeywords.Add(Keyword.Const);
                        break;
                    case "readonly":
                        if (_statementKeywords.Contains(Keyword.Const))
                            throw new CompilerException("Readonly is not valid for constant", CurrentLine);
                        _statementKeywords.Add(Keyword.Readonly);
                        break;
                    default:
                        throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                }
                _current++;
                Parse(node);
                break;
            case TokenType.Identifier:
                var type = node.Value;
                
                node = Step();
                
                if (node.Type != TokenType.Name)
                    throw new CompilerException("Unexpected token", CurrentLine);
                
                var name = node.Value;
                
                node = Step();
                
                if (node.Type == TokenType.StatementTerminator)
                {
                    EndStatement();
                    break;
                }
                else if (node.Type == TokenType.AssignmentOperator)
                {
                    node = Step();

                    if (node.Type == TokenType.Name)
                    {
                    }
                    else if (GetLiteralType(node.Type) == GetLiteralType(type))
                    {
                        var valueNode = node;
                        
                        node = Step();
                        if (node.Type != TokenType.StatementTerminator)
                            throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);

                        if (parent?.Type == TokenType.StatementBody)
                        {
                            var pointerLength = PrimitiveVariables.GetLength(type) + _lengthCounter;
                            _lengthCounter++;
                            
                            var pointer = type != "string" ? $"[rbp-{pointerLength}]" : $".LC{_stringCounter}";
                            
                            _currentFunction.Variables.Add(new FunctionVariable(pointer, GetPointerPrefix(type, false), name, type, valueNode));
                            
                            _currentFunction.Block.Add(new AsmStatement("mov", $"{GetPointerParam(type)} PTR [rbp-{pointerLength}], {GetPointerPrefix(type, false)}{pointer}", true));
                            
                            if (type == "string") _stringCounter++;
                        }
                        else
                        {
                            DeclareClassVariable(name, valueNode, type, _statementKeywords.ToArray());
                        }

                        EndStatement();
                        break;
                    }

                    throw new CompilerException($"Unexpected token '{node.Value}'", CurrentLine);
                }
                else if (node.Type == TokenType.Expression)
                {
                    var functionParameters = node.Params;

                    node = Step();

                    if (node.Type == TokenType.StatementBody)
                    {
                        Function function = new(name, type, functionParameters, node.Params, node, _current);
                        _functions.Add(function);

                        EndStatement();
                        break;
                    }
                    if (node.Type != TokenType.StatementTerminator)
                        throw new CompilerException($"Unexpected token '{node.Value}'", CurrentLine);
                    
                    EndStatement();
                    break;
                }
                else
                    throw new CompilerException($"Unexpected token '{node.Value}'", CurrentLine);
            case TokenType.ReturnStatement:
                if (_currentFunction == null)
                    throw new CompilerException("Return statements must be inside a function", CurrentLine);
                
                node = Step();

                if (_currentFunction.ReturnType != "void")
                {
                    switch (node.Type)
                    {
                        case TokenType.Name:
                            var variable = _currentFunction.Variables.Find(f => f.Name == node.Value);
                            if (variable == null)
                                throw new CompilerException(
                                    $"Cannot resolve symbol '{node.Value}'", CurrentLine);

                            if (_currentFunction.ReturnType == variable.Type)
                            {
                                _currentFunction.Block.Add(new AsmStatement("mov", $"eax, {variable.PointerName}", true));
                                
                                Step(true);
                                
                                _functionHasReturnStatement = true;
                                EndFunction();
                                break;
                            }
                            else
                                throw new CompilerException(
                                    $"Function with return type '{_currentFunction.ReturnType}' cannot have a return statement with typed '{node.Value}'", CurrentLine);
                        default:
                            if (GetLiteralType(_currentFunction.ReturnType) == GetLiteralType(node.Type))
                            {
                                _currentFunction.Block.Add(new AsmStatement("mov", $"eax, {node.Value}", true));
                                
                                Step(true);

                                _functionHasReturnStatement = true;
                                EndFunction();
                                break;
                            }
                            else
                                throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                    }
                }
                else if (node.Type != TokenType.StatementTerminator)
                {
                    throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                }
                _functionHasReturnStatement = true;
                break;
            default:
                throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
        }
    }

    private ParserToken Step(bool throwNoLineTerm = false)
    {
        if (_current + 1 >= _abstractSyntaxTree.Count)
            throw new CompilerException("Unexpected token", CurrentLine);
        _current++;
        
        if (throwNoLineTerm && _abstractSyntaxTree[_current].Type != TokenType.StatementTerminator)
            throw new CompilerException("; expected", CurrentLine);

        return _abstractSyntaxTree[_current];
    }

    private void ParseFunction(Function function)
    {
        if (function.BlockTokens.Count == 0)
            return;
        _current = _abstractSyntaxTree.Count;
        _abstractSyntaxTree.AddRange(function.BlockTokens);
        _currentFunction = function;
        while (_current < _abstractSyntaxTree.Count && _currentFunction != null)
        {
            Parse(function.ParentBlockNode);
            _current++;
        }
        if (function.ReturnType != "void" && !_functionHasReturnStatement)
            throw new CompilerException($"Function of return type '{function.ReturnType}' must have a return statement", function.ParentBlockNode.Line);
        _currentFunction = null;
    }

    private Variable GetVariable(ParserToken node)
    {
        Variable v = _currentFunction.Variables.Find(f => f.Name == node.Value);

        if (v == null)
        {
            v = _variables.Find(f => f.Name == node.Value);
            if (v == null)
                v = _constVariables.Find(f => f.Name == node.Value);
        }

        return v;
    }

    private void EndStatement()
    {
        _statementKeywords.Clear();
    }
    
    private void EndFunction()
    {
        EndStatement();
        _currentFunction = null;
    }
    
    private LiteralType GetLiteralType(TokenType type)
    {
        switch (type)
        {
            case TokenType.StringLiteral:
                return LiteralType.String;
            case TokenType.BooleanLiteral:
                return LiteralType.Boolean;
            case TokenType.IntegerLiteral:
                return LiteralType.Number;
            case TokenType.FloatLiteral:
                return LiteralType.Float;
        }

        throw new CompilerException($"Invalid literal type '{type}'", CurrentLine);
    }

    private LiteralType GetLiteralType(string type)
    {
        var t = PrimitiveVariables.Parse(type);
        if (t != null)
            return t.LiteralType;

        throw new CompilerException($"Invalid literal type '{type}'", CurrentLine);
    }

    private void DeclareClassVariable(string variableName, ParserToken valueToken, string variableType, Keyword[] keywords)
    {
        if (_variables.Find(f => f.Name == variableName) == null)
        {
            if (keywords.Contains(Keyword.Const))
            {
                Variable variable = new(false, variableType, variableName, GetPointerPrefix(variableType, true), valueToken.Value, valueToken);
                _constVariables.Add(variable);
            }
            else
            {
                Variable variable = new(keywords.Contains(Keyword.Readonly), variableType, variableName, GetPointerPrefix(variableType, false), $"{variableName}[rip]", valueToken);
                _variables.Add(variable);
            }
        }
        else
            throw new CompilerException("Member with the same signature is already declared", CurrentLine);
    }

    private string GetPointerPrefix(string variableType, bool isConstVariable)
    {
        if (variableType != "string")
        {
            return isConstVariable ? "" : $"{GetPointerParam(variableType)} PTR ";
        }
        
        return isConstVariable ? "" : "OFFSET FLAT:";
    }

    private string GetPointerParam(string variableType) =>
        variableType switch
        {
            "char" => "BYTE",
            "string" => "QWORD",
            "bool" => "BYTE",
            "float" => "QWORD",
            "decimal" => "QWORD",
            "double" => "QWORD",
            "int" => "DWORD",
            "long" => "QWORD",
            "short" => "WORD",
            _ => throw new CompilerException($"Could not resolve type '{variableType}'", CurrentLine)
        };
}
