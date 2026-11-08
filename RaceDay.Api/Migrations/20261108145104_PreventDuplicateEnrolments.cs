using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Api.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateEnrolments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Users_UserId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Users_RecordedByUserId",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_UserId",
                table: "Enrolments");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Enrolments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_UserId_CategoryId",
                table: "Enrolments",
                columns: new[] { "UserId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_UserId1",
                table: "Enrolments",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Users_UserId",
                table: "Enrolments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Users_UserId1",
                table: "Enrolments",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Users_RecordedByUserId",
                table: "Results",
                column: "RecordedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Users_UserId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Users_UserId1",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Results_Users_RecordedByUserId",
                table: "Results");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_UserId_CategoryId",
                table: "Enrolments");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_UserId1",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Enrolments");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_UserId",
                table: "Enrolments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Users_UserId",
                table: "Enrolments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Results_Users_RecordedByUserId",
                table: "Results",
                column: "RecordedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
