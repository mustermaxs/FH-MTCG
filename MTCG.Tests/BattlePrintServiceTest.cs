using NUnit.Framework;
using System;
using MTCG;
using System.Reflection;
using NUnit.Framework.Internal;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace UnitTest.MTCG
{
    [TestFixture]
    public class BattlePrintServiceTest
    {
        public Dictionary<string, string> t = new Dictionary<string, string> { { "a", "1 #2 #3" } };

        [Test]
        public void GetBattleLogAsTxt_ReturnsExpectedResult()
        {
            // mock for battle with 3 battle logs
            // mock for 3 battle logs

            // Arrange

            // var battleObj = new Battle();
            // battleObj.Id = Guid.NewGuid();
            // battleObj.Player1 = new User();
            // battleObj.Player1.Id = Guid.NewGuid();
            // battleObj.Player1.Name = "Player1";
            // battleObj.Player2 = new User();
            // battleObj.Player2.Id = Guid.NewGuid();
            // battleObj.Player2.Name = "Player2";
            // battleObj.Winner = battleObj.Player1;
            // battleObj.IsDraw = false;
            // battleObj.EndDateTime = DateTime.Now;
            // battleObj.BattleLog = new List<BattleLogEntry>();
            // battleObj.BattleLog.Add(new BattleLogEntry());
            // battleObj.BattleLog.Add(new BattleLogEntry());
            // battleObj.BattleLog.Add(new BattleLogEntry());
            // battleObj.BattleLog[0].Id = Guid.NewGuid();
            // battleObj.BattleLog[0].Player1 = battleObj.Player1;
            // battleObj.BattleLog[0].Player2 = battleObj.Player2;
            // battleObj.BattleLog[0].ActionDescriptions = "ActionDescriptions";
            // battleObj.BattleLog[0].CardPlayedPlayer1 = new Card();
            // battleObj.BattleLog[0].CardPlayedPlayer1.Id = Guid.NewGuid();
            // battleObj.BattleLog[0].CardPlayedPlayer1.Name = "CardPlayedPlayer1";
            // battleObj.BattleLog[0].CardPlayedPlayer2 = new Card();
            // battleObj.BattleLog[0].CardPlayedPlayer2.Id = Guid.NewGuid();
            // battleObj.BattleLog[0].CardPlayedPlayer2.Name = "CardPlayedPlayer2";
            // battleObj.BattleLog[0].RoundWinner = battleObj.Player1;
            // battleObj.BattleLog[0].TimeStamp = DateTime.Now;
            // battleObj.BattleLog[0].BattleId = battleObj.Id;
            // battleObj.BattleLog[0].RoundNumber = 1;
            // battleObj.BattleLog[0].IsDraw = false;
            // Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");

            // var bp = new BattlePrintService();
            // BattleConfig battleConfig = new BattleConfig();
            // battleConfig.Load<BattleConfig>("config.json");
            // bp.battleConfig = battleConfig;

            string test = "1 2 3";
            string a = Txt("a", ("2", "2"), ("3", "3"));

            Assert.That(test == a);


        }

        public string Txt(string templateName, params (string key, dynamic value)[] keyValPairs)
        {
            var pairs = keyValPairs.Select(x => ("#" + x.Item1, x.Item2)).ToArray();
            var template = t![templateName];
            // return pairs.Aggregate(template, (current, pair) => current.Replace("#" + pair.Item1, pair.Item2));

            foreach (var pair in pairs)
            {
                template = template.Replace(pair.Item1, pair.Item2);
            }

            Console.WriteLine(template);

            return template;
        }
    }
}