using ChameleonGame.Persistence;

namespace ChameleonGame.Model
{
    public class FieldChangedEventArgs : EventArgs
    {
        public readonly int X;
        public readonly int Y;
        public readonly string ChameleonColor;

        public FieldChangedEventArgs(int X, int Y, string ChameleonColor)
        {
            this.X = X;
            this.Y = Y;
            this.ChameleonColor = ChameleonColor;
        }
    }
}
