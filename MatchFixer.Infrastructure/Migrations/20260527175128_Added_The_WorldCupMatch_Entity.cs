using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_The_WorldCupMatch_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorldCupMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeTeamScore = table.Column<int>(type: "int", nullable: true),
                    AwayTeamScore = table.Column<int>(type: "int", nullable: true),
                    IsKnockout = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldCupMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorldCupMatches_MatchEvents_MatchEventId",
                        column: x => x.MatchEventId,
                        principalTable: "MatchEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorldCupMatches_MatchEventId",
                table: "WorldCupMatches",
                column: "MatchEventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorldCupMatches");
        }
    }
}
