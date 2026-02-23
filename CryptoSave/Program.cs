using System.Diagnostics;

namespace CryptoSave
{
    /// <summary>
    /// Entry point for CryptoSoft application.
    /// Encrypts a file passed as argument.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Path to the file to encrypt.</param>
        /// <returns>Exit code (0 = success, other = error).</returns>
        static int Main(string[] args)
        {
            bool notepadRunning = Process.GetProcessesByName("CryptoSoft").Length > 1;
            if (notepadRunning) {
                Console.WriteLine("CryptoSoft est déjà en cours d'exécution.");
                return 1;
            }

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: CryptoSoft.exe <filepath>");
                return 1;
            }

            string fullPath = Path.GetFullPath(args[0]);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("Error: File not found");
                return 2;
            }

            try
            {
                CryptoService cryptoService = new CryptoService();
                cryptoService.Encrypt(fullPath);
                Console.WriteLine("File encrypted successfully");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during encryption: {ex.Message}");
                return -1;
            }
        }
    }
}