using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Azure.IoT.Helpers
{
    public static class CertificateLoader
    {
        public static X509Certificate2 LoadCertificateFromFile(string fileName)
        {
            var certificatePassword = ReadCertificatePassword();

            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(fileName, certificatePassword, X509KeyStorageFlags.UserKeySet);

            X509Certificate2 certificate = null;

            foreach (var element in certificateCollection)
            {
                Console.WriteLine($"Found certificate: {element?.Thumbprint} {element?.Subject}; PrivateKey: {element?.HasPrivateKey}");
                if (certificate == null && element.HasPrivateKey)
                {
                    certificate = element;
                }
                else
                {
                    element.Dispose();
                }
            }

            if (certificate == null)
            {
                throw new FileNotFoundException($"{fileName} did not contain any certificate with a private key.");
            }
            else
            {
                Console.WriteLine($"Using certificate {certificate.Thumbprint} {certificate.Subject}");
            }

            return certificate;
        }

        private static string ReadCertificatePassword()
        {
            var password = new StringBuilder();
            Console.WriteLine($"Enter the PFX password: ");

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else
                {
                    Console.Write('*');
                    password.Append(key.KeyChar);
                }
            }

            return password.ToString();
        }
    }
}
