using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmScript
{
    public List<Variable> Variables { get; set; }
    
    public readonly AsmTextSection TextSection = new();

    public List<Function> Functions = new();

    public string Build()
    {
        var result = "%use masm\n\n";

        result += $"section .data\n\n{TextSection.Convert()}\n";

        foreach (var function in Functions)
        {
            foreach (var variable in function.Variables)
            {
                if (variable.Type == "string")
                    result += $"{variable.PointerName}:\n    .string \"{variable.Value.Value}\"\n\n";
            }
        }
        foreach (var variable in Variables)
        {
            if (variable.Type == "string")
                result += $"{variable.PointerName}:\n    .string \"{variable.Value.Value}\"\n\n";
        }

        foreach (var variable in Variables)
        {
            result += $"{variable.ToAssembly()}\n";
        }

        foreach (var function in Functions)
        {
            result += function.ToString();
        }

        return result;
    }
}
