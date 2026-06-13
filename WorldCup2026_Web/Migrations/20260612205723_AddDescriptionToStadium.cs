using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldCup2026_Web.Migrations
{
    public partial class AddDescriptionToStadium : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Stadiums",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Stadiums");
        }
    }
}
