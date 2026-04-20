using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscribeNotificationsAndUnsubscribeToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyEmailOnSubscribed",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyPushOnSubscribed",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyTelegramOnSubscribed",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnsubscribeToken",
                table: "newsletter_subscribers",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            // Backfill a unique unsubscribe token for existing subscribers
            // (gen_random_uuid() is only applied on new inserts otherwise).
            migrationBuilder.Sql(
                "UPDATE newsletter_subscribers SET \"UnsubscribeToken\" = gen_random_uuid() WHERE \"UnsubscribeToken\" = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.AddColumn<string>(
                name: "NotificationEmail",
                table: "agencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyEmailOnSubscribed",
                table: "agencies",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyPushOnSubscribed",
                table: "agencies",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyTelegramOnSubscribed",
                table: "agencies",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "TelegramNotificationsEnabled",
                table: "agencies",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyEmailOnSubscribed",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "NotifyPushOnSubscribed",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "NotifyTelegramOnSubscribed",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "UnsubscribeToken",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "NotificationEmail",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "NotifyEmailOnSubscribed",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "NotifyPushOnSubscribed",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "NotifyTelegramOnSubscribed",
                table: "agencies");

            migrationBuilder.DropColumn(
                name: "TelegramNotificationsEnabled",
                table: "agencies");
        }
    }
}
