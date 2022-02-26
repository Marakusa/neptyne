using Neptyne.Compiler.Models.Assembly;

namespace Neptyne.Compiler.Models;

public class Variable
{
    public bool Readonly { get; }
    
    public PrimitiveTypeObject Type { get; }
    
    public string Name { get; }
    
    public ParserToken Value { get; }

    public Variable(bool isReadonly, PrimitiveTypeObject type, string name, ParserToken value)
    {
        Readonly = isReadonly;
        Type = type;
        Name = name;
        Value = value;
    }

    public string ToAssembly()
    {
        string result = $"{Name}:\n";

        if (Value == null)
        {
            result += $"    .zero {PrimitiveVariables.GetLength(Type.Name)}\n";
        }
        else
        {
            switch (Type.Name)
            {
                case "byte":
                case "bool":
                    result += $"    .byte {Value.Value}\n";
                    break;
                case "short":
                case "ushort":
                    result += $"    .value {Value.Value}\n";
                    break;
                case "char":
                case "int":
                case "uint":
                    result += $"    .long {Value.Value}\n";
                    break;
                case "long":
                case "ulong":
                    result += $"    .quad {Value.Value}\n";
                    break;
            }
        }
        
        return result + "\n";
    }
}
