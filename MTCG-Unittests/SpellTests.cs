using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;

namespace MTCG_Unittests
{
    [TestClass]
    public class SpellTests
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
        public void Spell_Constructor_CreatesInstance()
        {
            // Arrange
            string id = "1";
            EDinoTypes dinoType = EDinoTypes.VOLCANIC;
            ESpellTypes spellType = ESpellTypes.NORMAL;
            string name = "Fireball";
            string description = "Deals damage to opponent's monster.";
            int shopCost = 10;
            int damage = 20;

            // Act
            Spell spell = new Spell(id, dinoType, spellType, name, description, shopCost, damage);

            // Assert
            Assert.IsNotNull(spell);
            Assert.AreEqual(id, spell.ID);
            Assert.AreEqual(dinoType, spell.DinoType);
            Assert.AreEqual(spellType, spell.SpellType);
            Assert.AreEqual(name, spell.Name);
            Assert.AreEqual(description, spell.Description);
            Assert.AreEqual(shopCost, spell.ShopCost);
            Assert.AreEqual(damage, spell.Damage);
        }

        [TestMethod]
        public void Spell_ToString_ReturnsCorrectString()
        {
            // Arrange
            string id = "1";
            EDinoTypes dinoType = EDinoTypes.AQUATIC;
            ESpellTypes spellType = ESpellTypes.BUFF;
            string name = "Healing Wave";
            string description = "Restores health to your monster.";
            int shopCost = 15;
            int damage = 0;

            Spell spell = new Spell(id, dinoType, spellType, name, description, shopCost, damage);

            // Act
            string result = spell.ToString();

            // Assert
            string expected = $"Name: {name} | Damage: {damage} {Enum.GetName(typeof(EDinoTypes), dinoType)}-damage | " +
                              $"Description: {description} | Cost: {shopCost}|\n";
            Assert.AreEqual(expected, result);
        }
    }
}
