using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixEnrolmentRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Users_UserId1",
                table: "Enrolments");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_UserId1",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Enrolments");

            migrationBuilder.AlterColumn<decimal>(
                name: "DistanceKm",
                table: "Categories",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Enrolments",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DistanceKm",
                table: "Categories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_UserId1",
                table: "Enrolments",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Users_UserId1",
                table: "Enrolments",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
