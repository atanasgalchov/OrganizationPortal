using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganizationPortal.Migrations
{
    public partial class MakeLatAndLagFieldString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Lat",
                table: "Locations",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Lang",
                table: "Locations",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Lat",
                table: "Locations",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Lang",
                table: "Locations",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
