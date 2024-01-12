using System;
using System.Collections.Generic;
using System.Threading;

namespace MTCG
{
    public class ResponseTextTranslator : IConfig, IService
    {
        public override string Name => "ResponseConfig";
        public override string Section { get; protected set; } = "responses";
        public Dictionary<string, Dictionary<string, string>> Response { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        public string DefaultLanguage { get; set; } = "english";
        protected ThreadLocal<string?> chosenLanguage = new ThreadLocal<string?>(() => null);
        public ResponseTextTranslator() {}
        // public ResponseTextTranslator(IConfigLoader configLoader) :base(configLoader) {}
        public bool SetLanguage(string lang)
        {
            if (!TranslationExists(lang))   
            {
                return false;
            }

            chosenLanguage.Value = lang;
            return true;
        }

        public bool TranslationExists(string lang)
        {
            return Response.ContainsKey(lang);
        }

        public string this[string key]
        {
            get
            {
                var language = string.IsNullOrEmpty(chosenLanguage.Value) ? DefaultLanguage : chosenLanguage.Value;

                if (Response.TryGetValue(language, out var languageSet) &&
                    languageSet.TryGetValue(key, out var value))
                {
                    return value;
                }

                // In case translation in the requested language wasn't found,
                // try to get text in the default language
                if (language != DefaultLanguage &&
                    Response.TryGetValue(DefaultLanguage, out var defaultLanguageSet) &&
                    defaultLanguageSet.TryGetValue(key, out var defaultTranslation))
                {
                    return defaultTranslation;
                }

                // Handle the case where no response was found
                throw new InvalidOperationException($"[ERROR] No response found for key: {key}");
            }
        }
    }
}
