using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddListingReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "available_quantity",
                table: "listings",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "listing_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_listing_reservations", x => x.id);
                    table.CheckConstraint("ck_listing_reservations_positive_quantity", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_listing_reservations_listings_listing_id",
                        column: x => x.listing_id,
                        principalTable: "listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_listings_nonnegative_quantity",
                table: "listings",
                sql: "available_quantity >= 0");

            migrationBuilder.CreateIndex(
                name: "ix_listing_reservations_listing_id",
                table: "listing_reservations",
                column: "listing_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "listing_reservations");

            migrationBuilder.DropCheckConstraint(
                name: "ck_listings_nonnegative_quantity",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "available_quantity",
                table: "listings");
        }
    }
}
