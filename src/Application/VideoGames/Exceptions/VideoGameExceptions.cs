using Domain.VideoGames;

namespace Application.VideoGames.Exceptions;

public abstract class VideoGameException(VideoGameId id, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public VideoGameId VideoGameId { get; } = id;
}

public sealed class VideoGameAlreadyExistsException(VideoGameId id, string title)
    : VideoGameException(id, $"Video game with title '{title}' already exists");

public sealed class VideoGameNotFoundException(VideoGameId id)
    : VideoGameException(id, $"Video game with id '{id}' was not found");

public sealed class VideoGameUnhandledException(VideoGameId id, Exception inner)
    : VideoGameException(id, $"Could not save the video game change for '{id}'", inner);
