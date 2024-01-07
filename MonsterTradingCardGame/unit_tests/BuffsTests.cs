using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class BuffsTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private Buffs CreateBuffs()
        {
            return new Buffs();
        }

        [TestMethod]
        public void AddBuff_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var buffs = CreateBuffs();
            EDinoTypes type = default;
            int amount = 0;

            // Act
            buffs.AddBuff(
                type,
                amount);

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void RemoveBuffs_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var buffs = CreateBuffs();

            // Act
            buffs.RemoveBuffs();

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
