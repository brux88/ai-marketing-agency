using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedAgencyIds",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllowedProjectIds",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreateApiKeys",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreateProjects",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AllowedAgencyIds",
                table: "TeamInvitations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllowedProjectIds",
                table: "TeamInvitations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreateApiKeys",
                table: "TeamInvitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreateProjects",
                table: "TeamInvitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedAgencyIds",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AllowedProjectIds",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CanCreateApiKeys",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CanCreateProjects",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AllowedAgencyIds",
                table: "TeamInvitations");

            migrationBuilder.DropColumn(
                name: "AllowedProjectIds",
                table: "TeamInvitations");

            migrationBuilder.DropColumn(
                name: "CanCreateApiKeys",
                table: "TeamInvitations");

            migrationBuilder.DropColumn(
                name: "CanCreateProjects",
                table: "TeamInvitations");
        }
    }
}
