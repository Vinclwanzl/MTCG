using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class BattleTests
    {
        private MockRepository mockRepository;

        private Mock<User> mockUser1;
        private Mock<User> mockUser2;
        
        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);

            mockUser1 = mockRepository.Create<User>();
            mockUser2 = mockRepository.Create<User>();
        }

        private Battle CreateBattle()
        {
            return new Battle(
                mockUser1.Object,
                mockUser2.Object);
        }

        [TestMethod]
        public void StartBattle_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var battle = CreateBattle();

            // Act
            var result = battle.StartBattle();

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
