namespace EasySave.Core.Localization
{
    /// <summary>
    /// Service de localisation pour gérer les messages multilingues.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Obtient une chaîne localisée.
        /// </summary>
        /// <param name="key">Clé de traduction (ex: "commands.add.success")</param>
        /// <param name="args">Arguments de remplacement</param>
        string Get(string key, params object[] args);

        /// <summary>
        /// Obtient la langue actuelle.
        /// </summary>
        Language CurrentLanguage { get; }

        /// <summary>
        /// Définit la langue.
        /// </summary>
        void SetLanguage(Language language);

        /// <summary>
        /// Obtient les langues disponibles.
        /// </summary>
        IReadOnlyList<Language> AvailableLanguages { get; }
    }

    /// <summary>
    /// Énumération des langues supportées.
    /// </summary>
    public enum Language
    {
        French = 0,
        English = 1
    }

    /// <summary>
    /// Extensions pour l'énumération Language.
    /// </summary>
    public static class LanguageExtensions
    {
        // On définit les infos une seule fois ici : (Code, DisplayName)
        private static readonly Dictionary<Language, (string Code, string DisplayName)> _languageInfos = new()
    {
        { Language.French, ("fr", "Français") },
        { Language.English, ("en", "English") }
    };

        public static Language GetEnumByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return Language.French;

            // On cherche l'entrée dont le Code correspond
            var match = _languageInfos.FirstOrDefault(x =>
                x.Value.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            // Si match est vide (default), on renvoie French, sinon la clé trouvée
            return IsMatchValid(match) ? match.Key : Language.French;
        }

        // --- 2. Récupérer l'Enum via le NOM (ex: "English") ---
        public static Language GetEnumByDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return Language.French;

            // On cherche l'entrée dont le DisplayName correspond
            var match = _languageInfos.FirstOrDefault(x =>
                x.Value.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase));

            return IsMatchValid(match) ? match.Key : Language.French;
        }

        // --- Méthodes d'extension (Enum -> String) ---

        public static string GetCode(this Language language)
        {
            return _languageInfos.TryGetValue(language, out var info) ? info.Code : "unknown";
        }

        public static string GetDisplayName(this Language language)
        {
            return _languageInfos.TryGetValue(language, out var info) ? info.DisplayName : "Unknown";
        }

        // --- Helper privé pour vérifier si le résultat LINQ est valide ---
        private static bool IsMatchValid(KeyValuePair<Language, (string Code, string DisplayName)> match)
        {
            // Vérifie si la structure trouvée n'est pas la valeur par défaut (vide)
            return !match.Equals(default(KeyValuePair<Language, (string, string)>));
        }
    }
}
