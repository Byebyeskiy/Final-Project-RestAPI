using Domain.Listings;

namespace Api.Dtos;

public record ListingDto(Guid Id, string Title, decimal Price, DateTime AvailableSince,
    ListingCategory Category, DateTime CreatedAt, string Status, string? RejectionReason)
{
    public static ListingDto FromDomainModel(Listing listing)
        => new(listing.Id.Value, listing.Title, listing.Price, listing.AvailableSince,
            listing.Category, listing.CreatedAt, listing.Status.ToString(), listing.RejectionReason);
}

public record CreateListingDto(
    string Title,
    decimal Price,
    DateTime AvailableSince,
    ListingCategory Category);

public record UpdateListingDto(
    string Title,
    decimal Price,
    DateTime AvailableSince,
    ListingCategory Category);

public record RejectListingDto(string Reason);
