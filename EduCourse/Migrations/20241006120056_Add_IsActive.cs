using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCourse.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_AuthorID",
                table: "Courses");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Exams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorID",
                table: "Courses",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_AuthorID",
                table: "Courses",
                column: "AuthorID",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_AuthorID",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Exams");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorID",
                table: "Courses",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_AuthorID",
                table: "Courses",
                column: "AuthorID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
