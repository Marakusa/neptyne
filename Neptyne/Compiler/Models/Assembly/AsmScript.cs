using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly
{
    public class AsmScript
    {
        public readonly AsmDataSection DataSection = new AsmDataSection(".data");

        public readonly AsmTextSection TextSection = new AsmTextSection();

        public List<AsmFunction> Functions = new List<AsmFunction>();

        public string Build()
        {
            string result = "";

            result += $"{DataSection.Convert()}\n";
            result += $"{TextSection.Convert()}\n";

            foreach (var function in Functions)
            {
                result += function.ToString();
            }

            return result;
        }
    }
}
