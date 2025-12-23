using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ma.TimeManagement.Migrations
{
    /// <inheritdoc />
    public partial class Remove_title_from_calander_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "WorkCalendarItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "WorkCalendarItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
