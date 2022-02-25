using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmTextSection : AsmSection
{
    public List<AsmTextSectionItem> Items { get; }

    public AsmTextSection(string name)
    {
        Name = name;
        Items = new List<AsmTextSectionItem>();
    }

    public override string Convert()
    {
        string result = $"section {Name}:\n";

        foreach (var item in Items)
        {
            result += $"    {item}\n";
        }

        return result;
    }
}
