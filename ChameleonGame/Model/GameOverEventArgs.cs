using ChameleonGame.Persistence;

namespace ChameleonGame.Model
{
    public class GameOverEventArgs : EventArgs
    {
        public readonly Color winner;

        public GameOverEventArgs(Color winner)
        {
            this.winner = winner;
        }
    }
}
