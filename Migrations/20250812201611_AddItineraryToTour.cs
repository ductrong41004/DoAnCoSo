using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourDuLich.Migrations
{
    /// <inheritdoc />
    public partial class AddItineraryToTour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Itinerary",
                table: "Tours",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Itinerary",
                table: "Tours");
        }
    }
}
