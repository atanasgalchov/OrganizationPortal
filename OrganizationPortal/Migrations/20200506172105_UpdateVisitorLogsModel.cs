using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganizationPortal.Migrations
{
    public partial class UpdateVisitorLogsModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientAgent",
                table: "VisitorLogs",
                newName: "Device");

            migrationBuilder.AddColumn<string>(
                name: "Browser",
                table: "VisitorLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Browser",
                table: "VisitorLogs");

            migrationBuilder.RenameColumn(
                name: "Device",
                table: "VisitorLogs",
                newName: "ClientAgent");
        }
    }
}
