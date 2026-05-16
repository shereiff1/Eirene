using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eirene.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberCountToCommunityGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberCount",
                table: "CommunityGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberCount",
                table: "CommunityGroups");
        }
    }
}
