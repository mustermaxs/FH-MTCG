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

public enum CardType
{
    Monster = 1,
    Spell = 2,
    Null = 0
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
    public CardType TypeEnum
    {
        get
        {
            if (TypeEnum != CardType.Null)
            {
                if (Type == "monster" && TypeEnum == CardType.Monster)
                    return TypeEnum;
                if (Type == "spell" && TypeEnum == CardType.Spell)
                    return TypeEnum;
                else
                    return CardType.Null;
            }
            else
            {
                if (Type == "monster")
                    return CardType.Monster;
                if (Type == "spell")
                    return CardType.Spell;
                else
                    return CardType.Null;
            }
        }
        set
        {
            if (value == CardType.Monster)
                Type = "monster";
            if (value == CardType.Spell)
                Type = "spell";
            else
                Type = "null";
        }
    }
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
        TypeEnum = CardType.Null;
    }
}