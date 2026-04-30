using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ICR.Infrastructure.Migrations
{
    public partial class AddMinisterInsurance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Insurance",
                table: "minister",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Insurance",
                table: "minister");
        }
    }
}
