using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sastri_Library_Backend.Migrations
{
    public partial class loans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Loans",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionMessage",
                table: "Loans",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "RejectionMessage",
                table: "Loans");
        }
    }
}
