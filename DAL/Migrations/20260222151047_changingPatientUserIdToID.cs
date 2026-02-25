using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class changingPatientUserIdToID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProfiles_AspNetUsers_UserId",
                table: "DoctorProfiles");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DoctorProfiles",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProfiles_AspNetUsers_Id",
                table: "DoctorProfiles",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProfiles_AspNetUsers_Id",
                table: "DoctorProfiles");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DoctorProfiles",
                newName: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProfiles_AspNetUsers_UserId",
                table: "DoctorProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
