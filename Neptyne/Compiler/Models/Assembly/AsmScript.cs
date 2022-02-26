using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmScript
{
    public readonly AsmDataSection DataSection = new(".data");
    
    public readonly AsmTextSection TextSection = new();

    public List<AsmFunction> Functions = new();

    public string Build()
    {
        string result = "";

        result += $"{DataSection.Convert()}\n";
        result += $"{TextSection.Convert()}\n";

        foreach (var function in Functions)
        {
            result += function.ToString();
        }

        return result;
    }
}
