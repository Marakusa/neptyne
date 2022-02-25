namespace Neptyne.Compiler.Models.Assembly;

public class AsmCallStatement : AsmStatement
{
    public AsmCallStatement(string functionName, string parameters = "") : base("call", string.Format("{0}{1}", functionName, parameters))
    {
    }
}
