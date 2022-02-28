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

    public static int CurrentLine => _abstractSyntaxTree[_current].Line;

    private static int _current;

    private static Function _currentFunction;
    private static bool _functionHasReturnStatement = false;

    public static string ParseToAssembly(ParserToken abstractSyntaxTree)
    {
        _variables = new();
        _constVariables = new();
        _functions = new();
        _currentFunction = null;
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
        
        var node = _abstractSyntaxTree[_current];
        switch (node.Type)
        {
            case ParserTokenType.EndStatementToken:
                EndStatement();
                break;
            case ParserTokenType.Keyword:
                if (parent == null || parent.Type != ParserTokenType.Keyword)
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
                    
                    throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                }
                else
                    throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
            case ParserTokenType.ReturnType:
                switch (node.Value)
                {
                    case "void":
                        if (parent == null || parent.Type == ParserTokenType.Keyword)
                            ParseFunction(node);
                        else
                            throw new CompilerException("Return statement not expected", CurrentLine);
                        break;
                    default:
                        throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                }
                break;
            case ParserTokenType.ReturnStatement:
                if (_currentFunction == null)
                    throw new CompilerException("Return statements must be inside a function", CurrentLine);
                
                node = Step(node);

                if (_currentFunction.ReturnType != "void")
                {
                    switch (node.Type)
                    {
                        case ParserTokenType.Name:
                            var variable = _currentFunction.Variables.Find(f => f.Name == node.Value);
                            if (variable == null)
                                throw new CompilerException(
                                    $"Cannot resolve symbol '{node.Value}'", CurrentLine);

                            if (GetLiteralType(_currentFunction.ReturnType) == variable.Type.LiteralType)
                            {
                                _currentFunction.Block.Add(new("mov", $"eax, DWORD PTR {variable.PointerName}", true));
                                
                                Step(node, true);
                                
                                _functionHasReturnStatement = true;
                                EndFunction();
                                break;
                            }
                            else
                                throw new CompilerException(
                                    $"Cannot convert expression of type to '{node.Value}'", CurrentLine);
                        default:
                            if (GetLiteralType(_currentFunction.ReturnType) == GetLiteralType(node.Type))
                            {
                                _currentFunction.Block.Add(new("mov", $"eax, {node.Value}", true));
                                
                                Step(node, true);
                                
                                _functionHasReturnStatement = true;
                                EndFunction();
                                break;
                            }
                            else
                                throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                    }
                }
                else if (node.Type != ParserTokenType.EndStatementToken)
                {
                    throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                }
                _functionHasReturnStatement = true;
                break;
            case ParserTokenType.Name:
                if (parent == null || parent.Type != ParserTokenType.CodeBlock)
                    throw new CompilerException("Calling variables and defined functions must be done inside a function", CurrentLine);

                if (_currentFunction != null)
                {
                    Variable v = GetVariable(node);

                    if (v == null)
                        throw new CompilerException($"Cannot resolve symbol '{node.Value}'", CurrentLine);

                    LiteralType literalType = v.Type.LiteralType;
                    string  variableType = v.Type.Name;
                    
                    node = Step(node);

                    switch (node.Type)
                    {
                        case ParserTokenType.AssignmentOperator:
                            if (_currentFunction == null)
                                throw new CompilerException("Assignment statements must be inside a function", CurrentLine);

                            if (literalType == LiteralType.Boolean || literalType == LiteralType.Character)
                                throw new CompilerException($"Assignment operator cannot be used in variables typed '{variableType}", CurrentLine);

                            node = Step(node);

                            AsmMathAssignmentCollection statements = new();

                            while (node.Type != ParserTokenType.EndStatementToken)
                            {
                                switch (node.Type)
                                {
                                    case ParserTokenType.AdditionOperator:
                                        if (_currentFunction == null)
                                            throw new CompilerException("Addition statements must be inside a function", CurrentLine);

                                        node = Step(node);

                                        Variable expressionVar = GetVariable(node);

                                        if (expressionVar == null)
                                        {
                                            if (GetLiteralType(node.Type) != literalType)
                                                throw new CompilerException(
                                                    $"Cannot convert expression '{node.Value}' to a type of '{variableType}'", CurrentLine);
                                        }
                                        else
                                        {
                                            LiteralType eLiteralType = expressionVar.Type.LiteralType;
                                            string  eVariableType = expressionVar.Type.Name;
                                            if (GetLiteralType(node.Type) != eLiteralType)
                                                throw new CompilerException(
                                                    $"Cannot convert expression '{node.Value}' to a type of '{eVariableType}'", CurrentLine);

                                        }

                                        statements.Add(MathAssignmentType.Add, node);

                                        break;
                                    case ParserTokenType.MinusOperator:
                                        if (_currentFunction == null)
                                            throw new CompilerException("Addition statements must be inside a function", CurrentLine);
                                        break;
                                    default:
                                        throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                                }

                                node = Step(node);
                            }

                            break;
                        default:
                            throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
                    }
                    break;
                }
                throw new CompilerException("Function calls not implemented yet :/", CurrentLine);
            default:
                throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
        }
    }

    private static Variable GetVariable(ParserToken node)
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

    private static ParserToken Step(ParserToken node, bool checkEnd = false)
    {
        if (_current + 1 >= _abstractSyntaxTree.Count)
            throw new CompilerException("; expected", CurrentLine);
        _current++;

        if (checkEnd && node.Type != ParserTokenType.EndStatementToken)
            throw new CompilerException("; expected", CurrentLine);
        
        return _abstractSyntaxTree[_current];
    }

    private static void EndFunction()
    {
        EndStatement();
        _currentFunction = null;
    }

    private static void ParsePrimitive(ParserToken node, bool isInBlock)
    {
        if (isInBlock && _statementKeywords.Count > 0)
            throw new CompilerException("Statements inside blocks can't have keywords", CurrentLine);

        ParserToken typeNode = node;
        int typeNodeIndex = _current;
        PrimitiveTypeObject type = PrimitiveVariables.Parse(node.Value);

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("Variable name expected", CurrentLine);
        node = _abstractSyntaxTree[_current];
        if (_variables.Find(f => f.Name == node.Value) != null)
            throw new CompilerException($"Variable named '{node.Value}' is already declared", CurrentLine);
        if (_currentFunction != null && _currentFunction.Variables.Find(f => f.Name == node.Value) != null)
            throw new CompilerException($"Variable named '{node.Value}' is already declared inside '{_currentFunction.Name}'", CurrentLine);
        if (node.Type != ParserTokenType.Name)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);

        string variableName = node.Value;

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("; expected", CurrentLine);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.AssignmentOperator && node.Type != ParserTokenType.EndStatementToken && node.Type != ParserTokenType.CallExpression)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);
        if (node.Type == ParserTokenType.EndStatementToken)
        {
            EndStatement();
            return;
        }
        if (node.Type == ParserTokenType.AssignmentOperator)
        {
            _current++;
            if (_current >= _abstractSyntaxTree.Count)
                throw new CompilerException("Value for variable expected", CurrentLine);
            node = _abstractSyntaxTree[_current];
            if (type.LiteralType == GetLiteralType(node.Type))
            {
                if (isInBlock || _currentFunction != null)
                {
                    string pointer = $"[rbp-{type.ByteLength / 8}]";
                    _currentFunction.Variables.Add(new(pointer, variableName, type, node));
                    _currentFunction.Block.Add(new("mov", $"DWORD PTR {pointer}, {node.Value}"));
                }
                else
                {
                    DeclareClassVariable(variableName, node, type, _statementKeywords.ToArray());
                }
            }
            else
                throw new CompilerException(
                    $"Cannot convert expression of type to '{type.Name}'", CurrentLine);
        }
        else if (node.Type == ParserTokenType.CallExpression)
        {
            _current = typeNodeIndex;
            ParseFunction(typeNode);
            return;
        }
        
        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("; expected", CurrentLine);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.EndStatementToken)
            throw new CompilerException("; expected", CurrentLine);
    }

    private static void ParseFunction(ParserToken node)
    {
        if (_currentFunction != null)
            throw new CompilerException("Functions cannot be declared inside a function", CurrentLine);
        if (_statementKeywords.Contains(Keyword.Const))
            throw new CompilerException("Functions cannot be constant", CurrentLine);
        if (_statementKeywords.Contains(Keyword.Readonly))
            throw new CompilerException("Functions cannot have a readonly keyword", CurrentLine);
        
        string returnType = node.Value;

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("Function name expected", CurrentLine);
        node = _abstractSyntaxTree[_current];
        if (_functions.Find(f => f.Name == node.Value) != null)
            throw new CompilerException($"Function named '{node.Value}' is already defined", CurrentLine);
        if (node.Type != ParserTokenType.Name)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);

        string functionName = node.Value;

        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("( expected", CurrentLine);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.CallExpression)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);

        List<ParserToken> functionParameters = node.Params;
        
        _current++;
        if (_current >= _abstractSyntaxTree.Count)
            throw new CompilerException("{ expected", CurrentLine);
        node = _abstractSyntaxTree[_current];
        if (node.Type != ParserTokenType.CodeBlock)
            throw new CompilerException($"Could not resolve symbol '{node.Value}'", CurrentLine);

        List<ParserToken> functionBlock = node.Params;

        Function function = new(functionName, returnType, functionParameters, functionBlock, node, _current);
        _functions.Add(function);
        
        if (_current + 1 < _abstractSyntaxTree.Count && _abstractSyntaxTree[_current + 1].Type == ParserTokenType.EndStatementToken)
            throw new CompilerException("Unexpected token", _abstractSyntaxTree[_current + 1].Line);
    }
    
    private static LiteralType GetLiteralType(ParserTokenType nodeType)
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

        throw new CompilerException("Invalid literal type", CurrentLine);
    }

    private static LiteralType GetLiteralType(string type)
    {
        var t = PrimitiveVariables.Parse(type);
        if (t != null)
            return t.LiteralType;

        throw new CompilerException("Invalid literal type", CurrentLine);
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
                Variable variable = new(false, variableType, variableName, valueToken.Value, valueToken);
                _constVariables.Add(variable);
            }
            else
            {
                Variable variable = new(keywords.Contains(Keyword.Readonly), variableType, variableName, $"{GetPointerParam(variableType)} PTR {variableName}[rip]", valueToken);
                _constVariables.Add(variable);
            }
        }
        else
            throw new CompilerException("Member with the same signature is already declared", CurrentLine);
    }

    private static string GetPointerParam(PrimitiveTypeObject variableType)
    {
        switch (variableType.LiteralType)
        {
            case LiteralType.Character:
                return "BYTE";
            case LiteralType.String:
                return "QWORD";
            case LiteralType.Boolean:
                return "BYTE";
            case LiteralType.Float:
                return "QWORD";
            case LiteralType.Number:
                if (variableType.Name == "int")
                    return "DWORD";
                else if (variableType.Name == "long")
                    return "QWORD";
                else if (variableType.Name == "short")
                    return "WORD";
                else
                    throw new CompilerException($"Could not resolve type '{variableType.Name}'", CurrentLine);
            default:
                throw new CompilerException($"Could not resolve type '{variableType.Name}'", CurrentLine);
        }
    }

    private static void ParseFunctinon(Function function)
    {
        if (function.BlockTokens.Count == 0)
            return;
        _current = _abstractSyntaxTree.Count;
        _abstractSyntaxTree.AddRange(function.BlockTokens);
        _currentFunction = function;
        while (_current < _abstractSyntaxTree.Count && _currentFunction != null)
        {
            ParseLine(function.ParentBlockNode);
            _current++;
        }
        if (function.ReturnType != "void" && !_functionHasReturnStatement)
            throw new CompilerException($"Function of return type '{function.ReturnType}' must have a return statement", function.ParentBlockNode.Line);
        _currentFunction = null;
    }
}
