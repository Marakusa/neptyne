namespace Neptyne.Compiler.Models.Assembly;

public class AsmStatement
{
    public string Instruction { get; }

    public string Data { get; }

    public AsmStatement(string instruction, string data)
    {
        Instruction = instruction;
        Data = data;
    }

    public override string ToString()
    {
        return $"{Instruction} {Data}";
    }
}
