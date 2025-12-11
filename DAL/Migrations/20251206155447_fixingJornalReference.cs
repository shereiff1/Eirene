using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class fixingJornalReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Journals_PatientProfiles_PatientId",
                table: "Journals");

            migrationBuilder.AddColumn<string>(
                name: "PatientProfileId",
                table: "Journals",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Journals_PatientProfileId",
                table: "Journals",
                column: "PatientProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Journals_AspNetUsers_PatientId",
                table: "Journals",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Journals_PatientProfiles_PatientProfileId",
                table: "Journals",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Journals_AspNetUsers_PatientId",
                table: "Journals");

            migrationBuilder.DropForeignKey(
                name: "FK_Journals_PatientProfiles_PatientProfileId",
                table: "Journals");

            migrationBuilder.DropIndex(
                name: "IX_Journals_PatientProfileId",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "PatientProfileId",
                table: "Journals");

            migrationBuilder.AddForeignKey(
                name: "FK_Journals_PatientProfiles_PatientId",
                table: "Journals",
                column: "PatientId",
                principalTable: "PatientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
