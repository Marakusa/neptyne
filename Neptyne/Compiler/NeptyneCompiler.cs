using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public class NeptyneCompiler
{
    public string Compile(string code, string name)
    {
        Token[] tokens = Tokenizer.Tokenize(code);
        ParserToken abstractSyntaxTree = Parser.ParseToSyntaxTree(tokens, name);
        ParseToAsm parser = new();
        return parser.Start(abstractSyntaxTree);
    }
}
