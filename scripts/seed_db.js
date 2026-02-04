#!/usr/bin/env node
/**
 * Seeds the database from db.json via psql in Docker container
 * NOTE: Materials data is NOT seeded here - it comes from MockSupplier.API
 */

const fs = require('fs');
const { execSync } = require('child_process');

// Read JSON data
const jsonPath = 'C:/Users/kalmy/RiderProjects/Domeo/db.json';
console.log('Reading db.json...');
const data = JSON.parse(fs.readFileSync(jsonPath, 'utf8'));

// Helper to escape SQL strings
function esc(val) {
    if (val === null || val === undefined) return 'NULL';
    if (typeof val === 'boolean') return val ? 'TRUE' : 'FALSE';
    if (typeof val === 'number') return val.toString();
    if (typeof val === 'object') return `'${JSON.stringify(val).replace(/'/g, "''")}'`;
    return `'${String(val).replace(/'/g, "''")}'`;
}

// Extract data arrays
const categories = data.categories || [];
const moduleTypes = data.moduleTypes || [];
const hardware = data.hardware || [];
const moduleHardware = data.moduleHardware || [];

// ====== BUILD MODULES SQL ======
console.log('Building SQL for modules schema...');

let modulesSql = `-- Clear modules schema
DELETE FROM modules.module_hardware;
DELETE FROM modules.hardware;
DELETE FROM modules.module_types;
DELETE FROM modules.module_categories;

`;

// Module categories
if (categories.length > 0) {
    modulesSql += `-- Module categories (${categories.length})\n`;
    for (const c of categories) {
        modulesSql += `INSERT INTO modules.module_categories (id, parent_id, name, description, order_index, is_active)
VALUES (${esc(c.id)}, ${esc(c.parentId)}, ${esc(c.name)}, ${esc(c.description)}, ${c.order}, ${esc(c.isActive)});\n`;
    }
    modulesSql += '\n';
}

// Module types
if (moduleTypes.length > 0) {
    modulesSql += `-- Module types (${moduleTypes.length})\n`;
    for (const mt of moduleTypes) {
        const p = mt.params || {};
        const width = p.width || {};
        const height = p.height || {};
        const depth = p.depth || {};
        let categoryId = mt.categoryId;
        if (!categoryId) {
            // Derive from type: "base-single-door" -> "base_single_door"
            categoryId = mt.type.replace(/-/g, '_');
        }
        modulesSql += `INSERT INTO modules.module_types (id, category_id, type, name, width_default, width_min, width_max, height_default, height_min, height_max, depth_default, depth_min, depth_max, panel_thickness, back_panel_thickness, facade_thickness, facade_gap, facade_offset, shelf_side_gap, shelf_rear_inset, shelf_front_inset, is_active)
VALUES (${mt.id}, ${esc(categoryId)}, ${esc(mt.type)}, ${esc(mt.name)}, ${width.default || 600}, ${width.min || 300}, ${width.max || 1200}, ${height.default || 720}, ${height.min || 400}, ${height.max || 900}, ${depth.default || 560}, ${depth.min || 300}, ${depth.max || 600}, ${p.panelThickness?.default || 16}, ${p.backPanelThickness?.default || 4}, ${p.facadeThickness?.default || 18}, ${p.facadeGap?.default || 2}, ${p.facadeOffset?.default || 0}, ${p.shelfSideGap?.default || 2}, ${p.shelfRearInset?.default || 20}, ${p.shelfFrontInset?.default || 10}, TRUE);\n`;
    }
    modulesSql += '\n';
}

// Hardware
if (hardware.length > 0) {
    modulesSql += `-- Hardware (${hardware.length})\n`;
    for (const hw of hardware) {
        const brand = hw.params?.brand || null;
        const model = hw.params?.model || null;
        modulesSql += `INSERT INTO modules.hardware (id, type, name, brand, model, model_url, params, is_active)
VALUES (${hw.id}, ${esc(hw.type)}, ${esc(hw.name)}, ${esc(brand)}, ${esc(model)}, ${esc(hw.modelUrl)}, ${esc(hw.params)}, TRUE);\n`;
    }
    modulesSql += '\n';
}

