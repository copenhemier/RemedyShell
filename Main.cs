using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace RemedyShellv1
{
    class Program
    {
        static readonly List<string> History = new List<string>();

        static int Main(string[] args)
        {
            PrintWelcome();

            while (true)
            {
                WritePrompt();

                string? line = ReadLineWithHistory();

                if (line == null)
                {
                    Console.WriteLine();
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                History.Add(line);

                var tokens = Tokenize(line);
                if (tokens.Count == 0)
                    continue;

                string command = tokens[0];
                string[] cmdArgs = tokens.Skip(1).ToArray();

                if (!TryRunBuiltin(command, cmdArgs))
                {
                    RunExternal(command, cmdArgs);
                }
            }
        }

        static void PrintWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("RemedyShell v1 — type 'help' for commands, 'exit' to quit.");
            Console.ResetColor();
        }

        static void WritePrompt()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Directory.GetCurrentDirectory());
            Console.ResetColor();
            Console.Write("> ");
        }

        static string? ReadLineWithHistory()
        {
            var buffer = new List<char>();
            int historyIndex = History.Count;
            int cursor = 0;

            while (true)
            {
                ConsoleKeyInfo key;
                try { key = Console.ReadKey(intercept: true); }
                catch (InvalidOperationException) { return Console.ReadLine(); }

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return new string(buffer.ToArray());
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (cursor > 0)
                    {
                        buffer.RemoveAt(cursor - 1);
                        cursor--;
                        RedrawLine(buffer, cursor);
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (historyIndex > 0)
                    {
                        historyIndex--;
                        buffer.Clear();
                        buffer.AddRange(History[historyIndex]);
                        cursor = buffer.Count;
                        RedrawLine(buffer, cursor);
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (historyIndex < History.Count - 1)
                    {
                        historyIndex++;
                        buffer.Clear();
                        buffer.AddRange(History[historyIndex]);
                        cursor = buffer.Count;
                        RedrawLine(buffer, cursor);
                    }
                    else
                    {
                        historyIndex = History.Count;
                        buffer.Clear();
                        cursor = 0;
                        RedrawLine(buffer, cursor);
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (cursor > 0) { cursor--; Console.Write("\b"); }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (cursor < buffer.Count) { Console.Write(buffer[cursor]); cursor++; }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    buffer.Insert(cursor, key.KeyChar);
                    cursor++;
                    RedrawLine(buffer, cursor);
                }
            }
        }

        static void RedrawLine(List<char> buffer, int cursor)
        {
            Console.Write("\r");
            int width = Console.WindowWidth;
            Console.Write(new string(' ', width - 1));
            Console.Write("\r");
            WritePrompt();
            Console.Write(new string(buffer.ToArray()));
            int diff = buffer.Count - cursor;
            for (int i = 0; i < diff; i++) Console.Write("\b");
        }

        static List<string> Tokenize(string line)
        {
            var tokens = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;
            char quoteChar = '"';

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == quoteChar) inQuotes = false;
                    else current.Append(c);
                }
                else if (c == '"' || c == '\'')
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                tokens.Add(current.ToString());

            return tokens;
        }

        static bool TryRunBuiltin(string command, string[] args)
        {
            switch (command.ToLowerInvariant())
            {
                case "exit":
                case "quit":
                    Environment.Exit(0);
                    return true;

                case "cd":
                    BuiltinCd(args);
                    return true;

                case "pwd":
                    Console.WriteLine(Directory.GetCurrentDirectory());
                    return true;

                case "history":
                    for (int i = 0; i < History.Count; i++)
                        Console.WriteLine($"{i + 1,4}  {History[i]}");
                    return true;

                case "clear":
                case "cls":
                    Console.Clear();
                    return true;

                case "encrypt":
                    if (args.Length == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("usage: encrypt <file>");
                        Console.ResetColor();
                    }
                    else
                    {
                        string pw = Crypto.ReadPassword("password: ");
                        Crypto.Encrypt(args[0], pw);
                    }
                    return true;

                case "decrypt":
                    if (args.Length == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("usage: decrypt <file>");
                        Console.ResetColor();
                    }
                    else
                    {
                        string pw = Crypto.ReadPassword("password: ");
                        Crypto.Decrypt(args[0], pw);
                    }
                    return true;

                case "security":
                    SecurityInfo.Print();
                    return true;

                case "help":
                    PrintHelp();
                    return true;

                default:
                    return false;
            }
        }

        static void BuiltinCd(string[] args)
        {
            try
            {
                string target;
                if (args.Length == 0)
                {
                    target = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                }
                else
                {
                    target = args[0];
                    if (target.StartsWith("~"))
                    {
                        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        target = home + target.Substring(1);
                    }
                }

                Directory.SetCurrentDirectory(target);
            }
            catch (Exception ex)
            {
                WriteError($"cd: {ex.Message}");
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("Built-in commands:");
            Console.WriteLine("  cd [dir]     change directory (no arg = home)");
            Console.WriteLine("  pwd          print current directory");
            Console.WriteLine("  history      show command history");
            Console.WriteLine("  clear/cls    clear the screen");
            Console.WriteLine("  encrypt <f>  encrypt a file with a password");
            Console.WriteLine("  decrypt <f>  decrypt a .rmdy file");
            Console.WriteLine("  security     show security information");
            Console.WriteLine("  help         show this help");
            Console.WriteLine("  exit/quit    leave the shell");
            Console.WriteLine();
            Console.WriteLine("Anything else is run as an external program.");
            Console.WriteLine("Use up/down arrows to recall previous commands.");
        }

        static void RunExternal(string command, string[] args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    CreateNoWindow = false,
                };
                foreach (var a in args)
                    psi.ArgumentList.Add(a);

                using var proc = Process.Start(psi);
                proc?.WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                WriteError($"command not found: {command}");
            }
            catch (Exception ex)
            {
                WriteError($"error running '{command}': {ex.Message}");
            }
        }

        static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}