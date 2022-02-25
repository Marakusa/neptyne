namespace Neptyne.Compiler.Models.Assembly;

public class AsmDataVariable
{
    public string Name { get; }
    
    public string Value { get; }
    
    public string Type { get; }
    
    public string VariableName => $"var_{Name}";
    
    public string LengthVariable => $"len_{Name}";
    
    public AsmDataVariable(string name, string value, string type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    public override string ToString()
    {
        string variableValue;
        switch (Type)
        {
            case "string":
                variableValue = $"'{Value.Replace("\'", "\\'").Replace("\\n", "',0x0a,'")}'";
                break;
            default:
                variableValue = Value;
                break;
        }
        
        if (variableValue.EndsWith("0x0a,''")) variableValue = variableValue.Substring(0, variableValue.Length - 3);

        string result = $"    var_{Name}: db {variableValue} ; Define bytes of the variable {Name}";
        if (Type == "string")
            result += $"\n    len_{Name}: equ $-var_{Name} ; Define the length of the variable {Name}";
        
        return result;
    }
}
