using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _684BakOMeter.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaderboardPeriodIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChugAttempts_PlayerId",
                table: "ChugAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_ChugAttempts_ChugType_Mode_StartedAt_DurationMs_PlayerId",
                table: "ChugAttempts",
                columns: new[] { "ChugType", "Mode", "StartedAt", "DurationMs", "PlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChugAttempts_ChugType_StartedAt_DurationMs_PlayerId",
                table: "ChugAttempts",
                columns: new[] { "ChugType", "StartedAt", "DurationMs", "PlayerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChugAttempts_PlayerId_ChugType_Mode_StartedAt",
                table: "ChugAttempts",
                columns: new[] { "PlayerId", "ChugType", "Mode", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChugAttempts_ChugType_Mode_StartedAt_DurationMs_PlayerId",
                table: "ChugAttempts");

            migrationBuilder.DropIndex(
                name: "IX_ChugAttempts_ChugType_StartedAt_DurationMs_PlayerId",
                table: "ChugAttempts");

            migrationBuilder.DropIndex(
                name: "IX_ChugAttempts_PlayerId_ChugType_Mode_StartedAt",
                table: "ChugAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_ChugAttempts_PlayerId",
                table: "ChugAttempts",
                column: "PlayerId");
        }
    }
}
