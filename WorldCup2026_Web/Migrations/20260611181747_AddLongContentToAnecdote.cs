using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldCup2026_Web.Migrations
{
    public partial class AddLongContentToAnecdote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LongContent",
                table: "Anecdotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongContent",
                table: "Anecdotes");
        }
    }
}
