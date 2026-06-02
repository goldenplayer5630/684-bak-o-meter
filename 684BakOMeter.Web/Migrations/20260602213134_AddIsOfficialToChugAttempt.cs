using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _684BakOMeter.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddIsOfficialToChugAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOfficial",
                table: "ChugAttempts",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOfficial",
                table: "ChugAttempts");
        }
    }
}
