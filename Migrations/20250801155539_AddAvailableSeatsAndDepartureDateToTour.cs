using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourDuLich.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableSeatsAndDepartureDateToTour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableSeats",
                table: "Tours",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDate",
                table: "Tours",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSeats",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "Tours");
        }
    }
}
