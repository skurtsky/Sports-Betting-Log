using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SportsBettingTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SportLeagues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportLeagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SportLeagueId = table.Column<int>(type: "int", nullable: false),
                    Match = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BetSelection = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Stake = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Odds = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    AmountWonLost = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bets_SportLeagues_SportLeagueId",
                        column: x => x.SportLeagueId,
                        principalTable: "SportLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SportLeagues",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "NFL" },
                    { 2, "NBA" },
                    { 3, "MLB" },
                    { 4, "NHL" },
                    { 5, "UFC" },
                    { 6, "Soccer - Premier League" },
                    { 7, "Soccer - MLS" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bets_SportLeagueId",
                table: "Bets",
                column: "SportLeagueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bets");

            migrationBuilder.DropTable(
                name: "SportLeagues");
        }
    }
}
