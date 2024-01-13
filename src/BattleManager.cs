using System;
using System.Linq;

namespace MTCG;

public struct CardAndOwner
{
    public Card card;
    public User owner;
}

public class BattleManager
{
    public BattleConfig config;
    public User player1 { get; set; }
    public User player2 { get; set; }
    public int roundsPlayed { get; private set; }
    public bool battleIsFinished { get; set; }
    public CardRepository cardRepo { get; set; }
    public Battle battle { get; set; }
    public BattleManager(User player1, User player2, BattleConfig config)
    {
        this.player1 = player1;
        this.player2 = player2;
        this.battle = new Battle();
        this.roundsPlayed = 0;
        this.battleIsFinished = false;
        this.config = config;
        this.cardRepo = new CardRepository();

        // Setup();
    }

    public void Setup()
    {
        // TODO resConfig für exception msg
        if (player1.Deck.Count() == 0 || player2.Deck.Count() == 0)
            throw new Exception("Deck is empty");

        if (!LoadUserDeck(player1) || !LoadUserDeck(player2))
            throw new Exception("Failed to load users deck");
    }

    public BattleLogEntry NewEntry(Card cardPlayer1, Card cardPlayer2, User? roundWinner = null, string description = "")
    {
        return new BattleLogEntry
        {
            Player1 = player1,
            Player2 = player2,
            CardPlayedPlayer1 = cardPlayer1,
            CardPlayedPlayer2 = cardPlayer2,
            RoundWinner = roundWinner,
            ActionDescriptions = ActionMsg(cardPlayer1, cardPlayer2) + $"\n{description}",
            TimeStamp = DateTime.Now
        };
    }

    public Battle Play()
    {
        while (NextRound())
            NextRound();

        return battle;

    }
    public bool NextRound()
    {
        var cardPlayer1 = DrawCard(player1.Deck);
        var cardPlayer2 = DrawCard(player2.Deck);

        if (!ShouldContinue()) return false; // end already here if deck empty

        var cardAndOwner1 = new CardAndOwner { card = cardPlayer1!, owner = player1 };
        var cardAndOwner2 = new CardAndOwner { card = cardPlayer2!, owner = player2 };

        BattleLogEntry battleLogEntry;

        if (CardType.Monster == (cardPlayer1!.Type() & cardPlayer2!.Type()))
            battleLogEntry = HandleMonsterVsMonster(cardPlayer1, cardPlayer2);

        else if (CardType.Spell == (cardPlayer1!.Type() & cardPlayer2!.Type()))
            battleLogEntry = HandleSpellVsSpell(cardAndOwner1, cardAndOwner2);

        // player 1 plays monster card, player 2 plays spell card or vice versa
        else if ((CardType.Monster | CardType.Spell) == (cardPlayer1!.Type() | cardPlayer2!.Type()))
            battleLogEntry = HandleSpellVsMonster(cardPlayer1, cardPlayer2);
        else
            throw new Exception("Unhandled card type");

        battle.BattleLog.Add(battleLogEntry);

        // TODO transfer card to winner

        roundsPlayed++;
        return ShouldContinue();
    }

    public BattleLogEntry HandleSpellVsSpell(CardAndOwner co1, CardAndOwner co2)
    {
        var result = HigherCalcDamage(co1, co2);
        var cardPlayer1 = co1.card;
        var cardPlayer2 = co2.card;

        string description = string.Empty;

        if (cardPlayer1.Element() == cardPlayer2.Element())
        {
            if (result == null)
            {
                return NewEntry(cardPlayer1, cardPlayer2, null, description);
            }
            else
            {
                return NewEntry(cardPlayer1, cardPlayer2, result?.owner);
            }
        }
        
        result = GetStrongerSpellCardAndOwner(co1, co2);

        return NewEntry(cardPlayer1, cardPlayer2, result?.owner);
    }

    public string ActionMsg(Card cardPlayer1, Card cardPlayer2)
    {
        return $"{player1.Name} plays {cardPlayer1.Name}\n{player2.Name} plays {cardPlayer2.Name}\n";
    }

    public void TransferCardToUser(User fromUser, User toUser, DeckCard card)
    {
        throw new NotImplementedException();
    }

    public BattleLogEntry HandleSpellVsMonster(DeckCard cardPlayer1, DeckCard cardPlayer2)
    {
        throw new NotImplementedException();
    }

