using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ILA_Server.Migrations
{
    public partial class ProfQuestions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Questions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CourseNews",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Answers",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.CreateTable(
                name: "ProfQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: false),
                    Question = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    LectureId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfQuestion_Lectures_LectureId",
                        column: x => x.LectureId,
                        principalTable: "Lectures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Answer = table.Column<string>(nullable: false),
                    ProfQuestionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfAnswer_ProfQuestion_ProfQuestionId",
                        column: x => x.ProfQuestionId,
                        principalTable: "ProfQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfQuestionAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    ProfAnswerId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfQuestionAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfQuestionAnswer_ProfAnswer_ProfAnswerId",
                        column: x => x.ProfAnswerId,
                        principalTable: "ProfAnswer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfQuestionAnswer_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfAnswer_ProfQuestionId",
                table: "ProfAnswer",
                column: "ProfQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfQuestion_LectureId",
                table: "ProfQuestion",
                column: "LectureId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfQuestionAnswer_ProfAnswerId",
                table: "ProfQuestionAnswer",
                column: "ProfAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfQuestionAnswer_UserId",
                table: "ProfQuestionAnswer",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfQuestionAnswer");

            migrationBuilder.DropTable(
                name: "ProfAnswer");

            migrationBuilder.DropTable(
                name: "ProfQuestion");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CourseNews");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Answers");
        }
    }
}
