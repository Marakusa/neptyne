using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmScript
{
    public List<Variable> Variables { get; set; }
    
    public readonly AsmTextSection TextSection = new();

    public List<Function> Functions = new();

    public string Build()
    {
        var result = "%use masm\n\nsection .data\n\n";

        foreach (var variable in Variables)
        {
            if (variable.Type.Name == "string")
                result += $"{variable.Name}:\n    .string \"{variable.Value}\"\n";
        }

        foreach (var variable in Variables)
        {
            result += $"{variable.ToAssembly()}\n";
        }
        
        result += $"{TextSection.Convert()}\n";

        foreach (var function in Functions)
        {
            result += function.ToString();
        }

        return result;
    }
}
