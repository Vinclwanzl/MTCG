using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;

namespace MTCG_Unittests
{
    [TestClass]
    public class MonsterTests
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
        public void Monster_Constructor_CreatesInstance()
        {
            // Arrange
            string id = "123";
            EDinoTypes dinoType = EDinoTypes.AQUATIC;
            string name = "Waterdino";
            string description = "Description";
            int shopCost = 30;
            int damage = 15;

            // Act
            Monster monster = new Monster(id, dinoType, name, description, shopCost, damage);

            // Assert
            Assert.IsNotNull(monster);
            Assert.AreEqual(id, monster.ID);
            Assert.AreEqual(dinoType, monster.DinoType);
            Assert.AreEqual(name, monster.Name);
            Assert.AreEqual(description, monster.Description);
            Assert.AreEqual(shopCost, monster.ShopCost);
            Assert.AreEqual(damage, monster.Damage);
        }

        [TestMethod]
        public void Monster_ToString_ReturnsExpectedString()
        {
            // Arrange
            string id = "123";
            EDinoTypes dinoType = EDinoTypes.AQUATIC;
            string name = "Waterdino";
            string description = "Description";
            int shopCost = 30;
            int damage = 15;

            Monster monster = new Monster(id, dinoType, name, description, shopCost, damage);

            // Act
            string result = monster.ToString();

            // Assert
            string expected = $"Name: {name} | Dinotype: {dinoType} | Damage: {damage} | Description: {description} | Cost: {shopCost}|";
            Assert.AreEqual(expected, result);
        }
    }
}
