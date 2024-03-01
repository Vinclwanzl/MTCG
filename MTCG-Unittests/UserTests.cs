using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;
using System.Collections.Generic;

namespace MTCG_Unittests
{
    [TestClass]
    public class UserTests
    {
        private MockRepository _mockRepository;
        private List<Card> _deck;
        

        [TestInitialize]
        public void TestInitialize()
        {
            this._mockRepository = new MockRepository(MockBehavior.Strict);
            _deck = new List<Card>
            {
                new Monster("1", EDinoTypes.AQUATIC, "Waterdino", "Description", 10, 10),
                new Spell("2", EDinoTypes.TERRESTRIAL, ESpellTypes.NORMAL, "EarthSpell", "Description", 20, 15),
                new BuffSpell("3", EDinoTypes.VOLCANIC, "FireBoost", "Description", 25, 20, 5),
                new TrapSpell("4", EDinoTypes.VOLCANIC, "FeuermitFeuer", "Description", 25, 20, EDinoTypes.VOLCANIC)
            };
        }

        [TestMethod]
        public void User_Constructor_CreatesInstance()
        {
            // Arrange
            string name = "Player1";
            
            // Act
            User user = new User(name, _deck);

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(name, user.Name);
            CollectionAssert.AreEqual(_deck, user.Deck);
        }

        [TestMethod]
        public void User_SetDeck_UpdatesDeck()
        {
            // Arrange
            string name = "Player1";
            List<Card> initialDeck = new List<Card>
            {
                new Monster("1", EDinoTypes.AQUATIC, "Waterdino", "Description", 10, 10),
                new Spell("2", EDinoTypes.TERRESTRIAL, ESpellTypes.NORMAL, "EarthSpell", "Description", 20, 15),
                new Monster("3", EDinoTypes.AQUATIC, "Waterdino", "Description", 10, 10),
                new Spell("4", EDinoTypes.TERRESTRIAL, ESpellTypes.NORMAL, "EarthSpell", "Description", 20, 15)
            };

            List<Card> newDeck = new List<Card>
            {
                new Monster("3", EDinoTypes.AERIAL, "Airdino", "Description", 15, 15),
                new BuffSpell("4", EDinoTypes.VOLCANIC, "FireBoost", "Description", 25, 20, 5)
            };

            User user = new User(name, initialDeck);

            // Act
            user.Deck = newDeck;

            // Assert
            CollectionAssert.AreEqual(newDeck, user.Deck);
        }

        [TestMethod]
        public void User_SetDeck_WithNullDoesNotUpdateDeck()
        {
            // Arrange
            string name = "Player1";
            List<Card> initialDeck = new List<Card>
            {
                new Monster("1", EDinoTypes.AQUATIC, "Waterdino", "Description", 10, 10),
                new Spell("2", EDinoTypes.TERRESTRIAL, ESpellTypes.NORMAL, "EarthSpell", "Description", 20, 15)
            };

            List<Card> newDeck = null;

            User user = new User(name, initialDeck);

            // Act
            user.Deck = newDeck;

            // Assert
            CollectionAssert.AreEqual(initialDeck, user.Deck);
        }
    }
}
