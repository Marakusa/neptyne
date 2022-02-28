using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Neptyne.Compiler;

namespace Neptyne;

public static class CommandExecutor
{
    private static readonly Command[] Commands =
    {
        new("help", "Print this help message", Help),
        new("exit", "Exit Neptyne", Exit),
        new("compile", "Compile a Neptyne script file (.npt)", Build),
        new("c", "Compile a Neptyne script file (.npt)", DevBuild)
    };

    public static async Task Execute(string command)
    {
        string[] tokens = command.Split(' ');
        var cmd = Array.Find(Commands, f => f.Name == tokens[0]);
        
        if (cmd != null)
        {
            var task = Task.Run(() => cmd.Action(tokens));
            await task.WaitAsync(CancellationToken.None);
        }
        else if (!string.IsNullOrEmpty(tokens[0]))
        {
            throw new Exception($"Command \"{tokens[0]}\" not found");
        }
    }
    
    private static void ThrowInsufficientArgumentException(int minArgs)
    {
        throw new Exception($"Insufficient amount of arguments. At least {minArgs} argument{(minArgs == 1 ? "" : "s")} {(minArgs == 1 ? "is" : "are")} required");
    }

    private static async Task Help(string[] _)
    {
        foreach (var command in Commands.OrderBy(f => f.Name))
        {
            Console.WriteLine($"{command.Name}\t\t{command.Description}");
        }
    }

    private static async Task Exit(string[] _)
    {
        Program.Exit();
    }

    private static async Task DevBuild(string[] args)
    {
        await Build(new []
        {
            "compile",
            "/home/markus/repos/Neptyne/Neptyne/Test.npt"
        });
    }

    private static async Task Build(string[] args)
    {
        int minArgs = 1;
        if (args.Length > minArgs)
        {
            var neptyneCompiler = new NeptyneCompiler();
            FileInfo file = new(args[1]);
            if (file.Exists)
            {
                Console.WriteLine("[npta] Starting a compile process...");

                DateTime startTime = DateTime.Now;

                string inputFilePath = file.FullName;
                string outputFullPath = file.FullName.Substring(0, inputFilePath.Length - file.Extension.Length);
                string outputAssembly = $"{outputFullPath}.asm";
                string outputObjectCode = $"{outputFullPath}.o";

                CleanBuild(file);

                Console.WriteLine($"\n[npta] Compiling {file.FullName}\n            -> {outputAssembly}");
                string compiled = neptyneCompiler.Compile(File.ReadAllText(args[1]), file.Name);

                try
                {
                    File.WriteAllBytes($"{outputAssembly}", Encoding.UTF8.GetBytes(compiled));

                    Console.WriteLine($"\n[npta] Writing {inputFilePath}\n            -> {outputAssembly}");

                    using Process nasm = new();

                    nasm.OutputDataReceived += (_, eventArgs) => Console.WriteLine($"[nasm] {eventArgs.Data}");
                    nasm.ErrorDataReceived += (_, eventArgs) => Console.WriteLine($"[nasm] E: {eventArgs.Data}");
                    nasm.Disposed += (_, _) => Console.WriteLine("[npta] E: nasm process exited");

                    nasm.StartInfo = new("nasm", $"-w+all -f elf64 -o {outputObjectCode} {outputAssembly}");

                    nasm.Start();
                    await nasm.WaitForExitAsync();

                    if (File.Exists(outputObjectCode))
                    {
                        using Process ld = new();

                        ld.OutputDataReceived += (_, eventArgs) => Console.WriteLine($"[ld] {eventArgs.Data}");
                        ld.ErrorDataReceived += (_, eventArgs) => Console.WriteLine($"[ld] E: {eventArgs.Data}");
                        ld.Disposed += (_, _) => Console.WriteLine("[ld] E: nasm process exited");

                        Console.WriteLine($"\n[npta] Building {outputObjectCode}\n            -> {outputFullPath}");
                        ld.StartInfo = new("ld", $"-o {outputFullPath} {outputObjectCode}");

                        ld.Start();
                        await ld.WaitForExitAsync();

                        if (File.Exists(outputFullPath))
                        {
                            Console.WriteLine($"[npta] Build finished in {(DateTime.Now.Subtract(startTime).TotalMilliseconds / 1000):0.000}s");

                        }
                        else
                        {
                            Console.WriteLine("[npta] E: ld process exited");
                            Console.WriteLine("Build failed");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[npta] E: nasm process exited");
                        Console.WriteLine("Build failed");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[npta] E: {ex.Message}");
                    Console.WriteLine("[npta] Build failed");
                }
            }
            else
                throw new Exception($"[npta] Script \"{args[1]}\" not found");
        }
        else
        {
            ThrowInsufficientArgumentException(minArgs);
        }
    }

    private static void CleanBuild(FileInfo file)
    {
        string writeFilePathNoExt = file.FullName.Substring(0, file.FullName.Length - file.Extension.Length);
        string writeFilePath = $"{writeFilePathNoExt}.asm";

        if (writeFilePath != file.FullName && File.Exists(writeFilePath))
            File.Delete(writeFilePath);
        if (writeFilePath != file.FullName && File.Exists($"{writeFilePathNoExt}.o"))
            File.Delete($"{writeFilePathNoExt}.o");
        if (writeFilePath != file.FullName && File.Exists(writeFilePathNoExt))
            File.Delete(writeFilePathNoExt);
    }
}

public class Command
{
    public string Name { get; }

    public string Description { get; }

    public Func<string[], Task> Action { get; }

    public Command(string name, string description, Func<string[], Task> action)
    {
        Name = name;
        Description = description;
        Action = action;
    }
}
