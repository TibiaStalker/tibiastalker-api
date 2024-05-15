using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TibiaStalker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoundInScan2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "found_in_scan",
                schema: "public",
                table: "characters",
                newName: "found_in_scan2");

            migrationBuilder.RenameIndex(
                name: "ix_characters_found_in_scan",
                schema: "public",
                table: "characters",
                newName: "ix_characters_found_in_scan2");

            migrationBuilder.AddColumn<bool>(
                name: "found_in_scan1",
                schema: "public",
                table: "characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_characters_found_in_scan1",
                schema: "public",
                table: "characters",
                column: "found_in_scan1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_characters_found_in_scan1",
                schema: "public",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "found_in_scan1",
                schema: "public",
                table: "characters");

            migrationBuilder.RenameColumn(
                name: "found_in_scan2",
                schema: "public",
                table: "characters",
                newName: "found_in_scan");

            migrationBuilder.RenameIndex(
                name: "ix_characters_found_in_scan2",
                schema: "public",
                table: "characters",
                newName: "ix_characters_found_in_scan");
        }
    }
}
