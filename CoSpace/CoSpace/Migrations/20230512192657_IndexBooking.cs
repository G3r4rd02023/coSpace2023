using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoSpace.Migrations
{
    /// <inheritdoc />
    public partial class IndexBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StartDate",
                table: "Bookings",
                column: "StartDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_StartDate",
                table: "Bookings");
        }
    }
}
