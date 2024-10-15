using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCourse.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentExamDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentExamId = table.Column<int>(type: "int", nullable: false),
                    QuestionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentExamDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentExamDetails_StudentExams_StudentExamId",
                        column: x => x.StudentExamId,
                        principalTable: "StudentExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamDetails_StudentExamId",
                table: "StudentExamDetails",
                column: "StudentExamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentExamDetails");
        }
    }
}
