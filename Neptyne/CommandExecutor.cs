using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Neptyne.Compiler;
using Neptyne.Compiler.Exceptions;

namespace Neptyne;

public static class CommandExecutor
{
    private const string CompilerName = "nptc";
    
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
            "/home/markus/repos/Neptyne/Neptyne/Test.npt",
            "-R"
        });
    }

    private static async Task Build(string[] args)
    {
        const int minArgs = 1;
        if (args.Length > minArgs)
        {
            var filename = "";
            var run = false;
        
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    switch (args[i].Substring(1))
                    {
                        case "R":
                            run = true;
                            break;
                        case "F":
                            if (string.IsNullOrEmpty(filename) && i + 1 < args.Length)
                            {
                                i++;
                                filename = args[i];
                            }
                            break;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(filename))
                        filename = args[i];
                }
            }

            if (filename == null)
                throw new Exception("Script path not given");
            
            FileInfo file = new(filename);
            if (file.Exists)
            {
                Console.WriteLine($"[{CompilerName}] Starting a compile process...");

                var startTime = DateTime.Now;

                var inputFilePath = file.FullName;
                var outputFullPath = file.FullName[..(inputFilePath.Length - file.Extension.Length)];
                var outputScript = $"{outputFullPath}.c";

                CleanBuild(file);

                Console.WriteLine($"[{CompilerName}] Compiling {file.FullName}\n            -> {outputFullPath}");
                string compiled;
                try
                {
                    compiled = NeptyneCompiler.Compile(await File.ReadAllTextAsync(filename), file.FullName);
                }
                catch (CompilerException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DetailedException($"[{CompilerName}] E: {ex}");
                }

                if (string.IsNullOrEmpty(compiled))
                {
                    Console.WriteLine($"[{CompilerName}] E: Compiling {file.FullName} failed for an unknown reason");
                    return;
                }

                try
                {
                    await File.WriteAllBytesAsync($"{outputScript}", Encoding.UTF8.GetBytes(compiled));
                    
                    using Process gcc = new();

                    gcc.OutputDataReceived += (_, eventArgs) => Console.WriteLine($"[gcc] {eventArgs.Data}");
                    gcc.ErrorDataReceived += (_, eventArgs) => Console.WriteLine($"[gcc] E: {eventArgs.Data}");
                    gcc.Disposed += (_, _) => Console.WriteLine("[npta] E: gcc process exited");

                    gcc.StartInfo = new ProcessStartInfo("gcc", $"-O0 -g -o {outputFullPath} {outputScript}");

                    gcc.Start();
                    await gcc.WaitForExitAsync();

                    if (File.Exists(outputFullPath))
                    {
                        Console.WriteLine($"[{CompilerName}] Build finished in {(DateTime.Now.Subtract(startTime).TotalMilliseconds / 1000):0.000}s");

                        if (run)
                        {
                            using Process buildExecutable = new();

                            buildExecutable.OutputDataReceived += (_, eventArgs) => Console.WriteLine(eventArgs.Data);
                            buildExecutable.ErrorDataReceived += (_, eventArgs) => Console.WriteLine(eventArgs.Data);

                            buildExecutable.StartInfo = new ProcessStartInfo(outputFullPath);

                            buildExecutable.Start();
                            await buildExecutable.WaitForExitAsync();

                            await Exit(Array.Empty<string>());
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{CompilerName}] E: gcc process exited");
                        Console.WriteLine("Build failed");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[{CompilerName}] E: {ex.Message}");
                    Console.WriteLine("[npta] Build failed");
                }
            }
            else
                throw new Exception($"Script \"{filename}\" not found");
        }
        else
        {
            ThrowInsufficientArgumentException(minArgs);
        }
    }

    private static void CleanBuild(FileInfo file)
    {
        var writeFilePathNoExt = file.FullName.Substring(0, file.FullName.Length - file.Extension.Length);
        var writeFilePath = $"{writeFilePathNoExt}.c";

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
