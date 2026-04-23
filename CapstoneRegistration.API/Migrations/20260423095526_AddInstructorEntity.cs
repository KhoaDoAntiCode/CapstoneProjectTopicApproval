using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneRegistration.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstructorId",
                table: "project_supervisors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Instructors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Pronouns = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_supervisors_InstructorId",
                table: "project_supervisors",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "idx_instructors_email",
                table: "Instructors",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_project_supervisors_Instructors_InstructorId",
                table: "project_supervisors",
                column: "InstructorId",
                principalTable: "Instructors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project_supervisors_Instructors_InstructorId",
                table: "project_supervisors");

            migrationBuilder.DropTable(
                name: "Instructors");

            migrationBuilder.DropIndex(
                name: "IX_project_supervisors_InstructorId",
                table: "project_supervisors");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "project_supervisors");
        }
    }
}
