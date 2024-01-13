using NUnit.Framework;
using MTCG;
using NUnit.Framework.Internal;
using System.IO;

namespace UnitTests.MTCG;

[TestFixture]
public class Test_FileHandler
{
    [SetUp]
    public void Setup()
    {
        Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");
    }



    [Test]
    public void FileHandler_FindsKeyValuePair()
    {
        Directory.SetCurrentDirectory("/home/mustermax/vscode_projects/MTCG/MTCG.Tests/");

        string res = FileHandler.SearchKeyValueInFile("config.txt", "PRICE_VIP_PACKAGE");

        Assert.IsTrue(res == "15", $"Value is {res}");
    }




    [Test]
    public void FileHandler_GetsJsonObjectFromFile()
    {
        var res = FileHandler.ReadJsonFromFile("config.json");

        Assert.IsTrue(res!["server"]["SERVER_IP"] == "127.0.0.1"
        && res!["server"]["SERVER_PORT"] == 12000);
    }
}
