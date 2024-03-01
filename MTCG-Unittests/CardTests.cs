using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;

namespace MTCG_Unittests
{
    [TestClass]
    public class CardTests
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
        public void Card_Constructor_CreatesInstance()
        {
            // Arrange
            string id = "1";
            EDinoTypes dinoType = EDinoTypes.TERRESTRIAL;
            string name = "EarthMonster";
            string description = "A sturdy terrestrial monster.";
            int shopCost = 25;
            int damage = 40;

            // Act
            Card card = new Card(id, dinoType, name, description, shopCost, damage);

            // Assert
            Assert.IsNotNull(card);
            Assert.AreEqual(id, card.ID);
            Assert.AreEqual(dinoType, card.DinoType);
            Assert.AreEqual(name, card.Name);
            Assert.AreEqual(description, card.Description);
            Assert.AreEqual(shopCost, card.ShopCost);
            Assert.AreEqual(damage, card.Damage);
        }

        [TestMethod]
        public void Card_ToString_ReturnsCorrectString()
        {
            // Arrange
            string id = "2";
            EDinoTypes dinoType = EDinoTypes.AQUATIC;
            string name = "WaterMonster";
            string description = "A swift aquatic monster.";
            int shopCost = 15;
            int damage = 30;

            Card card = new Card(id, dinoType, name, description, shopCost, damage);

            // Act
            string result = card.ToString();

            // Assert
            string expected = $"Name: {name} | Dinotype: {Enum.GetName(typeof(EDinoTypes), dinoType)} | Damage: {damage} | Description: {description} | Cost: {shopCost}|";
            Assert.AreEqual(expected, result);
        }
    }
}
