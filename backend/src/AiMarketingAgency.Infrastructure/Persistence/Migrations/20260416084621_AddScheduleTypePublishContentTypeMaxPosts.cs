using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleTypePublishContentTypeMaxPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxPostsPerPlatform",
                table: "content_schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublishContentType",
                table: "content_schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleType",
                table: "content_schedules",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPostsPerPlatform",
                table: "content_schedules");

            migrationBuilder.DropColumn(
                name: "PublishContentType",
                table: "content_schedules");

            migrationBuilder.DropColumn(
                name: "ScheduleType",
                table: "content_schedules");
        }
    }
}
