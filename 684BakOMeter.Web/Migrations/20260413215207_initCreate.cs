using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace _684BakOMeter.Web.Migrations
{
    /// <inheritdoc />
    public partial class initCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChugAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    ChugType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OneVsOneMatchIdAsPlayer1 = table.Column<int>(type: "integer", nullable: true),
                    OneVsOneMatchIdAsPlayer2 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChugAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChugAttempts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OneVsOneMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChugType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Player1Id = table.Column<int>(type: "integer", nullable: false),
                    Player1AttemptId = table.Column<int>(type: "integer", nullable: true),
                    Player2Id = table.Column<int>(type: "integer", nullable: false),
                    Player2AttemptId = table.Column<int>(type: "integer", nullable: true),
                    WinnerId = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneVsOneMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_ChugAttempts_Player1AttemptId",
                        column: x => x.Player1AttemptId,
                        principalTable: "ChugAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_ChugAttempts_Player2AttemptId",
                        column: x => x.Player2AttemptId,
                        principalTable: "ChugAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_Players_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_Players_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OneVsOneMatches_Players_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChugAttempts_PlayerId",
                table: "ChugAttempts",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_ChugType",
                table: "OneVsOneMatches",
                column: "ChugType");

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_CreatedAt",
                table: "OneVsOneMatches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_Player1AttemptId",
                table: "OneVsOneMatches",
                column: "Player1AttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_Player1Id_Player2Id",
                table: "OneVsOneMatches",
                columns: new[] { "Player1Id", "Player2Id" });

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_Player2AttemptId",
                table: "OneVsOneMatches",
                column: "Player2AttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_Player2Id",
                table: "OneVsOneMatches",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_OneVsOneMatches_WinnerId",
                table: "OneVsOneMatches",
                column: "WinnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OneVsOneMatches");

            migrationBuilder.DropTable(
                name: "ChugAttempts");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
