using System;
using System.Collections.Generic;
using System.Linq;

namespace MTCG;

public class Deck : IModel
{
    public List<Card> Cards { get; set; } = new List<Card>();
    public void RemoveCard(Card card)
    {
        var cardToRemove = Cards.SingleOrDefault<Card>(c => c == card);
        Cards.Remove(card);
    }
}