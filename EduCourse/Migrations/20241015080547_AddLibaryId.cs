using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCourse.Migrations
{
    /// <inheritdoc />
    public partial class AddLibaryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_CourseID",
                table: "Libraries");

            migrationBuilder.AddColumn<int>(
                name: "LibraryID",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_CourseID",
                table: "Libraries",
                column: "CourseID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_CourseID",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "LibraryID",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_CourseID",
                table: "Libraries",
                column: "CourseID");
        }
    }
}
