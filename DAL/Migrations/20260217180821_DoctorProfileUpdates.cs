using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class DoctorProfileUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorProfileId",
                table: "PatientProfiles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_DoctorProfileId",
                table: "PatientProfiles",
                column: "DoctorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_DoctorProfiles_DoctorProfileId",
                table: "PatientProfiles",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_DoctorProfiles_DoctorProfileId",
                table: "PatientProfiles");

            migrationBuilder.DropIndex(
                name: "IX_PatientProfiles_DoctorProfileId",
                table: "PatientProfiles");

            migrationBuilder.DropColumn(
                name: "DoctorProfileId",
                table: "PatientProfiles");
        }
    }
}
