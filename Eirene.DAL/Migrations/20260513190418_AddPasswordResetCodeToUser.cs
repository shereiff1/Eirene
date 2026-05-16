using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eirene.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetCodeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetCode",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeExpiration",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeExpiration",
                table: "AspNetUsers");
        }
    }
}
