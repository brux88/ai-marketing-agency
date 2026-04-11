using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImageLlmAndProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "llm_provider_keys",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImagePrompt",
                table: "generated_contents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "generated_contents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "generated_contents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "content_sources",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "agent_jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImageLlmProviderKeyId",
                table: "agencies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BrandVoice = table.Column<string>(type: "jsonb", nullable: false),
                    TargetAudience = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projects_agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_projects_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_generated_contents_ProjectId",
                table: "generated_contents",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_content_sources_ProjectId",
                table: "content_sources",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_jobs_ProjectId",
                table: "agent_jobs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_agencies_ImageLlmProviderKeyId",
                table: "agencies",
                column: "ImageLlmProviderKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_AgencyId",
                table: "projects",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_TenantId",
                table: "projects",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_agencies_llm_provider_keys_ImageLlmProviderKeyId",
                table: "agencies",
                column: "ImageLlmProviderKeyId",
                principalTable: "llm_provider_keys",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_agent_jobs_projects_ProjectId",
                table: "agent_jobs",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_content_sources_projects_ProjectId",
                table: "content_sources",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_generated_contents_projects_ProjectId",
                table: "generated_contents",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_agencies_llm_provider_keys_ImageLlmProviderKeyId",
                table: "agencies");

            migrationBuilder.DropForeignKey(
                name: "FK_agent_jobs_projects_ProjectId",
                table: "agent_jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_content_sources_projects_ProjectId",
                table: "content_sources");

            migrationBuilder.DropForeignKey(
                name: "FK_generated_contents_projects_ProjectId",
                table: "generated_contents");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropIndex(
                name: "IX_generated_contents_ProjectId",
                table: "generated_contents");

            migrationBuilder.DropIndex(
                name: "IX_content_sources_ProjectId",
                table: "content_sources");

            migrationBuilder.DropIndex(
                name: "IX_agent_jobs_ProjectId",
                table: "agent_jobs");

            migrationBuilder.DropIndex(
                name: "IX_agencies_ImageLlmProviderKeyId",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "llm_provider_keys");

            migrationBuilder.DropColumn(
                name: "ImagePrompt",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "content_sources");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "agent_jobs");

            migrationBuilder.DropColumn(
                name: "ImageLlmProviderKeyId",
                table: "agencies");
        }
    }
}
