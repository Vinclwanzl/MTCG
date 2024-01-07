using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class TrapSpellTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private TrapSpell CreateTrapSpell()
        {
            return new TrapSpell(
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
            var trapSpell = CreateTrapSpell();

            // Act
            var result = trapSpell.ToString();

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
