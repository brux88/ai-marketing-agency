using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectApprovalOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalMode",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AutoApproveMinScore",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoScheduleOnApproval",
                table: "agencies",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalMode",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "AutoApproveMinScore",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "AutoScheduleOnApproval",
                table: "agencies");
        }
    }
}
