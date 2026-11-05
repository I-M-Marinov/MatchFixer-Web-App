using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchFixer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBettingAccessControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AgeVerifiedOnUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                comment: "UTC date the user's age was verified");

            migrationBuilder.AddColumn<bool>(
                name: "IsAgeVerified",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Indicates if the user has verified their date of birth (18+) and may place bets");

            migrationBuilder.AddColumn<bool>(
                name: "IsBettingBlocked",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Indicates if an admin has blocked this user from placing bets");

            // Existing adults with a real DOB on file count as verified
            migrationBuilder.Sql(@"
UPDATE [AspNetUsers]
SET [IsAgeVerified] = 1,
    [AgeVerifiedOnUtc] = SYSUTCDATETIME()
WHERE [IsDeleted] = 0
  AND [DateOfBirth] > '1901-01-01'
  AND [DateOfBirth] <= DATEADD(year, -18, CAST(SYSUTCDATETIME() AS date));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeVerifiedOnUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsAgeVerified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsBettingBlocked",
                table: "AspNetUsers");
        }
    }
}
