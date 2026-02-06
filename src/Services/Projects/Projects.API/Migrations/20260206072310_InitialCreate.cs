using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projects.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "projects");

            migrationBuilder.CreateTable(
                name: "projects",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    questionnaire_data = table.Column<string>(type: "jsonb", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ceiling_height = table.Column<int>(type: "integer", nullable: false, defaultValue: 2700),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.id);
                    table.ForeignKey(
                        name: "FK_rooms_projects_project_id",
                        column: x => x.project_id,
                        principalSchema: "projects",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "room_vertices",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    x = table.Column<double>(type: "double precision", nullable: false),
                    y = table.Column<double>(type: "double precision", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_vertices", x => x.id);
                    table.ForeignKey(
                        name: "FK_room_vertices_rooms_room_id",
                        column: x => x.room_id,
                        principalSchema: "projects",
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "room_edges",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_vertex_id = table.Column<Guid>(type: "uuid", nullable: false),
                    end_vertex_id = table.Column<Guid>(type: "uuid", nullable: false),
                    wall_height = table.Column<int>(type: "integer", nullable: false, defaultValue: 2700),
                    has_window = table.Column<bool>(type: "boolean", nullable: false),
                    has_door = table.Column<bool>(type: "boolean", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_edges", x => x.id);
                    table.ForeignKey(
                        name: "FK_room_edges_room_vertices_end_vertex_id",
                        column: x => x.end_vertex_id,
                        principalSchema: "projects",
                        principalTable: "room_vertices",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_room_edges_room_vertices_start_vertex_id",
                        column: x => x.start_vertex_id,
                        principalSchema: "projects",
                        principalTable: "room_vertices",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_room_edges_rooms_room_id",
                        column: x => x.room_id,
                        principalSchema: "projects",
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zones",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    edge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    start_x = table.Column<double>(type: "double precision", nullable: false),
                    end_x = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zones", x => x.id);
                    table.ForeignKey(
                        name: "FK_zones_room_edges_edge_id",
                        column: x => x.edge_id,
                        principalSchema: "projects",
                        principalTable: "room_edges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cabinets",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    edge_id = table.Column<Guid>(type: "uuid", nullable: true),
                    zone_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assembly_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    placement_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    facade_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    position_x = table.Column<double>(type: "double precision", nullable: false),
                    position_y = table.Column<double>(type: "double precision", nullable: false),
                    rotation = table.Column<double>(type: "double precision", nullable: false),
                    width = table.Column<double>(type: "double precision", nullable: false),
                    height = table.Column<double>(type: "double precision", nullable: false),
                    depth = table.Column<double>(type: "double precision", nullable: false),
                    calculated_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cabinets", x => x.id);
                    table.ForeignKey(
                        name: "FK_cabinets_room_edges_edge_id",
                        column: x => x.edge_id,
                        principalSchema: "projects",
                        principalTable: "room_edges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_cabinets_rooms_room_id",
                        column: x => x.room_id,
                        principalSchema: "projects",
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cabinets_zones_zone_id",
                        column: x => x.zone_id,
                        principalSchema: "projects",
                        principalTable: "zones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "cabinet_hardware_overrides",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cabinet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assembly_part_id = table.Column<Guid>(type: "uuid", nullable: false),
                    component_id = table.Column<Guid>(type: "uuid", nullable: true),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    quantity_formula = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    position_x_formula = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    position_y_formula = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    position_z_formula = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    material_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cabinet_hardware_overrides", x => x.id);
                    table.ForeignKey(
                        name: "FK_cabinet_hardware_overrides_cabinets_cabinet_id",
                        column: x => x.cabinet_id,
                        principalSchema: "projects",
                        principalTable: "cabinets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cabinet_materials",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cabinet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    material_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    material_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cabinet_materials", x => x.id);
                    table.ForeignKey(
                        name: "FK_cabinet_materials_cabinets_cabinet_id",
                        column: x => x.cabinet_id,
                        principalSchema: "projects",
                        principalTable: "cabinets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cabinet_hardware_overrides_assembly_part_id",
                schema: "projects",
                table: "cabinet_hardware_overrides",
                column: "assembly_part_id");

            migrationBuilder.CreateIndex(
                name: "IX_cabinet_hardware_overrides_cabinet_id",
                schema: "projects",
                table: "cabinet_hardware_overrides",
                column: "cabinet_id");

            migrationBuilder.CreateIndex(
                name: "IX_cabinet_hardware_overrides_cabinet_id_assembly_part_id",
                schema: "projects",
                table: "cabinet_hardware_overrides",
                columns: new[] { "cabinet_id", "assembly_part_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cabinet_materials_cabinet_id",
                schema: "projects",
                table: "cabinet_materials",
                column: "cabinet_id");

            migrationBuilder.CreateIndex(
                name: "IX_cabinet_materials_cabinet_id_material_type",
                schema: "projects",
                table: "cabinet_materials",
                columns: new[] { "cabinet_id", "material_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cabinets_assembly_id",
                schema: "projects",
                table: "cabinets",
                column: "assembly_id");

            migrationBuilder.CreateIndex(
                name: "IX_cabinets_edge_id",
                schema: "projects",
                table: "cabinets",
                column: "edge_id");

            migrationBuilder.CreateIndex(
                name: "IX_cabinets_room_id",
                schema: "projects",
                table: "cabinets",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_cabinets_zone_id",
                schema: "projects",
                table: "cabinets",
                column: "zone_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_client_id",
                schema: "projects",
                table: "projects",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_user_id",
                schema: "projects",
                table: "projects",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_room_edges_end_vertex_id",
                schema: "projects",
                table: "room_edges",
                column: "end_vertex_id");

            migrationBuilder.CreateIndex(
                name: "IX_room_edges_room_id",
                schema: "projects",
                table: "room_edges",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_room_edges_start_vertex_id",
                schema: "projects",
                table: "room_edges",
                column: "start_vertex_id");

            migrationBuilder.CreateIndex(
                name: "IX_room_vertices_room_id",
                schema: "projects",
                table: "room_vertices",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_project_id",
                schema: "projects",
                table: "rooms",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_zones_edge_id",
                schema: "projects",
                table: "zones",
                column: "edge_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cabinet_hardware_overrides",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "cabinet_materials",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "cabinets",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "zones",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "room_edges",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "room_vertices",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "rooms",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "projects");
        }
    }
}
