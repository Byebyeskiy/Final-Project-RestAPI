using Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ListingReservationsConfiguration : IEntityTypeConfiguration<ListingReservation>
{
    public const string ListingForeignKey = "fk_listing_reservations_listings_listing_id";

    public void Configure(EntityTypeBuilder<ListingReservation> builder)
    {
        builder.ToTable(table => table.HasCheckConstraint("ck_listing_reservations_positive_quantity", "quantity > 0"));
        builder.HasKey(reservation => reservation.Id);
        builder.Property(reservation => reservation.Id).ValueGeneratedNever();
        builder.Property(reservation => reservation.ListingId)
            .HasConversion(id => id.Value, value => new ListingId(value));
        builder.HasOne<Listing>().WithMany()
            .HasForeignKey(reservation => reservation.ListingId)
            .HasConstraintName(ListingForeignKey)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
