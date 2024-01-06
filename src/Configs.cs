using System;

namespace MTCG
{
    public class UserConfig : IConfig
    {
        public override string Name => "UserConfig";
        public override string Section { get;  } = "user";
        public int StartAmountCoins { get; set; }
    }

    public class CardConfig : IConfig
    {
        public override string Name => "CardConfig";
        public override string Section { get; } = "cards";
        public int MaxCardsInDeck { get; set; }
        public int MinCardsInDeck { get; set; }
        public int ReqNbrCardsInPackage { get; set; }
    }

    public class ResponseConfig : IConfig
    {
        public override string Name => "ResponseConfig";
        public override string Section { get; } = "responses/english";
        public Dictionary<string, dynamic> Response { get; set; }
        public string this[string key]
        {
            get
            {
                if (Response.TryGetValue(key, out dynamic value))
                {
                    return value.ToString();
                }

                Console.WriteLine($"[ERROR] No response found for key: {key}");

                return "DEFAULT RESPONSE";
            }
        }
    }
}

