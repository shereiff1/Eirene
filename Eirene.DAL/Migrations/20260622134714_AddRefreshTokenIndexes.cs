using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eirene.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientTasks_TreatmentPlans_TreatmentPlanId",
                table: "PatientTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlans_AspNetUsers_UserId",
                table: "TreatmentPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlans_PatientProfiles_PatientProfileId",
                table: "TreatmentPlans");

            migrationBuilder.DropIndex(
                name: "IX_Journals_PatientId",
                table: "Journals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TreatmentPlans",
                table: "TreatmentPlans");

            migrationBuilder.RenameTable(
                name: "TreatmentPlans",
                newName: "TreatmentPlan");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlans_UserId",
                table: "TreatmentPlan",
                newName: "IX_TreatmentPlan_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlans_PatientProfileId",
                table: "TreatmentPlan",
                newName: "IX_TreatmentPlan_PatientProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TreatmentPlan",
                table: "TreatmentPlan",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked_IsUsed",
                table: "RefreshTokens",
                columns: new[] { "UserId", "IsRevoked", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_Journals_PatientId_CreatedAt",
                table: "Journals",
                columns: new[] { "PatientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_DoctorId_PatientId",
                table: "Conversations",
                columns: new[] { "DoctorId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_IsDeleted",
                table: "CommunityPosts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityPosts_PostedOn",
                table: "CommunityPosts",
                column: "PostedOn");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityGroups_Name",
                table: "CommunityGroups",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ConversationId",
                table: "ChatMessages",
                column: "ConversationId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientTasks_TreatmentPlan_TreatmentPlanId",
                table: "PatientTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlan_AspNetUsers_UserId",
                table: "TreatmentPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlan_PatientProfiles_PatientProfileId",
                table: "TreatmentPlan");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked_IsUsed",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Journals_PatientId_CreatedAt",
                table: "Journals");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_DoctorId_PatientId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPosts_IsDeleted",
                table: "CommunityPosts");

            migrationBuilder.DropIndex(
                name: "IX_CommunityPosts_PostedOn",
                table: "CommunityPosts");

            migrationBuilder.DropIndex(
                name: "IX_CommunityGroups_Name",
                table: "CommunityGroups");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ConversationId",
                table: "ChatMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TreatmentPlan",
                table: "TreatmentPlan");

            migrationBuilder.RenameTable(
                name: "TreatmentPlan",
                newName: "TreatmentPlans");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlan_UserId",
                table: "TreatmentPlans",
                newName: "IX_TreatmentPlans_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentPlan_PatientProfileId",
                table: "TreatmentPlans",
                newName: "IX_TreatmentPlans_PatientProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TreatmentPlans",
                table: "TreatmentPlans",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Journals_PatientId",
                table: "Journals",
                column: "PatientId");

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
    }
}
