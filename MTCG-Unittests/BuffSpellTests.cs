using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;

namespace MTCG_Unittests
{
    [TestClass]
    public class BuffSpellTests
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
        public void BuffSpell_Constructor_CreatesInstance()
        {
            // Arrange
            string id = "123";
            EDinoTypes dinoType = EDinoTypes.AQUATIC;
            string name = "BuffSpell";
            string description = "Description";
            int shopCost = 30;
            int damage = 15;
            int buffAmount = 5;

            // Act
            BuffSpell buffSpell = new BuffSpell(id, dinoType, name, description, shopCost, damage, buffAmount);

            // Assert
            Assert.IsNotNull(buffSpell);
            Assert.AreEqual(id, buffSpell.ID);
            Assert.AreEqual(dinoType, buffSpell.DinoType);
            Assert.AreEqual(ESpellTypes.BUFF, buffSpell.SpellType);
            Assert.AreEqual(name, buffSpell.Name);
            Assert.AreEqual(description, buffSpell.Description);
            Assert.AreEqual(shopCost, buffSpell.ShopCost);
            Assert.AreEqual(damage, buffSpell.Damage);
            Assert.AreEqual(buffAmount, buffSpell.BuffAmount);
        }

        [TestMethod]
        public void BuffSpell_ToString_ReturnsExpectedString()
        {
            // Arrange
            string id = "123";
            EDinoTypes dinoType = EDinoTypes.AQUATIC;
            string name = "BuffSpell";
            string description = "Description";
            int shopCost = 30;
            int damage = 15;
            int buffAmount = 5;

            BuffSpell buffSpell = new BuffSpell(id, dinoType, name, description, shopCost, damage, buffAmount);

            // Act
            string result = buffSpell.ToString();

            // Assert
            string expected = $"Name: {name} | Damage: {damage} {dinoType}-damage | and Buffs: {buffAmount} {dinoType}-damage | " +
                              $"Description: {description} | Cost: {shopCost}|\n";
            Assert.AreEqual(expected, result);
        }
    }
}
