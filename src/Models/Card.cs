using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MTCG;

public enum CardName
{
    WaterGoblin,
    FireGoblin,
    RegularGoblin,
    WaterTroll,
    FireTroll,
    RegularTroll,
    WaterElf,
    FireElf,
    RegularElf,
    WaterSpell,
    FireSpell,
    RegularSpell,
    Knight,
    Dragon,
    Ork,
    Kraken,
    FireKraken,
    DEFAULT
}

public enum CardElement
{
    Water = 1,
    Fire = 2,
    Normal = 4
}

// realizes bitwise comparises
public enum CardType
{
    Null = 0,
    Monster = 1,
    Spell = 2,
    MonsterVsSpell = Monster | Spell,
}


public class StackCard : Card
{
    public Guid? StackId { get; set; } = null;

}

public class DeckCard : Card
{
    public Guid? DeckId { get; set; } = null;
}

public class Card : IModel
{
    public Guid Id { get; set; }
    public string? Description { get; set; } = string.Empty;
    public float Damage { get; set; } = 0.0f;
    [JsonConverter(typeof(StringEnumConverter))]
    public string? Name { get; set; } = string.Empty;
    public string? Element { get; set; } = string.Empty;

    public bool Locked { get; set; } = false;
    public string? Type { get; set; } = string.Empty;
    // realized later that an enum would be easier to handle
    // public CardType TypeEnum
    // {
    //     get
    //     {

    //     }
    //     set
    //     {
    //         if (value == CardType.Monster)
    //             Type = "monster";
    //         if (value == CardType.Spell)
    //             Type = "spell";
    //         else
    //             Type = "null";
    //     }
    // }
    public object ToSerializableObj()
    {
        return new
        {
            Id = Id.ToString(),
            Description,
            Damage,
            Name,
            Element,
            Type
        };
    }
    public Card()
    {
        Id = Guid.NewGuid();
        Description = string.Empty;
        Damage = 0.0f;
        Name = string.Empty;
        Element = string.Empty;
        Type = string.Empty;
        // TypeEnum = CardType.Null;
    }
}

public static class CardExtensions
{
    /// <summary>
    /// Returns enum version of a cards type.
    /// </summary>
    /// <param name="card"></param>
    /// <returns>CardType enum.</returns>
    public static CardType Type(this Card card)
    {
        switch (card.Type)
        {
            case "monster":
                return CardType.Monster;
            case "spell":
                return CardType.Spell;
            case "monsterspell":

            default:
                return CardType.Null;
        }
    }

    public static CardElement Element(this Card card)
    {
        switch (card.Element)
        {
            case "water":
                return CardElement.Water;
            case "fire":
                return CardElement.Fire;
            default:
                return CardElement.Normal;
        }
    }

}