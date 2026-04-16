using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandBanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrandBannerColor",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LogoOverlayMode",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandBannerColor",
                table: "agencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LogoOverlayMode",
                table: "agencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandBannerColor",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "LogoOverlayMode",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "BrandBannerColor",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "LogoOverlayMode",
                table: "agencies");
        }
    }
}
