using Neptyne.Compiler.Models.Assembly;

namespace Neptyne.Compiler.Models;

public class Variable
{
    public bool Readonly { get; }
    
    public string Type { get; }
    
    public string Name { get; }
    
    public ParserToken Value { get; }

    public string PointerPrefix { get; }

    public string PointerName { get; }

    public Variable(bool isReadonly, string type, string name, string pointerPrefix, string pointerName, ParserToken value)
    {
        Readonly = isReadonly;
        Type = type;
        Name = name;
        PointerName = pointerName;
        PointerPrefix = pointerPrefix;
        Value = value;
    }

    public string ToAssembly()
    {
        if (Type != "string")
        {
            var result = $"{Name}:\n";

            if (Value == null)
            {
                result += $"    .zero {PrimitiveVariables.GetLength(Type)}\n";
            }
            else
            {
                switch (Type)
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

            return result;
        }
        else
        {
            var result = $"{Name}:\n";

            if (Value == null)
            {
                result += $"    .string {Value}\n";
            }
            else
            {
                result += $"    .string \"\"\n";
            }

            return result;
        }
    }
}
