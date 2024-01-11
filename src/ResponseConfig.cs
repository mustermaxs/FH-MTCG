using System;

namespace MTCG
{

  public class ResponseConfig : IConfig
    {
        public override string Name => "ResponseConfig";
        public void SetSection(string section) => this.Section = section;
        public override string Section { get; protected set;} = "responses";
        public Dictionary<string, Dictionary<string, string>> Response { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        public string DefaultLanguage { get; set; } = "english";
        protected string chosenLanguage = string.Empty;

        public bool SetLanguage(string lang)
        {
            if (!TranslationExists(lang)) return false;
            chosenLanguage = lang;

            return true;                        
        }



        public bool TranslationExists(string lang)
        {
            return Response.ContainsKey(lang);
        }
        
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        
        
        public string this[string key]
        {
            get
            {
                var language = chosenLanguage == string.Empty ? DefaultLanguage : chosenLanguage;
                var languageSet = Response[language];
                
                if (languageSet.TryGetValue(key, out string? value))
                {
                    return value;
                }

                Console.WriteLine($"[ERROR] No response found for key: {key}");

                return "DEFAULT RESPONSE";
            }
        }
    }
}

