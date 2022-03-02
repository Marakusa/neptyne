using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class PrimitiveVariables
{
    private static List<PrimitiveTypeObject> types = new()
    {
        new("byte", 8, LiteralType.Number),
        new("short", 16, LiteralType.Number),
        new("ushort", 16, LiteralType.Number),
        new("int", 32, LiteralType.Number),
        new("uint", 32, LiteralType.Number),
        new("long", 64, LiteralType.Number),
        new("ulong", 64, LiteralType.Number),
        new("char", 16, LiteralType.Number),
        new("bool", 8, LiteralType.Boolean)
    };
    
    public static bool IsValidPrimitive(string type)
    {
        return types.Find(f => f.Name == type) != null;
    }
    
    public static int GetLength(string type)
    {
        var n = 8;
        
        var t = types.Find(f => f.Name == type);
        if (t != null) n = t.ByteLength;
        
        return n / 8;
    }

    public static PrimitiveTypeObject Parse(string type)
    {
        return types.Find(f => f.Name == type);
    }
}

public class PrimitiveTypeObject
{
    public string Name { get; }
    
    public int ByteLength { get; }
    
    public LiteralType LiteralType { get; }

    public PrimitiveTypeObject(string name, int byteLength, LiteralType literalType)
    {
        Name = name;
        ByteLength = byteLength;
        LiteralType = literalType;
    }
}

public enum LiteralType
{
    Number, String, Float, Boolean, Character
}
