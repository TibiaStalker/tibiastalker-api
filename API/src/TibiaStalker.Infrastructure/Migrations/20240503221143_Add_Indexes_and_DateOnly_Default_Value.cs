using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TibiaStalker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesandDateOnlyDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "verified_date",
                schema: "public",
                table: "characters",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2001, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "traded_date",
                schema: "public",
                table: "characters",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2001, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_world_scans_scan_create_date_time",
                schema: "public",
                table: "world_scans",
                column: "scan_create_date_time");

            migrationBuilder.CreateIndex(
                name: "ix_characters_found_in_scan",
                schema: "public",
                table: "characters",
                column: "found_in_scan");

            migrationBuilder.CreateIndex(
                name: "ix_characters_traded_date",
                schema: "public",
                table: "characters",
                column: "traded_date");

            migrationBuilder.CreateIndex(
                name: "ix_characters_verified_date",
                schema: "public",
                table: "characters",
                column: "verified_date");

            migrationBuilder.CreateIndex(
                name: "ix_character_correlations_number_of_matches",
                schema: "public",
                table: "character_correlations",
                column: "number_of_matches");

            migrationBuilder.CreateIndex(
                name: "ix_character_actions_is_online",
                schema: "public",
                table: "character_actions",
                column: "is_online");

            migrationBuilder.Sql("UPDATE characters SET verified_date = '0001-01-01' WHERE verified_date IS NULL");
            migrationBuilder.Sql("UPDATE characters SET traded_date = '0001-01-01' WHERE traded_date IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_world_scans_scan_create_date_time",
                schema: "public",
                table: "world_scans");

            migrationBuilder.DropIndex(
                name: "ix_characters_found_in_scan",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_characters_traded_date",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_characters_verified_date",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_character_correlations_number_of_matches",
                schema: "public",
                table: "character_correlations");

            migrationBuilder.DropIndex(
                name: "ix_character_actions_is_online",
                schema: "public",
                table: "character_actions");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "verified_date",
                schema: "public",
                table: "characters",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValue: new DateOnly(2001, 1, 1));

            migrationBuilder.AlterColumn<DateOnly>(
                name: "traded_date",
                schema: "public",
                table: "characters",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValue: new DateOnly(2001, 1, 1));
        }
    }
}
