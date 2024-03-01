using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;
using System;

namespace MTCG_Unittests
{
    [TestClass]
    public class BuffsTests
    {
        private MockRepository _mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this._mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this._mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Buffs_Constructor_CreatesInstance()
        {
            // Arrange
            Buffs buffs = new Buffs();

            // Assert
            Assert.IsNotNull(buffs);
            Assert.IsFalse(buffs.BuffExists);
            CollectionAssert.AreEqual(new List<EDinoTypes>(), buffs.BuffTypes);
            CollectionAssert.AreEqual(new List<int>(), buffs.BuffAmount);
        }

        [TestMethod]
        public void Buffs_AddBuff_AddsBuffCorrectly()
        {
            // Arrange
            Buffs buffs = new Buffs();
            EDinoTypes buffType = EDinoTypes.AERIAL;
            int buffAmount = 3;

            // Act
            buffs.AddBuff(buffType, buffAmount);

            // Assert
            Assert.IsTrue(buffs.BuffExists);
            CollectionAssert.Contains(buffs.BuffTypes, buffType);
            CollectionAssert.Contains(buffs.BuffAmount, buffAmount);
        }

        [TestMethod]
        public void Buffs_RemoveBuffs_RemovesAllBuffs()
        {
            // Arrange
            Buffs buffs = new Buffs();
            buffs.AddBuff(EDinoTypes.AQUATIC, 2);
            buffs.AddBuff(EDinoTypes.TERRESTRIAL, 5);

            // Act
            buffs.RemoveBuffs();

            // Assert
            Assert.IsFalse(buffs.BuffExists);
            CollectionAssert.AreEqual(new List<EDinoTypes>(), buffs.BuffTypes);
            CollectionAssert.AreEqual(new List<int>(), buffs.BuffAmount);
        }

        [TestMethod]
        public void Buffs_RemoveBuffs_WhenNoBuffs_NoEffect()
        {
            // Arrange
            Buffs buffs = new Buffs();

            // Act
            buffs.RemoveBuffs();

            // Assert
            Assert.IsFalse(buffs.BuffExists);
            CollectionAssert.AreEqual(new List<EDinoTypes>(), buffs.BuffTypes);
            CollectionAssert.AreEqual(new List<int>(), buffs.BuffAmount);
        }
    }
}

