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

    private string CurrentFile => _abstractSyntaxTree[_current].File;
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

        foreach (var cLibrary in _broughtCLibraries)
        {
            _statements.Add(new Statement($"#include <{cLibrary}>"));
        }

        foreach (var function in _functions)
        {
            ParseFunction(function);
            _statements.Add(new Statement(function.ToString()));
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
            case TokenType.CStatement:
                _currentFunction.Block.Add(new Statement(node.Value));
                break;
            case TokenType.Keyword:
                if (parent != null || parent != null && parent.Type != TokenType.Keyword)
                    throw ThrowException($"Unexpected token '{node.Value}'");

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
                        throw ThrowException($"Unexpected token '{node.Value}'");
                    }
                    else if (node.Type == TokenType.Operator)
                    {
                        if (node.Value == "sizeof")
                        {
                            if (GetLiteralType(type) != LiteralType.Number)
                                throw ThrowException($"Can't convert from type '{type}' to a numeral type");

                            node = Step();
                            if (node.Type != TokenType.Expression)
                                throw ThrowException($"Unexpected token '{node.Value}'");

                            if (node.Params.Count != 1)
                                throw ThrowException($"Insufficient amount of parameters: 1 needed, {node.Params.Count} given");

                            if (node.Params[0].Type == TokenType.Name)
                            {
                                var variable = GetVariable(node.Params[0].Value);
                                if (variable == null || variable.Type == type)
                                    throw ThrowException($"Unexpected token '{node.Params[0].Value}'");
                            }
                            
                            _currentFunction.Variables.Add(new FunctionVariable(name, type, false));

                            _currentFunction.Block.Add(new Statement($"{type} {name} = sizeof({node.Params[0].Value});"));

                            EndStatement();
                            break;
                        }
                        throw ThrowException($"Unexpected token '{node.Value}'");
                    }
                    else if (GetLiteralType(node.Type) == GetLiteralType(type))
                    {
                        var valueNode = node;
                        
                        node = Step();
                        if (node.Type != TokenType.StatementTerminator)
                            throw ThrowException($"Unexpected token '{node.Value}'");

                        if (parent?.Type == TokenType.StatementBody)
                        {
                            var pointerLength = PrimitiveVariables.GetLength(type) + _lengthCounter;
                            _lengthCounter++;
                            
                            _currentFunction.Variables.Add(new FunctionVariable(name, type, false));

                            _currentFunction.Block.Add(type == "string"
                                ? new Statement($"char *{name} = \"{valueNode.Value}\";")
                                : new Statement($"{type} {name} = {valueNode.Value};"));

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
                        Function function = new(name, type, functionParameters, node.Params, node);
                        function.SetParameters();

                        if (_functions.Find(f => f.Name == name && f.HasSameParams(function.Params)) != null)
                            throw ThrowException($"Function named '{name}' with the parameters '{function.GetParamsString(true)}' is already defined");
                        
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
                            var variable = GetVariable(node.Value);
                            if (variable == null)
                                throw ThrowException(
                                    $"Cannot resolve symbol '{node.Value}'");

                            if (_currentFunction.ReturnType == variable.Type)
                            {
                                _currentFunction.Block.Add(new Statement($"return {variable.Name};", true));
                                
                                Step(true);
                                
                                _functionHasReturnStatement = true;
                                EndFunction();
                                break;
                            }
                            else
                                throw ThrowException(
                                    $"Function with return type '{_currentFunction.ReturnType}' cannot have a return statement with a type of '{variable.Type}'");
                        default:
                            if (GetLiteralType(_currentFunction.ReturnType) == GetLiteralType(node.Type))
                            {
                                if (GetLiteralType(_currentFunction.ReturnType) == LiteralType.String)
                                    _currentFunction.Block.Add(new Statement($"return \"{node.Value}\";", true));
                                else
                                    _currentFunction.Block.Add(new Statement($"return {node.Value};", true));
                                
                                Step(true);

                                _functionHasReturnStatement = true;
                                EndFunction();
                                break;
                            }
                            else
                                throw ThrowException($"Could not resolve symbol '{node.Value}'");
                    }
                    break;
                }
                else if (node.Type != TokenType.StatementTerminator)
                {
                    throw ThrowException($"Could not resolve symbol '{node.Value}'");
                }
                _currentFunction.Block.Add(new Statement("return 1;", true));
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
            case TokenType.Name:
                if (_currentFunction == null)
                    throw ThrowException($"Unexpected token '{node.Value}'");
                
                var nameVariable = GetVariable(node.Value);
                if (nameVariable != null)
                {
                    throw new NotImplementedException("variable set");
                }
                
                var nameFunction = _functions.Find(f => f.Name == node.Value);
                if (nameFunction != null)
                {
                    node = Step();
                    
                    if (node.Type != TokenType.Expression)
                        throw ThrowException($"Unexpected token '{node.Value}'");

                    var parameters = "";
                    var i = 0;

                    while (i < node.Params.Count)
                    {
                        if (node.Params[i].Type == TokenType.Name)
                        {
                            var paramVariable = GetVariable(node.Params[i].Value);
                            if (paramVariable == null)
                                throw ThrowException($"Unexpected token '{node.Params[i].Value}'");
                        
                            parameters += paramVariable.Name;

                            i++;

                            if (i >= node.Params.Count)
                                continue;
                        
                            parameters += ", ";
                            if (node.Params[i].Type != TokenType.Comma)
                                throw ThrowException($"Unexpected token '{node.Params[i].Value}'");
                            i++;
                        }
                        else
                        {
                            if (node.Params[i].Type == TokenType.StringLiteral)
                                parameters += $"\"{node.Params[i].Value}\"";
                            else
                                parameters += node.Params[i].Value;

                            i++;

                            if (i >= node.Params.Count)
                                continue;
                        
                            parameters += ", ";
                            if (node.Params[i].Type != TokenType.Comma)
                                throw ThrowException($"Unexpected token '{node.Params[i].Value}'");
                            i++;
                        }
                    }

                    _currentFunction.Block.Add(new Statement($"{nameFunction.Name}({parameters});"));
                }
                else
                    throw ThrowException($"Could not resolve symbol '{node.Value}'. Are you missing a library reference?");
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

    private Variable GetVariable(string name)
    {
        Variable v = _currentFunction.Variables.Find(f => f.Name == name);

        if (v != null)
            return v;
        {
            v = _variables.Find(f => f.Name == name) ?? _constVariables.Find(f => f.Name == name);
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
                Variable variable = new(false, variableType, variableName, true);
                _constVariables.Add(variable);
            }
            else
            {
                Variable variable = new(keywords.Contains(Keyword.Readonly), variableType, variableName);
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
        throw new CompilerException(message, CurrentFile, line <= 0 ? CurrentLine : line, CurrentLineIndex);
    }
}
