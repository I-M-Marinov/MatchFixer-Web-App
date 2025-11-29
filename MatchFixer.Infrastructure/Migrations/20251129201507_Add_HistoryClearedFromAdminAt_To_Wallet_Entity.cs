using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_HistoryClearedFromAdminAt_To_Wallet_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HistoryClearedFromAdminAt",
                table: "Wallets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OddsBoosts_MatchEventId_IsActive_StartUtc_EndUtc",
                table: "OddsBoosts",
                columns: new[] { "MatchEventId", "IsActive", "StartUtc", "EndUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OddsBoosts_MatchEventId_IsActive_StartUtc_EndUtc",
                table: "OddsBoosts");

            migrationBuilder.DropColumn(
                name: "HistoryClearedFromAdminAt",
                table: "Wallets");
        }
    }
}
