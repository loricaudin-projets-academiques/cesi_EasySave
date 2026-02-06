using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySave.Core.Services
{
    internal class JsonGestion
    {
        private static JsonGestion? _instance;

        private JsonGestion()
        {
        }

        public static JsonGestion GetInstance()
        {
            if (_instance == null)
            {
                _instance = new JsonGestion();
            }
            return _instance;
        }

        public T? GetObjectFromJsonString<T>(string jsonString)
        {
            T? obj = JsonSerializer.Deserialize<T>(jsonString);
            return obj;
        }

        public string GetJsonStringFromObject<T>(T obj)
        {
            string jsonString = JsonSerializer.Serialize<T>(obj, new JsonSerializerOptions { WriteIndented = true });
            return jsonString;
        }
    }
}
