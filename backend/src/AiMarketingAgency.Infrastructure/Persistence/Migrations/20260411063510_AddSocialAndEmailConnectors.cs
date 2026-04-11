using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialAndEmailConnectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedApiKeySecret",
                table: "llm_provider_keys",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "generated_contents",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VideoDurationSeconds",
                table: "generated_contents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoPrompt",
                table: "generated_contents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "generated_contents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableLogoOverlay",
                table: "agencies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LogoOverlayPosition",
                table: "agencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "VideoLlmProviderKeyId",
                table: "agencies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "email_connectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    SmtpHost = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SmtpPort = table.Column<int>(type: "integer", nullable: true),
                    SmtpUsername = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SmtpPassword = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApiKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FromEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FromName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_connectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_email_connectors_agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_email_connectors_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "newsletter_subscribers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnsubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newsletter_subscribers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_newsletter_subscribers_agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_newsletter_subscribers_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "social_connectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AccountId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_connectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_social_connectors_agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_social_connectors_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agencies_VideoLlmProviderKeyId",
                table: "agencies",
                column: "VideoLlmProviderKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_email_connectors_AgencyId",
                table: "email_connectors",
                column: "AgencyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_connectors_TenantId",
                table: "email_connectors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_newsletter_subscribers_AgencyId_Email",
                table: "newsletter_subscribers",
                columns: new[] { "AgencyId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_newsletter_subscribers_TenantId",
                table: "newsletter_subscribers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_social_connectors_AgencyId_Platform",
                table: "social_connectors",
                columns: new[] { "AgencyId", "Platform" });

            migrationBuilder.CreateIndex(
                name: "IX_social_connectors_TenantId",
                table: "social_connectors",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_agencies_llm_provider_keys_VideoLlmProviderKeyId",
                table: "agencies",
                column: "VideoLlmProviderKeyId",
                principalTable: "llm_provider_keys",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_agencies_llm_provider_keys_VideoLlmProviderKeyId",
                table: "agencies");

            migrationBuilder.DropTable(
                name: "email_connectors");

            migrationBuilder.DropTable(
                name: "newsletter_subscribers");

            migrationBuilder.DropTable(
                name: "social_connectors");

            migrationBuilder.DropIndex(
                name: "IX_agencies_VideoLlmProviderKeyId",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "EncryptedApiKeySecret",
                table: "llm_provider_keys");

            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "VideoDurationSeconds",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "VideoPrompt",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "EnableLogoOverlay",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "LogoOverlayPosition",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "VideoLlmProviderKeyId",
                table: "agencies");
        }
    }
}
