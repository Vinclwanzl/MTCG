using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardGame;
using Moq;

namespace MTCG_Unittests
{
    [TestClass]
    public class BattleTests
    {
        private MockRepository mockRepository;
        private User player1;
        private User player2;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            
            List<Card> deck1 = new List<Card>
            {
                new Monster("1", EDinoTypes.AQUATIC, "Waterdino", "Description", 10, 10),
                new Spell("2", EDinoTypes.TERRESTRIAL, ESpellTypes.NORMAL, "EarthSpell", "Description", 20, 15),
                new BuffSpell("4", EDinoTypes.VOLCANIC, "FireBoost", "Description", 25, 20, 5),
                new TrapSpell("4", EDinoTypes.VOLCANIC, "FeuermitFeuer", "Description", 25, 20, EDinoTypes.VOLCANIC)
            };

            List<Card> deck2 = new List<Card>
            {
                new Monster("3", EDinoTypes.AERIAL, "Airdino", "Description", 15, 15 ),
                new Spell("2", EDinoTypes.TERRESTRIAL, ESpellTypes.NORMAL, "EarthSpell", "Description", 20, 15),
                new BuffSpell("4", EDinoTypes.VOLCANIC, "FireBoost", "Description", 25, 20, 5),
                new TrapSpell("4", EDinoTypes.VOLCANIC, "FeuermitFeuer", "Description", 25, 20, EDinoTypes.VOLCANIC)
            };

            player1 = new User("Player1", deck1);
            player2 = new User("Player2", deck2);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Battle_Constructor_CreatesInstance()
        {
            // Act
            Battle battle = new Battle(player1, player2);

            // Assert
            Assert.IsNotNull(battle);
            Assert.AreEqual(player1, battle.Player1);
            Assert.AreEqual(player2, battle.Player2);
            Assert.IsFalse(battle.HasEnded);
            Assert.AreEqual("", battle.Winner);
        }

        [TestMethod]
        public void Battle_StartBattle_ReturnsWinner()
        {
            // Arrange
            Battle battle = new Battle(player1, player2);

            // Act
            string winner = battle.StartBattle();

            // Assert
            Assert.IsTrue(battle.HasEnded);
            Assert.IsNotNull(winner);
        }

        [TestMethod]
        public void Battle_StartBattle_WithNullPlayers_ReturnsErrorMessage()
        {
            // Arrange
            Battle battle = new Battle(null, null);

            // Act
            string result = battle.StartBattle();

            // Assert
            Assert.AreEqual("both players are non existant", result);
            Assert.IsTrue(battle.HasEnded);
        }

        [TestMethod]
        public void Battle_StartBattle_WithNullPlayer2_ReturnsErrorMessage()
        {
            // Arrange
            Battle battle = new Battle(player1, null);

            // Act
            string result = battle.StartBattle();

            // Assert
            Assert.AreEqual("player2 has vanished", result);
            Assert.IsTrue(battle.HasEnded);
        }

        [TestMethod]
        public void Battle_StartBattle_WithNullPlayer1_ReturnsErrorMessage()
        {
            // Arrange
            Battle battle = new Battle(null, player2);

            // Act
            string result = battle.StartBattle();

            // Assert
            Assert.AreEqual("player1 has vanished", result);
            Assert.IsTrue(battle.HasEnded);
        }
    }
}
