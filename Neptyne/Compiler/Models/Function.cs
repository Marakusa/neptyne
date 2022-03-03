using System.Collections.Generic;

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

    public string GetParamsString()
    {
        var p = "";
        
        for (var i = 0; i < Params.Count; i++)
        {
            p += $"{Params[i].Type} {Params[i].Value}";
            if (i + 1 < Params.Count)
                p += ",";
        }
        
        return $"({p})";
    }

    public override string ToString()
    {
        var result = "";
        
        result += $"{(ReturnType == "void" ? "int" : ReturnType)} {Name}{GetParamsString()} {{\n";
        
        var containsReturn = false;
        foreach (var statement in Block)
        {
            result += $"    {statement}\n";
            containsReturn = statement.IsReturnStatement;
        }
        
        if (!containsReturn)
            result += "    return 1;\n}\n";

        return result;
    }
}

public class FunctionParameter
{
    public string Type { get; }
    
    public string Value { get; }

    public FunctionParameter(string type, string value)
    {
        Type = type;
        Value = value;
    }
}

public class FunctionVariable : Variable
{
    public FunctionVariable(string variableName, string type, ParserToken value) 
        : base(false, type, variableName, value) { }
}
