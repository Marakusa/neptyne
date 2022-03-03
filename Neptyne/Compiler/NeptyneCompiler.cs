namespace Neptyne.Compiler;

public class NeptyneCompiler
{
    public static string Compile(string code, string name)
    {
        var tokens = Tokenizer.Tokenize(code, name);
        var abstractSyntaxTree = Parser.ParseToSyntaxTree(tokens, name);
        Compiler parser = new();
        return parser.Compile(abstractSyntaxTree);
    }
}
