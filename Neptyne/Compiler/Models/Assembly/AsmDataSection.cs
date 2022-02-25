using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmDataSection : AsmSection
{
    public List<AsmDataVariable> Items { get; }

    public AsmDataSection(string name)
    {
        Name = name;
        Items = new List<AsmDataVariable>();
    }

    public override string Convert()
    {
        string result = $"section {Name}:\n";

        foreach (var item in Items)
        {
            result += $"{item}\n";
        }

        return result;
    }
}
