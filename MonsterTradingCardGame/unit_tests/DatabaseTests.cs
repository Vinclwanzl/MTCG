using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class DatabaseTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private Database CreateDatabase()
        {
            return new Database();
        }

        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var database = CreateDatabase();

            // Act


            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
