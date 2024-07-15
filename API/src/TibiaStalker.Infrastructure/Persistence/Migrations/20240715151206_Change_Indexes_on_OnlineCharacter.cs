using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TibiaStalker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIndexesonOnlineCharacter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_online_characters",
                schema: "public",
                table: "online_characters");

            migrationBuilder.DropIndex(
                name: "ix_online_characters_name",
                schema: "public",
                table: "online_characters");

            migrationBuilder.AddPrimaryKey(
                name: "pk_online_characters",
                schema: "public",
                table: "online_characters",
                columns: new[] { "name", "online_date_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_online_characters",
                schema: "public",
                table: "online_characters");

            migrationBuilder.AddPrimaryKey(
                name: "pk_online_characters",
                schema: "public",
                table: "online_characters",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_online_characters_name",
                schema: "public",
                table: "online_characters",
                column: "name");
        }
    }
}
