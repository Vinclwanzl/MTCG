using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class MonsterTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private Monster CreateMonster()
        {
            return new Monster(
                TODO,
                TODO,
                TODO,
                TODO,
                TODO,
                TODO);
        }

        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var monster = CreateMonster();

            // Act


            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
