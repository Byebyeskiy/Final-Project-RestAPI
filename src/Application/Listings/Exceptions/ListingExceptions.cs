using Domain.Listings;

namespace Application.Listings.Exceptions;

public abstract class ListingException(ListingId id, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public ListingId ListingId { get; } = id;
}

public sealed class ListingAlreadyExistsException(ListingId id, string title)
    : ListingException(id, $"Listing with title '{title}' already exists");

public sealed class ListingNotFoundException(ListingId id)
    : ListingException(id, $"Listing with id '{id}' was not found");

public sealed class ListingUnhandledException(ListingId id, Exception inner)
    : ListingException(id, $"Could not save the resale item change for '{id}'", inner);
