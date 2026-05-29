using System;

namespace RemedyShellv1
{
    static class SecurityInfo
    {
        public static void Print()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("RemedyShell — Security Information");
            Console.WriteLine("==================================");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Encryption");
            Console.WriteLine("  AES-256-GCM for file encryption");
            Console.WriteLine("  PBKDF2 (SHA-256, 300,000 iterations) for deriving keys from passwords");
            Console.WriteLine("  Standard algorithms from the .NET cryptography library");
            Console.WriteLine();

            Console.WriteLine("Privacy");
            Console.WriteLine("  Runs entirely offline");
            Console.WriteLine("  No network connections, no telemetry, no analytics");
            Console.WriteLine();

            Console.WriteLine("No backdoor");
            Console.WriteLine("  No master key, no recovery mechanism");
            Console.WriteLine("  If a password is lost, the data cannot be recovered");
            Console.WriteLine();

            Console.WriteLine("Protects against");
            Console.WriteLine("  Casual access to your files by people with access to your computer");
            Console.WriteLine("  Files on a lost or stolen drive");
            Console.WriteLine();

            Console.WriteLine("Does not protect against");
            Console.WriteLine("  This is an open-source hobby project and has not been independently audited");
            Console.WriteLine("  Do not rely on it as sole protection for critical data");
            Console.WriteLine("  Always keep backups");
            Console.WriteLine();

            Console.WriteLine("Source");
            Console.WriteLine("  Full source code is available. You are encouraged to read it.");
            Console.WriteLine();
        }
    }
}