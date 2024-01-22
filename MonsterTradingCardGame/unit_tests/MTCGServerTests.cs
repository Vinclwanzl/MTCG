﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class MTCGServerTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private MTCGServer CreateMTCGServer()
        {
            return new MTCGServer();
        }

        [TestMethod]
        public async Task StartServer_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            string ipAddress = null;
            int portNumber = 0;
            string dbIPAddress = null;
            int dbPortNumber = 0;

            // Act
            await mTCGServer.StartServer(
                ipAddress,
                portNumber,
                dbIPAddress,
                dbPortNumber);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TurnHttpDataToParameterD_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            Dictionary httpDataD = null;
            Dictionary parameterD = null;

            // Act
            var result = mTCGServer.TurnHttpDataToParameterD(
                httpDataD,
                out parameterD);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetParametersFromJsonObject_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            JObject jsonData = null;

            // Act
            var result = mTCGServer.GetParametersFromJsonObject(
                jsonData);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetParametersFromJsonArray_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            JArray jsonArray = null;
            string path = null;

            // Act
            var result = mTCGServer.GetParametersFromJsonArray(
                jsonArray,
                path);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void WaitForOpponent_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();

            // Act
            var result = mTCGServer.WaitForOpponent();

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void AddPlayer_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            User player = null;

            // Act
            mTCGServer.AddPlayer(
                player);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetDataFromPath_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            string path = null;

            // Act
            var result = mTCGServer.GetDataFromPath(
                path);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ExtractValueOutOfJsonString_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            string jsonString = null;
            string key = null;

            // Act
            var result = mTCGServer.ExtractValueOutOfJsonString(
                jsonString,
                key);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ExtractHTTPdata_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            string request = null;
            Dictionary httpDataD = null;

            // Act
            var result = mTCGServer.ExtractHTTPdata(
                request,
                out httpDataD);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ValidatePORT_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            int input = 0;

            // Act
            var result = mTCGServer.ValidatePORT(
                input);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ValidateIP_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mTCGServer = CreateMTCGServer();
            string input = null;

            // Act
            var result = mTCGServer.ValidateIP(
                input);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
