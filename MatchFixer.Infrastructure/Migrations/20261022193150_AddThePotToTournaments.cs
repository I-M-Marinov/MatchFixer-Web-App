using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddThePotToTournaments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationWeeks",
                table: "Tournaments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPot",
                table: "Tournaments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PotFirstPct",
                table: "Tournaments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PotPrizePool",
                table: "Tournaments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PotSecondPct",
                table: "Tournaments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PotThirdPct",
                table: "Tournaments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationWeeks",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "IsPot",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "PotFirstPct",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "PotPrizePool",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "PotSecondPct",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "PotThirdPct",
                table: "Tournaments");
        }
    }
}
