using Microsoft.EntityFrameworkCore.Migrations;

namespace OrganizationPortal.Migrations
{
    public partial class addCityInVisitorsLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Categories_CategoryId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_News_Categories_CategoryId",
                table: "News");

            migrationBuilder.DropForeignKey(
                name: "FK_Notices_Categories_CategoryId",
                table: "Notices");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "VisitorLogs",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Notices",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "News",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Events",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Events",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Documents",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Documents",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Categories_CategoryId",
                table: "Documents",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_News_Categories_CategoryId",
                table: "News",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_Categories_CategoryId",
                table: "Notices",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Categories_CategoryId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_News_Categories_CategoryId",
                table: "News");

            migrationBuilder.DropForeignKey(
                name: "FK_Notices_Categories_CategoryId",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "City",
                table: "VisitorLogs");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Notices",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "News",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Events",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Events",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Documents",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Documents",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Categories_CategoryId",
                table: "Documents",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_News_Categories_CategoryId",
                table: "News",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_Categories_CategoryId",
                table: "Notices",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
