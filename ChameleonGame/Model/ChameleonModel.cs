using ChameleonGame.Persistence;

namespace ChameleonGame.Model
{
    public class ChameleonModel
    {
        private IPersistence persistence;
        private Board board;

        public int boardSize { get => board.size; }
        public Color currentPlayer { get => board.currentPlayer;  }

        public event EventHandler? GameLoaded;
        public event EventHandler<FieldChangedEventArgs>? FieldChanged;
        public event EventHandler? CurrentPlayerChanged;
        public event EventHandler<GameOverEventArgs>? GameOver;

        public ChameleonModel(IPersistence persistence)
        {
            this.persistence = persistence;
            board = new Board(5);
        }

        public void NewGame(int boardSize)
        {
            board = new Board(boardSize);

            /* Set the chameleons */
            for (int i = 0; i < board.size; i++)
            {
                for (int j = 0; j < board.size; j++)
                {
                    board[i, j].chameleon = board[i, j].color;
                    board[i, j].fieldChanged += OnFieldChanged;
                }
            }

            GameLoaded?.Invoke(this, EventArgs.Empty);
        }

        public async Task LoadGame(string path)
        {
            await LoadGame(File.OpenRead(path));
        }

        public async Task LoadGame(Stream stream)
        {
            if (persistence is null)
                throw new Exception();

            board = await persistence.LoadAsync(stream);

            for (int i = 0; i < board.size; i++)
                for (int j = 0; j < board.size; j++)
                    board[i, j].fieldChanged += OnFieldChanged;

            GameLoaded?.Invoke(this, EventArgs.Empty);
        }

        public async Task SaveGame(string path)
        {
            await SaveGame(File.OpenWrite(path));
        }

        public async Task SaveGame(Stream stream)
        {
            if (persistence is null)
                throw new Exception();

            await persistence.SaveAsync(stream, board);
        }

        private bool IsValidCoordinate(int x, int y) => Math.Min(x, y) >= 0 && Math.Max(x, y) < board.size;

        public Field? GetField(int x, int y) => IsValidCoordinate(x, y) ? board[x, y] : null;

        public Field? GetField((int x, int y) coord) => GetField(coord.x, coord.y);

        public (int x, int y) GetFieldCoordinates(Field field)
        {
            for (int i = 0; i < board.size; i++)
            {
                for (int j = 0; j < board.size; j++)
                {
                    if (board[i, j] == field)
                    {
                        return (i, j);
                    }
                }
            }
            throw new ArgumentException();
        }

        public bool IsValidStep((int x, int y) src, (int x, int y) dest)
        {
            Field? srcField = GetField(src);
            Field? destField = GetField(dest);

            if (srcField is null || srcField.chameleon == Color.Empty || srcField.chameleon != board.currentPlayer || destField is null || destField.chameleon != Color.Empty)
                return false;

            if (board.winner is not null)
                return false;

            int dx = dest.x - src.x;
            int dy = dest.y - src.y;

            Func<int, int, int> AbsMin = (int a, int b) => Math.Min(Math.Abs(a), Math.Abs(b));
            Func<int, int, int> AbsMax = (int a, int b) => Math.Max(Math.Abs(a), Math.Abs(b));

            if (AbsMin(dx, dy) != 0 || AbsMax(dx, dy) > 2 || AbsMax(dx, dy) == 0)
                return false;

            /* Jumping over another chameleon */
            if (AbsMax(dx, dy) == 2)
            {
                Field middleField = GetField(src.x + dx / 2, src.y + dy / 2)!;
                return middleField.chameleon != Color.Empty && middleField.chameleon != srcField.chameleon;
            }

            return true;
        }

        private Field? GetFieldWithEnemyChameleon(Color chameleonColor)
        {
            for (int i = 0; i < board.size; i++)
            {
                for (int j = 0; j < board.size; j++)
                {
                    Field f = board[i, j];
                    if (f.chameleon == chameleonColor && f.color != chameleonColor && f.color != Color.Empty)
                        return f;
                }
            }
            return null;
        }

        public void Step((int x, int y) src, (int x, int y) dest)
        {
            if (!IsValidStep(src, dest))
                throw new ArgumentException("Illegal step");

            Field srcField = GetField(src)!;
            Field destField = GetField(dest)!;

            /* Check which field has a chameleon that could potentially change color */
            Field? lastForeignTarget = GetFieldWithEnemyChameleon(board.currentPlayer);
            if (lastForeignTarget is not null && lastForeignTarget == srcField && destField.color != Color.Empty)
                lastForeignTarget = destField;

            /* Do the step */
            destField.chameleon = srcField.chameleon;
            srcField.chameleon = Color.Empty;

            int dx = dest.x - src.x;
            int dy = dest.y - src.y;

            Func<int, int, int> AbsMax = (int a, int b) => Math.Max(Math.Abs(a), Math.Abs(b));

            /* Jumping over another chameleon */
            if (AbsMax(dx, dy) == 2)
            {
                Field middleField = GetField(src.x + dx / 2, src.y + dy / 2)!;
                middleField.chameleon = Color.Empty;
            }

            /* Change chameleon color if needed */
            if (lastForeignTarget is not null && lastForeignTarget.chameleon != Color.Empty)
                lastForeignTarget.chameleon = lastForeignTarget.color;

            /* Change current player to the other player */
            board.ChangeCurrentPlayer();
            CurrentPlayerChanged?.Invoke(this, EventArgs.Empty);

            /* Check if game is over */
            Color? winner = board.winner;
            if (winner is not null)
                GameOver?.Invoke(this, new GameOverEventArgs((Color)winner));
        }

        public void OnFieldChanged(object? s, EventArgs e)
        {
            Field f = (s as Field)!;
            (int x, int y) = GetFieldCoordinates(f);
            FieldChanged?.Invoke(this, new FieldChangedEventArgs(x, y, f.chameleon.ToString()));
        }
    }
}
