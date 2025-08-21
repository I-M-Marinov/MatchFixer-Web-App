using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOddsBoostEntityToTheDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OddsBoostId",
                table: "Bets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OddsBoosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BoostValue = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxStakePerBet = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxUsesPerUser = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OddsBoosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OddsBoosts_MatchEvents_MatchEventId",
                        column: x => x.MatchEventId,
                        principalTable: "MatchEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bets_OddsBoostId",
                table: "Bets",
                column: "OddsBoostId");

            migrationBuilder.CreateIndex(
                name: "IX_OddsBoosts_MatchEventId_StartUtc_EndUtc_IsActive",
                table: "OddsBoosts",
                columns: new[] { "MatchEventId", "StartUtc", "EndUtc", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_OddsBoosts_OddsBoostId",
                table: "Bets",
                column: "OddsBoostId",
                principalTable: "OddsBoosts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_OddsBoosts_OddsBoostId",
                table: "Bets");

            migrationBuilder.DropTable(
                name: "OddsBoosts");

            migrationBuilder.DropIndex(
                name: "IX_Bets_OddsBoostId",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "OddsBoostId",
                table: "Bets");
        }
    }
}
