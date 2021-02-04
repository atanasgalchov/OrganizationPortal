using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganizationPortal.Migrations
{
    public partial class MakeLatAndLagFieldNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<decimal>(
                name: "Lat",
                table: "Locations",
                nullable: true,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<decimal>(
                name: "Lang",
                table: "Locations",
                nullable: true,
                oldClrType: typeof(decimal));

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Albums_AlbumId",
                table: "Photos",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Albums_AlbumId",
                table: "Photos");

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "Photos",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<decimal>(
                name: "Lat",
                table: "Locations",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Lang",
                table: "Locations",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Albums_AlbumId",
                table: "Photos",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
