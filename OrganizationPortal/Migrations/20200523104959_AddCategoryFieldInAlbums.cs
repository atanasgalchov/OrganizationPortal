using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganizationPortal.Migrations
{
    public partial class AddCategoryFieldInAlbums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Albums",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_CategoryId",
                table: "Albums",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_Categories_CategoryId",
                table: "Albums",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_Categories_CategoryId",
                table: "Albums");

            migrationBuilder.DropIndex(
                name: "IX_Albums_CategoryId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Albums");
        }
    }
}
