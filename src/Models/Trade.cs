using System;
using System.Net.Security;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


public class CoinTrade : IModel
{
    public Guid? Id { get; set; }
    public Guid? CardToTrade { get; set; }
    public int? CoinsReceived { get; set; }
    public Guid? OfferingUserId { get; set; }
    public bool Settled { get; set; } = false;

    public object ToSerializableObj()
    {
        return new
        {
            Id = Id.ToString(),
            CardToTrade = CardToTrade.ToString(),
            CoinsReceived,
        };
    }
}

public class CardTrade : IModel
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

    public Guid OfferingUserId { get; set; }
    public User? OfferingUser { get; set; }
    public Guid AcceptingUserId { get; set; }
    public User? AcceptingUser { get; set; }
    public Guid? AcceptedDeckCardId { get; set; }
    public bool Settled { get; set; } = false;

    public object ToSerializableObj()
    {
        return new
        {
            Id = Id.ToString(),
            CardToTrade = CardToTrade.ToString(),
            Type,
            MinimumDamage,
        };
    }

}