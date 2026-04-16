using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneRegistration.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "capstone_projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    semester_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    english_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    vietnamese_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    abbreviation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_research_project = table.Column<bool>(type: "boolean", nullable: false),
                    is_enterprise_project = table.Column<bool>(type: "boolean", nullable: false),
                    context = table.Column<string>(type: "text", nullable: true),
                    proposed_solutions = table.Column<string>(type: "text", nullable: true),
                    functional_requirements = table.Column<string>(type: "text", nullable: true),
                    non_functional_requirements = table.Column<string>(type: "text", nullable: true),
                    theory_and_practice = table.Column<string>(type: "text", nullable: true),
                    products = table.Column<string>(type: "text", nullable: true),
                    proposed_tasks = table.Column<string>(type: "text", nullable: true),
                    @class = table.Column<string>(name: "class", type: "character varying(20)", maxLength: 20, nullable: true),
                    duration_from = table.Column<DateOnly>(type: "date", nullable: true),
                    duration_to = table.Column<DateOnly>(type: "date", nullable: true),
                    profession = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    specialty = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    register_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capstone_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_capstone_projects_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewed_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_reviews_capstone_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "capstone_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_reviews_users_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    student_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    role_in_group = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_students", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_students_capstone_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "capstone_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_supervisors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_supervisors", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_supervisors_capstone_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "capstone_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_capstone_projects_project_code",
                table: "capstone_projects",
                column: "project_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_capstone_projects_semester_id",
                table: "capstone_projects",
                column: "semester_id");

            migrationBuilder.CreateIndex(
                name: "IX_capstone_projects_created_by_id",
                table: "capstone_projects",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "idx_project_reviews_project_id",
                table: "project_reviews",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_project_reviews_reviewed_by_id",
                table: "project_reviews",
                column: "reviewed_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_students_project_id",
                table: "project_students",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_supervisors_project_id",
                table: "project_supervisors",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_reviews");

            migrationBuilder.DropTable(
                name: "project_students");

            migrationBuilder.DropTable(
                name: "project_supervisors");

            migrationBuilder.DropTable(
                name: "capstone_projects");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
