using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eirene.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVerifiedToDoctorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "DoctorProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "DoctorProfiles");
        }
    }
}
