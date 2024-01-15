using System;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace MTCG
{
    public class BattleConfig : IConfig, IService
    {
        public override string Name => "BattleConfig";
        public override string Section { get; protected set; }
        public JsonElement SpecialRules { get; set; } = default!;
        public JsonElement BattleResultMsgs { get; set; } = default!;
        public JsonElement GamePlayTxt { get; set; } = default!;
        public virtual int MaxNbrRounds { get; set; }
        private string defaultLanguage = "english";
        public string DefaultLanguage
        {
            get
            {
                return string.IsNullOrEmpty(chosenLanguage) ? defaultLanguage : chosenLanguage;

            }
            set
            {
                defaultLanguage = value;
            }
        }
        protected string chosenLanguage = string.Empty;

        // public BattleConfig(IConfigLoader configLoader) : base(configLoader)
        // {
        //     this.Section = "battle";
        // }
        public BattleConfig()
        {
            this.Section = "battle";
        }

        public void SetLanguage(string lang)
        {
            chosenLanguage = lang;
        }


        public string this[string gameplayTxt]
        {
            get
            {
                var language = string.IsNullOrEmpty(chosenLanguage) ? defaultLanguage : chosenLanguage;

                if (GamePlayTxt.TryGetProperty(language, out JsonElement gameplay) &&
                    gameplay.TryGetProperty(gameplayTxt, out JsonElement text))
                {
                    return text.ToString();
                }

                Logger.ToConsole($"[ERROR] No text found for code: {gameplayTxt} in language: {chosenLanguage}.");

                return string.Empty;
            }
            private set { }
        }


        public string? GetBattleResultMsg(string msgCode)
        {
            if (BattleResultMsgs.TryGetProperty(chosenLanguage, out JsonElement msgs) &&
                msgs.TryGetProperty(msgCode, out JsonElement msg))
            {
                return msg.ToString();
            }

            Logger.ToConsole($"[ERROR] No message found for code: {msgCode} in language: {chosenLanguage}.");

            return string.Empty;
        }


        public string? CardDescription(string cardName, string language)
        {
            if (SpecialRules.TryGetProperty(cardName, out JsonElement monster) &&
                monster.TryGetProperty("descriptions", out JsonElement descriptions) &&
                descriptions.TryGetProperty(language, out JsonElement description))
            {
                return description.ToString();
            }

            Logger.ToConsole($"[ERROR] No description found for card: {cardName} in language: {language}.");
            return $"[ERROR] No description found for card: {cardName} in language: {language}.";
        }

        public (string name, string against, bool wins)? GetMonsterProperties(string cardName)
        {
            if (SpecialRules.TryGetProperty(cardName, out JsonElement monster))
            {
                return (
                    cardName,
                    monster.GetProperty("against").GetString() ?? string.Empty,
                    monster.GetProperty("wins").GetBoolean());
            }

            Logger.ToConsole($"[ERROR] No properties found for card: {cardName}.");

            return null;
        }
    }
}
