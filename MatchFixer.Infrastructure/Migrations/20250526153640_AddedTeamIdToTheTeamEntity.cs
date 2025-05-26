using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTeamIdToTheTeamEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Teams",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Teams");
        }
    }
}
