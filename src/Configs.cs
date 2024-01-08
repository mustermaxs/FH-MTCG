using System;

namespace MTCG
{
    public class UserConfig : IConfig
    {
        public override string Name => "UserConfig";
        public override string Section { get;  protected set;} = "user";
        public int StartAmountCoins { get; set; }
    }

    public class CardConfig : IConfig
    {
        public override string Name => "CardConfig";
        public override string Section { get; protected set;} = "cards";
        public int MaxCardsInDeck { get; set; }
        public int MinCardsInDeck { get; set; }
        public int ReqNbrCardsInPackage { get; set; }
        public int PricePerPackage { get; set; }
    }

    public class ResponseConfig : IConfig
    {
        public override string Name => "ResponseConfig";
        public void SetSection(string section) => this.Section = section;
        public override string Section { get; protected set;} = "responses";
        public Dictionary<string, Dictionary<string, string>> Response { get; set; }
        public string DefaultLanguage { get; set; }
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
                if (languageSet.TryGetValue(key, out string value))
                {
                    return value;
                }

                Console.WriteLine($"[ERROR] No response found for key: {key}");

                return "DEFAULT RESPONSE";
            }
        }
    }
}

