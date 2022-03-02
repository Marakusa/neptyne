using System.Collections.Generic;
using System.Linq;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;
using Neptyne.Compiler.Models.Assembly;

namespace Neptyne.Compiler;

public class ParseToAsm
{
    private List<Variable> _variables = new();
    private List<Variable> _constVariables = new();
    private List<Function> _functions = new();
    private List<ParserToken> _abstractSyntaxTree;

    public int CurrentLine => _abstractSyntaxTree[_current].Line;
    private int _current;

    private Function _currentFunction;

    private List<Keyword> _statementKeywords = new();

    public string Start(ParserToken abstractSyntaxTree)
    {
        _abstractSyntaxTree = abstractSyntaxTree.Params;
        while (_current < _abstractSyntaxTree.Count)
        {
            Parse(null);
            _current++;
        }

        foreach (var function in _functions)
        {
            //ParseFunctinon(function);
        }

        var main = _functions.Find(f => f.Name == "main");
        
        if (main == null)
        {
            throw new CompilerException("Program does not contain a 'main' function suitable for an entry point", 1);
        }

        AsmScript assemblyCode = new();
        assemblyCode.Variables = _variables;
        assemblyCode.Functions = _functions; 
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
                        var value = node.Value;
                        
                        node = Step();
                        if (node.Type != TokenType.StatementTerminator)
                            throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                        
                        //DefineVariable(name, type, value);

                        EndStatement();
                        break;
                    }

                    throw new CompilerException($"Unexpected token '{node.Value}'", CurrentLine);
                }
                else if (node.Type == TokenType.Expression)
                {
                    node = Step();

                    if (node.Type == TokenType.StatementBody)
                    {
                        Function function = new(name, type, paramsTokens, blockTokens, node, );

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
            default:
                throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
        }
    }

    private ParserToken Step()
    {
        if (_current + 1 >= _abstractSyntaxTree.Count)
            throw new CompilerException("Unexpected token", CurrentLine);
        _current++;
        return _abstractSyntaxTree[_current];
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

        throw new CompilerException("Invalid literal type", CurrentLine);
    }

    private LiteralType GetLiteralType(string type)
    {
        var t = PrimitiveVariables.Parse(type);
        if (t != null)
            return t.LiteralType;

        throw new CompilerException("Invalid literal type", CurrentLine);
    }

    private void DeclareClassVariable(string variableName, ParserToken valueToken, PrimitiveTypeObject variableType, Keyword[] keywords)
    {
        if (_variables.Find(f => f.Name == variableName) == null)
        {
            if (keywords.Contains(Keyword.Const))
            {
                Variable variable = new(false, variableType, variableName, "", valueToken.Value, valueToken);
                _constVariables.Add(variable);
            }
            else
            {
                Variable variable = new(keywords.Contains(Keyword.Readonly), variableType, variableName, $"{GetPointerParam(variableType)} PTR ", $"{variableName}[rip]", valueToken);
                _variables.Add(variable);
            }
        }
        else
            throw new CompilerException("Member with the same signature is already declared", CurrentLine);
    }

    private string GetPointerParam(PrimitiveTypeObject variableType)
    {
        return variableType.LiteralType switch
        {
            LiteralType.Character => "BYTE",
            LiteralType.String => "QWORD",
            LiteralType.Boolean => "BYTE",
            LiteralType.Float => "QWORD",
            LiteralType.Number => variableType.Name switch
            {
                "int" => "DWORD",
                "long" => "QWORD",
                "short" => "WORD",
                _ => throw new CompilerException($"Could not resolve type '{variableType.Name}'", CurrentLine)
            },
            _ => throw new CompilerException($"Could not resolve type '{variableType.Name}'", CurrentLine)
        };
    }
}
