using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.API.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceShapeAndRemoveCutouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "cutouts",
                schema: "modules",
                table: "assembly_parts",
                newName: "shape");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "shape",
                schema: "modules",
                table: "assembly_parts",
                newName: "cutouts");
        }
    }
}
