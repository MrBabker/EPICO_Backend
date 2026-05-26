using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace epico_backend.Migrations
{
    /// <inheritdoc />
    public partial class addedLevelInTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "players");
        }
    }
}
