using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyTelegramAndEmailConnectorProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_email_connectors_AgencyId",
                table: "email_connectors");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "email_connectors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotToken",
                table: "agencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotUsername",
                table: "agencies",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_connectors_AgencyId_ProjectId",
                table: "email_connectors",
                columns: new[] { "AgencyId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_connectors_ProjectId",
                table: "email_connectors",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_email_connectors_projects_ProjectId",
                table: "email_connectors",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_email_connectors_projects_ProjectId",
                table: "email_connectors");

            migrationBuilder.DropIndex(
                name: "IX_email_connectors_AgencyId_ProjectId",
                table: "email_connectors");

            migrationBuilder.DropIndex(
                name: "IX_email_connectors_ProjectId",
                table: "email_connectors");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "email_connectors");

            migrationBuilder.DropColumn(
                name: "TelegramBotToken",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "TelegramBotUsername",
                table: "agencies");

            migrationBuilder.CreateIndex(
                name: "IX_email_connectors_AgencyId",
                table: "email_connectors",
                column: "AgencyId",
                unique: true);
        }
    }
}
