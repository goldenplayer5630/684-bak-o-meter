using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _684BakOMeter.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddChugAttemptHighScoreIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChugAttempts_PlayerId",
                table: "ChugAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_ChugAttempts_PlayerId_ChugType_IsHighScore",
                table: "ChugAttempts",
                columns: new[] { "PlayerId", "ChugType", "IsHighScore" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChugAttempts_PlayerId_ChugType_IsHighScore",
                table: "ChugAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_ChugAttempts_PlayerId",
                table: "ChugAttempts",
                column: "PlayerId");
        }
    }
}
