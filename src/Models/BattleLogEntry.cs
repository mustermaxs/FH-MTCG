namespace MTCG
{
  public class BattleLogEntry : IModel
    {
        public Guid? Id { get; set; }
        public User? Player1 { get; set; }
        public User? Player2 { get; set; }
        public string? ActionDescriptions { get; set; }
        public Card? CardPlayedPlayer1 { get; set; }
        public Card? CardPlayedPlayer2 { get; set; }
        public int CountCardsLeftPlayer1 { get; set; }
        public int CountCardsLeftPlayer2 { get; set; }
        public User? RoundWinner { get; set; }
        public DateTime? TimeStamp { get; set; }
        public Guid? BattleId { get; set; }
        public int RoundNumber { get; set; }
        public bool IsDraw { get; set; }
        public string Thief { get; set; }

        public BattleLogEntry()
        {
            Id = null;
            Player1 = null;
            Player2 = null;
            ActionDescriptions = null;
            CardPlayedPlayer1 = null;
            CardPlayedPlayer2 = null;
            RoundWinner = null;
            TimeStamp = null;
            BattleId = null;
            RoundNumber = 0;
            IsDraw = false;
            CountCardsLeftPlayer1 = 0;
            CountCardsLeftPlayer2 = 0;
            Thief = string.Empty;
        }

        public object ToSerializableObj()
        {
            return new
            {
                Player1 = Player1?.Name,
                Player2 = Player2?.Name,
                ActionDescriptions,
                CardPlayedPlayer1 = CardPlayedPlayer1?.ToSerializableObj(),
                CardPlayedPlayer2 = CardPlayedPlayer2?.ToSerializableObj(),
                RoundWinner = RoundWinner?.Name,
                TimeStamp,
                IsDraw,
                CountCardsLeftPlayer1,
                CountCardsLeftPlayer2,
                Thief
            };
        }
    }
}
