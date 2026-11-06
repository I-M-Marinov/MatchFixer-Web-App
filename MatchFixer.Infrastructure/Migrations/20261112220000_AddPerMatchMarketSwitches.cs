using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerMatchMarketSwitches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing matches keep offering every market
            migrationBuilder.AddColumn<bool>(
                name: "OverUnderEnabled",
                table: "MatchEvents",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "BothTeamsToScoreEnabled",
                table: "MatchEvents",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoubleChanceEnabled",
                table: "MatchEvents",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverUnderEnabled",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "BothTeamsToScoreEnabled",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "DoubleChanceEnabled",
                table: "MatchEvents");
        }
    }
}
