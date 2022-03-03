using System;
using System.Collections.Generic;
using System.Linq;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;
using Neptyne.Compiler.Models.Assembly;

namespace Neptyne.Compiler;

public class Compiler
{
    private readonly List<string> _broughtCLibraries = new();
    private readonly List<string> _broughtLibraries = new();
    private readonly List<Statement> _statements = new();
    private readonly List<Variable> _variables = new();
    private readonly List<Variable> _constVariables = new();
    private readonly List<Function> _functions = new();
    private List<ParserToken> _abstractSyntaxTree;

    private int CurrentLine => _abstractSyntaxTree[_current].Line;

    private int CurrentLineIndex => _abstractSyntaxTree[_current].LineIndex;
    private int _current;

    private Function _currentFunction;
    private bool _functionHasReturnStatement;

    private readonly List<Keyword> _statementKeywords = new();
    private int _stringCounter;
    private int _lengthCounter;

    public string Compile(ParserToken abstractSyntaxTree)
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
            throw ThrowException("Program does not contain a 'main' function suitable for an entry point", 1, 0);
        }

        foreach (var function in _functions)
        {
            ParseFunction(function);
        }

        return string.Join('\n', _statements);
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
                if (node.Value == "csta")
                {
                    List<ParserToken> tokens = new();
                    while (node.Type != TokenType.StatementTerminator)
                    {
                        node = Step();
                        tokens.Add(node);
                    }
                    var cStatement = CStatementFormatter.Format(tokens);
                    if (_currentFunction != null)
                    {
                        _currentFunction.Block.Add(new Statement(cStatement));
                    }
                    else
                    {
                        throw ThrowException("This statement cannot be called outside a function");
                    }
                    break;
                }
                if (parent != null || parent != null && parent.Type != TokenType.Keyword)
                    throw ThrowException($"Could not resolve symbol '{node.Value}'");

                switch (node.Value)
                {
                    case "const":
                        if (_statementKeywords.Contains(Keyword.Readonly))
                            throw ThrowException("Readonly is not valid for constant");
                        _statementKeywords.Add(Keyword.Const);
                        break;
                    case "readonly":
                        if (_statementKeywords.Contains(Keyword.Const))
                            throw ThrowException("Readonly is not valid for constant");
                        _statementKeywords.Add(Keyword.Readonly);
                        break;
                    default:
                        throw ThrowException($"Could not resolve symbol '{node.Value}'");
                }
                _current++;
                Parse(node);
                break;
            case TokenType.Identifier:
                var type = node.Value;
                
                node = Step();
                
                if (node.Type != TokenType.Name)
                    throw ThrowException("Unexpected token");
                
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
                            throw ThrowException($"Could not resolve symbol '{node.Value}'");

                        if (parent?.Type == TokenType.StatementBody)
                        {
                            var pointerLength = PrimitiveVariables.GetLength(type) + _lengthCounter;
                            _lengthCounter++;
                            
                            var pointer = type != "string" ? $"[rbp-{pointerLength}]" : $".LC{_stringCounter}";
                            
                            _currentFunction.Variables.Add(new FunctionVariable(pointer, GetPointerPrefix(type, false), name, type, valueNode));
                            
                            _currentFunction.Block.Add(new Statement());
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

                    throw ThrowException($"Unexpected token '{node.Value}'");
                }
                else if (node.Type is TokenType.Expression or TokenType.Colon)
                {
                    var functionParameters = node.Type == TokenType.Colon ? new List<ParserToken>() : node.Params;

                    if (node.Type != TokenType.Colon)
                    {
                        node = Step();
                        if (node.Type != TokenType.Colon)
                            throw ThrowException(": expected");
                    }

                    node = Step();
                    
                    if (node.Type == TokenType.StatementBody)
                    {
                        Function function = new(name, type, functionParameters, node.Params, node, _current);
                        _functions.Add(function);

                        EndStatement();
                        break;
                    }
                    if (node.Type != TokenType.StatementTerminator)
                        throw ThrowException($"Unexpected token '{node.Value}'");
                    
                    EndStatement();
                    break;
                }
                else
                    throw ThrowException($"Unexpected token '{node.Value}'");
            case TokenType.ReturnStatement:
                if (_currentFunction == null)
                    throw ThrowException("Return statements must be inside a function");
                
                node = Step();

                if (_currentFunction.ReturnType != "void")
                {
                    switch (node.Type)
                    {
                        case TokenType.Name:
                            var variable = _currentFunction.Variables.Find(f => f.Name == node.Value);
                            if (variable == null)
                                throw ThrowException(
                                    $"Cannot resolve symbol '{node.Value}'");

                            if (_currentFunction.ReturnType == variable.Type)
                            {
                                _currentFunction.Block.Add(new AsmStatement("mov", $"eax, {variable.PointerName}", true));
                                
                                Step(true);
                                
                                _functionHasReturnStatement = true;
                                EndFunction();
                                break;
                            }
                            else
                                throw ThrowException(
                                    $"Function with return type '{_currentFunction.ReturnType}' cannot have a return statement with typed '{node.Value}'");
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
                                throw ThrowException($"Could not resolve symbol '{node.Value}'");
                    }
                }
                else if (node.Type != TokenType.StatementTerminator)
                {
                    throw ThrowException($"Could not resolve symbol '{node.Value}'");
                }
                _functionHasReturnStatement = true;
                break;
            case TokenType.StatementIdentifier:
                switch (node.Value)
                {
                    case "bring":
                        node = Step();

                        if (node.Type != TokenType.Name)
                        {
                            if (node.Type != TokenType.Keyword || node.Value != "clib")
                            {
                                throw ThrowException($"Unexpected token '{node.Value}'");
                            }
                            
                            node = Step();
                                
                            if (node.Type != TokenType.StringLiteral)
                                throw ThrowException($"Unexpected token '{node.Value}'");
                                
                            _broughtCLibraries.Add(node.Value);
                            Step(true);
                            break;
                        }

                        if (_broughtLibraries.Contains(node.Value))
                            throw ThrowException($"Library is already brought '{node.Value}'");
                        
                        _broughtLibraries.Add(node.Value);

                        var libTokens = Libraries.Bring(node.Value, node);
                        _abstractSyntaxTree.InsertRange(CurrentLine + 2, libTokens);

                        Step(true);
                        break;
                    default:
                        throw ThrowException($"Could not resolve symbol '{node.Value}'");
                }
                break;
            default:
                throw ThrowException($"Could not resolve symbol '{node.Value}'");
        }
    }

    private ParserToken Step(bool throwNoLineTerm = false)
    {
        if (_current + 1 >= _abstractSyntaxTree.Count)
            throw ThrowException("Unexpected token");
        _current++;
        
        if (throwNoLineTerm && _abstractSyntaxTree[_current].Type != TokenType.StatementTerminator)
            throw ThrowException("; expected");

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
            throw ThrowException($"Function of return type '{function.ReturnType}' must have a return statement", function.ParentBlockNode.Line, function.ParentBlockNode.LineIndex);
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

        throw ThrowException($"Invalid literal type '{type}'");
    }

    private LiteralType GetLiteralType(string type)
    {
        var t = PrimitiveVariables.Parse(type);
        if (t != null)
            return t.LiteralType;

        throw ThrowException($"Invalid literal type '{type}'");
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
            throw ThrowException("Member with the same signature is already declared");
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
            _ => throw ThrowException($"Could not resolve type '{variableType}'")
        };

    private Exception ThrowException(string message, int line = 0, int lineIndex = -1)
    {
        throw new CompilerException(message, _abstractSyntaxTree[0].File, line <= 0 ? CurrentLine : line, CurrentLineIndex);
    }
}
