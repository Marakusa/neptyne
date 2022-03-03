namespace Neptyne.Compiler.Models;

public class Statement
{
    private string _cStatement;
    
    private ParserToken[] Tokens { get; }

    public Statement(ParserToken[] tokens)
    {
        Tokens = tokens;
    }

    public Statement(string cStatement)
    {
        _cStatement = cStatement;
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(_cStatement))
            _cStatement = CStatementFormatter.Format(Tokens);
        return _cStatement;
    }
}
