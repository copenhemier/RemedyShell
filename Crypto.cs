using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RemedyShellv1
{
    static class Crypto
    {
        const int SaltSize = 16;
        const int NonceSize = 12;
        const int TagSize = 16;
        const int KeySize = 32;
        const int Iterations = 300000;
        static readonly byte[] Magic = Encoding.ASCII.GetBytes("RMDY");

        public static void Encrypt(string inputPath, string password)
        {
            if (!File.Exists(inputPath))
            {
                WriteError($"encrypt: file not found: {inputPath}");
                return;
            }

            string outputPath = inputPath + ".rmdy";
            if (File.Exists(outputPath))
            {
                WriteError($"encrypt: '{outputPath}' already exists, refusing to overwrite");
                return;
            }

            try
            {
                byte[] plaintext = File.ReadAllBytes(inputPath);

                byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
                byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
                byte[] key = DeriveKey(password, salt);

                byte[] ciphertext = new byte[plaintext.Length];
                byte[] tag = new byte[TagSize];

                using (var aes = new AesGcm(key, TagSize))
                {
                    aes.Encrypt(nonce, plaintext, ciphertext, tag);
                }

                using (var fs = new FileStream(outputPath, FileMode.CreateNew))
                {
                    fs.Write(Magic);
                    fs.Write(salt);
                    fs.Write(nonce);
                    fs.Write(tag);
                    fs.Write(ciphertext);
                }

                Array.Clear(key, 0, key.Length);
                Array.Clear(plaintext, 0, plaintext.Length);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Encrypted -> {outputPath}");
                Console.ResetColor();
                Console.WriteLine("Original kept. If you lose the password, the encrypted file CANNOT be recovered.");
            }
            catch (Exception ex)
            {
                WriteError($"encrypt: {ex.Message}");
            }
        }

        public static void Decrypt(string inputPath, string password)
        {
            if (!File.Exists(inputPath))
            {
                WriteError($"decrypt: file not found: {inputPath}");
                return;
            }

            try
            {
                byte[] all = File.ReadAllBytes(inputPath);

                int headerSize = Magic.Length + SaltSize + NonceSize + TagSize;
                if (all.Length < headerSize)
                {
                    WriteError("decrypt: file is too small or not a RemedyShell encrypted file");
                    return;
                }

                int offset = 0;
                for (int i = 0; i < Magic.Length; i++)
                {
                    if (all[i] != Magic[i])
                    {
                        WriteError("decrypt: not a RemedyShell encrypted file (bad header)");
                        return;
                    }
                }
                offset += Magic.Length;

                byte[] salt = new byte[SaltSize];
                Array.Copy(all, offset, salt, 0, SaltSize);
                offset += SaltSize;

                byte[] nonce = new byte[NonceSize];
                Array.Copy(all, offset, nonce, 0, NonceSize);
                offset += NonceSize;

                byte[] tag = new byte[TagSize];
                Array.Copy(all, offset, tag, 0, TagSize);
                offset += TagSize;

                byte[] ciphertext = new byte[all.Length - offset];
                Array.Copy(all, offset, ciphertext, 0, ciphertext.Length);

                byte[] key = DeriveKey(password, salt);
                byte[] plaintext = new byte[ciphertext.Length];

                try
                {
                    using var aes = new AesGcm(key, TagSize);
                    aes.Decrypt(nonce, ciphertext, tag, plaintext);
                }
                catch (CryptographicException)
                {
                    Array.Clear(key, 0, key.Length);
                    WriteError("decrypt: wrong password or the file has been tampered with.");
                    return;
                }

                string outputPath;
                if (inputPath.EndsWith(".rmdy", StringComparison.OrdinalIgnoreCase))
                    outputPath = inputPath.Substring(0, inputPath.Length - 5);
                else
                    outputPath = inputPath + ".decrypted";

                if (File.Exists(outputPath))
                    outputPath = outputPath + ".decrypted";

                File.WriteAllBytes(outputPath, plaintext);

                Array.Clear(key, 0, key.Length);
                Array.Clear(plaintext, 0, plaintext.Length);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Decrypted -> {outputPath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                WriteError($"decrypt: {ex.Message}");
            }
        }

        static byte[] DeriveKey(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(KeySize);
        }

        public static string ReadPassword(string prompt)
        {
            Console.Write(prompt);
            var sb = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                }
            }
            return sb.ToString();
        }

        static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
