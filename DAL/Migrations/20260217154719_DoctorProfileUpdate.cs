using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class DoctorProfileUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                table: "DoctorProfiles");

            migrationBuilder.RenameColumn(
                name: "AvailableTo",
                table: "DoctorProfiles",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Biography",
                table: "DoctorProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedAt",
                table: "DoctorProfiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoUrl",
                table: "DoctorProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "DoctorProfiles",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "DoctorProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "DoctorProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Biography",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "JoinedAt",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoUrl",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "DoctorProfiles");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "DoctorProfiles",
                newName: "AvailableTo");

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableFrom",
                table: "DoctorProfiles",
                type: "datetime2",
                nullable: true);
        }
    }
}
