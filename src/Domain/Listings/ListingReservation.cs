namespace Domain.Listings;

public sealed record ListingReservation(Guid Id, ListingId ListingId, int Quantity, DateTime CreatedAt);
