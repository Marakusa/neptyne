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
        var tokens = command.Split(' ');
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
        const int minArgs = 1;
        if (args.Length > minArgs)
        {
            FileInfo file = new(args[1]);
            if (file.Exists)
            {
                Console.WriteLine("[npta] Starting a compile process...");

                var startTime = DateTime.Now;

                var inputFilePath = file.FullName;
                var outputFullPath = file.FullName.Substring(0, inputFilePath.Length - file.Extension.Length);
                var outputAssembly = $"{outputFullPath}.asm";
                var outputObjectCode = $"{outputFullPath}.o";

                CleanBuild(file);

                Console.WriteLine($"[npta] Compiling {file.FullName}\n            -> {outputAssembly}");
                var compiled = NeptyneCompiler.Compile(await File.ReadAllTextAsync(args[1]), file.FullName);

                try
                {
                    await File.WriteAllBytesAsync($"{outputAssembly}", Encoding.UTF8.GetBytes(compiled));

                    Console.WriteLine($"[npta] Writing {inputFilePath}\n            -> {outputAssembly}");

                    using Process nasm = new();

                    nasm.OutputDataReceived += (_, eventArgs) => Console.WriteLine($"[nasm] {eventArgs.Data}");
                    nasm.ErrorDataReceived += (_, eventArgs) => Console.WriteLine($"[nasm] E: {eventArgs.Data}");
                    nasm.Disposed += (_, _) => Console.WriteLine("[npta] E: nasm process exited");

                    nasm.StartInfo = new ProcessStartInfo("nasm", $"-w+all -f elf64 -o {outputObjectCode} {outputAssembly}");

                    nasm.Start();
                    await nasm.WaitForExitAsync();

                    if (File.Exists(outputObjectCode))
                    {
                        using Process ld = new();

                        ld.OutputDataReceived += (_, eventArgs) => Console.WriteLine($"[ld] {eventArgs.Data}");
                        ld.ErrorDataReceived += (_, eventArgs) => Console.WriteLine($"[ld] E: {eventArgs.Data}");
                        ld.Disposed += (_, _) => Console.WriteLine("[ld] E: nasm process exited");

                        Console.WriteLine($"[npta] Building {outputObjectCode}\n            -> {outputFullPath}");
                        ld.StartInfo = new ProcessStartInfo("ld", $"-o {outputFullPath} {outputObjectCode}");

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
        var writeFilePathNoExt = file.FullName.Substring(0, file.FullName.Length - file.Extension.Length);
        var writeFilePath = $"{writeFilePathNoExt}.asm";

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
