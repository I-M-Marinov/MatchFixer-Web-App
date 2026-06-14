using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_The_Newly_Added_WorldCupMatch_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayTeamScore",
                table: "WorldCupMatches");

            migrationBuilder.DropColumn(
                name: "HomeTeamScore",
                table: "WorldCupMatches");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "WorldCupMatches",
                newName: "RoundPosition");

            migrationBuilder.AlterColumn<int>(
                name: "Stage",
                table: "WorldCupMatches",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoundPosition",
                table: "WorldCupMatches",
                newName: "SortOrder");

            migrationBuilder.AlterColumn<string>(
                name: "Stage",
                table: "WorldCupMatches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AwayTeamScore",
                table: "WorldCupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeTeamScore",
                table: "WorldCupMatches",
                type: "int",
                nullable: true);
        }
    }
}
