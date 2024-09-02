using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCourse.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Question : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Quizzes_QuizID",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Categories_CategoryID",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Lessons_LessonID",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizzes_Quizzes_QuizID",
                table: "StudentQuizzes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Quizzes",
                table: "Quizzes");

            migrationBuilder.RenameTable(
                name: "Quizzes",
                newName: "Quiz");

            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_LessonID",
                table: "Quiz",
                newName: "IX_Quiz_LessonID");

            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_CategoryID",
                table: "Quiz",
                newName: "IX_Quiz_CategoryID");

            migrationBuilder.AlterColumn<int>(
                name: "QuizID",
                table: "Questions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Quiz",
                table: "Quiz",
                column: "QuizID");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Quiz_QuizID",
                table: "Questions",
                column: "QuizID",
                principalTable: "Quiz",
                principalColumn: "QuizID");

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_Categories_CategoryID",
                table: "Quiz",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_Lessons_LessonID",
                table: "Quiz",
                column: "LessonID",
                principalTable: "Lessons",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizzes_Quiz_QuizID",
                table: "StudentQuizzes",
                column: "QuizID",
                principalTable: "Quiz",
                principalColumn: "QuizID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Quiz_QuizID",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_Categories_CategoryID",
                table: "Quiz");

            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_Lessons_LessonID",
                table: "Quiz");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentQuizzes_Quiz_QuizID",
                table: "StudentQuizzes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Quiz",
                table: "Quiz");

            migrationBuilder.RenameTable(
                name: "Quiz",
                newName: "Quizzes");

            migrationBuilder.RenameIndex(
                name: "IX_Quiz_LessonID",
                table: "Quizzes",
                newName: "IX_Quizzes_LessonID");

            migrationBuilder.RenameIndex(
                name: "IX_Quiz_CategoryID",
                table: "Quizzes",
                newName: "IX_Quizzes_CategoryID");

            migrationBuilder.AlterColumn<int>(
                name: "QuizID",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Quizzes",
                table: "Quizzes",
                column: "QuizID");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Quizzes_QuizID",
                table: "Questions",
                column: "QuizID",
                principalTable: "Quizzes",
                principalColumn: "QuizID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Categories_CategoryID",
                table: "Quizzes",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Lessons_LessonID",
                table: "Quizzes",
                column: "LessonID",
                principalTable: "Lessons",
                principalColumn: "LessonID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQuizzes_Quizzes_QuizID",
                table: "StudentQuizzes",
                column: "QuizID",
                principalTable: "Quizzes",
                principalColumn: "QuizID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
