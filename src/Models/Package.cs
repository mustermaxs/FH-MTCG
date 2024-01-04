using System;

namespace MTCG;

public class Package : IModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<Card> Cards { get; set; }

    public Package()
    {
        Id = Guid.NewGuid();
        Cards = new List<Card>();
    }

    public object ToSerializableObj()
    {
        return new
        {
            Id,
            UserId,
            Cards = Cards.Select(c => c.ToSerializableObj()).ToList()
        };
    }

}