using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPushNotificationsAndDeviceTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyPushOnApprovalNeeded",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyPushOnGeneration",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyPushOnPublication",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "user_device_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FcmToken = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Platform = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_device_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_device_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_device_tokens_FcmToken",
                table: "user_device_tokens",
                column: "FcmToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_device_tokens_TenantId_UserId",
                table: "user_device_tokens",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_user_device_tokens_UserId",
                table: "user_device_tokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_device_tokens");

            migrationBuilder.DropColumn(
                name: "NotifyPushOnApprovalNeeded",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "NotifyPushOnGeneration",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "NotifyPushOnPublication",
                table: "projects");
        }
    }
}
