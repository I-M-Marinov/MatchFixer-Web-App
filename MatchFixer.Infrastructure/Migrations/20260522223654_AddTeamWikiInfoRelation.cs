using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamWikiInfoRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamWikiInfos_TeamId",
                table: "TeamWikiInfos");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWikiInfos_TeamId",
                table: "TeamWikiInfos",
                column: "TeamId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamWikiInfos_TeamId",
                table: "TeamWikiInfos");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWikiInfos_TeamId",
                table: "TeamWikiInfos",
                column: "TeamId");
        }
    }
}
