using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projects.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE projects.cabinet_parts ALTER COLUMN material_id TYPE uuid USING material_id::uuid;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "material_id",
                schema: "projects",
                table: "cabinet_parts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
