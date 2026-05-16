using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eirene.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ApplyExplicitMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "ApplicationUserCommunityGroup",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeoutUntil",
                table: "ApplicationUserCommunityGroup",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBanned",
                table: "ApplicationUserCommunityGroup");

            migrationBuilder.DropColumn(
                name: "TimeoutUntil",
                table: "ApplicationUserCommunityGroup");
        }
    }
}
