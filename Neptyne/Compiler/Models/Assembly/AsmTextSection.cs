using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmTextSection : AsmSection
{
    public override string Convert()
    {
        string result = "section .text:\n";
        result += "    global _start\n\n";

        Function start = new("_start", "void", null, null, null, 0);
        start.Block = new List<AsmStatement>
        {
            new("call", "main")
        };
        result += start.ToString();

        return result;
    }
}
