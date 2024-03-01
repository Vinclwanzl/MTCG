using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;
using System;

namespace MonsterTradingCardGame
{
    [TestClass]
    public class MTCGDatabaseTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private MTCGDatabase CreateMTCGDatabase()
        {
            return new MTCGDatabase();
        }

        [TestMethod]
        public void EstablishConnection_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGDatabase = this.CreateMTCGDatabase();
            string dbIPAddress = null;
            int dbPortNumber = 0;

            // Act
            var result = mTCGDatabase.EstablishConnection(
                dbIPAddress,
                dbPortNumber);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ExecuteSQLCodeSanitized_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGDatabase = this.CreateMTCGDatabase();
            string sqlCommand = null;
            Dictionary parameterD = null;

            // Act
            var result = mTCGDatabase.ExecuteSQLCodeSanitized(
                sqlCommand,
                parameterD);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void IsTableEmpty_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGDatabase = this.CreateMTCGDatabase();
            string tableName = null;

            // Act
            var result = mTCGDatabase.IsTableEmpty(
                tableName);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetDeckForBattle_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGDatabase = this.CreateMTCGDatabase();
            string username = null;
            List deck = null;

            // Act
            var result = mTCGDatabase.GetDeckForBattle(
                username,
                out deck);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
