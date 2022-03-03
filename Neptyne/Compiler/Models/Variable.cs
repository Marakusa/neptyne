namespace Neptyne.Compiler.Models;

public class Variable
{
    public bool Readonly { get; }
    
    public bool Constant { get; }
    
    public string Type { get; }
    
    public string Name { get; }
    
    public ParserToken Value { get; }
    
    public Variable(bool isReadonly, string type, string name, ParserToken value, bool constant = false)
    {
        Readonly = isReadonly;
        Type = type;
        Name = name;
        Value = value;
        Constant = constant;
    }
}
