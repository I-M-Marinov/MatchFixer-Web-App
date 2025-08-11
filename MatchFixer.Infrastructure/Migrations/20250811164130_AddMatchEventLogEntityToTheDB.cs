using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchEventLogEntityToTheDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchEventLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchEventLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchEventLogs_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchEventLogs_MatchEvents_MatchEventId",
                        column: x => x.MatchEventId,
                        principalTable: "MatchEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchEventLogs_ChangedByUserId",
                table: "MatchEventLogs",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEventLogs_MatchEventId",
                table: "MatchEventLogs",
                column: "MatchEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchEventLogs");
        }
    }
}
