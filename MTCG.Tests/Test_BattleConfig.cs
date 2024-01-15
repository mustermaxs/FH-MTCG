using NUnit.Framework;
using MTCG;
using NUnit.Framework.Internal;
using System.IO;

namespace UnitTests.MTCG;

//////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////

[TestFixture]
public class Test_BattleConfig
{
    [SetUp]
    public void Setup()
    {
        Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");
    }



    [TestCase("Goblin", "english", "Goblins are too afraid of Dragons to attack.")]
    [TestCase("Dragon", "german", "Die Feuerelfen kennen Drachen seit ihrer Kindheit und können ihren Angriffen ausweichen.")]
    public void GetTranslationsFromConfig(string monster, string language, string description)
    {
        var config = JsonConfigLoader.Load<BattleConfig>("config.json", "battle");
        string? txt = config!.CardDescription(monster, language);

        if (txt == string.Empty) Assert.Fail();

        Assert.That(txt == description, "Failed to get description.");
    }




    [TestCase("Goblin", "Dragon", false)]
    public void GetPropertiesForMonster(string attacker, string defender, bool wins)
    {
        var config = JsonConfigLoader.Load<BattleConfig>("config.json", "battle");
        var properties = config!.GetMonsterProperties(attacker);

        if (properties == null) Assert.Fail();

        (string player, string opponent, bool win) = properties.Value;

        Assert.That(opponent == defender, "Failed to get properties.");
        Assert.That(win == wins, "Failed to get properties.");
        Assert.That(player == attacker, "Failed to get properties.");
    }
}
