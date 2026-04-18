using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectIdToNewsletterSubscriber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "newsletter_subscribers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_newsletter_subscribers_ProjectId",
                table: "newsletter_subscribers",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_newsletter_subscribers_projects_ProjectId",
                table: "newsletter_subscribers",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_newsletter_subscribers_projects_ProjectId",
                table: "newsletter_subscribers");

            migrationBuilder.DropIndex(
                name: "IX_newsletter_subscribers_ProjectId",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "newsletter_subscribers");
        }
    }
}
