using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;
using System;
using System.Threading.Tasks;

namespace MTCG_Unittests
{
    [TestClass]
    public class MTCGServerTests
    {
        private MockRepository _mockRepository;
        private MTCGServer _mTCGSever;


        [TestInitialize]
        public void TestInitialize()
        {
            this._mockRepository = new MockRepository(MockBehavior.Strict);
            _mTCGSever = new MTCGServer();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this._mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ValidatePORT_ValidPort_ReturnsTrue()
        {
            // Arrange
            int validPort = 1234;

            // Act
            bool result = _mTCGSever.ValidatePORT(validPort);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidatePORT_InvalidPort_ReturnsFalse()
        {
            // Arrange
            int invalidPort = -1;

            // Act
            bool result = _mTCGSever.ValidatePORT(invalidPort);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateIP_ValidIP_ReturnsTrue()
        {
            // Arrange
            string validIP = "192.168.0.1";

            // Act
            bool result = _mTCGSever.ValidateIP(validIP);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidateIP_InvalidIP_ReturnsFalse()
        {
            // Arrange
            string invalidIP = "not_an_ip";

            // Act
            bool result = _mTCGSever.ValidateIP(invalidIP);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ReplaceUnwantedSurroundings_RemovesBrackets_ReturnsModifiedString()
        {
            // Arrange
            string input = "[Test]";

            // Act
            string result = MTCGServer.ReplaceUnwantedSurroundings(input);

            // Assert
            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public void GetDataFromPath_ReturnsFileName()
        {
            // Arrange
            string path = "folder/subfolder/filename.txt";

            // Act
            string result = _mTCGSever.GetDataFromPath(path);

            // Assert
            Assert.AreEqual("filename.txt", result);
        }

        [TestMethod]
        public void ExtractValueFromJsonString_ReturnsExtractedValue()
        {
            // Arrange
            string jsonString = "{\"key\": \"value\"}";
            string key = "key";

            // Act
            string result = _mTCGSever.ExtractValueFromJsonString(jsonString, key);

            // Assert
            Assert.AreEqual("value", result);
        }
    }
}
