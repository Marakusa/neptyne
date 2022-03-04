using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public static class Tokenizer
{
    private static int _lineIndex = 1;
    private static int _row = 1;
    
    public static Token[] Tokenize(string inputExpression, string name)
    {
        _lineIndex = 1;
        _row = 1;
        var tokens = new List<Token>();

        const string numberRegex = @"[0-9]";
        const string floatRegex = @"[+-]?([0-9]*[.])?[0-9]+";
        const string alphabetRegex = @"[a-zA-Z]";
        const string whitespaceRegex = @"\s";

        for (var i = 0; i < inputExpression.Length; i++)
        {
            var c = inputExpression[i];

            #region Parse comments and whitespaces
            if (Regex.IsMatch(c.ToString(), whitespaceRegex))
            {
                if (c == '\n')
                {
                    _row++;
                    _lineIndex = 1;
                }
                continue;
            }
            
            if (c == '/')
            {
                if (i + 1 >= inputExpression.Length)
                    continue;
                i = IncrementIndex(i);
                    
                c = inputExpression[i];
                switch (c)
                {
                    case '/':
                    {
                        while (i + 1 < inputExpression.Length)
                        {
                            i = IncrementIndex(i);
                            
                            c = inputExpression[i];
                            if (c != '\n')
                                continue;
                            _row++;
                            _lineIndex = 1;
                            break;
                        }
                        break;
                    }
                    case '*':
                    {
                        while (i + 1 < inputExpression.Length)
                        {
                            i = IncrementIndex(i);
                            
                            c = inputExpression[i];
                            if (c != '*')
                                continue;
                            i = IncrementIndex(i);
                                
                            c = inputExpression[i];
                            if (c == '/')
                            {
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            #endregion
            
            // Parentheses
            else if (c == '(')
            {
                tokens.Add(new Token(TokenType.OpenParentheses, c.ToString(), _row, _lineIndex, name));
            }
            else if (c == ')')
            {
                tokens.Add(new Token(TokenType.CloseParentheses, c.ToString(), _row, _lineIndex, name));
            }
            
            // Braces
            else if (c == '{')
            {
                tokens.Add(new Token(TokenType.OpenBraces, c.ToString(), _row, _lineIndex, name));
            }
            else if (c == '}')
            {
                tokens.Add(new Token(TokenType.CloseBraces, c.ToString(), _row, _lineIndex, name));
            }
            
            // Brackets
            else if (c == '[')
            {
                tokens.Add(new Token(TokenType.OpenBrackets, c.ToString(), _row, _lineIndex, name));
            }
            else if (c == ']')
            {
                tokens.Add(new Token(TokenType.CloseBrackets, c.ToString(), _row, _lineIndex, name));
            }
            
            // String literal
            else if (c == '"')
            {
                var value = "";
                while (i + 1 < inputExpression.Length)
                {
                    i = IncrementIndex(i);
                    
                    c = inputExpression[i];
                    if (c == '\\')
                    {
                        value += c.ToString();
                        i = IncrementIndex(i);
                        
                        c = inputExpression[i];
                        value += c.ToString();
                    }
                    else if (c == '"')
                    {
                        tokens.Add(new Token(TokenType.StringLiteral, value, _row, _lineIndex, name));
                        break;
                    }
                    else
                        value += c.ToString();
                }
            }
            
            // Equals sign tokens
            else if (c == '=')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Comparison operator
                    if (inputExpression[i + 1] == '=')
                    {
                        i = IncrementIndex(i);
                        
                        tokens.Add(new Token(TokenType.EqualityOperator, "==", _row, _lineIndex, name));
                        continue;
                    }
                }
                tokens.Add(new Token(TokenType.AssignmentOperator, c.ToString(), _row, _lineIndex, name));
            }
            
            // Addition operator
            else if (c == '+')
            {
                if (i + 1 < inputExpression.Length)
                {
                    switch (inputExpression[i + 1])
                    {
                        // Addition assignment operator
                        case '=':
                            i = IncrementIndex(i);
                        
                            tokens.Add(new Token(TokenType.AdditionAssignmentOperator, "+=", _row, _lineIndex, name));
                            continue;
                        // Increment operator
                        case '+':
                            i = IncrementIndex(i);
                        
                            tokens.Add(new Token(TokenType.IncrementOperator, "++", _row, _lineIndex, name));
                            continue;
                    }

                }
                tokens.Add(new Token(TokenType.AdditionOperator, c.ToString(), _row, _lineIndex, name));
            }
            
            // Subtraction operator
            else if (c == '-')
            {
                if (i + 1 < inputExpression.Length)
                {
                    switch (inputExpression[i + 1])
                    {
                        // Subtraction assignment operator
                        case '=':
                            i = IncrementIndex(i);
                        
                            tokens.Add(new Token(TokenType.SubtractionAssignmentOperator, "-=", _row, _lineIndex, name));
                            continue;
                        // Increment operator
                        case '-':
                            i = IncrementIndex(i);
                        
                            tokens.Add(new Token(TokenType.DecrementOperator, "--", _row, _lineIndex, name));
                            continue;
                    }

                }
                tokens.Add(new Token(TokenType.SubtractionOperator, c.ToString(), _row, _lineIndex, name));
            }
            
            // Multiplication operator
            else if (c == '*')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Multiplication assignment operator
                    if (inputExpression[i + 1] == '=')
                    {
                        i = IncrementIndex(i);
                        
                        tokens.Add(new Token(TokenType.MultiplicationAssignmentOperator, "*=", _row, _lineIndex, name));
                        continue;
                    }
                }
                tokens.Add(new Token(TokenType.MultiplicationOperator, c.ToString(), _row, _lineIndex, name));
            }
            
            // Division operator
            else if (c == '/')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Division assignment operator
                    if (inputExpression[i + 1] == '=')
                    {
                        i = IncrementIndex(i);
                        
                        tokens.Add(new Token(TokenType.DivisionAssignmentOperator, "/=", _row, _lineIndex, name));
                        continue;
                    }
                }
                tokens.Add(new Token(TokenType.DivisionOperator, c.ToString(), _row, _lineIndex, name));
            }
            
            // Logical AND operator
            else if (c == '&')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Division assignment operator
                    if (inputExpression[i + 1] == '&')
                    {
                        i = IncrementIndex(i);
                        
                        tokens.Add(new Token(TokenType.LogicalAndOperator, "&&", _row, _lineIndex, name));
                        continue;
                    }
                }
                throw new CompilerException($"Cannot resolve symbol '{c}'", name, _row, _lineIndex);
            }

            // Logical OR operator
            else if (c == '|')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Division assignment operator
                    if (inputExpression[i + 1] == '|')
                    {
                        i = IncrementIndex(i);
                        
                        tokens.Add(new Token(TokenType.LogicalOrOperator, "||", _row, _lineIndex, name));
                        continue;
                    }
                }
                throw new CompilerException($"Cannot resolve symbol '{c}'", name, _row, _lineIndex);
            }

            // Logical NOT operator
            else if (c == '!')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Logical NOT assignment operator
                    if (inputExpression[i + 1] == '=')
                    {
                        i = IncrementIndex(i);
                        
                        tokens.Add(new Token(TokenType.LogicalOrOperator, "!=", _row, _lineIndex, name));
                        continue;
                    }
                }
                
                tokens.Add(new Token(TokenType.LogicalNotOperator, c.ToString(), _row, _lineIndex, name));
            }

            // Statement terminator
            else if (c == ';')
            {
                tokens.Add(new Token(TokenType.StatementTerminator, c.ToString(), _row, _lineIndex, name));
            }
            
            // Colon
            else if (c == ':')
            {
                tokens.Add(new Token(TokenType.Colon, c.ToString(), _row, _lineIndex, name));
            }
            
            // Comma
            else if (c == ',')
            {
                tokens.Add(new Token(TokenType.Comma, c.ToString(), _row, _lineIndex, name));
            }

            // Point
            else if (c == '.')
            {
                if (i + 2 < inputExpression.Length)
                {
                    // Parameter pack
                    if (inputExpression[i + 1] == '.' && inputExpression[i + 2] == '.')
                    {
                        i = IncrementIndex(i);
                        i = IncrementIndex(i);
                        
                        tokens.Add(new Token(TokenType.ParameterPack, "...", _row, _lineIndex, name));
                        continue;
                    }
                }
                tokens.Add(new Token(TokenType.Point, c.ToString(), _row, _lineIndex, name));
            }

            // Integer literal
            else if (Regex.IsMatch(c.ToString(), numberRegex))
            {
                var value = "";
                
                value += c.ToString();
                
                while (i + 1 < inputExpression.Length && Regex.IsMatch(inputExpression[i + 1].ToString(), numberRegex))
                {
                    i = IncrementIndex(i);
                    
                    c = inputExpression[i];
                    value += c.ToString();
                }

                tokens.Add(new Token(TokenType.IntegerLiteral, value, _row, _lineIndex, name));
            }
            
            // Other
            else if (Regex.IsMatch(c.ToString(), alphabetRegex))
            {
                var value = "";
                
                value += c.ToString();
                
                while (i + 1 < inputExpression.Length && Regex.IsMatch(inputExpression[i + 1].ToString(), alphabetRegex))
                {
                    i = IncrementIndex(i);
                    c = inputExpression[i];
                    value += c.ToString();
                }
            
                // Float literal
                if (Regex.IsMatch(value, floatRegex))
                {
                    tokens.Add(new Token(TokenType.FloatLiteral, value, _row, _lineIndex, name));
                    continue;
                }

                switch (value)
                {
                    case "true":
                    case "false":
                        tokens.Add(new Token(TokenType.BooleanLiteral, value, _row, _lineIndex, name));
                        break;

                    case "csta":
                        var cStatementValue = "";

                        i = IncrementIndex(i);
                        c = inputExpression[i];
                        
                        while (i + 1 < inputExpression.Length && inputExpression[i] != ';')
                        {
                            i = IncrementIndex(i);
                            c = inputExpression[i];
                            cStatementValue += c.ToString();
                        }

                        tokens.Add(new Token(TokenType.CStatement, cStatementValue, _row, _lineIndex, name));
                        break;
                    
                    case "clib":
                    case "const":
                    case "readonly":
                        tokens.Add(new Token(TokenType.Keyword, value, _row, _lineIndex, name));
                        break;

                    case "byte":
                    case "char":
                    case "short":
                    case "ushort":
                    case "int":
                    case "uint":
                    case "long":
                    case "ulong":
                    case "float":
                    case "string":
                    case "void":
                        tokens.Add(new Token(TokenType.Identifier, value, _row, _lineIndex, name));
                        break;
                    
                    case "bring":
                        tokens.Add(new Token(TokenType.StatementIdentifier, value, _row, _lineIndex, name));
                        break;
                    
                    case "if":
                    case "else":
                    case "while":
                        tokens.Add(new Token(TokenType.StatementIdentifier, value, _row, _lineIndex, name));
                        break;
                    
                    case "sizeof":
                        tokens.Add(new Token(TokenType.Operator, value, _row, _lineIndex, name));
                        break;
                    
                    case "return":
                        tokens.Add(new Token(TokenType.ReturnStatement, value, _row, _lineIndex, name));
                        break;
                    
                    default:
                        tokens.Add(new Token(TokenType.Name, value, _row, _lineIndex, name));
                        break;
                }
            }
            else
            {
                throw new CompilerException($"Cannot resolve symbol '{c}'", name, _row, _lineIndex);
            }
        }

        return tokens.ToArray();
    }

    private static int IncrementIndex(int i)
    {
        i++;
        _lineIndex++;
        return i;
    }
}