// Module hardware
if (moduleHardware.length > 0) {
    modulesSql += `-- Module hardware (${moduleHardware.length})\n`;
    for (const mh of moduleHardware) {
        const pos = mh.position || {};
        modulesSql += `INSERT INTO modules.module_hardware (id, module_type_id, hardware_id, role, quantity_formula, position_x_formula, position_y_formula, position_z_formula)
VALUES (${mh.id}, ${mh.moduleTypeId}, ${mh.hardwareId}, ${esc(mh.role)}, ${esc(mh.quantity || '1')}, ${esc(pos.x)}, ${esc(pos.y)}, ${esc(pos.z)});\n`;
    }
    modulesSql += '\n';
}

// Write SQL file
const modulesSqlPath = 'C:/Users/kalmy/RiderProjects/Domeo/scripts/seed_modules.sql';
console.log(`Writing SQL file...`);
fs.writeFileSync(modulesSqlPath, modulesSql, 'utf8');

console.log('\nSQL file created. Stats:');
console.log(`  - Module categories: ${categories.length}`);
console.log(`  - Module types: ${moduleTypes.length}`);
console.log(`  - Hardware: ${hardware.length}`);
console.log(`  - Module hardware: ${moduleHardware.length}`);

// Execute via docker
console.log('\nExecuting SQL in PostgreSQL container...');

try {
    console.log('Seeding modules schema...');
    execSync(`docker cp "${modulesSqlPath.replace(/\\/g, '/')}" domeo-postgres:/tmp/seed_modules.sql`, { stdio: 'inherit', shell: true });
    execSync(`docker exec domeo-postgres psql -U domeo -d domeo -f /tmp/seed_modules.sql`, { stdio: 'inherit', shell: true });
    console.log('Modules schema seeded successfully!');
} catch (error) {
    console.error('Error seeding modules:', error.message);
}

// Seed clients
try {
    console.log('\nSeeding clients...');
    const clientsSql = `
DELETE FROM clients.clients;
INSERT INTO clients.clients (id, name, phone, email, address, notes, user_id, created_at, updated_at) VALUES
  ('a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'Иванов Иван Иванович', '+7 (999) 123-45-67', 'ivanov@example.com', 'г. Москва, ул. Ленина, д. 1, кв. 1', 'Постоянный клиент', '00000000-0000-0000-0000-000000000001', NOW(), NOW()),
  ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Петров Пётр Петрович', '+7 (999) 234-56-78', 'petrov@example.com', 'г. Санкт-Петербург, Невский пр., д. 10', 'Новый клиент', '00000000-0000-0000-0000-000000000001', NOW(), NOW()),
  ('c3d4e5f6-a7b8-9012-cdef-123456789012', 'Сидорова Анна Михайловна', '+7 (999) 345-67-89', 'sidorova@example.com', 'г. Казань, ул. Баумана, д. 5', NULL, '00000000-0000-0000-0000-000000000001', NOW(), NOW()),
  ('d4e5f6a7-b8c9-0123-def0-234567890123', 'ООО "Ремонт Плюс"', '+7 (495) 111-22-33', 'info@remont-plus.ru', 'г. Москва, ул. Строителей, д. 15', 'Корпоративный клиент, скидка 10%', '00000000-0000-0000-0000-000000000001', NOW(), NOW()),
  ('e5f6a7b8-c9d0-1234-ef01-345678901234', 'Козлов Дмитрий Сергеевич', '+7 (999) 456-78-90', 'kozlov@example.com', 'г. Новосибирск, ул. Кирова, д. 20, кв. 15', 'Заказ кухни на дачу', '00000000-0000-0000-0000-000000000001', NOW(), NOW());
`;
    const clientsSqlPath = 'C:/Users/kalmy/RiderProjects/Domeo/scripts/seed_clients.sql';
    fs.writeFileSync(clientsSqlPath, clientsSql, 'utf8');
    execSync(`docker cp "${clientsSqlPath.replace(/\\/g, '/')}" domeo-postgres:/tmp/seed_clients.sql`, { stdio: 'inherit', shell: true });
    execSync(`docker exec domeo-postgres psql -U domeo -d domeo_clients -f /tmp/seed_clients.sql`, { stdio: 'inherit', shell: true });
    console.log('Clients seeded successfully! (5 clients)');
} catch (error) {
    console.error('Error seeding clients:', error.message);
}

console.log('\nDone!');
console.log('\nNOTE: Materials data is not seeded to the database.');
console.log('Materials come from MockSupplier.API which reads from db.json directly.');
