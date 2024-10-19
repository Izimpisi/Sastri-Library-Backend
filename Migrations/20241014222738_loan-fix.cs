using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sastri_Library_Backend.Migrations
{
    public partial class loanfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Books",
                newName: "BookId");

            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_BookId",
                table: "Loans",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Books_BookId",
                table: "Loans",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Books_BookId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_BookId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "Books",
                newName: "Id");
        }
    }
}
