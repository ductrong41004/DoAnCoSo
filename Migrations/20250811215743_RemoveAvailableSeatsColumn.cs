using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourDuLich.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAvailableSeatsColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSeats",
                table: "Tours");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableSeats",
                table: "Tours",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
