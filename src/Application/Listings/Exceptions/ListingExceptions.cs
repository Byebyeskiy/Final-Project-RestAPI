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

public sealed class ListingAlreadyModeratedException(ListingId id)
    : ListingException(id, $"Listing with id '{id}' has already been moderated");

public sealed class ListingUnhandledException(ListingId id, Exception inner)
    : ListingException(id, $"Could not save the resale item change for '{id}'", inner);

public sealed class ListingUnavailableException(ListingId id)
    : ListingException(id, $"Listing '{id}' is not approved or does not have enough stock");

public sealed class ListingHasReservationsException(ListingId id)
    : ListingException(id, $"Listing '{id}' cannot be deleted while it has reservations");
