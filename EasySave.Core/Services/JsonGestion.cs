using System.Text.Json;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Singleton service for JSON serialization/deserialization.
    /// </summary>
    internal class JsonGestion
    {
        private static JsonGestion? _instance;

        private JsonGestion() { }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static JsonGestion GetInstance()
        {
            return _instance ??= new JsonGestion();
        }

        /// <summary>
        /// Deserializes a JSON string to an object.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="jsonString">JSON string to deserialize.</param>
        /// <returns>Deserialized object or null.</returns>
        public T? GetObjectFromJsonString<T>(string jsonString)
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>Formatted JSON string.</returns>
        public string GetJsonStringFromObject<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
