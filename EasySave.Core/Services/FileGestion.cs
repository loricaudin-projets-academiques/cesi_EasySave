namespace EasySave.Core.Services
{
    /// <summary>
    /// Singleton service for basic file operations.
    /// </summary>
    internal class FileGestion
    {
        private static FileGestion? _instance;

        private FileGestion() { }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static FileGestion GetInstance()
        {
            return _instance ??= new FileGestion();
        }

        /// <summary>
        /// Writes content to a file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="content">Content to write.</param>
        public void WriteFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Reads content from a file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>File content as string.</returns>
        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
