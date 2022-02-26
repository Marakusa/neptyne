namespace Neptyne.Compiler.Models.Assembly;

public class AsmTextSectionItem
{
    public string Key { get; }
    
    public string Value { get; }

    public AsmTextSectionItem(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Key} {Value}";
    }
}
