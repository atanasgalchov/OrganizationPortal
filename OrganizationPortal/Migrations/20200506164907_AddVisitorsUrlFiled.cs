using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganizationPortal.Migrations
{
    public partial class AddVisitorsUrlFiled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "VisitorLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "VisitorLogs");
        }
    }
}
