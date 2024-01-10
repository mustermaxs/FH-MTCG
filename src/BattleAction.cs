using System;

namespace MTCG
{
    public class BattleAction : IModel
    {
        public Guid IdPlayer1 { get; set; }
        public Guid IdPlayer2 { get; set; }
        public IEnumerable<Card> DeckPlayer1 { get; set; } = new List<Card>();
        public object ToSerializableObj()
        {
            throw new NotImplementedException();
        }
    }
}