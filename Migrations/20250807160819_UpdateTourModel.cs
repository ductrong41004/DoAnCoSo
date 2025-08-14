using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourDuLich.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTourModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Tours",
                newName: "RegistrationStartDate");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Tours",
                newName: "RegistrationEndDate");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Tours",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "Tours",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Tours");

            migrationBuilder.RenameColumn(
                name: "RegistrationStartDate",
                table: "Tours",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "RegistrationEndDate",
                table: "Tours",
                newName: "EndDate");
        }
    }
}
