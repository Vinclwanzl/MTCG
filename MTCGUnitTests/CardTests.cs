using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class CardTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private Card CreateCard()
        {
            return new Card(
                TODO,
                TODO,
                TODO,
                TODO,
                TODO,
                TODO);
        }

        [TestMethod]
        public void ToString_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var card = CreateCard();

            // Act
            var result = card.ToString();

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
