namespace Neptyne.Compiler.Models
{
    public class Statement
    {
        private string _cStatement;

        private ParserToken[] Tokens { get; }

        public bool IsReturnStatement { get; }

        public Statement(ParserToken[] tokens, bool isReturnStatement = false)
        {
            Tokens = tokens;
            IsReturnStatement = isReturnStatement;
        }

        public Statement(string cStatement, bool isReturnStatement = false)
        {
            _cStatement = cStatement;
            IsReturnStatement = isReturnStatement;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_cStatement))
                _cStatement = CStatementFormatter.Format(Tokens);
            return _cStatement;
        }
    }
}
