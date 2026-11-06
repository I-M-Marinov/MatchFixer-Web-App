using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketToBet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Market",
                table: "Bets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Betting market of this bet (0 = Match Result)");

            migrationBuilder.AddColumn<string>(
                name: "Selection",
                table: "Bets",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                comment: "Selection code for non 1X2 markets (null for Match Result, which uses Pick)");

            migrationBuilder.AddColumn<decimal>(
                name: "Line",
                table: "Bets",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                comment: "Goals line for Over/Under bets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Market",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "Selection",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "Line",
                table: "Bets");
        }
    }
}
