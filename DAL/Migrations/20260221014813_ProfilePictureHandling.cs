using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class ProfilePictureHandling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "FK_SupervisionRequests_PatientProfiles_PatientProfileId",
                table: "SupervisionRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileId",
                table: "TreatmentPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientProfiles",
                table: "PatientProfiles");

            migrationBuilder.DropIndex(
                name: "IX_PatientProfiles_UserId",
                table: "PatientProfiles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PatientProfiles");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                table: "TreatmentPlan",
                newName: "PatientProfileUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlan_PatientProfileId",
                table: "TreatmentPlan",
                newName: "IX_TreatmentPlan_PatientProfileUserId");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientProfiles",
                table: "PatientProfiles",
                column: "UserId");

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
                name: "FK_SupervisionRequests_PatientProfiles_PatientProfileId",
                table: "SupervisionRequests",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileUserId",
                table: "TreatmentPlan",
                column: "PatientProfileUserId",
                principalTable: "PatientProfiles",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FK_SupervisionRequests_PatientProfiles_PatientProfileId",
                table: "SupervisionRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileUserId",
                table: "TreatmentPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PatientProfiles",
                table: "PatientProfiles");

            migrationBuilder.RenameColumn(
                name: "PatientProfileUserId",
                table: "TreatmentPlan",
                newName: "PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlan_PatientProfileUserId",
                table: "TreatmentPlan",
                newName: "IX_TreatmentPlan_PatientProfileId");

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

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "PatientProfiles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PatientProfiles",
                table: "PatientProfiles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_UserId",
                table: "PatientProfiles",
                column: "UserId",
                unique: true);

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
                name: "FK_SupervisionRequests_PatientProfiles_PatientProfileId",
                table: "SupervisionRequests",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileId",
                table: "TreatmentPlan",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");
        }
    }
}
