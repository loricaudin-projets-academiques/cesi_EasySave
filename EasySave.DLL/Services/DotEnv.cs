using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog.Services
{
    public class DotEnv
    {
        private Dictionary<string, string> envVars;
        public DotEnv()
        {
            string filePath = GetEnvPath();
            envVars = LoadEnvFile(filePath);
        }

        public string GetEnvPath()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ProSoft", "EasySave", "env", ".env"
                );
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return ".env";
            }
            else
            {
                return "";
            }
        }

        private Dictionary<string, string> LoadEnvFile(string filePath)
        {
            var envVars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Fichier .env introuvable : {filePath}");

            foreach (var line in File.ReadAllLines(filePath))
            {
                string trimmed = line.Trim();

                // Ignorer lignes vides ou commentaires
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith("//"))
                    continue;

                // Séparer clé et valeur
                int separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex < 0)
                    continue; // Ligne invalide

                string key = trimmed.Substring(0, separatorIndex).Trim();
                string value = trimmed.Substring(separatorIndex + 1).Trim();

                // Retirer guillemets éventuels
                if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                    (value.StartsWith("'") && value.EndsWith("'")))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                envVars[key] = value;
            }

            return envVars;
        }

        public string? GetValue(string key)
        {
            return envVars.GetValueOrDefault(key);
        }
    }
}
