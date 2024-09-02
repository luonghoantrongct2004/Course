using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCourse.Migrations
{
    /// <inheritdoc />
    public partial class Alter_CourseQuestionLessionOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OptionText",
                table: "Options",
                newName: "Content");

            migrationBuilder.AddColumn<string>(
                name: "Choices",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LessonID",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ShowTime",
                table: "Questions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_LessonID",
                table: "Questions",
                column: "LessonID");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Lessons_LessonID",
                table: "Questions",
                column: "LessonID",
                principalTable: "Lessons",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Lessons_LessonID",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_LessonID",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Choices",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "LessonID",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ShowTime",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Options",
                newName: "OptionText");
        }
    }
}
