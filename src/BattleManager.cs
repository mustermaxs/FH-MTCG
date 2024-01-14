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
    public List<DeckCard> playedCardsPlayer1 { get; set; } = new List<DeckCard>();
    public List<DeckCard> playedCardsPlayer2 { get; set; } = new List<DeckCard>();
    public int roundsPlayed { get; private set; }
    public bool battleIsFinished { get; set; }
    public CardRepository? cardRepo { get; set; } 
    public Battle battle { get; set; }
    public string? battleToken { get; private set; } = null;


    public BattleManager(User player1, User player2, BattleConfig config)
    {
        this.player1 = player1;
        this.player2 = player2;
        this.battle = new Battle();
        this.roundsPlayed = 0;
        this.battleIsFinished = false;
        this.config = config;
        // this.cardRepo = ServiceProvider.GetDisposable<CardRepository>();
    }

    public void SetBattleToken(string token)
    {
        if (battleToken == string.Empty || battleToken == null)
            battleToken = token;
    }

    public void Setup()
    {
        // TODO resConfig für exception msg


        if (!LoadUserDeck(player1) || !LoadUserDeck(player2))
            throw new Exception("Failed to load users deck");

            battle.Player1 = player1;
            battle.Player2 = player2;
    }


    public void UseCardRepo(IService cardRepo)
    {
        this.cardRepo = (CardRepository)cardRepo;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


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


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Entry point for battle.
    /// </summary>
    /// <returns>Battle result.</returns>
    public Battle Play()
    {
        Setup();

        while (NextRound()) ;

        if (roundsPlayed == config.MaxNbrRounds)
            battle.Winner = null;
        
        battle.CountRounds = roundsPlayed;
        battle.EndDateTime = DateTime.Now;

        return battle;
    }



    /// <summary>
    /// Starts next round.
    /// </summary>
    /// <returns>Bool. Inidicates if battle meets ending criiteria or not.</returns>
    /// <exception cref="Exception">When card played that isnt registered as an allowed type.</exception>
    public bool NextRound()
    {
        if (!ShouldContinue()) return false;

        var cardPlayer1 = DrawCard(player1);
        var cardPlayer2 = DrawCard(player2);

        var cardAndOwner1 = new CardAndOwner { card = cardPlayer1!, owner = player1 };
        var cardAndOwner2 = new CardAndOwner { card = cardPlayer2!, owner = player2 };

        BattleLogEntry battleLogEntry;

        if (CardType.Monster == (cardPlayer1!.Type() & cardPlayer2!.Type()))
            battleLogEntry = HandleMonsterVsMonster(cardAndOwner1, cardAndOwner2);

        else if (CardType.Spell == (cardPlayer1!.Type() & cardPlayer2!.Type()))
            battleLogEntry = HandleSpellVsSpell(cardAndOwner1, cardAndOwner2);

        else if ((CardType.Monster | CardType.Spell) == (cardPlayer1!.Type() | cardPlayer2!.Type()))
            battleLogEntry = HandleSpellVsMonster(cardAndOwner1, cardAndOwner2);
        else
            throw new Exception("Unhandled card type");

        battle.BattleLog.Add(battleLogEntry);

        // TODO transfer card to winner
        playedCardsPlayer1.Add(cardPlayer1!);
        playedCardsPlayer2.Add(cardPlayer2!);
        roundsPlayed++;

        return ShouldContinue();
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public BattleLogEntry HandleSpellVsSpell(CardAndOwner co1, CardAndOwner co2)
    {
        var result = HigherCalcDamage(co1, co2);
        var cardPlayer1 = co1.card;
        var cardPlayer2 = co2.card;

        string description = string.Empty;
        // REMOVE
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

        result = GetStrongerElementCard(co1, co2);

        return NewEntry(cardPlayer1, cardPlayer2, result?.owner);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    public string ActionMsg(Card cardPlayer1, Card cardPlayer2)
    {
        return $"{player1.Name} plays {cardPlayer1.Name}\n{player2.Name} plays {cardPlayer2.Name}\n";
    }

    public void TransferCardToUser(User fromUser, User toUser, DeckCard card)
    {
        throw new NotImplementedException();
    }

    public BattleLogEntry HandleSpellVsMonster(CardAndOwner co1, CardAndOwner co2)
    {
        CardAndOwner? winner = HigherCalcDamage(co1, co2);

        return NewEntry(co1.card, co2.card, winner?.owner);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    public BattleLogEntry HandleMonsterVsMonster(CardAndOwner co1, CardAndOwner co2)
    {
        // throw new NotImplementedException();
        var cardPlayer1 = co1.card;
        var cardPlayer2 = co2.card;
        CardAndOwner? winner = null;

        if (cardPlayer1.ToCardName() == cardPlayer2.ToCardName())
            winner = HigherCalcDamage(co1, co2, 1.0, null);
        else
            winner = GetStrongerMonsterCard(co1, co2);

        return NewEntry(cardPlayer1, cardPlayer2, winner?.owner);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public CardAndOwner? GetStrongerMonsterCard(CardAndOwner co1, CardAndOwner co2)
    {
        var cardPlayer1 = co1.card;
        var cardPlayer2 = co2.card;

        var goblinVsDragon = (CardName.Goblin | CardName.Dragon) == (cardPlayer1.ToCardName() | cardPlayer2.ToCardName());
        var wizardVsOrk = (CardName.Wizard | CardName.Ork) == (cardPlayer1.ToCardName() | cardPlayer2.ToCardName());
        var fireelfVsDragon = (CardName.FireElf | CardName.Dragon) == (cardPlayer1.ToCardName() | cardPlayer2.ToCardName());

        if (goblinVsDragon)
            return HigherCalcDamage(co1, co2, 0, cardPlayer1.ToCardName() == CardName.Goblin ? cardPlayer1 : cardPlayer2);
        else if (wizardVsOrk)
            return HigherCalcDamage(co1, co2, 0, cardPlayer1.ToCardName() == CardName.Ork ? cardPlayer1 : cardPlayer2);
        else if (fireelfVsDragon)
            return HigherCalcDamage(co1, co2, 0, cardPlayer1.ToCardName() == CardName.Dragon ? cardPlayer1 : cardPlayer2);
        else
            return HigherCalcDamage(co1, co2);
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



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


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////



    /// <summary>
    /// Returns the card with the higher calculated damage and its owner.
    /// Based on the effectedByFactor param, it determines the card
    /// to which it should apply the multiplier to.
    /// </summary>
    /// <param name="c1">
    /// CardAndOwner Player1.
    /// </param>
    /// <param name="c2">
    /// CardAndOwner Player2.
    /// </param>
    /// <param name="factor">
    /// Factor by which damage should be multiplied with.
    /// </param>
    /// <returns>Stronger CardAndOwner. Null if they are equally strong,
    /// even after applying a damage factor.</returns>
    public CardAndOwner? HigherCalcDamage(CardAndOwner c1, CardAndOwner c2, double factor = 1.0, Card? effectedByFactor = null)
    {
        var card1 = c1.card;
        var card2 = c2.card;
        double factor1 = 1;
        double factor2 = 1;

        if (effectedByFactor != null)
        {
            bool card1IsEffectedCard = effectedByFactor == card1 ? true : false;
            if (card1IsEffectedCard) factor1 = factor;
            if (!card1IsEffectedCard) factor2 = factor;
        }

        if (card1.Damage * factor1 == card2.Damage * factor2)
            return null;

        bool card1Stronger = card1.Damage * factor1 > card2.Damage * factor2;

        return card1Stronger ? c1 : c2;
    }



    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public CardAndOwner? GetStrongerElementCard(CardAndOwner cp1, CardAndOwner cp2)
    {
        var cardPlayer1 = cp1.card;
        var cardPlayer2 = cp2.card;

        var damage1 = cardPlayer1.Damage;
        var damage2 = cardPlayer2.Damage;

        // check card combination
        bool waterAndFire = (CardElement.Water | CardElement.Fire) == (cardPlayer1.Element() | cardPlayer2.Element());
        bool fireAndNormal = (CardElement.Normal | CardElement.Fire) == (cardPlayer1.Element() | cardPlayer2.Element());
        bool normalAndWater = (CardElement.Normal | CardElement.Water) == (cardPlayer1.Element() | cardPlayer2.Element());
        bool normalAndNormal = (CardElement.Normal | CardElement.Normal) == (cardPlayer1.Element() | cardPlayer2.Element());

        // handle individual cases with their special rules
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


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public bool LoadUserDeck(User user)
    {
        var deck = user.Deck;
        deck = cardRepo.GetDeckByUserId(user.ID);

        if (deck.Count() == 0) return false;

        user.Deck = deck;

        return true;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public bool ShouldContinue()
    {
        int cardCountPlayer1 = player1.Deck?.Count() ?? 0;
        int cardCountPlayer2 = player2.Deck?.Count() ?? 0;
        bool reachedMaxRound = roundsPlayed >= config.MaxNbrRounds;

        return cardCountPlayer1 != 0
            && cardCountPlayer2 != 0
            && !reachedMaxRound;
    }


    //////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////


    public DeckCard? DrawCard(User player)
    {
        List<DeckCard> deck = player.Deck.ToList();

        int cardCount = deck.Count;

        if (cardCount == 0) return null;

        Random random = new Random();
        var randIndex = random.Next(0, cardCount);

        var card = deck[randIndex];
        deck.RemoveAt(randIndex);

        player.Deck = deck;

        return card;
    }

}