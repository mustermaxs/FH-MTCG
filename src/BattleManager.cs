using System;
using System.Linq;

namespace MTCG;

public class BattleManager
{
    protected BattleConfig config = ConfigService.Get<BattleConfig>();
    protected User player1 { get; set; }
    protected User player2 { get; set; }
    public readonly int roundsPlayed;
    public bool battleIsFinished { get; set; }
    protected CardRepository cardRepo { get; set; }
    protected Battle battle { get; set; }
    public BattleManager(User player1, User player2)
    {
        this.player1 = player1;
        this.player2 = player2;
        this.battle = new Battle();
        this.roundsPlayed = 0;
        this.battleIsFinished = false;
        this.cardRepo = new CardRepository();

        Setup();
    }

    public void Setup()
    {
        // TODO resConfig für exception msg
        if (!LoadUserDeck(player1) || !LoadUserDeck(player2))
            throw new Exception("Failed to load users deck");
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

        if (MeetsEndConditions()) return false;

        // TODO so wie in Router.ClientHasPermission bitwise vergleichen
        // um zu checken clients den gleichen karten-typ haben
        // oder unterschiedliche (siehe Regeln)
        // ansonsten individuell Handler aufrufen
        // return userAccessLevel == (requestAccessLevel & userAccessLevel);

        // var combinedCards = 

        // if (cardPlayer1 & )

            return true;


    }

    protected void HandleMonsterCards(Card cardPlayer1, Card cardPlayer2)
    {

    }



    protected bool LoadUserDeck(User user)
    {
        var deck = user.Deck;
        deck = cardRepo.GetDeckByUserId(user.ID);

        if (deck.Count() == 0) return false;

        user.Deck = deck;

        return true;
    }

    protected bool MeetsEndConditions()
    {
        int cardCountPlayer1 = player1.Deck?.Count() ?? 0;
        int cardCountPlayer2 = player2.Deck?.Count() ?? 0;
        bool reachedMaxRound = roundsPlayed >= config.MaxNbrRounds;

        return cardCountPlayer1 == 0
            || cardCountPlayer2 == 0
            || reachedMaxRound;
    }

    protected Card? DrawCard(IEnumerable<Card>? deck)
    {
        int cardCount = deck?.Count() ?? 0;

        if (deck == null || cardCount == 0) return null;

        Random random = new Random();
        var randIndex = random.Next(0, cardCount);

        // TODO remove card from deck after played? or after lost round
        return deck.ElementAt(randIndex);
    }

}