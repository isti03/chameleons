using ChameleonGame.Model;
using ChameleonGame.Persistence;
using Moq;

namespace ChameleonGame.Test
{
    [TestClass]
    public class ChameleonModelTest
    {
        private ChameleonModel model = null!;
        private Board mockedBoard = null!;
        private Mock<IPersistence> mockedPersistence = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            mockedBoard = new Board(5);
            mockedBoard[0, 0].chameleon = Persistence.Color.Green;
            mockedBoard[1, 0].chameleon = Persistence.Color.Red;

            mockedPersistence = new Mock<IPersistence>();
            mockedPersistence.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                             .Returns(() => Task.FromResult(mockedBoard));

            model = new ChameleonModel(mockedPersistence.Object);
        }

        [TestMethod]
        public void TestStep()
        {
            model.NewGame(5);
            model.Step((2, 3), (2, 2));
            Assert.AreEqual(Persistence.Color.Empty, model.GetField(2, 3)!.chameleon);
            Assert.AreEqual(Persistence.Color.Green, model.GetField(2, 2)!.chameleon);
        }

        [TestMethod]
        public void TestJump()
        {
            model.NewGame(5);
            model.Step((0, 2), (2, 2));
            Assert.AreEqual(Persistence.Color.Empty, model.GetField(0, 2)!.chameleon);
            Assert.AreEqual(Persistence.Color.Green, model.GetField(2, 2)!.chameleon);
            Assert.AreEqual(Persistence.Color.Empty, model.GetField(1, 2)!.chameleon);
        }

        [TestMethod]
        public void TestPlayerTurns()
        {
            model.NewGame(5);
            
            // Red can not start
            Assert.ThrowsException<ArgumentException>(() => model.Step((2, 1), (2, 2)));

            // Green moves
            model.Step((0, 2), (2, 2));

            // Now Green can not move again
            Assert.ThrowsException<ArgumentException>(() => model.Step((0, 1), (0, 2)));

            // Red moves
            model.Step((1, 1), (1, 2));
        }

        [TestMethod]
        public void TestIllegalSteps()
        {
            model.NewGame(7);
            model.Step((5, 3), (3, 3));

            // Not moving at all
            Assert.ThrowsException<ArgumentException>(() => model.Step((0, 0), (0, 0)));

            // To an occupied field
            Assert.ThrowsException<ArgumentException>(() => model.Step((3, 2), (4, 2)));

            // Jumping over a players own chameleon
            Assert.ThrowsException<ArgumentException>(() => model.Step((4, 1), (4, 3)));

            // Jumping over an empty field
            Assert.ThrowsException<ArgumentException>(() => model.Step((6, 3), (4, 3)));

            // Stepping diagonally
            Assert.ThrowsException<ArgumentException>(() => model.Step((4, 4), (5, 3)));

            // Jumping diagonally
            Assert.ThrowsException<ArgumentException>(() => model.Step((6, 1), (4, 3)));

            // Stepping outside the board
            Assert.ThrowsException<ArgumentException>(() => model.Step((6, 0), (6, -1)));
            Assert.ThrowsException<ArgumentException>(() => model.Step((6, 0), (7, 0)));
        }

        [TestMethod]
        public void TestColorChange()
        {
            model.NewGame(3);
            model.Step((1, 0), (1, 1));

            // Stepping with a red chameleon to a green field
            model.Step((1, 2), (1, 0)); 
            model.Step((0, 1), (1, 1));
            // Make sure that the color doesn't change after just 1 round
            Assert.AreEqual(Persistence.Color.Red, model.GetField(1, 0)!.chameleon);

            // Stepping with another red chameleon
            model.Step((2, 2), (1, 2));
            // Now the red chameleon that was moved first should have turned green
            Assert.AreEqual(Persistence.Color.Green, model.GetField(1, 0)!.chameleon);
        }

        [TestMethod]
        public async Task TestVictory()
        {
            await model.LoadGame(String.Empty);
            Color? winner = null;
            model.GameOver += (s, e) => { winner = e.winner; };
            model.Step((0, 0), (2, 0));
            Assert.AreEqual(Color.Green, winner);
        }

        [TestMethod]
        public async Task TestLoadGame()
        {
            await model.LoadGame(String.Empty);
            Assert.AreEqual(mockedBoard.size, model.boardSize);
            for (int i = 0; i < mockedBoard.size; i++)
            {
                for (int j = 0; j < mockedBoard.size; j++)
                {
                    Assert.AreEqual(mockedBoard[i, j].color, model.GetField(i, j)!.color);
                    Assert.AreEqual(mockedBoard[i, j].chameleon, model.GetField(i, j)!.chameleon);
                }
            }
        }
    }
}