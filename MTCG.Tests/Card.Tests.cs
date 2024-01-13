using NUnit.Framework;
using System;
using MTCG;
using System.Reflection;
using NUnit.Framework.Internal;
using Moq;
using System.Collections.Generic;

namespace UnitTests.MTCG;

[TestFixture]
public class Test_Cards
{
    [SetUp]
    public void Setup()
    {

    }

    [TestCase("goblin", CardName.Goblin)]
    [TestCase("Goblin", CardName.Goblin)]
    [TestCase("DRAGON", CardName.Dragon)]
    [TestCase("dragon", CardName.Dragon)]
    [TestCase("wiZaRd", CardName.Wizard)]
    public void CardExtension_ReturnsEnumForName(string cardName, CardName enumValue)
    {
        var mockCard = new Card { Name = cardName };

        Assert.That(mockCard.ToCardName() == enumValue, $"Failed to convert {cardName} to enum.");
    }
}