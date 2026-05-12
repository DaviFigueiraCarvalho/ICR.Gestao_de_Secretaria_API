using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ICR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInternationalPhoneAndAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "minister");

            migrationBuilder.RenameColumn(
                name: "CellPhone",
                table: "members",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "ZipCode",
                table: "church",
                newName: "PostalCode");

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "minister",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "minister",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "minister",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "minister",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Complement",
                table: "minister",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country_Code",
                table: "minister",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country_CultureCode",
                table: "minister",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country_Name",
                table: "minister",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country_PhoneCountryCode",
                table: "minister",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountyOrRegion",
                table: "minister",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "minister",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country_Code",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country_CultureCode",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country_Name",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country_PhoneCountryCode",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneDisplayFormat",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneE164Format",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneInternationalFormat",
                table: "members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complement",
                table: "church",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country_Code",
                table: "church",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country_CultureCode",
                table: "church",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country_Name",
                table: "church",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country_PhoneCountryCode",
                table: "church",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountyOrRegion",
                table: "church",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complement",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "Country_Code",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "Country_CultureCode",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "Country_Name",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "Country_PhoneCountryCode",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "CountyOrRegion",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "minister");

            migrationBuilder.DropColumn(
                name: "Country_Code",
                table: "members");

            migrationBuilder.DropColumn(
                name: "Country_CultureCode",
                table: "members");

            migrationBuilder.DropColumn(
                name: "Country_Name",
                table: "members");

            migrationBuilder.DropColumn(
                name: "Country_PhoneCountryCode",
                table: "members");

            migrationBuilder.DropColumn(
                name: "PhoneDisplayFormat",
                table: "members");

            migrationBuilder.DropColumn(
                name: "PhoneE164Format",
                table: "members");

            migrationBuilder.DropColumn(
                name: "PhoneInternationalFormat",
                table: "members");

            migrationBuilder.DropColumn(
                name: "Complement",
                table: "church");

            migrationBuilder.DropColumn(
                name: "Country_Code",
                table: "church");

            migrationBuilder.DropColumn(
                name: "Country_CultureCode",
                table: "church");

            migrationBuilder.DropColumn(
                name: "Country_Name",
                table: "church");

            migrationBuilder.DropColumn(
                name: "Country_PhoneCountryCode",
                table: "church");

            migrationBuilder.DropColumn(
                name: "CountyOrRegion",
                table: "church");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "members",
                newName: "CellPhone");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "church",
                newName: "ZipCode");

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "minister",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "minister",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "minister",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "minister",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "minister",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
