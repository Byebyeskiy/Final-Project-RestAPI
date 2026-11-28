namespace Domain.Listings;

public class Listing
{
    public ListingId Id { get; }
    public string Title { get; private set; }
    public decimal Price { get; private set; }
    public DateTime AvailableSince { get; private set; }
    public ListingCategory Category { get; private set; }
    public DateTime CreatedAt { get; }
    public int AvailableQuantity { get; private set; } = 1;
    public ListingStatus Status { get; private set; } = ListingStatus.Pending;
    public string? RejectionReason { get; private set; }

    private Listing(ListingId id, string title, decimal price, DateTime availableSince,
        ListingCategory category, DateTime createdAt)
        => (Id, Title, Price, AvailableSince, Category, CreatedAt)
            = (id, title, price, availableSince, category, createdAt);

    public static Listing New(ListingId id, string title, decimal price,
        DateTime availableSince, ListingCategory category, int quantity = 1)
        => new(id, title, price, availableSince, category, DateTime.UtcNow)
        {
            AvailableQuantity = quantity
        };

    public void UpdateDetails(string title, decimal price, DateTime availableSince,
        ListingCategory category)
        => (Title, Price, AvailableSince, Category) = (title, price, availableSince, category);
}
