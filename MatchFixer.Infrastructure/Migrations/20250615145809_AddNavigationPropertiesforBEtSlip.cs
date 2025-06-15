using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationPropertiesforBEtSlip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_AspNetUsers_ApplicationUserId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_Bets_BetSlip_BetSlipId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_BetSlip_AspNetUsers_UserId",
                table: "BetSlip");

            migrationBuilder.DropIndex(
                name: "IX_Bets_ApplicationUserId",
                table: "Bets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BetSlip",
                table: "BetSlip");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Bets");

            migrationBuilder.RenameTable(
                name: "BetSlip",
                newName: "BetSlips");

            migrationBuilder.RenameIndex(
                name: "IX_BetSlip_UserId",
                table: "BetSlips",
                newName: "IX_BetSlips_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BetSlips",
                table: "BetSlips",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_BetSlips_BetSlipId",
                table: "Bets",
                column: "BetSlipId",
                principalTable: "BetSlips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BetSlips_AspNetUsers_UserId",
                table: "BetSlips",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_BetSlips_BetSlipId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_BetSlips_AspNetUsers_UserId",
                table: "BetSlips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BetSlips",
                table: "BetSlips");

            migrationBuilder.RenameTable(
                name: "BetSlips",
                newName: "BetSlip");

            migrationBuilder.RenameIndex(
                name: "IX_BetSlips_UserId",
                table: "BetSlip",
                newName: "IX_BetSlip_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "Bets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BetSlip",
                table: "BetSlip",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Bets_ApplicationUserId",
                table: "Bets",
                column: "ApplicationUserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_BetSlip_AspNetUsers_UserId",
                table: "BetSlip",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
