using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectEmailNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotificationEmail",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyEmailOnApprovalNeeded",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyEmailOnGeneration",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyEmailOnPublication",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationEmail",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "NotifyEmailOnApprovalNeeded",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "NotifyEmailOnGeneration",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "NotifyEmailOnPublication",
                table: "projects");
        }
    }
}
