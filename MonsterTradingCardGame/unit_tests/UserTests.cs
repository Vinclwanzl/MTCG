using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class UserTests
    {
        private MockRepository mockRepository;

        private Mock<List<Card>> mockList;

        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);

            mockList = mockRepository.Create<List<Card>>();
        }

        private User CreateUser()
        {
            return new User(
                TODO,
                mockList.Object);
        }

        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var user = CreateUser();

            // Act


            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
