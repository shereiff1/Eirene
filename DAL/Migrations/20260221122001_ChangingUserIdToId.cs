using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangingUserIdToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diagnosis_PatientProfiles_PatientProfileUserId",
                table: "Diagnosis");

            migrationBuilder.DropForeignKey(
                name: "FK_Journals_PatientProfiles_PatientProfileUserId",
                table: "Journals");

            migrationBuilder.DropForeignKey(
                name: "FK_MoodTracker_PatientProfiles_PatientProfileUserId",
                table: "MoodTracker");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_AspNetUsers_UserId",
                table: "PatientProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileUserId",
                table: "TreatmentPlan");

            migrationBuilder.RenameColumn(
                name: "PatientProfileUserId",
                table: "TreatmentPlan",
                newName: "PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlan_PatientProfileUserId",
                table: "TreatmentPlan",
                newName: "IX_TreatmentPlan_PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PatientProfiles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PatientProfileUserId",
                table: "MoodTracker",
                newName: "PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_MoodTracker_PatientProfileUserId",
                table: "MoodTracker",
                newName: "IX_MoodTracker_PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "PatientProfileUserId",
                table: "Journals",
                newName: "PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Journals_PatientProfileUserId",
                table: "Journals",
                newName: "IX_Journals_PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "PatientProfileUserId",
                table: "Diagnosis",
                newName: "PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Diagnosis_PatientProfileUserId",
                table: "Diagnosis",
                newName: "IX_Diagnosis_PatientProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnosis_PatientProfiles_PatientProfileId",
                table: "Diagnosis",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Journals_PatientProfiles_PatientProfileId",
                table: "Journals",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MoodTracker_PatientProfiles_PatientProfileId",
                table: "MoodTracker",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_AspNetUsers_Id",
                table: "PatientProfiles",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileId",
                table: "TreatmentPlan",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diagnosis_PatientProfiles_PatientProfileId",
                table: "Diagnosis");

            migrationBuilder.DropForeignKey(
                name: "FK_Journals_PatientProfiles_PatientProfileId",
                table: "Journals");

            migrationBuilder.DropForeignKey(
                name: "FK_MoodTracker_PatientProfiles_PatientProfileId",
                table: "MoodTracker");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_AspNetUsers_Id",
                table: "PatientProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileId",
                table: "TreatmentPlan");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                table: "TreatmentPlan",
                newName: "PatientProfileUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlan_PatientProfileId",
                table: "TreatmentPlan",
                newName: "IX_TreatmentPlan_PatientProfileUserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PatientProfiles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                table: "MoodTracker",
                newName: "PatientProfileUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MoodTracker_PatientProfileId",
                table: "MoodTracker",
                newName: "IX_MoodTracker_PatientProfileUserId");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                table: "Journals",
                newName: "PatientProfileUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Journals_PatientProfileId",
                table: "Journals",
                newName: "IX_Journals_PatientProfileUserId");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                table: "Diagnosis",
                newName: "PatientProfileUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Diagnosis_PatientProfileId",
                table: "Diagnosis",
                newName: "IX_Diagnosis_PatientProfileUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnosis_PatientProfiles_PatientProfileUserId",
                table: "Diagnosis",
                column: "PatientProfileUserId",
                principalTable: "PatientProfiles",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Journals_PatientProfiles_PatientProfileUserId",
                table: "Journals",
                column: "PatientProfileUserId",
                principalTable: "PatientProfiles",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MoodTracker_PatientProfiles_PatientProfileUserId",
                table: "MoodTracker",
                column: "PatientProfileUserId",
                principalTable: "PatientProfiles",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_AspNetUsers_UserId",
                table: "PatientProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileUserId",
                table: "TreatmentPlan",
                column: "PatientProfileUserId",
                principalTable: "PatientProfiles",
                principalColumn: "UserId");
        }
    }
}
