using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectAutoScheduleAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoScheduleOnApproval",
                table: "projects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnabledSocialPlatforms",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "generated_contents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "generated_contents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoScheduleOnApproval",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "EnabledSocialPlatforms",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "generated_contents");
        }
    }
}
