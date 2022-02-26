namespace Neptyne.Compiler.Models.Assembly;

public abstract class AsmSection
{
    public string Name { get; set; }

    public abstract string Convert();
}
