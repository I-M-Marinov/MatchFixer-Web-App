using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamEntityToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayTeam",
                table: "MatchResults");

            migrationBuilder.DropColumn(
                name: "AwayTeamLogo",
                table: "MatchResults");

            migrationBuilder.DropColumn(
                name: "HomeTeam",
                table: "MatchResults");

            migrationBuilder.DropColumn(
                name: "HomeTeamLogo",
                table: "MatchResults");

            migrationBuilder.DropColumn(
                name: "AwayTeam",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "HomeTeam",
                table: "MatchEvents");

            migrationBuilder.AddColumn<Guid>(
                name: "AwayTeamId",
                table: "MatchResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "HomeTeamId",
                table: "MatchResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AwayTeamId",
                table: "MatchEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "HomeTeamId",
                table: "MatchEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_AwayTeamId",
                table: "MatchResults",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_HomeTeamId",
                table: "MatchResults",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_AwayTeamId",
                table: "MatchEvents",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_HomeTeamId",
                table: "MatchEvents",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name",
                table: "Teams",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchEvents_Teams_AwayTeamId",
                table: "MatchEvents",
                column: "AwayTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchEvents_Teams_HomeTeamId",
                table: "MatchEvents",
                column: "HomeTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResults_Teams_AwayTeamId",
                table: "MatchResults",
                column: "AwayTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResults_Teams_HomeTeamId",
                table: "MatchResults",
                column: "HomeTeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchEvents_Teams_AwayTeamId",
                table: "MatchEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchEvents_Teams_HomeTeamId",
                table: "MatchEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchResults_Teams_AwayTeamId",
                table: "MatchResults");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchResults_Teams_HomeTeamId",
                table: "MatchResults");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_MatchResults_AwayTeamId",
                table: "MatchResults");

            migrationBuilder.DropIndex(
                name: "IX_MatchResults_HomeTeamId",
                table: "MatchResults");

            migrationBuilder.DropIndex(
                name: "IX_MatchEvents_AwayTeamId",
                table: "MatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_MatchEvents_HomeTeamId",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "AwayTeamId",
                table: "MatchResults");

            migrationBuilder.DropColumn(
                name: "HomeTeamId",
                table: "MatchResults");

            migrationBuilder.DropColumn(
                name: "AwayTeamId",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "HomeTeamId",
                table: "MatchEvents");

            migrationBuilder.AddColumn<string>(
                name: "AwayTeam",
                table: "MatchResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AwayTeamLogo",
                table: "MatchResults",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HomeTeam",
                table: "MatchResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HomeTeamLogo",
                table: "MatchResults",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AwayTeam",
                table: "MatchEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HomeTeam",
                table: "MatchEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
