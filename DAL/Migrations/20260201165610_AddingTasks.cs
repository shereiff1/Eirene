using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddingTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TreatmentPlan",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PatientTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreatmentPlanId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientTasks_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PatientTasks_TreatmentPlan_TreatmentPlanId",
                        column: x => x.TreatmentPlanId,
                        principalTable: "TreatmentPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlan_UserId",
                table: "TreatmentPlan",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientTasks_PatientId",
                table: "PatientTasks",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientTasks_TreatmentPlanId",
                table: "PatientTasks",
                column: "TreatmentPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentPlan_AspNetUsers_UserId",
                table: "TreatmentPlan",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentPlan_AspNetUsers_UserId",
                table: "TreatmentPlan");

            migrationBuilder.DropTable(
                name: "PatientTasks");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentPlan_UserId",
                table: "TreatmentPlan");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TreatmentPlan");
        }
    }
}
