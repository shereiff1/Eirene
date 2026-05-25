using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eirene.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCleanedUpModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diagnosis_AspNetUsers_PatientId",
                table: "Diagnosis");

            migrationBuilder.DropForeignKey(
                name: "FK_Diagnosis_PatientProfiles_PatientProfileId",
                table: "Diagnosis");

            migrationBuilder.DropForeignKey(
                name: "FK_MoodTracker_AspNetUsers_UserId",
                table: "MoodTracker");

            migrationBuilder.DropForeignKey(
                name: "FK_MoodTracker_PatientProfiles_PatientProfileId",
                table: "MoodTracker");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientTasks_TreatmentPlan_TreatmentPlanId",
                table: "PatientTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlan_AspNetUsers_UserId",
                table: "TreatmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileId",
                table: "TreatmentPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TreatmentPlan",
                table: "TreatmentPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MoodTracker",
                table: "MoodTracker");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Diagnosis",
                table: "Diagnosis");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AdminProfiles");

            migrationBuilder.RenameTable(
                name: "TreatmentPlan",
                newName: "TreatmentPlans");

            migrationBuilder.RenameTable(
                name: "MoodTracker",
                newName: "MoodTrackers");

            migrationBuilder.RenameTable(
                name: "Diagnosis",
                newName: "Diagnoses");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlan_UserId",
                table: "TreatmentPlans",
                newName: "IX_TreatmentPlans_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlan_PatientProfileId",
                table: "TreatmentPlans",
                newName: "IX_TreatmentPlans_PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_MoodTracker_UserId",
                table: "MoodTrackers",
                newName: "IX_MoodTrackers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MoodTracker_PatientProfileId",
                table: "MoodTrackers",
                newName: "IX_MoodTrackers_PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Diagnosis_PatientProfileId",
                table: "Diagnoses",
                newName: "IX_Diagnoses_PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Diagnosis_PatientId",
                table: "Diagnoses",
                newName: "IX_Diagnoses_PatientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TreatmentPlans",
                table: "TreatmentPlans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MoodTrackers",
                table: "MoodTrackers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Diagnoses",
                table: "Diagnoses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnoses_AspNetUsers_PatientId",
                table: "Diagnoses",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnoses_PatientProfiles_PatientProfileId",
                table: "Diagnoses",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MoodTrackers_AspNetUsers_UserId",
                table: "MoodTrackers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MoodTrackers_PatientProfiles_PatientProfileId",
                table: "MoodTrackers",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientTasks_TreatmentPlans_TreatmentPlanId",
                table: "PatientTasks",
                column: "TreatmentPlanId",
                principalTable: "TreatmentPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentPlans_AspNetUsers_UserId",
                table: "TreatmentPlans",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentPlans_PatientProfiles_PatientProfileId",
                table: "TreatmentPlans",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diagnoses_AspNetUsers_PatientId",
                table: "Diagnoses");

            migrationBuilder.DropForeignKey(
                name: "FK_Diagnoses_PatientProfiles_PatientProfileId",
                table: "Diagnoses");

            migrationBuilder.DropForeignKey(
                name: "FK_MoodTrackers_AspNetUsers_UserId",
                table: "MoodTrackers");

            migrationBuilder.DropForeignKey(
                name: "FK_MoodTrackers_PatientProfiles_PatientProfileId",
                table: "MoodTrackers");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientTasks_TreatmentPlans_TreatmentPlanId",
                table: "PatientTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlans_AspNetUsers_UserId",
                table: "TreatmentPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlans_PatientProfiles_PatientProfileId",
                table: "TreatmentPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TreatmentPlans",
                table: "TreatmentPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MoodTrackers",
                table: "MoodTrackers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Diagnoses",
                table: "Diagnoses");

            migrationBuilder.RenameTable(
                name: "TreatmentPlans",
                newName: "TreatmentPlan");

            migrationBuilder.RenameTable(
                name: "MoodTrackers",
                newName: "MoodTracker");

            migrationBuilder.RenameTable(
                name: "Diagnoses",
                newName: "Diagnosis");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlans_UserId",
                table: "TreatmentPlan",
                newName: "IX_TreatmentPlan_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlans_PatientProfileId",
                table: "TreatmentPlan",
                newName: "IX_TreatmentPlan_PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_MoodTrackers_UserId",
                table: "MoodTracker",
                newName: "IX_MoodTracker_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MoodTrackers_PatientProfileId",
                table: "MoodTracker",
                newName: "IX_MoodTracker_PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Diagnoses_PatientProfileId",
                table: "Diagnosis",
                newName: "IX_Diagnosis_PatientProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Diagnoses_PatientId",
                table: "Diagnosis",
                newName: "IX_Diagnosis_PatientId");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AdminProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TreatmentPlan",
                table: "TreatmentPlan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MoodTracker",
                table: "MoodTracker",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Diagnosis",
                table: "Diagnosis",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnosis_AspNetUsers_PatientId",
                table: "Diagnosis",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnosis_PatientProfiles_PatientProfileId",
                table: "Diagnosis",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MoodTracker_AspNetUsers_UserId",
                table: "MoodTracker",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MoodTracker_PatientProfiles_PatientProfileId",
                table: "MoodTracker",
                column: "PatientProfileId",
                principalTable: "PatientProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientTasks_TreatmentPlan_TreatmentPlanId",
                table: "PatientTasks",
                column: "TreatmentPlanId",
                principalTable: "TreatmentPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentPlan_AspNetUsers_UserId",
                table: "TreatmentPlan",
                column: "UserId",
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
    }
}
