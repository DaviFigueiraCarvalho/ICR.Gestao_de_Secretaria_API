using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ICR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveFlagToChurch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "church",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "church");
        }
    }
}
