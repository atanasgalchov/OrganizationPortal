using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganizationPortal.Migrations
{
    public partial class AddCategoryInDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Documents",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CategoryId",
                table: "Documents",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Categories_CategoryId",
                table: "Documents",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Categories_CategoryId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CategoryId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Documents");
        }
    }
}
