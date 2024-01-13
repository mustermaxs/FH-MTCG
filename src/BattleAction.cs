using System;

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
        public User? RoundWinner { get; set; }
        public DateTime? TimeStamp { get; set; }
        public Guid? BattleId { get; set; }

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
        }

        public object ToSerializableObj()
        {
            return new
            {
                Player1 = Player1?.Name,
                Player2 = Player2?.Name,
                ActionDescriptions
            };
        }
    }




    public class Battle : IModel
    {
        public Guid? Id { get; set; }
        public User? Player1 { get; set; }
        public User? Player2 { get; set; }
        public User? Winner { get; set; }
        public bool IsDraw { get; set; }
        public DateTime EndDateTime { get; set; }
        public int CountRounds { get; set; }
        public int GainedPoints { get; set; }
        public List<BattleLogEntry> BattleLog { get; set; }

        public Battle()
        {
            Id = null;
            Player1 = null;
            Player2 = null;
            Winner = null;
            IsDraw = false;
            EndDateTime = DateTime.Now;
            CountRounds = 0;
            GainedPoints = 0;
            BattleLog = new List<BattleLogEntry>();
        }    

        public object ToSerializableObj()
        {
            return new
            {
                Player1 = Player1?.Name,
                Player2 = Player2?.Name,
                Winner = Winner?.Name,
                IsDraw,
                EndDateTime,
                CountRounds,
                GainedPoints,
                BattleLog = BattleLog.Select(c => c.ToSerializableObj())
            };
        }
    }
}