using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservation.Service.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReservationToHaveBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockDescription",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlockTitle",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlock",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockDescription",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "BlockTitle",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsBlock",
                table: "Reservations");
        }
    }
}
