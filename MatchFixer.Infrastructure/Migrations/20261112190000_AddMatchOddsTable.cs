using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchOddsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchOdds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Market = table.Column<int>(type: "int", nullable: false, comment: "Betting market this price belongs to"),
                    Selection = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Selection code within the market, e.g. Over, Yes, 1X"),
                    Line = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true, comment: "Goals line for Over/Under markets, null otherwise"),
                    Odds = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false, comment: "Decimal odds for this selection")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchOdds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchOdds_MatchEvents_MatchEventId",
                        column: x => x.MatchEventId,
                        principalTable: "MatchEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchOdds_MatchEventId_Market_Selection",
                table: "MatchOdds",
                columns: new[] { "MatchEventId", "Market", "Selection" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchOdds");
        }
    }
}
