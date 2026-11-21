using Domain.VideoGames;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VideoGamesConfiguration : IEntityTypeConfiguration<VideoGame>
{
    public void Configure(EntityTypeBuilder<VideoGame> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new VideoGameId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Title)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.HasIndex(x => x.Title)
            .IsUnique();

        builder.Property(x => x.Price)
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(x => x.ReleaseDate)
            .HasConversion(new DateTimeUtcConverter())
            .IsRequired();

        builder.Property(x => x.Platform)
            .HasConversion<string>()
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
    }
}
