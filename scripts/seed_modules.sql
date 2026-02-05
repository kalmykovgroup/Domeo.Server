-- Clear modules schema
DELETE FROM modules.module_hardware;
DELETE FROM modules.hardware;
DELETE FROM modules.module_types;
DELETE FROM modules.module_categories;

-- Module categories (19)
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('base', NULL, 'Нижние шкафы', 'Шкафы для нижней зоны кухни', 1, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('base_single_door', 'base', 'Однодверный', 'Шкаф с одной распашной дверью', 1, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('base_double_door', 'base', 'Двудверный', 'Шкаф с двумя распашными дверями', 2, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('base_triple_door', 'base', 'Трёхдверный', 'Шкаф с тремя распашными дверями', 3, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('base_with_drawers', 'base', 'С ящиками', 'Шкаф с выдвижными ящиками', 4, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('base_corner', 'base', 'Угловой', 'Угловой шкаф (L-образный, диагональный, глухой)', 5, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('base_for_appliance', 'base', 'Под технику', 'Шкаф под встроенную технику', 6, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('wall', NULL, 'Верхние шкафы', 'Шкафы для верхней зоны кухни', 2, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('wall_single_door', 'wall', 'Однодверный', 'Шкаф с одной распашной дверью', 1, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('wall_double_door', 'wall', 'Двудверный', 'Шкаф с двумя распашными дверями', 2, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('wall_open', 'wall', 'Открытый', 'Шкаф без дверей (открытые полки)', 3, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('wall_corner', 'wall', 'Угловой', 'Угловой верхний шкаф', 4, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('mezzanine', NULL, 'Антресоль', 'Шкафы-антресоли под потолком', 3, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('mezzanine_single_door', 'mezzanine', 'Однодверный', 'Антресоль с одной дверью', 1, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('mezzanine_double_door', 'mezzanine', 'Двудверный', 'Антресоль с двумя дверями', 2, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('tall', NULL, 'Высокие шкафы', 'Высокие шкафы-пеналы', 4, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('tall_single_door', 'tall', 'Однодверный', 'Пенал с одной дверью', 1, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('tall_double_door', 'tall', 'Двудверный', 'Пенал с двумя дверями', 2, TRUE);
INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES ('tall_for_appliance', 'tall', 'Под духовку', 'Пенал под встроенную духовку', 3, TRUE);

-- Module types (21)
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (1, 'base_single_door', 'base-single-door', 'Нижний однодверный', 450, 300, 600, 720, 720, 720, 560, 500, 600, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (2, 'base_double_door', 'base-double-door', 'Нижний двудверный', 800, 600, 1200, 720, 720, 720, 560, 500, 600, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (3, 'base_with_drawers', 'base-with-drawers', 'Нижний с ящиками', 600, 400, 900, 720, 720, 720, 560, 500, 600, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (4, 'base_corner', 'base-corner', 'Нижний угловой L-образный', 600, 300, 1200, 720, 720, 720, 900, 800, 1000, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (5, 'base_corner', 'base-corner-diagonal', 'Нижний угловой диагональный', 900, 800, 1000, 720, 720, 720, 900, 800, 1000, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (6, 'base_corner', 'base-corner-blind-left', 'Нижний угловой глухой (левый)', 1000, 900, 1200, 720, 720, 720, 560, 500, 600, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (7, 'base_corner', 'base-corner-blind-right', 'Нижний угловой глухой (правый)', 1000, 900, 1200, 720, 720, 720, 560, 500, 600, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (8, 'base_for_appliance', 'base-appliance', 'Нижний под технику', 600, 450, 600, 720, 720, 720, 560, 550, 600, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (9, 'wall_single_door', 'wall-single-door', 'Верхний однодверный', 450, 300, 600, 720, 550, 920, 320, 280, 350, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (10, 'wall_double_door', 'wall-double-door', 'Верхний двудверный', 800, 600, 1200, 720, 550, 920, 320, 280, 350, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (11, 'wall_open', 'wall-open', 'Верхний открытый', 600, 300, 900, 720, 550, 720, 320, 280, 350, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (12, 'wall_corner', 'wall-corner', 'Верхний угловой L-образный', 600, 300, 1200, 720, 550, 920, 600, 550, 650, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (13, 'wall_corner', 'wall-corner-diagonal', 'Верхний угловой диагональный', 600, 550, 650, 720, 550, 920, 600, 550, 650, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (14, 'wall_corner', 'wall-corner-blind-left', 'Верхний угловой глухой (левый)', 1000, 900, 1200, 720, 550, 920, 320, 300, 400, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (15, 'wall_corner', 'wall-corner-blind-right', 'Верхний угловой глухой (правый)', 1000, 900, 1200, 720, 550, 920, 320, 300, 400, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (16, 'mezzanine_single_door', 'mezzanine-single-door', 'Антресоль однодверная', 600, 300, 900, 360, 250, 550, 320, 280, 350, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (17, 'mezzanine_double_door', 'mezzanine-double-door', 'Антресоль двудверная', 800, 600, 1200, 360, 250, 550, 320, 280, 350, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (18, 'tall_single_door', 'tall-single-door', 'Пенал однодверный', 600, 400, 600, 2100, 2000, 2400, 560, 500, 600, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (19, 'tall_double_door', 'tall-double-door', 'Пенал двудверный', 800, 600, 900, 2100, 2000, 2400, 560, 500, 600, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (20, 'tall_for_appliance', 'tall-appliance-fridge', 'Пенал под холодильник', 600, 560, 600, 2100, 2000, 2200, 560, 560, 560, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);
INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (21, 'tall_for_appliance', 'tall-appliance-oven', 'Пенал под духовку', 600, 560, 600, 2100, 2000, 2200, 560, 560, 560, 16, 4, 18, 2, 0, 2, 20, 10, TRUE);

-- Hardware (11)
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (1, 'hinge', 'Петля Blum Clip Top', 'Blum', 'Clip Top', '/models/hardware/blum-clip-top.glb', '{"brand":"Blum","model":"Clip Top","offsetFromEdge":50,"thickness":12}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (2, 'handle', 'Ручка ИКЕА 128мм хром', 'IKEA', NULL, '/models/hardware/ikea-handle-128.glb', '{"brand":"IKEA","length":128,"finish":"chrome"}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (3, 'leg', 'Ножка регулируемая 100-150мм', NULL, NULL, '/models/hardware/adjustable-leg.glb', '{"minHeight":100,"maxHeight":150,"diameter":40}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (10, 'facade', 'Фасад МДФ одна дверь', NULL, NULL, NULL, '{"facadeType":"swing-single","material":"МДФ","finish":"painted"}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (11, 'facade', 'Фасад МДФ две двери', NULL, NULL, NULL, '{"facadeType":"swing-double","material":"МДФ","finish":"painted"}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (12, 'facade', 'Фасад ящика', NULL, NULL, NULL, '{"facadeType":"drawer","material":"МДФ","finish":"painted"}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (13, 'facade', 'Фасад стеклянный', NULL, NULL, NULL, '{"facadeType":"glass","material":"Стекло + рамка МДФ","finish":"painted"}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (14, 'back-panel', 'Задняя стенка ДВП 4мм без вырезов', NULL, NULL, NULL, '{"material":"ДВП","thickness":4,"color":"#8B4513"}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (15, 'back-panel', 'Задняя стенка ДВП 4мм с вырезами (2 двери)', NULL, NULL, NULL, '{"material":"ДВП","thickness":4,"color":"#8B4513","hingeCutouts":{"doorCount":2,"cutoutWidth":50,"cutoutHeight":25}}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (16, 'mounting-bracket', 'Кронштейн для подвешивания', NULL, NULL, '/models/hardware/wall-mounting-bracket.glb', '{"type":"adjustable","maxLoad":50,"offsetFromTop":50}', TRUE);
INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (17, 'divider', 'Глухая планка', NULL, NULL, NULL, '{"material":"ЛДСП","color":"#A0826D"}', TRUE);

-- Module hardware (36)
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (1, 1, 10, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (2, 2, 11, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (3, 3, 12, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (8, 8, 10, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (9, 9, 10, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (10, 10, 11, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (17, 17, 11, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (18, 18, 10, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (19, 19, 11, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (20, 20, 10, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (21, 21, 10, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (16, 16, 10, 'facade', '1', 'W/2', 'D + facadeOffset + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (100, 1, 15, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (101, 2, 15, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (102, 3, 14, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (103, 8, 14, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (104, 9, 15, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (105, 10, 15, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (106, 11, 14, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (107, 16, 15, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (108, 17, 15, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (109, 18, 15, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (110, 19, 15, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (111, 20, 14, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (112, 21, 14, 'back-panel', '1', 'W/2', 'backPanelGrooveOffset + backPanelThickness/2', 'panelThickness + (H - 2*panelThickness)/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (113, 9, 16, 'mounting-bracket', '2', '["W * 0.15","W * 0.85"]', 'D/2', 'H - 50');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (114, 10, 16, 'mounting-bracket', '2', '["W * 0.15","W * 0.85"]', 'D/2', 'H - 50');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (115, 11, 16, 'mounting-bracket', '2', '["W * 0.15","W * 0.85"]', 'D/2', 'H - 50');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (116, 6, 17, 'divider', '1', 'W - dividerWidth/2 - facadeGap', 'D + facadeGap + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (117, 7, 17, 'divider', '1', 'facadeGap + dividerWidth/2', 'D + facadeGap + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (118, 14, 17, 'divider', '1', 'facadeWidth + dividerWidth/2', 'D + facadeGap + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (119, 15, 17, 'divider', '1', 'W - facadeWidth - dividerWidth/2', 'D + facadeGap + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (128, 6, 10, 'facade', '1', '(W - dividerWidth - 2*facadeGap)/2', 'D + facadeGap + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (129, 7, 10, 'facade', '1', 'W - (W - dividerWidth - 2*facadeGap)/2', 'D + facadeGap + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (130, 14, 10, 'facade', '1', 'facadeWidth/2', 'D + facadeGap + facadeThickness/2', 'H/2');
INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (131, 15, 10, 'facade', '1', 'W - facadeWidth/2', 'D + facadeGap + facadeThickness/2', 'H/2');

