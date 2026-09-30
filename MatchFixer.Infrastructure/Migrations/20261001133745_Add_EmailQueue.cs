using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_EmailQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueuedEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Unique identifier for a queued email"),
                    Recipient = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, comment: "Recipient email address"),
                    RecipientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Optional id of the recipient user"),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false, comment: "Email subject"),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Email body (HTML) - stored as nvarchar(max)"),
                    Category = table.Column<int>(type: "int", nullable: false, comment: "Which kind of email this is (Winning, Event, Voided, Birthday, Other)"),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "Review / send status"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "When the email was enqueued (UTC)"),
                    SendAfterUtc = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Earliest time the dispatcher may send this (UTC) - used for interval sending"),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "When the email was actually sent (UTC)"),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true, comment: "Admin who approved / dropped the email"),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "When an admin approved / dropped the email (UTC)"),
                    AttemptCount = table.Column<int>(type: "int", nullable: false, comment: "Number of send attempts"),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Last send error, if any"),
                    RelatedInfo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true, comment: "Human-friendly context, e.g. teams or slip id")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueuedEmails_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEmails_RecipientUserId",
                table: "QueuedEmails",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEmails_Status_Category",
                table: "QueuedEmails",
                columns: new[] { "Status", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_QueuedEmails_Status_SendAfterUtc",
                table: "QueuedEmails",
                columns: new[] { "Status", "SendAfterUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueuedEmails");
        }
    }
}
