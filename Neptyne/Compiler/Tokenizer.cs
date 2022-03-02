using System.Collections.Generic;
using System.Text.RegularExpressions;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public static class Tokenizer
{
    public static Token[] Tokenize(string inputExpression)
    {
        var tokens = new List<Token>();

        const string numberRegex = @"[0-9]";
        const string floatRegex = @"[+-]?([0-9]*[.])?[0-9]+";
        const string alphabetRegex = @"[a-zA-Z]";
        const string whitespaceRegex = @"\s";

        var row = 1;

        for (var i = 0; i < inputExpression.Length; i++)
        {
            var c = inputExpression[i];

            #region Parse comments and whitespaces
            if (Regex.IsMatch(c.ToString(), whitespaceRegex))
            {
                if (c == '\n') row++;
                continue;
            }
            
            if (c == '/')
            {
                if (i + 1 < inputExpression.Length)
                {
                    i++;
                    c = inputExpression[i];
                    if (c == '/')
                    {
                        while (i + 1 < inputExpression.Length)
                        {
                            i++;
                            c = inputExpression[i];
                            if (c == '\n')
                            {
                                row++;
                                break;
                            }
                        }
                    }
                    else if (c == '*')
                    {
                        while (i + 1 < inputExpression.Length)
                        {
                            i++;
                            c = inputExpression[i];
                            if (c == '*')
                            {
                                i++;
                                c = inputExpression[i];
                                if (c == '/')
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            
            // Parentheses
            else if (c == '(')
            {
                tokens.Add(new(TokenType.OpenParentheses, c.ToString(), row));
            }
            else if (c == ')')
            {
                tokens.Add(new(TokenType.CloseParentheses, c.ToString(), row));
            }
            
            // Braces
            else if (c == '{')
            {
                tokens.Add(new(TokenType.OpenBraces, c.ToString(), row));
            }
            else if (c == '}')
            {
                tokens.Add(new(TokenType.CloseBraces, c.ToString(), row));
            }
            
            // Brackets
            else if (c == '[')
            {
                tokens.Add(new(TokenType.OpenBrakcets, c.ToString(), row));
            }
            else if (c == ']')
            {
                tokens.Add(new(TokenType.CloseBrakcets, c.ToString(), row));
            }
            
            // String literal
            else if (c == '"')
            {
                var value = "";
                while (i + 1 < inputExpression.Length)
                {
                    i++;
                    c = inputExpression[i];
                    if (c == '\\')
                    {
                        value += c.ToString();
                        i++;
                        c = inputExpression[i];
                        value += c.ToString();
                    }
                    else if (c == '"')
                    {
                        tokens.Add(new(TokenType.StringLiteral, value, row));
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
                        i++;
                        tokens.Add(new(TokenType.EqualityOperator, "==", row));
                        continue;
                    }
                }
                tokens.Add(new(TokenType.AssignmentOperator, c.ToString(), row));
            }
            
            // Addition operator
            else if (c == '+')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Addition assignment operator
                    if (inputExpression[i + 1] == '=')
                    {
                        i++;
                        tokens.Add(new(TokenType.AdditionAssignmentOperator, "+=", row));
                        continue;
                    }
                    
                    // Increment operator
                    if (inputExpression[i + 1] == '+')
                    {
                        i++;
                        tokens.Add(new(TokenType.IncrementOperator, "++", row));
                        continue;
                    }
                }
                tokens.Add(new(TokenType.AdditionOperator, c.ToString(), row));
            }
            
            // Subtraction operator
            else if (c == '-')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Subtraction assignment operator
                    if (inputExpression[i + 1] == '=')
                    {
                        i++;
                        tokens.Add(new(TokenType.SubtractionAssignmentOperator, "-=", row));
                        continue;
                    }
                    
                    // Increment operator
                    if (inputExpression[i + 1] == '-')
                    {
                        i++;
                        tokens.Add(new(TokenType.DecrementOperator, "--", row));
                        continue;
                    }
                }
                tokens.Add(new(TokenType.SubtractionOperator, c.ToString(), row));
            }
            
            // Multiplication operator
            else if (c == '*')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Multiplication assignment operator
                    if (inputExpression[i + 1] == '=')
                    {
                        i++;
                        tokens.Add(new(TokenType.MultiplicationAssignmentOperator, "*=", row));
                        continue;
                    }
                }
                tokens.Add(new(TokenType.MultiplicationOperator, c.ToString(), row));
            }
            
            // Division operator
            else if (c == '/')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Division assignment operator
                    if (inputExpression[i + 1] == '=')
                    {
                        i++;
                        tokens.Add(new(TokenType.DivisionAssignmentOperator, "/=", row));
                        continue;
                    }
                }
                tokens.Add(new(TokenType.DivisionOperator, c.ToString(), row));
            }
            
            // Logical AND operator
            else if (c == '&')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Division assignment operator
                    if (inputExpression[i + 1] == '&')
                    {
                        i++;
                        tokens.Add(new(TokenType.LogicalAndOperator, "&&", row));
                        continue;
                    }
                }
                throw new CompilerException($"Cannot resolve symbol '{c}'", row);
            }

            // Logical OR operator
            else if (c == '|')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Division assignment operator
                    if (inputExpression[i + 1] == '|')
                    {
                        i++;
                        tokens.Add(new(TokenType.LogicalOrOperator, "||", row));
                        continue;
                    }
                }
                throw new CompilerException($"Cannot resolve symbol '{c}'", row);
            }

            // Logical NOT operator
            else if (c == '!')
            {
                if (i + 1 < inputExpression.Length)
                {
                    // Logical NOT assignment operator
                    if (inputExpression[i + 1] == '=')
                    {
                        i++;
                        tokens.Add(new(TokenType.LogicalOrOperator, "!=", row));
                        continue;
                    }
                }
                
                tokens.Add(new(TokenType.LogicalNotOperator, c.ToString(), row));
            }

            // Statement terminator
            else if (c == ';')
            {
                tokens.Add(new(TokenType.StatementTerminator, c.ToString(), row));
            }
            
            // Colon
            else if (c == ':')
            {
                tokens.Add(new(TokenType.Colon, c.ToString(), row));
            }
            
            // Comma
            else if (c == ',')
            {
                tokens.Add(new(TokenType.Comma, c.ToString(), row));
            }

            // Point
            else if (c == '.')
            {
                tokens.Add(new(TokenType.Point, c.ToString(), row));
            }

            // Integer literal
            else if (Regex.IsMatch(c.ToString(), numberRegex))
            {
                var value = "";
                
                value += c.ToString();
                
                while (i + 1 < inputExpression.Length && Regex.IsMatch(inputExpression[i + 1].ToString(), numberRegex))
                {
                    i++;
                    c = inputExpression[i];
                    value += c.ToString();
                }

                tokens.Add(new(TokenType.IntegerLiteral, value, row));
            }
            
            // Other
            else if (Regex.IsMatch(c.ToString(), alphabetRegex))
            {
                var value = "";
                
                value += c.ToString();
                
                while (i + 1 < inputExpression.Length && Regex.IsMatch(inputExpression[i + 1].ToString(), alphabetRegex))
                {
                    i++;
                    c = inputExpression[i];
                    value += c.ToString();
                }
            
                // Float literal
                if (Regex.IsMatch(value, floatRegex))
                {
                    tokens.Add(new(TokenType.FloatLiteral, value, row));
                    continue;
                }

                switch (value)
                {
                    case "true":
                    case "false":
                        tokens.Add(new(TokenType.BooleanLiteral, value, row));
                        break;

                    case "const":
                    case "readonly":
                        tokens.Add(new(TokenType.Keyword, value, row));
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
                        tokens.Add(new(TokenType.Identifier, value, row));
                        break;
                    
                    case "bring":
                    case "if":
                    case "else":
                    case "while":
                        tokens.Add(new(TokenType.StatementIdentifier, value, row));
                        break;
                    
                    case "return":
                        tokens.Add(new(TokenType.ReturnStatement, value, row));
                        break;
                    
                    default:
                        tokens.Add(new(TokenType.Name, value, row));
                        break;
                }
            }
            else
            {
                throw new CompilerException($"Cannot resolve symbol '{c}'", row);
            }
        }

        return tokens.ToArray();
    }
}