    public BattleLogEntry HandleMonsterVsMonster(DeckCard cardPlayer1, DeckCard cardPlayer2)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Returns the card with the higher damage and its owner
    /// </summary>
    /// <param name="cardPlayer1"></param>
    /// <param name="cardPlayer2"></param>
    /// <returns>(strongerCard, owner) </returns>
    public CardAndOwner? HigherDamageAndOwner(DeckCard cardPlayer1, DeckCard cardPlayer2)
    {
        if (cardPlayer1.Damage == cardPlayer2.Damage)
            return null;

        return cardPlayer1.Damage > cardPlayer2.Damage
            ? new CardAndOwner { card = cardPlayer1, owner = player1 }
            : new CardAndOwner { card = cardPlayer2, owner = player2 };
    }


    /// <summary>
    /// Returns the card with the higher damage and its owner.
    /// Applies a multiplier (factor) to player1 damage. E.g. fire vs. water:
    /// according to the rules, wateer should have its damage doubled - if the
    /// second card has element water, the first card will be multiplied with
    /// the inverse of the factor (0.5).
    /// T is either Enum CardElement, or CardType. Used to determine the beneficial card.
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <param name="factor"></param>
    /// <returns>Stronger CardAndOwner. Null if they are equally strong,
    /// even after applying a damage factor.</returns>
    public CardAndOwner? HigherCalcDamage(CardAndOwner c1, CardAndOwner c2, double factor = 1.0, Card? beneficialCard = null)
    {
        var card1 = c1.card;
        var card2 = c2.card;
        
        if (beneficialCard != null)
        {
            bool player1OwnsBeneficialCard = beneficialCard == card1 ? true : false;
            factor = player1OwnsBeneficialCard ? factor : (1/factor);
        }

        if (card1.Damage * factor == card2.Damage)
            return null;

        bool card1Stronger = card1.Damage * factor > card2.Damage;

        return card1Stronger ? c1 : c2;
    }



    public CardAndOwner? GetStrongerSpellCardAndOwner(CardAndOwner cp1, CardAndOwner cp2)
    {
        // CardAndOwner cardAndOwner = default;

        var cardPlayer1 = cp1.card;
        var cardPlayer2 = cp2.card;

        var damage1 = cardPlayer1.Damage;
        var damage2 = cardPlayer2.Damage;

        bool waterAndFire = (CardElement.Water | CardElement.Fire) == (cardPlayer1.Element() | cardPlayer2.Element());
        bool fireAndNormal = (CardElement.Normal | CardElement.Fire) == (cardPlayer1.Element() | cardPlayer2.Element());
        bool normalAndWater = (CardElement.Normal | CardElement.Water) == (cardPlayer1.Element() | cardPlayer2.Element());
        bool normalAndNormal = (CardElement.Normal | CardElement.Normal) == (cardPlayer1.Element() | cardPlayer2.Element());


        if (waterAndFire)
        {
            var waterCard = CardElement.Water == cardPlayer1.Element() ? cardPlayer1 : cardPlayer2;
            return HigherCalcDamage(cp1, cp2, 2, waterCard);
        }
        else if (fireAndNormal)
        {
            var fireCard = CardElement.Fire == cardPlayer1.Element() ? cardPlayer1 : cardPlayer2;
            return HigherCalcDamage(cp1, cp2, 2, fireCard);
        }
        else if (normalAndWater)
        {
            var normalCard = CardElement.Normal == cardPlayer1.Element() ? cardPlayer1 : cardPlayer2;
            return HigherCalcDamage(cp1, cp2, 2, normalCard);
        }
        else
            return HigherCalcDamage(cp1, cp2);
    }


    public bool LoadUserDeck(User user)
    {
        var deck = user.Deck;
        deck = cardRepo.GetDeckByUserId(user.ID);

        if (deck.Count() == 0) return false;

        user.Deck = deck;

        return true;
    }

    public bool ShouldContinue()
    {
        int cardCountPlayer1 = player1.Deck?.Count() ?? 0;
        int cardCountPlayer2 = player2.Deck?.Count() ?? 0;
        bool reachedMaxRound = roundsPlayed >= config.MaxNbrRounds;

        return cardCountPlayer1 != 0
            && cardCountPlayer2 != 0
            && !reachedMaxRound;
    }

    public DeckCard? DrawCard(IEnumerable<DeckCard>? deck)
    {
        int cardCount = deck?.Count() ?? 0;

        if (deck == null || cardCount == 0) return null;

        Random random = new Random();
        var randIndex = random.Next(0, cardCount);

        // TODO remove card from deck after played? or after lost round
        return deck.ElementAt(randIndex);
    }

}