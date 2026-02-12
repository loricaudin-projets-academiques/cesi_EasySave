namespace CryptoSave
{
    class Program
    {
        static int Main(string[] args)
        {
          if (args.Length != 1)
            {
                Console.WriteLine("Usage: CryptoSoft.exe <filepath>");
                return 1; 
            }

            
            string fullPath = Path.GetFullPath(args[0]);

            
            if (!File.Exists(fullPath))
            {
                Console.WriteLine("Erreur : Fichier introuvable");
                return 2; 
            }

           
            try
            {
                CryptoService cryptoService = new CryptoService();
                cryptoService.Encrypt(fullPath);
                Console.WriteLine("Fichier crypté avec succès");
                return 0; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du cryptage : {ex.Message}");
                return -1; 
            }
        }
    }
}