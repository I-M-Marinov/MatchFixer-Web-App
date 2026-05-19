using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Updated_The_New_TeamWikiInfo_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "TeamWikiInfos");

            migrationBuilder.DropColumn(
                name: "TeamName",
                table: "TeamWikiInfos");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "TeamWikiInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TeamWikiInfos_TeamId",
                table: "TeamWikiInfos",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamWikiInfos_Teams_TeamId",
                table: "TeamWikiInfos",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamWikiInfos_Teams_TeamId",
                table: "TeamWikiInfos");

            migrationBuilder.DropIndex(
                name: "IX_TeamWikiInfos_TeamId",
                table: "TeamWikiInfos");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "TeamWikiInfos");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "TeamWikiInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TeamName",
                table: "TeamWikiInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
