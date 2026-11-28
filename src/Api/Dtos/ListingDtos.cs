using Domain.Listings;

namespace Api.Dtos;

public record ListingDto(Guid Id, string Title, decimal Price, DateTime AvailableSince,
    ListingCategory Category, DateTime CreatedAt, string Status, string? RejectionReason, int AvailableQuantity)
{
    public static ListingDto FromDomainModel(Listing listing)
        => new(listing.Id.Value, listing.Title, listing.Price, listing.AvailableSince,
            listing.Category, listing.CreatedAt, listing.Status.ToString(), listing.RejectionReason, listing.AvailableQuantity);
}

public record CreateListingDto(
    string Title,
    decimal Price,
    DateTime AvailableSince,
    ListingCategory Category, int Quantity = 1);

public record UpdateListingDto(
    string Title,
    decimal Price,
    DateTime AvailableSince,
    ListingCategory Category);

public record RejectListingDto(string Reason);

public record ReserveListingDto(int Quantity);

public record ListingReservationDto(Guid Id, Guid ListingId, int Quantity, DateTime CreatedAt)
{
    public static ListingReservationDto FromDomainModel(ListingReservation reservation) =>
        new(reservation.Id, reservation.ListingId.Value, reservation.Quantity, reservation.CreatedAt);
}
