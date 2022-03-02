using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public class NeptyneCompiler
{
    public string Compile(string code, string name)
    {
        var tokens = Tokenizer.Tokenize(code);
        var abstractSyntaxTree = Parser.ParseToSyntaxTree(tokens, name);
        ParseToAsm parser = new();
        return parser.Start(abstractSyntaxTree);
    }
}
