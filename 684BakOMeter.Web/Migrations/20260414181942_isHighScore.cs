using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _684BakOMeter.Web.Migrations
{
    /// <inheritdoc />
    public partial class isHighScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHighScore",
                table: "ChugAttempts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHighScore",
                table: "ChugAttempts");
        }
    }
}
