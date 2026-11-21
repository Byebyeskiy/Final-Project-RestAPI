namespace Domain.VideoGames;

public class VideoGame
{
    public VideoGameId Id { get; }
    public string Title { get; private set; }
    public decimal Price { get; private set; }
    public DateTime ReleaseDate { get; private set; }
    public Platform Platform { get; private set; }
    public DateTime CreatedAt { get; }

    private VideoGame(VideoGameId id, string title, decimal price, DateTime releaseDate,
        Platform platform, DateTime createdAt)
        => (Id, Title, Price, ReleaseDate, Platform, CreatedAt)
            = (id, title, price, releaseDate, platform, createdAt);

    public static VideoGame New(VideoGameId id, string title, decimal price,
        DateTime releaseDate, Platform platform)
        => new(id, title, price, releaseDate, platform, DateTime.UtcNow);

    public void UpdateDetails(string title, decimal price, DateTime releaseDate,
        Platform platform)
        => (Title, Price, ReleaseDate, Platform) = (title, price, releaseDate, platform);
}
