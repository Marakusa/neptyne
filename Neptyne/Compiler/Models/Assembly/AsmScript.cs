using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly;

public class AsmScript
{
    public readonly AsmDataSection DataSection = new(".data");
    
    public readonly AsmTextSection TextSection = new(".text");

    public readonly List<AsmEntryPoint> EntryPoints = new();

    public string Build()
    {
        string result = "";

        result += $"{DataSection.Convert()}\n";
        result += $"{TextSection.Convert()}\n";

        result +=
            $"_start:\n mov eax,4\n mov ebx,1\n mov ecx,{DataSection.Items[0].Name}\n mov edx,{DataSection.Items[1].Name}\n int 80h\n mov eax,1\n mov ebx,0\n int 80h;";

        return result;
    }
}
