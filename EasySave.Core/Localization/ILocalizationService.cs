namespace EasySave.Core.Localization
{
    /// <summary>
    /// Service interface for managing multilingual messages.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets a localized string.
        /// </summary>
        /// <param name="key">Translation key (e.g., "commands.add.success").</param>
        /// <param name="args">Replacement arguments.</param>
        /// <returns>Localized string.</returns>
        string Get(string key, params object[] args);

        /// <summary>Gets the current language.</summary>
        Language CurrentLanguage { get; }

        /// <summary>Sets the current language.</summary>
        void SetLanguage(Language language);

        /// <summary>Gets the list of available languages.</summary>
        IReadOnlyList<Language> AvailableLanguages { get; }
    }

    /// <summary>
    /// Supported languages enumeration.
    /// </summary>
    public enum Language
    {
        French = 0,
        English = 1
    }

    /// <summary>
    /// Extension methods for the Language enumeration.
    /// </summary>
    public static class LanguageExtensions
    {
        private static readonly Dictionary<Language, (string Code, string DisplayName)> _languageInfos = new()
        {
            { Language.French, ("fr", "Français") },
            { Language.English, ("en", "English") }
        };

        /// <summary>
        /// Gets language enum from language code.
        /// </summary>
        /// <param name="code">Language code (e.g., "fr", "en").</param>
        /// <returns>Corresponding Language enum value.</returns>
        public static Language GetEnumByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return Language.French;

            var match = _languageInfos.FirstOrDefault(x =>
                x.Value.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            return IsMatchValid(match) ? match.Key : Language.French;
        }

        /// <summary>
        /// Gets language enum from display name.
        /// </summary>
        /// <param name="displayName">Display name (e.g., "English", "Français").</param>
        /// <returns>Corresponding Language enum value.</returns>
        public static Language GetEnumByDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return Language.French;

            var match = _languageInfos.FirstOrDefault(x =>
                x.Value.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));

            return IsMatchValid(match) ? match.Key : Language.French;
        }

        /// <summary>Gets the language code (e.g., "fr", "en").</summary>
        public static string GetCode(this Language language)
        {
            return _languageInfos.TryGetValue(language, out var info) ? info.Code : "unknown";
        }

        /// <summary>Gets the display name (e.g., "Français", "English").</summary>
        public static string GetDisplayName(this Language language)
        {
            return _languageInfos.TryGetValue(language, out var info) ? info.DisplayName : "Unknown";
        }

        private static bool IsMatchValid(KeyValuePair<Language, (string Code, string DisplayName)> match)
        {
            return !match.Equals(default(KeyValuePair<Language, (string, string)>));
        }
    }
}
