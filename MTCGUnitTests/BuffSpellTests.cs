using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MonsterTradingCardGame.unit_tests
{
    [TestClass]
    public class BuffSpellTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private BuffSpell CreateBuffSpell()
        {
            return new BuffSpell(
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
            var buffSpell = CreateBuffSpell();

            // Act
            var result = buffSpell.ToString();

            // Assert
            Assert.Fail();
            mockRepository.VerifyAll();
        }
    }
}
