using System;

namespace MTCG;

public class User : IModel
{
    virtual public string Name { get; set; } = string.Empty;
    virtual public string Bio { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Coins { get; set; }
    public Role UserAccessLevel { get; set;} = Role.ANONYMOUS;
    public string Token { get; set; } = string.Empty;
    virtual public Guid ID { get; set; }
    public string Language { get; set; } = "english";
    public IEnumerable<Card> Stack { get; set; } = new List<Card>();
    public IEnumerable<DeckCard> Deck { get; set; } = new List<DeckCard>();

    public User()
    {
    }

    public object ToSerializableObj()
    {
        return new
        {
            Name,
            Bio,
            Image,
            Coins,
            ID = ID.ToString()
        };
    }
}