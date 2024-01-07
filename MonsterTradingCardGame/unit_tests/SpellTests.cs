using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class SpellTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private Spell CreateSpell()
        {
            return new Spell(
                TODO,
                TODO,
                TODO,
                TODO,
                TODO,
                TODO,
                TODO);
        }

        [TestMethod]
        public void ToString_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var spell = CreateSpell();

            // Act
            var result = spell.ToString();

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
