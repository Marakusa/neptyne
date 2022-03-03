using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Neptyne.Compiler.Exceptions;
using Neptyne.Compiler.Models;

namespace Neptyne.Compiler;

public static class Libraries
{
    private static readonly string RootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    private static readonly Dictionary<string, List<ParserToken>> CompiledLibraries = new();
    
    public static IEnumerable<ParserToken> Bring(string nodeValue, ParserToken node)
    {
        var libFile = FetchLibraries(Path.Join(RootPath, "/lib"), nodeValue.Split("."), 0, true, node);

        if (libFile == null)
            throw new CompilerException($"Library '{nodeValue}' doesn't exist", node.File, node.Line, node.LineIndex);

        if (CompiledLibraries.ContainsKey(nodeValue))
            return CompiledLibraries[nodeValue];
        
        var tokens = Tokenizer.Tokenize(File.ReadAllText(libFile), libFile);
        var compiledLib = Parser.ParseToSyntaxTree(tokens, nodeValue);
        CompiledLibraries.Add(nodeValue, compiledLib.Params);
        return CompiledLibraries[nodeValue];
    }

    private static string FetchLibraries(string path, IReadOnlyList<string> bringLibrary, int layer, bool builtIn, ParserToken node)
    {
        if (builtIn)
        {
            layer++;

            if (bringLibrary.Count == layer)
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    var f = new FileInfo(file);
                    if (f.Exists && f.Name[..^f.Extension.Length] == bringLibrary[layer - 1])
                        return file;
                }
            }

            if (bringLibrary.Count <= layer)
                return null;
            
            foreach (var directory in Directory.GetDirectories(path))
            {
                var d = new DirectoryInfo(directory);

                if (d.Name == bringLibrary[layer - 1])
                    continue;
                var s = FetchLibraries(directory, bringLibrary, layer, true, node);
                if (s != null)
                    return s;
            }

            throw new CompilerException($"Library named '{bringLibrary[layer - 1]}' doesn't exist", node.File, node.Line, node.LineIndex);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
