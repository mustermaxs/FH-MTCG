namespace MTCG
{
  public class CardConfig : BaseConfig
    {
        public override string Name => "CardConfig";
        public override string Section { get; protected set;} = "cards";
        public int MaxCardsInDeck { get; set; }
        public int MinCardsInDeck { get; set; }
        public int ReqNbrCardsInPackage { get; set; }
        public int PricePerPackage { get; set; }
        // public CardConfig(IConfigLoader configLoader) :base(configLoader) {}
    }
}

