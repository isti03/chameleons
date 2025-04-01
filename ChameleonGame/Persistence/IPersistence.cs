namespace ChameleonGame.Persistence
{
    public interface IPersistence
    {
        Task<Board> LoadAsync(String path);

        Task<Board> LoadAsync(Stream stream);

        Task SaveAsync(String path, Board board);

        Task SaveAsync(Stream stream, Board board);
    }
}
