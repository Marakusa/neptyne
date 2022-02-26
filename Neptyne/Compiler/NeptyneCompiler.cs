using Neptyne.Compiler.Models;

namespace Neptyne.Compiler
{
    public class NeptyneCompiler
    {
        public string Compile(string code, string name)
        {
            Token[] tokens = Tokenizer.Tokenize(code);
            ParserToken abstractSyntaxTree = Parser.Parse(tokens, name);
            string assemblyScript = ParseToAsm.ParseToAssembly(abstractSyntaxTree);
            return assemblyScript;
        }

        public void ExecuteStatement(string statement)
        {

        }
    }
}
