using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBettingLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing matches keep offering the extra markets
            migrationBuilder.AddColumn<bool>(
                name: "ExtraMarketsEnabled",
                table: "MatchEvents",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "BettingLimitSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    OverUnderEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "Over/Under goals market offered on the site"),
                    BothTeamsToScoreEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "Both Teams To Score market offered on the site"),
                    DoubleChanceEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "Double Chance market offered on the site"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BettingLimitSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeagueBetLimits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeagueName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "League or competition name, or __default__"),
                    MaxStake1 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaxStake2 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaxStake3 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaxStake4 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaxStake5Plus = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ExtraMaxStake1 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ExtraMaxStake2 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ExtraMaxStake3 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ExtraMaxStake4 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ExtraMaxStake5Plus = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaxWin = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true, comment: "Maximum potential return (stake x odds) for a slip"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueBetLimits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueBetLimits_LeagueName",
                table: "LeagueBetLimits",
                column: "LeagueName",
                unique: true);

            // All markets on, and a Default limits row with no limits set:
            // nothing changes for players until an admin fills in the Limits page.
            migrationBuilder.InsertData(
                table: "BettingLimitSettings",
                columns: new[] { "Id", "OverUnderEnabled", "BothTeamsToScoreEnabled", "DoubleChanceEnabled" },
                values: new object[] { 1, true, true, true });

            migrationBuilder.InsertData(
                table: "LeagueBetLimits",
                columns: new[] { "Id", "LeagueName" },
                values: new object[] { new Guid("6f1d2c8e-4b7a-4e0a-9a51-3c2b7d9e0f11"), "__default__" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BettingLimitSettings");

            migrationBuilder.DropTable(
                name: "LeagueBetLimits");

            migrationBuilder.DropColumn(
                name: "ExtraMarketsEnabled",
                table: "MatchEvents");
        }
    }
}
