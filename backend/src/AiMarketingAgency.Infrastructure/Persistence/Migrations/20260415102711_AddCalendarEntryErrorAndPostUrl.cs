using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarEntryErrorAndPostUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "editorial_calendar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostUrl",
                table: "editorial_calendar",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "editorial_calendar");

            migrationBuilder.DropColumn(
                name: "PostUrl",
                table: "editorial_calendar");
        }
    }
}
