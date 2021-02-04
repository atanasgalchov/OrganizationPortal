using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganizationPortal.Migrations
{
    public partial class AddAlbumModel2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Albums_AlbumId",
                table: "Photos");

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "Photos",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Albums_AlbumId",
                table: "Photos",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Albums_AlbumId",
                table: "Photos");

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "Photos",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Albums_AlbumId",
                table: "Photos",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
