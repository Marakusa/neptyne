using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmFunction
{
    public string Name { get; }
    
    public string ReturnType { get; }
    
    public List<AsmFunctionParameter> Params { get; set; }
    
    public List<ParserToken> ParamsTokens { get; }
    
    public List<AsmStatement> Block { get; set; }
    
    public List<ParserToken> BlockTokens { get; }

    public AsmFunction(string name, string returnType, List<ParserToken> paramsTokens, List<ParserToken> block)
    {
        Name = name;
        ReturnType = returnType;
        Block = new();
        Params = new();
        ParamsTokens = paramsTokens;
        BlockTokens = block;
    }

    public string GetParamsString()
    {
        string[] result = new string[Params.Count];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = $"{Params[i].Type} {Params[i].Value}";
        }
        
        if (result.Length == 0)
            return "";
        
        return $"({string.Join(",", result)})";
    }
}

public class AsmFunctionParameter
{
    public string Type { get; }
    
    public string Value { get; }

    public AsmFunctionParameter(string type, string value)
    {
        Type = type;
        Value = value;
    }
}
