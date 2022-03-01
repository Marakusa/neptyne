using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmScript
{
    public List<Variable> Variables { get; set; }
    
    public List<Variable> ConstantVariables { get; set; }
    
    public readonly AsmTextSection TextSection = new();

    public List<Function> Functions = new();

    public string Build()
    {
        string result = "%use masm\n\nsection .data\n\n";

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
