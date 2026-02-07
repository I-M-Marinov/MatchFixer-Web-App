using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Updated_The_TeamAlias_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamAlias_Teams_TeamId",
                table: "TeamAlias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeamAlias",
                table: "TeamAlias");

            migrationBuilder.RenameTable(
                name: "TeamAlias",
                newName: "TeamAliases");

            migrationBuilder.RenameIndex(
                name: "IX_TeamAlias_TeamId",
                table: "TeamAliases",
                newName: "IX_TeamAliases_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_TeamAlias_Alias",
                table: "TeamAliases",
                newName: "IX_TeamAliases_Alias");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeamAliases",
                table: "TeamAliases",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamAliases_Teams_TeamId",
                table: "TeamAliases",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamAliases_Teams_TeamId",
                table: "TeamAliases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeamAliases",
                table: "TeamAliases");

            migrationBuilder.RenameTable(
                name: "TeamAliases",
                newName: "TeamAlias");

            migrationBuilder.RenameIndex(
                name: "IX_TeamAliases_TeamId",
                table: "TeamAlias",
                newName: "IX_TeamAlias_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_TeamAliases_Alias",
                table: "TeamAlias",
                newName: "IX_TeamAlias_Alias");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeamAlias",
                table: "TeamAlias",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamAlias_Teams_TeamId",
                table: "TeamAlias",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
