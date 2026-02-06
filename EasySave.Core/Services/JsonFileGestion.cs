using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Services
{
    public class JsonFileGestion
    {
        private static JsonFileGestion? _instance;
        private JsonGestion jsonGestion;
        private FileGestion fileGestion;

        private JsonFileGestion()
        {
            this.jsonGestion = JsonGestion.GetInstance();
            this.fileGestion = FileGestion.GetInstance();
        }

        public static JsonFileGestion GetInstance()
        {
            if (_instance == null)
            {
                _instance = new JsonFileGestion();
            }
            return _instance;
        }

        public T? Open<T>(string path)
        {
            string fileContent = this.fileGestion.ReadFile(path);
            T? obj = this.jsonGestion.GetObjectFromJsonString<T>(fileContent);
            try
            {
                fileContent = this.fileGestion.ReadFile(path);
                obj = this.jsonGestion.GetObjectFromJsonString<T>(fileContent);
                return obj;
            }
            catch (Exception e)
            {
                throw new Exception($"Error of opening json file {e.Message}");
            }
        }

        public void Save<T>(string path, T obj)
        {
            try
            {
                string directory = Path.GetDirectoryName(path);
                Directory.CreateDirectory(directory);

                string fileContent = this.jsonGestion.GetJsonStringFromObject<T>(obj);
                this.fileGestion.WriteFile(path, fileContent);
            }
            catch (Exception e)
            {
                throw new Exception($"Error of saving json file: {e.Message}");
            }
        }
    }
}
