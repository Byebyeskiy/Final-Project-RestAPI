using Domain.VideoGames;

namespace Tests.Data.VideoGames;

public static class VideoGameTestData
{
    public static VideoGame NewGame() => VideoGame.New(VideoGameId.New(), "First test game", 49.99m,
        new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc), Platform.Pc);

    public static VideoGame Existing() => VideoGame.New(VideoGameId.New(), "Existing test game", 29.99m,
        new DateTime(2024, 7, 12, 0, 0, 0, DateTimeKind.Utc), Platform.Xbox);

    public static VideoGame Updated() => VideoGame.New(VideoGameId.New(), "Updated test game", 59.99m,
        new DateTime(2025, 3, 21, 0, 0, 0, DateTimeKind.Utc), Platform.PlayStation);
}
