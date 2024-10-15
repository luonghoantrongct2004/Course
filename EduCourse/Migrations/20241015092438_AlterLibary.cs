using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCourse.Migrations
{
    /// <inheritdoc />
    public partial class AlterLibary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_Files_FileID",
                table: "Libraries");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_FileID",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "FileID",
                table: "Libraries");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Libraries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePaths",
                table: "Libraries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "UploadedByID",
                table: "Libraries",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedDate",
                table: "Libraries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_UploadedByID",
                table: "Libraries",
                column: "UploadedByID");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_Users_UploadedByID",
                table: "Libraries",
                column: "UploadedByID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_Users_UploadedByID",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_UploadedByID",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "FilePaths",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "UploadedByID",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "UploadedDate",
                table: "Libraries");

            migrationBuilder.AddColumn<int>(
                name: "FileID",
                table: "Libraries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    FileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedByID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.FileID);
                    table.ForeignKey(
                        name: "FK_Files_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_FileID",
                table: "Libraries",
                column: "FileID");

            migrationBuilder.CreateIndex(
                name: "IX_Files_UserId",
                table: "Files",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_Files_FileID",
                table: "Libraries",
                column: "FileID",
                principalTable: "Files",
                principalColumn: "FileID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
