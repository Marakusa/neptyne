namespace Neptyne.Compiler.Models.Assembly;

public class AsmDataSectionItem
{
    public string Name { get; }
    
    public string Instruction { get; }
    
    public string Value { get; }

    public AsmDataSectionItem(string name, string instruction, string value)
    {
        Name = name;
        Instruction = instruction;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Name}: {Instruction} {Value}";
    }
}
