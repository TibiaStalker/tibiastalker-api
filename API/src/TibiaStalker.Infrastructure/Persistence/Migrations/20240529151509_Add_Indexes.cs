using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TibiaStalker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_characters_character_id",
                schema: "public",
                table: "characters");

            migrationBuilder.CreateIndex(
                name: "ix_world_scan_id_world_id_is_deleted",
                schema: "public",
                table: "world_scans",
                columns: new[] { "world_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "ix_world_scans_is_deleted",
                schema: "public",
                table: "world_scans",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_characters_scan1_scan2",
                schema: "public",
                table: "characters",
                columns: new[] { "found_in_scan1", "found_in_scan2" });

            migrationBuilder.CreateIndex(
                name: "ix_characters_scan2_scan1",
                schema: "public",
                table: "characters",
                columns: new[] { "found_in_scan2", "found_in_scan1" });

            migrationBuilder.CreateIndex(
                name: "ix_correlations_login_character_id_logout_character_id",
                schema: "public",
                table: "character_correlations",
                columns: new[] { "login_character_id", "logout_character_id" });

            migrationBuilder.CreateIndex(
                name: "ix_correlations_logout_character_id_login_character_id",
                schema: "public",
                table: "character_correlations",
                columns: new[] { "logout_character_id", "login_character_id" });

            migrationBuilder.CreateIndex(
                name: "ix_character_actions_is_online_character_name",
                schema: "public",
                table: "character_actions",
                columns: new[] { "is_online", "character_name" });

            migrationBuilder.CreateIndex(
                name: "ix_character_actions_logout_or_login_date",
                schema: "public",
                table: "character_actions",
                column: "logout_or_login_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_world_scan_id_world_id_is_deleted",
                schema: "public",
                table: "world_scans");

            migrationBuilder.DropIndex(
                name: "ix_world_scans_is_deleted",
                schema: "public",
                table: "world_scans");

            migrationBuilder.DropIndex(
                name: "ix_characters_scan1_scan2",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_characters_scan2_scan1",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_correlations_login_character_id_logout_character_id",
                schema: "public",
                table: "character_correlations");

            migrationBuilder.DropIndex(
                name: "ix_correlations_logout_character_id_login_character_id",
                schema: "public",
                table: "character_correlations");

            migrationBuilder.DropIndex(
                name: "ix_character_actions_is_online_character_name",
                schema: "public",
                table: "character_actions");

            migrationBuilder.DropIndex(
                name: "ix_character_actions_logout_or_login_date",
                schema: "public",
                table: "character_actions");

            migrationBuilder.CreateIndex(
                name: "ix_characters_character_id",
                schema: "public",
                table: "characters",
                column: "character_id");
        }
    }
}
