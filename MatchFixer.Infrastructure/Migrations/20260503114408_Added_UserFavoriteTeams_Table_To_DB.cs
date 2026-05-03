using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_UserFavoriteTeams_Table_To_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteTeam_AspNetUsers_UserId",
                table: "UserFavoriteTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteTeam_Teams_TeamId",
                table: "UserFavoriteTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavoriteTeam",
                table: "UserFavoriteTeam");

            migrationBuilder.RenameTable(
                name: "UserFavoriteTeam",
                newName: "UserFavoriteTeams");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavoriteTeam_TeamId",
                table: "UserFavoriteTeams",
                newName: "IX_UserFavoriteTeams_TeamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteTeams",
                table: "UserFavoriteTeams",
                columns: new[] { "UserId", "TeamId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteTeams_AspNetUsers_UserId",
                table: "UserFavoriteTeams",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteTeams_Teams_TeamId",
                table: "UserFavoriteTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteTeams_AspNetUsers_UserId",
                table: "UserFavoriteTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteTeams_Teams_TeamId",
                table: "UserFavoriteTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavoriteTeams",
                table: "UserFavoriteTeams");

            migrationBuilder.RenameTable(
                name: "UserFavoriteTeams",
                newName: "UserFavoriteTeam");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavoriteTeams_TeamId",
                table: "UserFavoriteTeam",
                newName: "IX_UserFavoriteTeam_TeamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteTeam",
                table: "UserFavoriteTeam",
                columns: new[] { "UserId", "TeamId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteTeam_AspNetUsers_UserId",
                table: "UserFavoriteTeam",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteTeam_Teams_TeamId",
                table: "UserFavoriteTeam",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
