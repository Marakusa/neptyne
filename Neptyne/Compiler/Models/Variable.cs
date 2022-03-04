namespace Neptyne.Compiler.Models;

public class Variable
{
    public bool Readonly { get; }
    
    public bool Constant { get; }
    
    public string Type { get; }
    
    public string Name { get; }
    
    public Variable(bool isReadonly, string type, string name, bool constant = false)
    {
        Readonly = isReadonly;
        Type = type;
        Name = name;
        Constant = constant;
    }
}
