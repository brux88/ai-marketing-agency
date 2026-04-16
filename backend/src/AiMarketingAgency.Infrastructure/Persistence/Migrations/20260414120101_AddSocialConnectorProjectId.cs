using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialConnectorProjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_social_connectors_AgencyId_Platform",
                table: "social_connectors");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "social_connectors",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_social_connectors_AgencyId_ProjectId_Platform",
                table: "social_connectors",
                columns: new[] { "AgencyId", "ProjectId", "Platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_social_connectors_ProjectId",
                table: "social_connectors",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_social_connectors_projects_ProjectId",
                table: "social_connectors",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_social_connectors_projects_ProjectId",
                table: "social_connectors");

            migrationBuilder.DropIndex(
                name: "IX_social_connectors_AgencyId_ProjectId_Platform",
                table: "social_connectors");

            migrationBuilder.DropIndex(
                name: "IX_social_connectors_ProjectId",
                table: "social_connectors");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "social_connectors");

            migrationBuilder.CreateIndex(
                name: "IX_social_connectors_AgencyId_Platform",
                table: "social_connectors",
                columns: new[] { "AgencyId", "Platform" });
        }
    }
}
