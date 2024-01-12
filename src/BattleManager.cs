// using System;
// using System.Linq;

// namespace MTCG;

// public class BattleManager
// {
//     protected BattleConfig config = ConfigService.Get<BattleConfig>();
//     protected User player1 { get; set; }
//     protected User player2 { get; set; }
//     public readonly int roundsPlayed;
//     public bool battleIsFinished { get; set; }
//     protected CardRepository cardRepo { get; set; }
//     protected Battle battle { get; set; }
//     public BattleManager(User player1, User player2)
//     {
//         this.player1 = player1;
//         this.player2 = player2;
//         this.battle = new Battle();
//         this.roundsPlayed = 0;
//         this.battleIsFinished = false;
//         this.cardRepo = new CardRepository();

//         Setup();
//     }

//     public void Setup()
//     {
//         // TODO resConfig für exception msg
//         if (!LoadUserDeck(player1) || !LoadUserDeck(player2))
//             throw new Exception("Failed to load users deck");
//     }

//     public Battle Play()
//     {
//         while (NextRound())
//             NextRound();

//         return battle;

//     }
//     public bool NextRound()
//     {
//         var cardPlayer1 = DrawCard(player1.Deck);
//         var cardPlayer2 = DrawCard(player2.Deck);

//         if (MeetsEndConditions()) return false; // end already here if deck empty

//         // TODO so wie in Router.ClientHasPermission bitwise vergleichen
//         // um zu checken clients den gleichen karten-typ haben
//         // oder unterschiedliche (siehe Regeln)
//         // ansonsten individuell Handler aufrufen
//         // return userAccessLevel == (requestAccessLevel & userAccessLevel);

//         // var combinedCards = 

//         var battleLogEntry = new BattleLogEntry();

//         if (CardType.Monster == (cardPlayer1!.Type() & cardPlayer2!.Type()))
//             battleLogEntry = HandleMonsterVsMonster(cardPlayer1, cardPlayer2);

//         if (CardType.Spell == (cardPlayer1!.Type() & cardPlayer2!.Type()))
//             battleLogEntry = HandleSpellVsSpell(cardPlayer1, cardPlayer2);

//         // player 1 plays monster card, player 2 plays spell card or vice versa
//         if ((CardType.Monster | CardType.Spell) == (cardPlayer1!.Type() | cardPlayer2!.Type()))
//             battleLogEntry = HandleSpellVsMonster(cardPlayer1, cardPlayer2);

//         battle.BattleLog.Add(battleLogEntry);


//         return MeetsEndConditions();
//     }

//     private BattleLogEntry HandleSpellVsSpell(DeckCard cardPlayer1, DeckCard cardPlayer2)
//     {
//         DeckCard? cardWithHigherDamage = CardWithHigherDamager(cardPlayer1, cardPlayer2);
//         string description = string.Empty;

//         if (cardPlayer1.Element() == cardPlayer2.Element())
//         {
//             if (cardWithHigherDamage == null)
//                 description = config.
//             return new BattleLogEntry
//             { Player1 = player1, Player2 = player2, RoundWinner = null, ActionDescriptions = };
//         }


//         return new BattleLogEntry
//         { Player1 = player1, Player2 = player2, RoundWinner = cardWithHigherDamage.Owner, ActionDescriptions = { "Draw" } };
//     }

//     private void TransferCardToUser(User fromUser, User toUser, DeckCard card)
//     {
//         throw new NotImplementedException();
//     }

//     private BattleLogEntry HandleSpellVsMonster(DeckCard cardPlayer1, DeckCard cardPlayer2)
//     {
//         throw new NotImplementedException();
//     }

//     private BattleLogEntry HandleMonsterVsMonster(DeckCard cardPlayer1, DeckCard cardPlayer2)
//     {
//         throw new NotImplementedException();
//     }


//     /// <summary>
//     /// Returns the card with the higher damage.
//     /// </summary>
//     /// <param name="cardPlayer1"></param>
//     /// <param name="cardPlayer2"></param>
//     /// <returns>Card with higher damage. Or null if they are equally strong.</returns>
//     private DeckCard? CardWithHigherDamager(DeckCard cardPlayer1, DeckCard cardPlayer2)
//     {
//         if (cardPlayer1.Damage == cardPlayer2.Damage)
//             return null;

//         return cardPlayer1.Damage > cardPlayer2.Damage ? cardPlayer1 : cardPlayer2;
//     }


//     protected bool LoadUserDeck(User user)
//     {
//         var deck = user.Deck;
//         deck = cardRepo.GetDeckByUserId(user.ID);

//         if (deck.Count() == 0) return false;

//         user.Deck = deck;

//         return true;
//     }

//     protected bool MeetsEndConditions()
//     {
//         int cardCountPlayer1 = player1.Deck?.Count() ?? 0;
//         int cardCountPlayer2 = player2.Deck?.Count() ?? 0;
//         bool reachedMaxRound = roundsPlayed >= config.MaxNbrRounds;

//         return cardCountPlayer1 == 0
//             || cardCountPlayer2 == 0
//             || reachedMaxRound;
//     }

//     protected DeckCard? DrawCard(IEnumerable<DeckCard>? deck)
//     {
//         int cardCount = deck?.Count() ?? 0;

//         if (deck == null || cardCount == 0) return null;

//         Random random = new Random();
//         var randIndex = random.Next(0, cardCount);

//         // TODO remove card from deck after played? or after lost round
//         return deck.ElementAt(randIndex);
//     }

// }