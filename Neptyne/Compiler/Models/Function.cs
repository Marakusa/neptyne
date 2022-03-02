using System.Collections.Generic;
using Neptyne.Compiler.Models.Assembly;

namespace Neptyne.Compiler.Models;

public class Function
{
    public string Name { get; }
    
    public string ReturnType { get; }
    
    public List<FunctionParameter> Params { get; }
    
    public List<ParserToken> ParamsTokens { get; }
    
    public List<AsmStatement> Block { get; set; }
    
    public List<ParserToken> BlockTokens { get; }
    
    public ParserToken ParentBlockNode { get; }
    
    public List<FunctionVariable> Variables { get; set; }

    public Function(string name, string returnType, List<ParserToken> paramsTokens, List<ParserToken> block, ParserToken blockNode, int blockIndex)
    {
        Name = name;
        ReturnType = returnType;
        Params = new List<FunctionParameter>();
        Block = new List<AsmStatement>();
        ParamsTokens = paramsTokens;
        BlockTokens = block;
        ParentBlockNode = blockNode;
        Variables = new List<FunctionVariable>();
    }

    public string GetParamsString()
    {
        var result = new string[Params.Count];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = $"{Params[i].Type} {Params[i].Value}";
        }
        
        return result.Length == 0 ? "" : $"({string.Join(",", result)})";
    }

    public override string ToString()
    {
        var result = "";
        
        result += $"{Name}{GetParamsString()}:\n";
        
        if (Name != "_start")
        {
            result += "    push rbp\n";
            result += "    mov rbp, rsp\n";
        }

        var containsReturn = false;
        foreach (var statement in Block)
        {
            result += $"    {statement}\n";
            containsReturn = statement.IsReturnStatement;
        }
        
        if (Name != "_start")
        {
            if (!containsReturn)
                result += "    mov eax, 0\n";
            result += "    pop rbp\n";
            result += "    ret\n\n";
        }

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
    public FunctionVariable(string pointer, string variableNamePrefix, string variableName, string type, ParserToken value) 
        : base(false, type, variableName, variableNamePrefix, pointer, value) { }
}
