using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiMarketingAgency.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentPublishedAtAndCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AiGenerationCostUsd",
                table: "generated_contents",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AiImageCostUsd",
                table: "generated_contents",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "generated_contents",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiGenerationCostUsd",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "AiImageCostUsd",
                table: "generated_contents");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "generated_contents");
        }
    }
}
