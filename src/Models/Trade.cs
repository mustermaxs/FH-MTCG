using System;
using System.Net.Security;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


public class UserTrade : StoreTrade, IModel
{
    protected Card? acceptedCard;
    public Card? GetAcceptedCard() => acceptedCard;
    public void SetAcceptedCard(Card card) { acceptedCard = card; }
    new public object ToSerializableObj()
    {
        return new
        {
            Id = Id.ToString(),
            CardToTrade = CardToTrade.ToString(),
            Type,
            MinimumDamage,
            OfferingUserId = offeringUserId.ToString(),
            AcceptedCard = acceptedCard?.ToSerializableObj()
        };
    }

}

public class StoreTrade : IModel
{
    public Guid? Id { get; set; }
    public Guid? CardToTrade { get; set; }
    /// <summary>
    /// The required card type.
    /// </summary>
    public string? Type { get; set; } = string.Empty;
    /// <summary>
    /// The required minimum damage.
    /// </summary>
    public float MinimumDamage { get; set; } = 0.0f;

    public Guid GetOfferingUserId() => offeringUserId;
    public void SetOfferingUserId(Guid id) { offeringUserId = id; }
    protected Guid offeringUserId;
    protected User? offeringUser;
    public void SetOfferingUser(User user) { offeringUser = user; }
    public User? GetOfferingUser() => offeringUser;

    public object ToSerializableObj()
    {
        return new
        {
            Id = Id.ToString(),
            CardToTrade = CardToTrade.ToString(),
            Type,
            MinimumDamage,
            OfferingUserId = offeringUserId.ToString()
        };
    }

}