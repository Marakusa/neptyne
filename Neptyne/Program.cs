using System;
using System.Reflection;
using Neptyne.Compiler.Exceptions;

namespace Neptyne;

public class Program
{
    private static bool _running = true;

    public static void Main(string[] args)
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Welcome to Neptyne v{Assembly.GetExecutingAssembly().GetName().Version}.\nType \"help\" for more information.\nPress \"Ctrl+C\" or type \"exit\" to exit\nNOTE: This is not a REPL (code cannot be executed here)");

        Console.CancelKeyPress += delegate { Exit(); };

        while (_running)
        {
            try
            {
                Console.Write("> ");
                CommandExecutor.Execute(Console.ReadLine());
                //ExecuteStatement(Console.ReadLine());
            }
            catch (CompilerException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{ex.Message} - Type \"help\" for more information.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }

    public static void Exit()
    {
        _running = false;
    }
}
