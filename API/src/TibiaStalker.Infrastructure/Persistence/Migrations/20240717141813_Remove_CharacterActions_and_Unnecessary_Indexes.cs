using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TibiaStalker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCharacterActionsandUnnecessaryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "character_actions",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_characters_found_in_scan1",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_characters_found_in_scan2",
                schema: "public",
                table: "characters");

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
                name: "ix_correlations_number_matches_last_match_date",
                schema: "public",
                table: "character_correlations");

            migrationBuilder.DropColumn(
                name: "found_in_scan1",
                schema: "public",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "found_in_scan2",
                schema: "public",
                table: "characters");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "last_match_date",
                schema: "public",
                table: "character_correlations",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValue: new DateOnly(2022, 12, 6));

            migrationBuilder.AlterColumn<DateOnly>(
                name: "create_date",
                schema: "public",
                table: "character_correlations",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValue: new DateOnly(2022, 12, 6));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "found_in_scan1",
                schema: "public",
                table: "characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "found_in_scan2",
                schema: "public",
                table: "characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "last_match_date",
                schema: "public",
                table: "character_correlations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2022, 12, 6),
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "create_date",
                schema: "public",
                table: "character_correlations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2022, 12, 6),
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.CreateTable(
                name: "character_actions",
                schema: "public",
                columns: table => new
                {
                    characteractionid = table.Column<int>(name: "character_action_id", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    worldid = table.Column<short>(name: "world_id", type: "smallint", nullable: false),
                    worldscanid = table.Column<int>(name: "world_scan_id", type: "integer", nullable: false),
                    charactername = table.Column<string>(name: "character_name", type: "character varying(100)", maxLength: 100, nullable: false),
                    isonline = table.Column<bool>(name: "is_online", type: "boolean", nullable: false),
                    logoutorlogindate = table.Column<DateOnly>(name: "logout_or_login_date", type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_actions", x => x.characteractionid);
                    table.ForeignKey(
                        name: "fk_character_actions_world_scans_world_scan_id",
                        column: x => x.worldscanid,
                        principalSchema: "public",
                        principalTable: "world_scans",
                        principalColumn: "world_scan_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_character_actions_worlds_world_id",
                        column: x => x.worldid,
                        principalSchema: "public",
                        principalTable: "worlds",
                        principalColumn: "world_id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_characters_found_in_scan1",
                schema: "public",
                table: "characters",
                column: "found_in_scan1");

            migrationBuilder.CreateIndex(
                name: "ix_characters_found_in_scan2",
                schema: "public",
                table: "characters",
                column: "found_in_scan2");

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
                name: "ix_correlations_number_matches_last_match_date",
                schema: "public",
                table: "character_correlations",
                columns: new[] { "number_of_matches", "last_match_date" });

            migrationBuilder.CreateIndex(
                name: "ix_character_actions_character_name",
                schema: "public",
                table: "character_actions",
                column: "character_name");

            migrationBuilder.CreateIndex(
                name: "ix_character_actions_is_online",
                schema: "public",
                table: "character_actions",
                column: "is_online");

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

            migrationBuilder.CreateIndex(
                name: "ix_character_actions_world_id",
                schema: "public",
                table: "character_actions",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_actions_world_scan_id",
                schema: "public",
                table: "character_actions",
                column: "world_scan_id");
        }
    }
}
