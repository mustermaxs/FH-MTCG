using System;

namespace MTCG;

public class Package : IModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<Card> Cards { get; set; }
    public List<Guid> CardIds { get; set; }

    public Package()
    {
        Id = Guid.Empty;
        Cards = new List<Card>();
        CardIds = new List<Guid>();
    }

    public object ToSerializableObj()
    {
        return new
        {
            Id,
            Cards = Cards.Select(c => c.ToSerializableObj()).ToList(),
            CardIds = CardIds.Select(c => c.ToString()).ToList()
        };
    }

}