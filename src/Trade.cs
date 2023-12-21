using System;

namespace MTCG;


//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////


public class Trade : IModel
{
    public Guid Id { get; set; }
    public Card? CardToTrade { get; set; } = null;
    public string? Type { get; set; } = string.Empty;
    public float MinimumDamage { get; set; } = 0.0f;
}