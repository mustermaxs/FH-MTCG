using System;

namespace MTCG
{
    public class UserConfig : IConfig
    {
        public override string Name => "UserConfig";
        public override string Section => "user";
        public int StartAmountCoins { get; set; }
    }

    public class CardConfig : IConfig
    {
        public override string Name => "CardConfig";
        public override string Section => "cards";
        public int MaxCardsInDeck { get; set; }
        public int ReqNbrCardsInPackage { get; set; }
    }

    
}

