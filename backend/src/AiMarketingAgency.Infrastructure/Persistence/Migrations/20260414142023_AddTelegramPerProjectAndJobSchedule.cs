using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramPerProjectAndJobSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TelegramConnections_AgencyId",
                table: "TelegramConnections");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "TelegramConnections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotToken",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotUsername",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduleId",
                table: "agent_jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramConnections_AgencyId_ProjectId",
                table: "TelegramConnections",
                columns: new[] { "AgencyId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramConnections_ProjectId",
                table: "TelegramConnections",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_jobs_ScheduleId",
                table: "agent_jobs",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_agent_jobs_content_schedules_ScheduleId",
                table: "agent_jobs",
                column: "ScheduleId",
                principalTable: "content_schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramConnections_projects_ProjectId",
                table: "TelegramConnections",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_agent_jobs_content_schedules_ScheduleId",
                table: "agent_jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_TelegramConnections_projects_ProjectId",
                table: "TelegramConnections");

            migrationBuilder.DropIndex(
                name: "IX_TelegramConnections_AgencyId_ProjectId",
                table: "TelegramConnections");

            migrationBuilder.DropIndex(
                name: "IX_TelegramConnections_ProjectId",
                table: "TelegramConnections");

            migrationBuilder.DropIndex(
                name: "IX_agent_jobs_ScheduleId",
                table: "agent_jobs");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "TelegramConnections");

            migrationBuilder.DropColumn(
                name: "TelegramBotToken",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "TelegramBotUsername",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "agent_jobs");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramConnections_AgencyId",
                table: "TelegramConnections",
                column: "AgencyId");
        }
    }
}
