using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class CardCompendiumTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private CardCompendium CreateCardCompendium()
        {
            return new CardCompendium();
        }

        [TestMethod]
        public void AddCardToCompendium_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var cardCompendium = CreateCardCompendium();
            Card card = null;

            // Act
            var result = cardCompendium.AddCardToCompendium(
                card);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ListCardCompendium_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var cardCompendium = CreateCardCompendium();

            // Act
            var result = cardCompendium.ListCardCompendium();

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
