namespace Domain.VideoGames;

public sealed record VideoGameId(Guid Value)
{
    public static VideoGameId New() => new(Guid.NewGuid());

    public static VideoGameId Empty() => new(Guid.Empty);

    public override string ToString() => Value.ToString();
}
