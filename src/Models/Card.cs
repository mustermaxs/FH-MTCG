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
    DEFAULT
}

public class Card : IModel
{
    public Guid Id { get; set; }
    public string? Description { get; set; } = string.Empty;
    public float Damage { get; set; } = 0.0f;
    [JsonConverter(typeof(StringEnumConverter))]
    public CardName Name { get; set; } = CardName.DEFAULT;
    public string? Element { get; set; } = string.Empty;
    public string? Type { get; set; } = string.Empty;


    public Card()
    {
        Id = Guid.NewGuid();
        Description = string.Empty;
        Damage = 0.0f;
        Name = CardName.DEFAULT;
        Element = string.Empty;
        Type = string.Empty;
    }
}