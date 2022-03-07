using System.Collections.Generic;

namespace Neptyne.Compiler.Models
{
    public class PrimitiveVariables
    {
        private static readonly List<PrimitiveTypeObject> Types = new()
        {
            new PrimitiveTypeObject("byte", 8, LiteralType.Number),
            new PrimitiveTypeObject("short", 16, LiteralType.Number),
            new PrimitiveTypeObject("ushort", 16, LiteralType.Number),
            new PrimitiveTypeObject("int", 32, LiteralType.Number),
            new PrimitiveTypeObject("uint", 32, LiteralType.Number),
            new PrimitiveTypeObject("long", 64, LiteralType.Number),
            new PrimitiveTypeObject("ulong", 64, LiteralType.Number),
            new PrimitiveTypeObject("char", 16, LiteralType.Number),
            new PrimitiveTypeObject("bool", 8, LiteralType.Boolean),
            new PrimitiveTypeObject("string", 64, LiteralType.String)
        };

        public static int GetLength(string type)
        {
            var n = 8;

            var t = Types.Find(f => f.Name == type);
            if (t != null) n = t.ByteLength;

            return n / 8;
        }

        public static PrimitiveTypeObject Parse(string type)
        {
            return Types.Find(f => f.Name == type);
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
}
