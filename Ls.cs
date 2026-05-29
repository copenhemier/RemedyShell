using System;
using System.IO;
using System.Linq;

namespace RemedyShellv1
{
    static class Ls
    {
        public static void Run(string[] args)
        {
            string path = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

            try
            {
                if (!Directory.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ls: no such directory: {path}");
                    Console.ResetColor();
                    return;
                }

                var dirs = Directory.GetDirectories(path)
                    .Select(d => new DirectoryInfo(d))
                    .Where(d => (d.Attributes & FileAttributes.Hidden) == 0)
                    .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase);

                var files = Directory.GetFiles(path)
                    .Select(f => new FileInfo(f))
                    .Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
                    .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var d in dirs)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(d.Name + "/");
                }

                foreach (var f in files)
                {
                    string ext = f.Extension.ToLowerInvariant();
                    if (ext == ".exe" || ext == ".bat" || ext == ".cmd" || ext == ".ps1")
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ResetColor();
                    Console.WriteLine(f.Name);
                }

                Console.ResetColor();
            }
            catch (UnauthorizedAccessException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ls: access denied: {path}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ls: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
