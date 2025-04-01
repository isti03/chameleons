using System.Data;

namespace ChameleonGame.Persistence
{
    public class BinaryFilePersistence : IPersistence
    {
        public async Task<Board> LoadAsync(String path)
        {
            return await LoadAsync(File.OpenRead(path));
        }

        public async Task<Board> LoadAsync(Stream stream)
        {
            try
            {
                byte[] fileData = new byte[stream.Length];
                _ = await stream.ReadAsync(fileData, 0, (int)stream.Length);

                int size = fileData[0];
                int currentPlayerID = fileData[1];
                
                Board board = new Board(size, currentPlayerID);
                
                for (int i = 0; i < size * size; i++)
                    board[i / size, i % size].chameleon = (Color)fileData[i+2];

                return board;
            }
            catch
            {
                throw new DataException("Error occurred during reading.");
            }
        }

        public async Task SaveAsync(String path, Board board)
        {
            await SaveAsync(File.OpenWrite(path), board);
        }

        public async Task SaveAsync(Stream stream, Board board)
        {
            try
            {
                Byte[] fileData = new Byte[2 + board.size * board.size];

                fileData[0] = (Byte)board.size;
                fileData[1] = (Byte)board.currentPlayer;

                for (int i = 0; i < board.size; i++)
                    for (int j = 0; j < board.size; j++)
                        fileData[2 + i * board.size + j] = (Byte)board[i, j].chameleon;

                await stream.WriteAsync(fileData);
            }
            catch
            {
                throw new DataException("Error occurred during writing.");
            }
        }
    }
}
