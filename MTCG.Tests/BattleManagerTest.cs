using NUnit.Framework;
using System;
using MTCG;
using Moq;
using System.IO;
using System.Collections.Generic;

namespace UnitTest.MTCG
{
    [TestFixture]
    public class BattleManagerTest
    {
        public BattleManager? bm;
        public Mock<User>? mockp1 = null;
        public Mock<User>? mockp2 = null;
        public MockProgram Program = new();


        [SetUp]
        public void Setup()
        {
            mockp1 = new Mock<User>();
            mockp2 = new Mock<User>();
            mockp1.Setup(m => m.Name).Returns("PLAYER 1");
            mockp2.Setup(m => m.Name).Returns("PLAYER 2");


            bm = new BattleManager(mockp1.Object, mockp2.Object, Program.servicesMock.Object.Get<BattleConfig>());
        }


        [Test]
        public void StrongerSpellAndCardOwner_WaterFire_Water_Is_Stronger()
        {
            var waterCard = new Card { Damage = 5, Element = "water" };
            var fireCard = new Card { Damage = 5, Element = "fire" };
            CardAndOwner cp1 = new CardAndOwner { card = waterCard, owner = mockp1!.Object };
            CardAndOwner cp2 = new CardAndOwner { card = fireCard, owner = mockp2!.Object };

            var winner = bm!.GetStrongerSpellCardAndOwner(cp1, cp2);
            var owner = winner?.owner;

            Assert.That(winner != null, "Result is null.");
            Assert.That(owner == mockp1.Object, $"Stronger card was {winner?.card.Element} instead of {fireCard.Element}");
        }

        [Test]
        public void StrongerSpellAndCardOwner_WaterFire_Fire_Is_Stronger()
        {
            var mockCard1 = new Card { Damage = 5, Element = "water" };
            var mockCard2 = new Card { Damage = 20, Element = "fire" };
            CardAndOwner cp1 = new CardAndOwner { card = mockCard1, owner = mockp1!.Object };
            CardAndOwner cp2 = new CardAndOwner { card = mockCard2, owner = mockp2!.Object };

            var winner = bm!.GetStrongerSpellCardAndOwner(cp1, cp2);
            var owner = winner?.owner;

            Assert.That(owner == mockp2.Object, $"Stronger owner was {owner?.Name} instead of {mockp2.Object.Name}");
        }

        [Test]
        public void StrongerSpellAndCardOwner_NormalAndWater_EqualDamage_Is_Null()
        {
            var mockCard1 = new Card { Damage = 5, Element = "normal" };
            var mockCard2 = new Card { Damage = 10, Element = "water" };
            CardAndOwner cp1 = new CardAndOwner { card = mockCard1, owner = mockp1!.Object };
            CardAndOwner cp2 = new CardAndOwner { card = mockCard2, owner = mockp2!.Object };

            var winner = bm!.GetStrongerSpellCardAndOwner(cp1, cp2);

            Assert.That(winner == null, "Result is not null. Cards should be equally strong.");
        }

        [Test]
        public void NextRound_EndsAfter_1_RoundIf_1_CardInDeck()
        {
            var mockCard1 = new DeckCard { Damage = 5, Element = "normal" };
            var mockCard2 = new DeckCard { Damage = 10, Element = "water" };
            var deck1 = new List<DeckCard> { mockCard1 };
            var deck2 = new List<DeckCard> { mockCard2 };
            mockp1!.Object.Deck = deck1;
            mockp2!.Object.Deck = deck2;

            Assert.That(bm!.NextRound() == false, "Should have ended after 1 round.");
        }

        [Test]
        public void Play_EndsAfter_1_RoundWith_1_Card()
        {
            var mockCard1 = new DeckCard { Damage = 5, Element = "normal" };
            var mockCard2 = new DeckCard { Damage = 10, Element = "water" };
            var deck1 = new List<DeckCard> { mockCard1 };
            var deck2 = new List<DeckCard> { mockCard2 };
            mockp1!.Object.Deck = deck1;
            mockp2!.Object.Deck = deck2;

            bm!.Play();

            Assert.That(bm!.roundsPlayed == 1, "Battle should have ended after 1 round.");
        }
    }


    public class MockProgram
    {
        public Mock<ServiceProvider> servicesMock;
        public ServiceProvider services;

        public MockProgram()
        {
            Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");

            servicesMock = new Mock<ServiceProvider>();

            var battleConfig = new BattleConfig();
            battleConfig.Load<BattleConfig>();

            servicesMock.Setup(s => s.Get<BattleConfig>()).Returns(battleConfig);

            services = servicesMock.Object;

            Console.WriteLine("MockProgram initialized");
        }
    }


}
// [Test]
// public void BattleManager_Play_ShouldFinishBattle()
// {
//     // Arrange
//     var player1 = new User();
//     var player2 = new User();
//     var battleManager = new BattleManager(player1, player2);

//     // Act
//     var battle = battleManager.Play();

//     // Assert
//     Assert.IsTrue(battleManager.battleIsFinished);
//     Assert.AreEqual(battle, battleManager.battle);
// }

//     [Test]
//     public void BattleManager_NextRound_ShouldReturnFalseWhenEndConditionsMet()
//     {
//         // Arrange
//         var player1 = new User();
//         var player2 = new User();
//         var battleManager = new BattleManager(player1, player2);

//         // Set up end conditions
//         player1.Deck.Clear();
//         player2.Deck.Clear();

//         // Act
//         var result = battleManager.NextRound();

//         // Assert
//         Assert.IsFalse(result);
//     }

//     [Test]
//     public void BattleManager_HigherDamageAndOwner_ShouldReturnStrongerCardAndOwner()
//     {
//         // Arrange
//         var player1 = new User();
//         var player2 = new User();
//         var battleManager = new BattleManager(player1, player2);

//         var card1 = new Card { Damage = 10 };
//         var card2 = new Card { Damage = 5 };

//         // Act
//         var result = battleManager.HigherDamageAndOwner(card1, card2);

//         // Assert
//         Assert.IsNotNull(result);
//         Assert.AreEqual(card1, result.card);
//         Assert.AreEqual(player1, result.owner);
//     }

//     // Add more tests for other methods in the BattleManager class

// }
