using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBetSlipEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_AspNetUsers_UserId",
                table: "Bets");

            migrationBuilder.DropIndex(
                name: "IX_Bets_UserId",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "IsSettled",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "WinAmount",
                table: "Bets");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "Bets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BetSlipId",
                table: "Bets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Odds",
                table: "Bets",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                comment: "Odds for this particular pick");

            migrationBuilder.CreateTable(
                name: "BetSlip",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Unique identifier for a BetSlip"),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, comment: "Amount of the Bet"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Id of the user who placed the bet slip"),
                    BetTime = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Time the bet slip was placed"),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the bet slip has been settled"),
                    WinAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, comment: "The total win amount for the bet slip if successful")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetSlip", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BetSlip_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bets_ApplicationUserId",
                table: "Bets",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bets_BetSlipId",
                table: "Bets",
                column: "BetSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_BetSlip_UserId",
                table: "BetSlip",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_AspNetUsers_ApplicationUserId",
                table: "Bets",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_BetSlip_BetSlipId",
                table: "Bets",
                column: "BetSlipId",
                principalTable: "BetSlip",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_AspNetUsers_ApplicationUserId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_Bets_BetSlip_BetSlipId",
                table: "Bets");

            migrationBuilder.DropTable(
                name: "BetSlip");

            migrationBuilder.DropIndex(
                name: "IX_Bets_ApplicationUserId",
                table: "Bets");

            migrationBuilder.DropIndex(
                name: "IX_Bets_BetSlipId",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "BetSlipId",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "Odds",
                table: "Bets");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Bets",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                comment: "Amount of the Bet");

            migrationBuilder.AddColumn<bool>(
                name: "IsSettled",
                table: "Bets",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Signifies if the bet has been settled or not");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Bets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Id of the user that placed the bet");

            migrationBuilder.AddColumn<decimal>(
                name: "WinAmount",
                table: "Bets",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                comment: "Amount that would be won on the bet");

            migrationBuilder.CreateIndex(
                name: "IX_Bets_UserId",
                table: "Bets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_AspNetUsers_UserId",
                table: "Bets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
