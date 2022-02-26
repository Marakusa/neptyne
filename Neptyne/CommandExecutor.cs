using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iced.Intel;
using Neptyne.Compiler;
using Decoder = Iced.Intel.Decoder;
using Encoder = Iced.Intel.Encoder;

namespace Neptyne
{
    public static class CommandExecutor
    {
        private static readonly Command[] Commands =
        {
            new Command("help", "Print this help message", (_) =>
            {
                foreach (var command in Commands.OrderBy(f => f.Name))
                {
                    Console.WriteLine($"{command.Name}\t\t{command.Description}");
                }
            }),
            new Command("exit", "Exit Neptyne", (_) => { Program.Exit(); }),
            new Command("compile", "[-C {nasm}] Compile a Neptyne script file (.npt)", (args) =>
            {
                int minArgs = 1;
                if (args.Length > minArgs)
                {
                    var neptyneCompiler = new NeptyneCompiler();
                    FileInfo file = new FileInfo(args[1]);
                    if (file.Exists)
                    {
                        string writeFilePath = file.FullName.Substring(0, file.FullName.Length - file.Extension.Length);
                        string compiled = neptyneCompiler.Compile(File.ReadAllText(args[1]), file.Name);
                        string filepath = $"{writeFilePath}.s";
                        File.WriteAllBytes(filepath, Encoding.UTF8.GetBytes(compiled));
                        Console.WriteLine(compiled);
                    }
                    else
                        throw new Exception($"Script \"{args[1]}\" not found");
                }
                else
                {
                    ThrowInsufficientArgumentException(minArgs);
                }
            }),
            new Command("load", "Load, compile and execute a Neptyne script file (.npt)", (args) =>
            {
                int minArgs = 1;
                if (args.Length > minArgs)
                {
                    var neptyneCompiler = new NeptyneCompiler();
                    FileInfo file = new FileInfo(args[1]);
                    if (file.Exists)
                    {
                        string c = neptyneCompiler.Compile(File.ReadAllText(args[1]), file.Name);
                    }
                    else
                        throw new Exception($"Script \"{args[1]}\" not found");
                }
                else
                {
                    ThrowInsufficientArgumentException(minArgs);
                }
            })
        };

        private static void ThrowInsufficientArgumentException(int minArgs)
        {
            throw new Exception(
                $"Insufficient amount of arguments. At least {minArgs} argument{(minArgs == 1 ? "" : "s")} {(minArgs == 1 ? "is" : "are")} required");
        }

        public static void Execute(string command)
        {
            string[] tokens = command.Split(' ');
            var cmd = Array.Find(Commands, f => f.Name == tokens[0]);

            if (cmd != null)
            {
                cmd.Action(tokens);
            }
            else if (!string.IsNullOrEmpty(tokens[0]))
            {
                throw new Exception($"Command \"{tokens[0]}\" not found");
            }
        }
    }

    public class Command
    {
        public string Name { get; }

        public string Description { get; }

        public Action<string[]> Action { get; }

        public Command(string name, string description, Action<string[]> action)
        {
            Name = name;
            Description = description;
            Action = action;
        }
    }
}
