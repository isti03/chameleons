namespace ChameleonGame.Persistence
{
    public enum Color { Green, Red, Empty }

    internal enum Direction { Up, Right, Down, Left }

    public class Board
    {
        public int size { get; private set; }
        private Field[,] fields;

        public Field this[int x, int y]
        {
            get => fields[x, y];
        }

        private int currentPlayerID = 0;
        public Color currentPlayer { get => (Color)currentPlayerID; }

        public Color? winner
        {
            get {
                Color[] remainingPlayers = fields.Cast<Field>().Select(f => f.chameleon).Where(c => c != Color.Empty).Distinct().ToArray();
                return remainingPlayers.Count() == 1 ? remainingPlayers[0] : null;
            }
        }

        public Board(int size, int currentPlayerID = 0)
        {
            if (size < 3 || size % 2 == 0 || currentPlayerID < 0 || currentPlayerID > 1)
            {
                throw new ArgumentException();
            }
            this.currentPlayerID = currentPlayerID;
            this.size = size;
            fields = new Field[size, size];

            InitializeFields();
        }

        private void InitializeFields()
        {
            // Create the spirals, by changing the color of the appropriate fields
            int x = 0;
            int y = size;
            Direction dir = Direction.Up;

            // Longest end of the spiral
            for (int i = 0; i < size; i++)
            {
                (x, y) = OffsetCoordinate((x, y), dir);
                fields[x, y] = new Field(Color.Green);
            }

            // Intermediate sections of the spiral
            for (int i = size - 2; i > 1; i -= 2)
            {
                for (int j = 0; j < 2; j++)
                {
                    dir = Rotate(dir);
                    for (int k = 0; k < i; k++)
                    {
                        (x, y) = OffsetCoordinate((x, y), dir);
                        fields[x, y] = new Field(Color.Green);
                    }
                }
            }

            // Last section of the spiral (1 field long)
            dir = Rotate(dir);
            (x, y) = OffsetCoordinate((x, y), dir);
            fields[x, y] = new Field(Color.Green);

            // Initialize the remaining fields appropriately
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (fields[i, j] is null)
                    {
                        if (i == size / 2 && j == size / 2)
                            fields[i, j] = new Field(Color.Empty);
                        else
                            fields[i, j] = new Field(Color.Red);
                    }
                }
            }
        }

        private static (int, int) OffsetCoordinate((int, int) coord, Direction dir, int steps = 1)
        {
            var (x, y) = coord;
            switch (dir)
            {
                case Direction.Up: return (x, y - steps);
                case Direction.Right: return (x + steps, y);
                case Direction.Down: return (x, y + steps);
                case Direction.Left: return (x - steps, y);
                default: throw new ArgumentException();
            }
        }

        private static Direction Rotate(Direction dir) => (Direction)(((int)dir + 1) % 4);

        public void ChangeCurrentPlayer()
        {
            // Toggles currentPlayerID between 0 and 1
            currentPlayerID = 1 - currentPlayerID;
        }
    }
}
