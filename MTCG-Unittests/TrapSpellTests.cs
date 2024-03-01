using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;

namespace MTCG_Unittests
{
    [TestClass]
    public class TrapSpellTests
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
        public void TrapSpell_Constructor_CreatesInstance()
        {
            // Arrange
            string id = "123";
            EDinoTypes dinoType = EDinoTypes.AQUATIC;
            string name = "TrapSpell";
            string description = "Description";
            int shopCost = 30;
            int damage = 15;
            EDinoTypes trapTrigger = EDinoTypes.VOLCANIC;

            // Act
            TrapSpell trapSpell = new TrapSpell(id, dinoType, name, description, shopCost, damage, trapTrigger);

            // Assert
            Assert.IsNotNull(trapSpell);
            Assert.AreEqual(id, trapSpell.ID);
            Assert.AreEqual(dinoType, trapSpell.DinoType);
            Assert.AreEqual(ESpellTypes.TRAP, trapSpell.SpellType);
            Assert.AreEqual(name, trapSpell.Name);
            Assert.AreEqual(description, trapSpell.Description);
            Assert.AreEqual(shopCost, trapSpell.ShopCost);
            Assert.AreEqual(damage, trapSpell.Damage);
            Assert.AreEqual(trapTrigger, trapSpell.TrapTrigger);
        }

        [TestMethod]
        public void TrapSpell_ToString_ReturnsExpectedString()
        {
            // Arrange
            string id = "123";
            EDinoTypes dinoType = EDinoTypes.AQUATIC;
            string name = "TrapSpell";
            string description = "Description";
            int shopCost = 30;
            int damage = 15;
            EDinoTypes trapTrigger = EDinoTypes.VOLCANIC;

            TrapSpell trapSpell = new TrapSpell(id, dinoType, name, description, shopCost, damage, trapTrigger);

            // Act
            string result = trapSpell.ToString();

            // Assert
            string expected = $"Name: {name} | Damage: {damage} {dinoType}-damage | Trap-trigger: {trapTrigger} | Description: {description} | Cost: {shopCost}|";
            Assert.AreEqual(expected, result);
        }
    }
}
