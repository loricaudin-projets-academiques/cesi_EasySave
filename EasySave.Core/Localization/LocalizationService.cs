using System.Reflection;
using System.Text.Json;

namespace EasySave.Core.Localization
{
    /// <summary>
    /// Localization service implementation.
    /// Loads translations from embedded JSON resource files.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private readonly Dictionary<Language, Dictionary<string, string>> _translations = new();
        private Language _currentLanguage;

        public Language CurrentLanguage => _currentLanguage;

        public IReadOnlyList<Language> AvailableLanguages => 
            Enum.GetValues(typeof(Language)).Cast<Language>().ToList();

        public LocalizationService(Language defaultLanguage = Language.French)
        {
            _currentLanguage = defaultLanguage;
            LoadTranslations();
        }

        /// <inheritdoc/>
        public string Get(string key, params object[] args)
        {
            if (!_translations[_currentLanguage].TryGetValue(key, out var translation))
                return $"[MISSING: {key}]";

            return args.Length > 0 ? string.Format(translation, args) : translation;
        }

        /// <inheritdoc/>
        public void SetLanguage(Language language)
        {
            if (!_translations.ContainsKey(language))
                throw new ArgumentException($"Unsupported language: {language}");

            _currentLanguage = language;
        }

        private void LoadTranslations()
        {
            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                _translations[lang] = LoadLanguageFile(lang);
            }
        }

        private Dictionary<string, string> LoadLanguageFile(Language language)
        {
            var resourceName = $"EasySave.Core.Localization.Resources.{language.GetCode()}.json";

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream(resourceName);

                if (stream == null)
                    return new Dictionary<string, string>();

                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                var doc = JsonDocument.Parse(json);

                var translations = new Dictionary<string, string>();
                FlattenJson(doc.RootElement, "", translations);

                return translations;
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Flattens nested JSON structure into dot-separated keys.
        /// Example: { "commands": { "add": { "success": "..." } } } => "commands.add.success"
        /// </summary>
        private void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> result)
        {
            foreach (var property in element.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

                if (property.Value.ValueKind == JsonValueKind.Object)
                    FlattenJson(property.Value, key, result);
                else if (property.Value.ValueKind == JsonValueKind.String)
                    result[key] = property.Value.GetString() ?? "";
            }
        }
    }
}
