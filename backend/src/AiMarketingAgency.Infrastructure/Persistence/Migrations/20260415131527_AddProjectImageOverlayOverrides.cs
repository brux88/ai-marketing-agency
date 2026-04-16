using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectImageOverlayOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableLogoOverlay",
                table: "projects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LogoOverlayPosition",
                table: "projects",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableLogoOverlay",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "LogoOverlayPosition",
                table: "projects");
        }
    }
}
