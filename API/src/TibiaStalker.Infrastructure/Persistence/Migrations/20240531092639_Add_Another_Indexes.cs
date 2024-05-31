using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TibiaStalker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnotherIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_world_scan_world_id_is_deleted_scan_date_time",
                schema: "public",
                table: "world_scans",
                columns: new[] { "world_id", "is_deleted", "scan_create_date_time" });

            migrationBuilder.CreateIndex(
                name: "ix_correlations_number_matches_last_match_date",
                schema: "public",
                table: "character_correlations",
                columns: new[] { "number_of_matches", "last_match_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_world_scan_world_id_is_deleted_scan_date_time",
                schema: "public",
                table: "world_scans");

            migrationBuilder.DropIndex(
                name: "ix_correlations_number_matches_last_match_date",
                schema: "public",
                table: "character_correlations");
        }
    }
}
