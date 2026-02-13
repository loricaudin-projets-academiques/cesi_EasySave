namespace EasySave.Core.Services.Serialization
{
    /// <summary>
    /// Singleton service for JSON file operations.
    /// Combines file I/O with JSON serialization.
    /// </summary>
    public class JsonFileGestion
    {
        private static JsonFileGestion? _instance;
        private readonly JsonGestion jsonGestion;
        private readonly FileGestion fileGestion;

        private JsonFileGestion()
        {
            this.jsonGestion = JsonGestion.GetInstance();
            this.fileGestion = FileGestion.GetInstance();
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static JsonFileGestion GetInstance()
        {
            return _instance ??= new JsonFileGestion();
        }

        /// <summary>
        /// Opens and deserializes a JSON file.
        /// </summary>
        /// <typeparam name="T">Type to deserialize to.</typeparam>
        /// <param name="path">Path to the JSON file.</param>
        /// <returns>Deserialized object or null.</returns>
        public T? Open<T>(string path)
        {
            try
            {
                string fileContent = this.fileGestion.ReadFile(path);
                return this.jsonGestion.GetObjectFromJsonString<T>(fileContent);
            }
            catch (Exception e)
            {
                throw new Exception($"Error opening JSON file: {e.Message}");
            }
        }

        /// <summary>
        /// Serializes and saves an object to a JSON file.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="path">Path to save the file.</param>
        /// <param name="obj">Object to serialize.</param>
        public void Save<T>(string path, T obj)
        {
            try
            {
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                string fileContent = this.jsonGestion.GetJsonStringFromObject(obj);
                this.fileGestion.WriteFile(path, fileContent);
            }
            catch (Exception e)
            {
                throw new Exception($"Error saving JSON file: {e.Message}");
            }
        }
    }
}
