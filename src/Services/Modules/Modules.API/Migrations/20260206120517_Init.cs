using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.API.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "modules");

            migrationBuilder.CreateTable(
                name: "components",
                schema: "modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    @params = table.Column<string>(name: "params", type: "jsonb", nullable: true),
                    tags = table.Column<List<string>>(type: "varchar[]", nullable: false),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_components", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "module_categories",
                schema: "modules",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    parent_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_module_categories_module_categories_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "modules",
                        principalTable: "module_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "storage_connections",
                schema: "modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "s3"),
                    endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    bucket = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    access_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    secret_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_connections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "assemblies",
                schema: "modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    parameters = table.Column<string>(type: "jsonb", nullable: false),
                    param_constraints = table.Column<string>(type: "jsonb", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assemblies", x => x.id);
                    table.ForeignKey(
                        name: "FK_assemblies_module_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "modules",
                        principalTable: "module_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assembly_parts",
                schema: "modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assembly_id = table.Column<Guid>(type: "uuid", nullable: false),
                    component_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    length_expr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    width_expr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    x = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    y = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    z = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rotation_x = table.Column<double>(type: "double precision", nullable: false),
                    rotation_y = table.Column<double>(type: "double precision", nullable: false),
                    rotation_z = table.Column<double>(type: "double precision", nullable: false),
                    condition = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    shape = table.Column<string>(type: "jsonb", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    quantity_formula = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assembly_parts", x => x.id);
                    table.ForeignKey(
                        name: "FK_assembly_parts_assemblies_assembly_id",
                        column: x => x.assembly_id,
                        principalSchema: "modules",
                        principalTable: "assemblies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assembly_parts_components_component_id",
                        column: x => x.component_id,
                        principalSchema: "modules",
                        principalTable: "components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assemblies_category_id",
                schema: "modules",
                table: "assemblies",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_assemblies_is_active",
                schema: "modules",
                table: "assemblies",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_assemblies_type",
                schema: "modules",
                table: "assemblies",
                column: "type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assembly_parts_assembly_id",
                schema: "modules",
                table: "assembly_parts",
                column: "assembly_id");

            migrationBuilder.CreateIndex(
                name: "IX_assembly_parts_component_id",
                schema: "modules",
                table: "assembly_parts",
                column: "component_id");

            migrationBuilder.CreateIndex(
                name: "IX_components_is_active",
                schema: "modules",
                table: "components",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_module_categories_parent_id",
                schema: "modules",
                table: "module_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_storage_connections_is_active",
                schema: "modules",
                table: "storage_connections",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assembly_parts",
                schema: "modules");

            migrationBuilder.DropTable(
                name: "storage_connections",
                schema: "modules");

            migrationBuilder.DropTable(
                name: "assemblies",
                schema: "modules");

            migrationBuilder.DropTable(
                name: "components",
                schema: "modules");

            migrationBuilder.DropTable(
                name: "module_categories",
                schema: "modules");
        }
    }
}
