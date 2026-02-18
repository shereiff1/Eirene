using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingProfileForDoctors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_DoctorProfiles_DoctorProfileId",
                table: "PatientProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorProfiles",
                table: "DoctorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_DoctorProfiles_UserId",
                table: "DoctorProfiles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DoctorProfiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorProfiles",
                table: "DoctorProfiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_DoctorProfiles_DoctorProfileId",
                table: "PatientProfiles",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_DoctorProfiles_DoctorProfileId",
                table: "PatientProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorProfiles",
                table: "DoctorProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "DoctorProfiles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorProfiles",
                table: "DoctorProfiles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProfiles_UserId",
                table: "DoctorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_DoctorProfiles_DoctorProfileId",
                table: "PatientProfiles",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id");
        }
    }
}
