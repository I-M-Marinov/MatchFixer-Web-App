using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdjustmentsTo_The_Newly_Added_WorldCupMatch_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorldCupMatches_MatchEvents_MatchEventId",
                table: "WorldCupMatches");

            migrationBuilder.DropIndex(
                name: "IX_WorldCupMatches_MatchEventId",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "MatchEventId",
                table: "WorldCupMatches");

            migrationBuilder.AddColumn<int>(
                name: "ApiEventId",
                table: "WorldCupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AwayLogo",
                table: "WorldCupMatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AwayScore",
                table: "WorldCupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AwayTeam",
                table: "WorldCupMatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HomeLogo",
                table: "WorldCupMatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HomeScore",
                table: "WorldCupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeTeam",
                table: "WorldCupMatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "WorldCupMatches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLive",
                table: "WorldCupMatches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MatchDate",
                table: "WorldCupMatches",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiEventId",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "AwayLogo",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "AwayScore",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "AwayTeam",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "HomeLogo",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "HomeScore",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "HomeTeam",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "IsLive",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "MatchDate",
                table: "WorldCupMatches");

            migrationBuilder.AddColumn<Guid>(
                name: "MatchEventId",
                table: "WorldCupMatches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_WorldCupMatches_MatchEventId",
                table: "WorldCupMatches",
                column: "MatchEventId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorldCupMatches_MatchEvents_MatchEventId",
                table: "WorldCupMatches",
                column: "MatchEventId",
                principalTable: "MatchEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
