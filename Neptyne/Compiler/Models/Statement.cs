namespace Neptyne.Compiler.Models;

public class Statement
{
    public ParserToken[] Tokens { get; }

    public Statement(ParserToken[] tokens)
    {
        Tokens = tokens;
    }
}
