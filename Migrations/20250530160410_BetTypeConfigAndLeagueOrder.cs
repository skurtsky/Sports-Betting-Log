using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SportsBettingTracker.Migrations
{
    /// <inheritdoc />
    public partial class BetTypeConfigAndLeagueOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SportLeagues",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SportLeagues",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SportLeagues",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SportLeagues",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SportLeagues",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "SportLeagueId",
                table: "Bets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Result",
                table: "Bets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Odds",
                table: "Bets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Match",
                table: "Bets",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "BetSelection",
                table: "Bets",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BetDate",
                table: "Bets",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Bets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "BetType",
                table: "Bets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BetTypeConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BetType = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetTypeConfigurations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BetTypeConfigurations",
                columns: new[] { "Id", "BetType", "Description", "DisplayName", "DisplayOrder", "IsActive" },
                values: new object[,]
                {
                    { 1, 0, "Bet on which team wins the game outright", "Moneyline", 1, true },
                    { 2, 1, "Bet on the margin of victory", "Spread", 2, true },
                    { 3, 2, "Bet on the total combined score", "Over/Under", 3, true },
                    { 4, 3, "Bet on specific events within the game", "Prop Bet", 4, true },
                    { 5, 4, "Multiple bets combined into one wager", "Parlay", 5, true },
                    { 6, 5, "Bet on long-term outcomes like championship winners", "Future", 6, true },
                    { 7, 6, "Other bet types", "Other", 7, true }
                });

            migrationBuilder.UpdateData(
                table: "SportLeagues",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "DisplayOrder", "IsActive" },
                values: new object[] { "National Football League", 1, true });

            migrationBuilder.UpdateData(
                table: "SportLeagues",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "DisplayOrder", "IsActive" },
                values: new object[] { "National Basketball Association", 2, true });

            migrationBuilder.UpdateData(
                table: "SportLeagues",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DisplayOrder", "IsActive" },
                values: new object[] { "Major League Baseball", 3, true });

            migrationBuilder.UpdateData(
                table: "SportLeagues",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "DisplayOrder", "IsActive" },
                values: new object[] { "National Hockey League", 4, true });

            migrationBuilder.UpdateData(
                table: "SportLeagues",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "DisplayOrder", "IsActive" },
                values: new object[] { "Ultimate Fighting Championship", 5, true });

            migrationBuilder.UpdateData(
                table: "SportLeagues",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "DisplayOrder", "IsActive" },
                values: new object[] { "English Premier League", 6, true });

            migrationBuilder.UpdateData(
                table: "SportLeagues",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "DisplayOrder", "IsActive" },
                values: new object[] { "Major League Soccer", 7, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BetTypeConfigurations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SportLeagues");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SportLeagues");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SportLeagues");

            migrationBuilder.DropColumn(
                name: "BetType",
                table: "Bets");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SportLeagues",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SportLeagues",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<int>(
                name: "SportLeagueId",
                table: "Bets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "Result",
                table: "Bets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "Odds",
                table: "Bets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Match",
                table: "Bets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "BetSelection",
                table: "Bets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BetDate",
                table: "Bets",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Bets",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);
        }
    }
}
