using System.Collections.Generic;
using System.Linq;
using Neptyne.Compiler.Exceptions;

namespace Neptyne.Compiler.Models;

public class Function
{
    public string Name { get; }
    
    public string ReturnType { get; }
    
    public List<FunctionParameter> Params { get; }
    
    public List<ParserToken> ParamsTokens { get; }
    
    public List<Statement> Block { get; set; }
    
    public List<ParserToken> BlockTokens { get; }
    
    public ParserToken ParentBlockNode { get; }
    
    public List<FunctionVariable> Variables { get; set; }

    public Function(string name, string returnType, List<ParserToken> paramsTokens, List<ParserToken> block, ParserToken blockNode)
    {
        Name = name;
        ReturnType = returnType;
        Params = new List<FunctionParameter>();
        Block = new List<Statement>();
        ParamsTokens = paramsTokens;
        BlockTokens = block;
        ParentBlockNode = blockNode;
        Variables = new List<FunctionVariable>();
    }

    public string GetParamsString(bool onlyTypes = false)
    {
        if (!onlyTypes)
        {
            var p = "";

            for (var i = 0; i < Params.Count; i++)
            {
                p += $"{(Params[i].Constant ? "const " : "")}{(Params[i].Type == "string" ? "char *" : Params[i].Type)} {Params[i].Name}";
                if (i + 1 < Params.Count)
                    p += ",";
            }

            return $"({p})";
        }
        else
        {
            var p = "";

            for (var i = 0; i < Params.Count; i++)
            {
                p += $"{(Params[i].Constant ? "const " : "")}{Params[i].Type}";
                if (i + 1 < Params.Count)
                    p += ",";
            }

            return $"({p})";
        }
    }

    public override string ToString()
    {
        var result = "";
        
        result += $"{(ReturnType switch { "void" => "int", "string" => "char *", _ => ReturnType })} {Name}{GetParamsString()} {{\n";
        
        var containsReturn = false;
        foreach (var statement in Block)
        {
            result += $"    {statement}\n";
            containsReturn = statement.IsReturnStatement;
            if (containsReturn)
                break;
        }
        
        if (!containsReturn)
            result += "    return 1;\n";

        return $"{result}}}\n";
    }

    public void SetParameters()
    {
        var constant = false;
        string type = null;
        string value = null;
        var i = 0;
        while (i < ParamsTokens.Count)
        {
            if (ParamsTokens[i].Type == TokenType.Keyword && ParamsTokens[i].Value == "const")
            {
                constant = true;
                i++;
                continue;
            }

            if (string.IsNullOrEmpty(type))
            {
                if (ParamsTokens[i].Type == TokenType.ParameterPack)
                {
                    Params.Add(new FunctionParameter("", ParamsTokens[i].Value, false));
                    Variables.Add(new FunctionVariable(ParamsTokens[i].Value, "", false));
                    type = null;
                    value = null;
                    i++;
                    continue;
                }
                
                if (ParamsTokens[i].Type != TokenType.Identifier)
                    throw new CompilerException($"Unexpected token '{ParamsTokens[i].Value}'", ParamsTokens[i].File, ParamsTokens[i].Line,
                        ParamsTokens[i].LineIndex);
                type = ParamsTokens[i].Value;
                i++;
                continue;
            }
            if (string.IsNullOrEmpty(value))
            {
                if (ParamsTokens[i].Type != TokenType.Name)
                    throw new CompilerException($"Unexpected token '{ParamsTokens[i].Value}'", ParamsTokens[i].File, ParamsTokens[i].Line,
                        ParamsTokens[i].LineIndex);
                value = ParamsTokens[i].Value;
                i++;
                if (i >= ParamsTokens.Count)
                {
                    Params.Add(new FunctionParameter(type, value, constant));
                    Variables.Add(new FunctionVariable(value, type, constant));
                    type = null;
                    value = null;
                }
                continue;
            }
            if (ParamsTokens[i].Type != TokenType.Comma)
                throw new CompilerException($"Unexpected token '{ParamsTokens[i].Value}'", ParamsTokens[i].File, ParamsTokens[i].Line,
                    ParamsTokens[i].LineIndex);
            Params.Add(new FunctionParameter(type, value, constant));
            Variables.Add(new FunctionVariable(value, type, constant));
            type = null;
            value = null;
            i++;
        }
    }

    public bool HasSameParams(List<FunctionParameter> functionParams) => Params.Count == functionParams.Count && Params.Where((t, i) => t.Constant == functionParams[i].Constant && t.Type == functionParams[i].Type).Any();
}

public class FunctionParameter
{
    public string Type { get; }
    
    public string Name { get; }
    
    public bool Constant { get; }

    public FunctionParameter(string type, string name, bool constant)
    {
        Type = type;
        Name = name;
        Constant = constant;
    }
}

public class FunctionVariable : Variable
{
    public FunctionVariable(string variableName, string type, bool constant) 
        : base(false, type, variableName, constant) { }
}
