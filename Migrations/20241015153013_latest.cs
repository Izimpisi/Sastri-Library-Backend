using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sastri_Library_Backend.Migrations
{
    public partial class latest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_AspNetUsers_StudentId",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Loans",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_StudentId",
                table: "Loans",
                newName: "IX_Loans_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_AspNetUsers_UserId",
                table: "Loans",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_AspNetUsers_UserId",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Loans",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_UserId",
                table: "Loans",
                newName: "IX_Loans_StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_AspNetUsers_StudentId",
                table: "Loans",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
