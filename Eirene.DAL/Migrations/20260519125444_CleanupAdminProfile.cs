using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eirene.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CleanupAdminProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AdminProfiles");
            migrationBuilder.DropColumn(
                name: "CanBanUsers",
                table: "AdminProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AdminProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");
            
            migrationBuilder.AddColumn<string>(
                name: "CanBanUsers",
                table: "AdminProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            
        }
    }
}
