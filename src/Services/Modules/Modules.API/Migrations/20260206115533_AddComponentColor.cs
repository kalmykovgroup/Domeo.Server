using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.API.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "color",
                schema: "modules",
                table: "components",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color",
                schema: "modules",
                table: "components");
        }
    }
}
