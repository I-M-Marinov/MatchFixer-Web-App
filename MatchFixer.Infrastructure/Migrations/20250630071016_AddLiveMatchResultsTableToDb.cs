using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveMatchResultsTableToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LiveMatchResult_MatchEvents_MatchEventId",
                table: "LiveMatchResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LiveMatchResult",
                table: "LiveMatchResult");

            migrationBuilder.RenameTable(
                name: "LiveMatchResult",
                newName: "LiveMatchResults");

            migrationBuilder.AlterColumn<int>(
                name: "LogoQuizScore",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                comment: "Indicates the number of points the user has from the Logo Quiz Game",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Indicates the number of points the user has in the MatchFix Game");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LiveMatchResults",
                table: "LiveMatchResults",
                column: "MatchEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_LiveMatchResults_MatchEvents_MatchEventId",
                table: "LiveMatchResults",
                column: "MatchEventId",
                principalTable: "MatchEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LiveMatchResults_MatchEvents_MatchEventId",
                table: "LiveMatchResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LiveMatchResults",
                table: "LiveMatchResults");

            migrationBuilder.RenameTable(
                name: "LiveMatchResults",
                newName: "LiveMatchResult");

            migrationBuilder.AlterColumn<int>(
                name: "LogoQuizScore",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                comment: "Indicates the number of points the user has in the MatchFix Game",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Indicates the number of points the user has from the Logo Quiz Game");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LiveMatchResult",
                table: "LiveMatchResult",
                column: "MatchEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_LiveMatchResult_MatchEvents_MatchEventId",
                table: "LiveMatchResult",
                column: "MatchEventId",
                principalTable: "MatchEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
