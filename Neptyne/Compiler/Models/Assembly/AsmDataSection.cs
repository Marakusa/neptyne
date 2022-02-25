using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmDataSection : AsmSection
{
    public List<AsmDataSectionItem> Items { get; }

    public AsmDataSection(string name)
    {
        Name = name;
        Items = new List<AsmDataSectionItem>();
    }

    public override string Convert()
    {
        string result = $"section {Name}\n";

        foreach (var item in Items)
        {
            result += $"    {item}\n";
        }

        return result;
    }
}
