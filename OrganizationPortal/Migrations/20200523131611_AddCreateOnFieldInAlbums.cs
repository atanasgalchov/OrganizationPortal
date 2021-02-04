using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganizationPortal.Migrations
{
    public partial class AddCreateOnFieldInAlbums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateOn",
                table: "Albums",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateOn",
                table: "Albums");
        }
    }
}
