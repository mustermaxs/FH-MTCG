using System;

namespace MTCG
{
    public class Battle : IModel
    {
        public User? Player1 { get; set; }
        public User? Player2 { get; set; }
        List<string> Actions { get; set; } = new List<string>();

        public Battle()
        {
            Player1 = null;
            Player2 = null;
        }

        public object ToSerializableObj()
        {
            return new
            {
                Player1 = Player1?.Name,
                Player2 = Player2?.Name,
                Actions
            };
        }
    }
}