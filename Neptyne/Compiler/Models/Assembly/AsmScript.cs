using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmScript
{
    public readonly AsmDataSection DataSection = new(".data");
    
    public readonly AsmTextSection TextSection = new(".text");

    public List<AsmFunction> Functions = new();

    public string Build()
    {
        string result = "";

        result += $"{DataSection.Convert()}\n";
        result += $"{TextSection.Convert()}\n";

        foreach (var function in Functions)
        {
            result += $"{function.Name}{function.GetParamsString()}:\n";
            foreach (var statement in function.Block)
            {
                result += $"    {statement}\n";
            }
            result += "    int 80h;";
        }

        return result;
    }
}
