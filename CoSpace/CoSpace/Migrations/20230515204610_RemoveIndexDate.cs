using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoSpace.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIndexDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Spaces_Name",
                table: "Spaces",
                column: "Name",
                unique: true);
            migrationBuilder.DropIndex(
               name: "IX_Bookings_StartDate",
               table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Spaces_Name",
                table: "Spaces");
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StartDate",
                table: "Bookings",
                column: "StartDate",
                unique: true);
        }
    }
}
