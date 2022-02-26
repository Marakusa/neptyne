using System.Collections.Generic;
using System.Text.RegularExpressions;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public static class Tokenizer
{
    public static Token[] Tokenize(string inputExpression)
    {
        List<Token> tokens = new List<Token>();

        const string numberRegex = @"[0-9]";
        const string alphabetRegex = @"[a-zA-Z]";
        const string whitespaceRegex = @"\s";

        int row = 1;

        for (int i = 0; i < inputExpression.Length; i++)
        {
            char c = inputExpression[i];
            
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
            else if (c == '(')
            {
                tokens.Add(new(TokenType.OpenParenthesis, c.ToString(), row));
            }
            else if (c == ')')
            {
                tokens.Add(new(TokenType.CloseParenthesis, c.ToString(), row));
            }
            else if (c == '{')
            {
                tokens.Add(new(TokenType.OpenCurlyBrackets, c.ToString(), row));
            }
            else if (c == '}')
            {
                tokens.Add(new(TokenType.CloseCurlyBrackets, c.ToString(), row));
            }
            else if (c == '"')
            {
                string value = "";
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
                        tokens.Add(new(TokenType.String, value, row));
                        break;
                    }
                    else
                        value += c.ToString();
                }
            }
            else if (c == '=')
            {
                tokens.Add(new(TokenType.EqualsSign, c.ToString(), row));
            }
            else if (c == ';')
            {
                tokens.Add(new(TokenType.Semicolon, c.ToString(), row));
            }
            else if (c == ':')
            {
                tokens.Add(new(TokenType.Point, c.ToString(), row));
            }
            else if (Regex.IsMatch(c.ToString(), numberRegex))
            {
                string value = "";
                
                value += c.ToString();
                
                while (i + 1 < inputExpression.Length && Regex.IsMatch(inputExpression[i + 1].ToString(), numberRegex))
                {
                    i++;
                    c = inputExpression[i];
                    value += c.ToString();
                }

                tokens.Add(new(TokenType.Number, value, row));
            }
            else if (Regex.IsMatch(c.ToString(), alphabetRegex))
            {
                string value = "";
                
                value += c.ToString();
                
                while (i + 1 < inputExpression.Length && Regex.IsMatch(inputExpression[i + 1].ToString(), alphabetRegex))
                {
                    i++;
                    c = inputExpression[i];
                    value += c.ToString();
                }

                switch (value)
                {
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
                        tokens.Add(new(TokenType.PrimitiveType, value, row));
                        break;

                    case "string":
                    case "void":
                        tokens.Add(new(TokenType.Type, value, row));
                        break;
                    
                    case "if":
                    case "else":
                    case "while":
                        tokens.Add(new(TokenType.Statement, value, row));
                        break;
                    
                    case "return":
                        tokens.Add(new(TokenType.ReturnStatement, value, row));
                        break;
                    
                    case "true":
                    case "false":
                        tokens.Add(new(TokenType.Boolean, value, row));
                        break;
                    
                    default:
                        if (value.EndsWith("f"))
                            tokens.Add(new(TokenType.Float, value, row));
                        else
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
