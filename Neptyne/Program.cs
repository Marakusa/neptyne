using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Neptyne.Compiler.Exceptions;

namespace Neptyne
{
    public static class Program
    {
        private static bool _running = true;

        public static async Task Main(string[] args)
        {
            var defaultColor = Console.ForegroundColor;

            Console.WriteLine($"Neptyne v{Assembly.GetExecutingAssembly().GetName().Version}");

            if (args.Length > 0)
            {
                try
                {
                    await CommandExecutor.Execute($"compile -R {args[0]}");
                }
                catch (CompilerException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = defaultColor;
                }
                catch (DetailedException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = defaultColor;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{ex.Message} - Type \"help\" for more information.");
                    Console.ForegroundColor = defaultColor;
                }
                Exit();
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Welcome to Neptyne! Type \"help\" for more information.\nPress \"Ctrl+C\" or type \"exit\" to exit\nNOTE: This is not a REPL (code cannot be executed here)");
            Console.ForegroundColor = defaultColor;

            Console.CancelKeyPress += delegate { Exit(); };

            while (_running)
            {
                try
                {
                    Console.Write("> ");
                    var task = Task.Run(() => CommandExecutor.Execute(Console.ReadLine()));
                    await task.WaitAsync(CancellationToken.None);
                }
                catch (CompilerException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = defaultColor;
                }
                catch (DetailedException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = defaultColor;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{ex.Message} - Type \"help\" for more information.");
                    Console.ForegroundColor = defaultColor;
                }
            }
        }

        public static void Exit()
        {
            _running = false;
        }
    }
}
