using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Services
{
    internal class FileGestion
    {
        private static FileGestion? _instance;

        private FileGestion()
        {
        }

        public static FileGestion GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FileGestion();
            }
            return _instance;
        }

        public void WriteFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public string ReadFile(string path)
        {
            string content = File.ReadAllText(path);
            return content;
        }
    }
}
