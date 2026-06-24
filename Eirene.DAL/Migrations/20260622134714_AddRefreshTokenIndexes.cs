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
            migrationBuilder.DropIndex(
                name: "IX_Journals_PatientId",
                table: "Journals");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_Journals_PatientId",
                table: "Journals",
                column: "PatientId");
        }
    }
}
