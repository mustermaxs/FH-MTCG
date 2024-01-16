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
        public Dictionary<string, string> testValues = new Dictionary<string, string> { { "a", "1 #2 #3" } };

        [Test]
        public void GetBattleLogAsTxt_ReturnsExpectedResult()
        {
            string expected = "1 2 3";
            string a = Txt("a", ("2", "2"), ("3", "3"));

            Assert.That(expected == a);


        }

        public string Txt(string templateName, params (string key, dynamic value)[] keyValPairs)
        {
            var pairs = keyValPairs.Select(x => ("#" + x.Item1, x.Item2)).ToArray();
            var template = testValues![templateName];
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