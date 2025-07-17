using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedStatusPropertyToTheBetEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Bets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Status of the bet");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Bets");
        }
    }
}
