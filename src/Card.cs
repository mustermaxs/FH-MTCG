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
    Kraken
}

public class Card : IModel
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public float Damage { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public CardName Name { get; set; }
    public string Element { get; set; }
    public string Type { get; set; }
}