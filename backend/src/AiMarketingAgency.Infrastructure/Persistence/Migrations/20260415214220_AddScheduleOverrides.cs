using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalMode",
                table: "content_schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AutoApproveMinScore",
                table: "content_schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoScheduleOnApproval",
                table: "content_schedules",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnabledSocialPlatforms",
                table: "content_schedules",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ApprovalMode", table: "content_schedules");
            migrationBuilder.DropColumn(name: "AutoApproveMinScore", table: "content_schedules");
            migrationBuilder.DropColumn(name: "AutoScheduleOnApproval", table: "content_schedules");
            migrationBuilder.DropColumn(name: "EnabledSocialPlatforms", table: "content_schedules");
        }
    }
}
