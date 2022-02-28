namespace Neptyne.Compiler.Models.Assembly;

public class AsmStatement
{
    public string Instruction { get; }

    public string Data { get; }

    public bool IsReturnStatement { get; }

    public AsmStatement(string instruction, string data, bool isReturnStatement = false)
    {
        Instruction = instruction;
        Data = data;
        IsReturnStatement = isReturnStatement;
    }

    public override string ToString()
    {
        return $"{Instruction} {Data}";
    }
}
