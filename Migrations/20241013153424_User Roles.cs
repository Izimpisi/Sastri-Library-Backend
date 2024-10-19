using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sastri_Library_Backend.Migrations
{
    public partial class UserRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StudentIdNumber",
                table: "AspNetUsers",
                newName: "UserIdNumber");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserIdNumber",
                table: "AspNetUsers",
                newName: "StudentIdNumber");
        }
    }
}
