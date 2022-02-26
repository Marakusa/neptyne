using System.Collections.Generic;

namespace Neptyne.Compiler.Models.Assembly
{
    public class AsmEntryPoint
    {
        public string Name { get; }

        public List<AsmTextSectionItem> Items { get; }

        public AsmEntryPoint(string name)
        {
            Name = name;
            Items = new List<AsmTextSectionItem>();
        }
    }
}
