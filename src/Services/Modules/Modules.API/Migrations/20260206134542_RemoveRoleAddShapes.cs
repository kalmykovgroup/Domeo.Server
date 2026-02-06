using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoleAddShapes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role",
                schema: "modules",
                table: "assembly_parts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role",
                schema: "modules",
                table: "assembly_parts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
