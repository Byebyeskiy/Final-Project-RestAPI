using Domain.Listings;

namespace Tests.Data.Listings;

public static class ListingTestData
{
    public static Listing NewGame() => Listing.New(ListingId.New(), "Wool Coat", 49.99m,
        new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc), ListingCategory.Clothing);

    public static Listing Existing() => Listing.New(ListingId.New(), "Leather Boots", 29.99m,
        new DateTime(2024, 7, 12, 0, 0, 0, DateTimeKind.Utc), ListingCategory.Footwear);

    public static Listing Updated() => Listing.New(ListingId.New(), "Canvas Tote", 59.99m,
        new DateTime(2025, 3, 21, 0, 0, 0, DateTimeKind.Utc), ListingCategory.Accessories);
}
