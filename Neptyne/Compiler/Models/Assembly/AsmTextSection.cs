using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmTextSection : AsmSection
{
    public override string Convert()
    {
        string result = "section .text:\n";
        result += "    global _start\n\n";

        AsmFunction start = new("_start", null, null, null);
        start.Block = new List<AsmStatement>
        {
            new("call", "main")
        };
        result += start.ToString();

        return result;
    }
}
