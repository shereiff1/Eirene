using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eirene.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CascadeOnDeleteDoctorSupervisionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupervisionRequests_DoctorProfiles_DoctorProfileId",
                table: "SupervisionRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_SupervisionRequests_DoctorProfiles_DoctorProfileId",
                table: "SupervisionRequests",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupervisionRequests_DoctorProfiles_DoctorProfileId",
                table: "SupervisionRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_SupervisionRequests_DoctorProfiles_DoctorProfileId",
                table: "SupervisionRequests",
                column: "DoctorProfileId",
                principalTable: "DoctorProfiles",
                principalColumn: "Id");
        }
    }
}
