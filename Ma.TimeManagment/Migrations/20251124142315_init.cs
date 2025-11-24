using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ma.TimeManagement.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    Revision = table.Column<long>(type: "INTEGER", nullable: false),
                    Visibility = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultTeamImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    LastUpdateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NamespaceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequiredPermissions = table.Column<int>(type: "INTEGER", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalEstimate = table.Column<double>(type: "REAL", nullable: false),
                    RemainingWork = table.Column<double>(type: "REAL", nullable: false),
                    CompletedWork = table.Column<double>(type: "REAL", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    WorkItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectName = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectReferenceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItems_TeamProjects_ProjectReferenceId",
                        column: x => x.ProjectReferenceId,
                        principalTable: "TeamProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkCalendarItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationHour = table.Column<double>(type: "REAL", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    WorkItemID = table.Column<int>(type: "INTEGER", nullable: true),
                    Synced = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCalendarItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkCalendarItems_WorkItems_WorkItemID",
                        column: x => x.WorkItemID,
                        principalTable: "WorkItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkCalendarItems_WorkItemID",
                table: "WorkCalendarItems",
                column: "WorkItemID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectReferenceId",
                table: "WorkItems",
                column: "ProjectReferenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkCalendarItems");

            migrationBuilder.DropTable(
                name: "WorkItems");

            migrationBuilder.DropTable(
                name: "TeamProjects");
        }
    }
}
