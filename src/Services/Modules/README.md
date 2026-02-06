# Modules Service

**Порт:** 5008
**Gateway префикс:** `/api/modules`, `/api/assemblies`, `/api/module-categories`
**Описание:** Каталог модулей мебели — категории, параметрические сборки (assemblies), компоненты, их части и файловое хранилище GLB.

---

## Общая обёртка ответа

Все ответы обёрнуты в `ApiResponse<T>`:

```json
{
  "success": true,
  "data": T,
  "message": "string | null",
  "errors": []
}
```

При ошибке:

```json
{
  "success": false,
  "data": null,
  "message": "string | null",
  "errors": ["описание ошибки"]
}
```

---

## Авторизация

Аутентификация через заголовки от API Gateway:

| Заголовок | Описание |
|-----------|----------|
| `X-User-Id` | UUID пользователя |
| `X-User-Role` | Роль: `sales`, `designer`, `catalogAdmin`, `systemAdmin` |

- **Чтение** (GET): `sales`, `designer`, `catalogAdmin`, `systemAdmin`
- **Запись** (POST/PUT/DELETE): `catalogAdmin`, `systemAdmin`
- **Storage**: только `systemAdmin`

Тестовый пользователь:
```
X-User-Id: 44444444-4444-4444-4444-444444444444
X-User-Role: systemAdmin
```

---

## Типы данных

### ComponentParams (полиморфный, дискриминатор `type`)

**PanelParams:**
```json
{ "type": "panel", "thickness": 16.0 }
```

**GlbParams:**
```json
{ "type": "glb", "glbUrl": "/uploads/glb/file.glb", "scale": 1.0 }
```

### ParamConstraint

```json
{ "min": 300, "max": 900, "step": null }
```

| Поле | Тип | Описание |
|------|-----|----------|
| `min` | number | Минимальное значение |
| `max` | number | Максимальное значение |
| `step` | number? | Шаг (null = произвольный) |

### ShapeSegment (полиморфный, дискриминатор `type`)

**LineSegment:**
```json
{ "type": "line", "x": "W - 2*t", "y": "0" }
```

**ArcSegment:**
```json
{ "type": "arc", "x": "100", "y": "200", "radius": "50", "largeArc": false, "clockwise": true }
```

---

## Categories

### GET /categories

Список категорий модулей (дерево с вложенными children).

**Query-параметры:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `activeOnly` | bool? | Только активные |

**Ответ:** `ApiResponse<List<ModuleCategoryDto>>`

```json
{
  "success": true,
  "data": [
    {
      "id": "base",
      "parentId": null,
      "name": "Нижние шкафы",
      "description": "Шкафы для нижней зоны кухни",
      "orderIndex": 1,
      "isActive": true,
      "children": [
        {
          "id": "base_single_door",
          "parentId": "base",
          "name": "Однодверный",
          "description": "Шкаф с одной распашной дверью",
          "orderIndex": 1,
          "isActive": true,
          "children": []
        }
      ]
    }
  ]
}
```

### POST /categories

Создать категорию.

**Тело запроса:**

```json
{
  "id": "wall_open",
  "name": "Открытый",
  "parentId": "wall",
  "description": "Открытая полка без дверей",
  "orderIndex": 4
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `id` | string | + | Уникальный slug-идентификатор |
| `name` | string | + | Название |
| `parentId` | string? | - | ID родительской категории |
| `description` | string? | - | Описание |
| `orderIndex` | int | + | Порядок сортировки |

**Ответ (201):** `ApiResponse<ModuleCategoryDto>`

### PUT /categories/{id}

Обновить категорию.

**Тело запроса:**

```json
{ "name": "Новое название", "description": "Описание", "orderIndex": 2 }
```

**Ответ:** `ApiResponse<ModuleCategoryDto>`

### DELETE /categories/{id}

Удалить категорию. **Ответ:** `ApiResponse`

---

## Components

### GET /components

Список компонентов с фильтрацией.

**Query-параметры:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `tag` | string? | Фильтр по тегу |
| `activeOnly` | bool? | Только активные |

**Ответ:** `ApiResponse<List<ComponentDto>>`

```json
{
  "success": true,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Стенка",
      "tags": ["panel", "wall"],
      "params": { "type": "panel", "thickness": 16.0 },
      "color": "#D4A574",
      "isActive": true,
      "createdAt": "2026-02-06T12:00:00Z"
    }
  ]
}
```

### GET /components/{id}

Получение компонента по ID.

**Ответ:** `ApiResponse<ComponentDto>` | **404** если не найден.

### POST /components

Создать компонент.

**Тело запроса:**

```json
{
  "name": "Полка",
  "params": { "type": "panel", "thickness": 16.0 },
  "tags": ["panel", "shelf"],
  "color": "#B8956A"
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `name` | string | + | Название |
| `params` | ComponentParams? | - | PanelParams или GlbParams |
| `tags` | string[]? | - | Теги для фильтрации |
| `color` | string? | - | HEX-цвет для визуализации (например `#D4A574`) |

**Ответ (201):** `ApiResponse<ComponentDto>`

### PUT /components/{id}

Обновить компонент.

**Тело запроса:**

```json
{
  "name": "Обновлённое название",
  "params": { "type": "panel", "thickness": 18.0 },
  "tags": ["panel", "custom"],
  "color": "#C89660"
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `name` | string | + | Название |
| `params` | ComponentParams? | - | Параметры |
| `tags` | string[]? | - | Теги (null = не менять) |
| `color` | string? | - | HEX-цвет (null = без цвета) |

**Ответ:** `ApiResponse<ComponentDto>`

### DELETE /components/{id}

Удалить компонент. **Ответ:** `ApiResponse`

### POST /components/{id}/glb

Загрузить GLB-файл для компонента. Автоматически обновляет `params` на `GlbParams`.

**Тело запроса:** `multipart/form-data`

| Поле | Тип | Описание |
|------|-----|----------|
| `file` | IFormFile | Файл `.glb` |

**Ответ:** `ApiResponse<ComponentDto>`

**Ошибки:** `400` — файл пустой или не `.glb`; `404` — компонент не найден.

---

## Assemblies

### GET /assemblies

Список сборок. Если указаны `page`+`limit` — пагинация, иначе полный список.

**Query-параметры:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `categoryId` | string? | Фильтр по категории |
| `activeOnly` | bool? | Только активные |
| `search` | string? | Поиск по названию |
| `page` | int? | Номер страницы (с 1) |
| `limit` | int? | Размер страницы |

**Ответ без пагинации:**

```json
{
  "success": true,
  "data": {
    "items": [ AssemblyDto, ... ]
  }
}
```

**Ответ с пагинацией:**

```json
{
  "success": true,
  "data": {
    "items": [ AssemblyDto, ... ],
    "total": 13,
    "page": 1,
    "limit": 10
  }
}
```

**AssemblyDto:**

```json
{
  "id": "guid",
  "categoryId": "base_single_door",
  "type": "base-single-door",
  "name": "Нижняя база однодверный",
  "parameters": {
    "W": 450, "H": 720, "D": 560,
    "t": 16, "tBack": 4, "facadeGap": 2,
    "shelfSideGap": 2, "shelfRearInset": 20, "shelfFrontInset": 10
  },
  "paramConstraints": {
    "W": { "min": 300, "max": 600, "step": null },
    "H": { "min": 720, "max": 720, "step": null },
    "D": { "min": 500, "max": 600, "step": null }
  },
  "isActive": true,
  "createdAt": "2026-02-06T12:00:00Z",
  "parts": [
    {
      "id": "guid",
      "assemblyId": "guid",
      "componentId": "guid",
      "lengthExpr": null,
      "widthExpr": null,
      "x": "t", "y": "0", "z": "0",
      "rotationX": 0, "rotationY": -90, "rotationZ": 0,
      "condition": null,
      "shape": [
        {"type":"line","x":"0","y":"0"},
        {"type":"line","x":"D","y":"0"},
        {"type":"line","x":"D","y":"H"},
        {"type":"line","x":"0","y":"H"}
      ],
      "quantity": 1,
      "quantityFormula": null,
      "sortOrder": 0,
      "component": {
        "id": "guid",
        "name": "Стенка",
        "tags": ["panel", "wall"],
        "params": { "type": "panel", "thickness": 16.0 },
        "color": "#D4A574",
        "isActive": true,
        "createdAt": "2026-02-06T12:00:00Z"
      }
    }
  ]
}
```

### POST /assemblies

Создать сборку.

**Тело запроса:**

```json
{
  "categoryId": "base_single_door",
  "type": "base-single-door-custom",
  "name": "Кастомный нижний однодверный",
  "parameters": { "W": 500, "H": 720, "D": 560, "t": 16 },
  "paramConstraints": {
    "W": { "min": 300, "max": 900, "step": null }
  }
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `categoryId` | string | + | ID категории |
| `type` | string | + | Уникальный тип (slug) |
| `name` | string | + | Название |
| `parameters` | Record<string, number> | + | Словарь параметров с дефолтами |
| `paramConstraints` | Record<string, ParamConstraint>? | - | Ограничения параметров |

**Ответ (201):** `ApiResponse<AssemblyDto>`

### PUT /assemblies/{id}

Обновить сборку (имя, параметры, ограничения).

**Тело запроса:**

```json
{
  "name": "Обновлённое название",
  "parameters": { "W": 600, "H": 720, "D": 560, "t": 16 },
  "paramConstraints": { "W": { "min": 300, "max": 1200, "step": null } }
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `name` | string | + | Название |
| `parameters` | Record<string, number> | + | Параметры |
| `paramConstraints` | Record<string, ParamConstraint>? | - | Ограничения |

**Ответ:** `ApiResponse<AssemblyDto>`

### DELETE /assemblies/{id}

Удалить сборку. **Ответ:** `ApiResponse`

---

## Assembly Parts

### POST /assemblies/{assemblyId}/parts

Добавить деталь к сборке.

**Тело запроса:**

```json
{
  "componentId": "550e8400-e29b-41d4-a716-446655440000",
  "lengthExpr": null,
  "widthExpr": null,
  "x": "t+shelfSideGap",
  "y": "shelfY+t",
  "z": "shelfFrontInset",
  "rotationX": 90,
  "rotationY": 0,
  "rotationZ": 0,
  "condition": null,
  "shape": [
    {"type":"line","x":"0","y":"0"},
    {"type":"line","x":"W-2*t-2*shelfSideGap","y":"0"},
    {"type":"line","x":"W-2*t-2*shelfSideGap","y":"D-2-shelfRearInset-shelfFrontInset"},
    {"type":"line","x":"0","y":"D-2-shelfRearInset-shelfFrontInset"}
  ],
  "quantity": 1,
  "quantityFormula": null,
  "sortOrder": 5
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `componentId` | Guid | + | ID компонента |
| `lengthExpr` | string? | - | Выражение длины (legacy, может быть null) |
| `widthExpr` | string? | - | Выражение ширины (legacy, может быть null) |
| `x` | string? | - | Выражение позиции X |
| `y` | string? | - | Выражение позиции Y |
| `z` | string? | - | Выражение позиции Z |
| `rotationX` | number | + | Поворот по X (градусы): 90 для горизонтальных |
| `rotationY` | number | + | Поворот по Y (градусы): −90 для боковых стенок |
| `rotationZ` | number | + | Поворот по Z (градусы) |
| `condition` | string? | - | Условие включения (например `"W > 800"`) |
| `shape` | ShapeSegment[] | + | 2D-контур (всегда заполнен) |
| `quantity` | int | + | Количество |
| `quantityFormula` | string? | - | Формула количества |
| `sortOrder` | int | + | Порядок сортировки |

**Ответ (201):** `ApiResponse<AssemblyPartDto>`

### PUT /parts/{id}

Обновить деталь. Тело запроса — те же поля что и при создании.

**Ответ:** `ApiResponse<AssemblyPartDto>`

### DELETE /parts/{id}

Удалить деталь. **Ответ:** `ApiResponse`

---

## Storage

Управление подключениями к файловому хранилищу (S3-совместимое).

**Логика хранения GLB:** если есть активное S3-подключение — файлы идут в S3, иначе — локально в `wwwroot/uploads/glb/`.

### GET /storage/connections

Список подключений. **Ответ:** `ApiResponse<List<StorageConnectionDto>>`

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Yandex Cloud Production",
      "type": "s3",
      "endpoint": "storage.yandexcloud.net",
      "bucket": "domeo-glb",
      "region": "ru-central1",
      "isActive": true,
      "createdAt": "2026-02-01T10:00:00Z",
      "updatedAt": "2026-02-05T15:30:00Z"
    }
  ]
}
```

### POST /storage/connections

Создать подключение (создаётся неактивным).

**Тело запроса:**

```json
{
  "name": "Yandex Cloud Production",
  "endpoint": "storage.yandexcloud.net",
  "bucket": "domeo-glb",
  "region": "ru-central1",
  "accessKey": "YCAJExxxxxxxxxx",
  "secretKey": "YCPxxxxxxxxxxxxxxxxxx"
}
```

**Ответ (201):** `ApiResponse<StorageConnectionDto>`

### PUT /storage/connections/{id}

Обновить подключение. Можно активировать/деактивировать.

**Тело запроса:**

```json
{
  "name": "Yandex Cloud Production",
  "endpoint": "storage.yandexcloud.net",
  "bucket": "domeo-glb-v2",
  "region": "ru-central1",
  "accessKey": "YCAJExxxxxxxxxx",
  "secretKey": "YCPxxxxxxxxxxxxxxxxxx",
  "isActive": true
}
```

**Ответ:** `ApiResponse<StorageConnectionDto>`

### DELETE /storage/connections/{id}

Удалить подключение. **Ответ:** `ApiResponse`

### POST /storage/connections/{id}/test

Проверить подключение (загружает и удаляет тестовый файл).

**Ответ:** `ApiResponse<bool>`

```json
{ "success": true, "data": true, "message": "Connection successful" }
```

### POST /storage/migrate

Перенести локальные GLB-файлы в активное S3-хранилище.

**Ответ:** `ApiResponse<int>` — количество перенесённых файлов.

**Ошибка:** `{ "success": false, "errors": ["No active S3 storage connection found"] }`

---

## Gateway маршруты

| Gateway путь | Сервис | Преобразование |
|-------------|--------|----------------|
| `/api/modules/{**catch-all}` | Modules (5008) | Удаление префикса `/api/modules` |
| `/api/assemblies/{**catch-all}` | Modules (5008) | Перезапись на `/assemblies/{**catch-all}` |
| `/api/module-categories/{**catch-all}` | Modules (5008) | Перезапись на `/categories/{**catch-all}` |

**Примеры через Gateway:**
- `GET /api/modules/components` → `GET /components`
- `POST /api/modules/components/{id}/glb` → `POST /components/{id}/glb`
- `GET /api/assemblies?categoryId=base` → `GET /assemblies?categoryId=base`
- `POST /api/assemblies/{id}/parts` → `POST /assemblies/{id}/parts`
- `PUT /api/modules/parts/{id}` → `PUT /parts/{id}`
- `POST /api/modules/storage/connections` → `POST /storage/connections`
- `GET /api/module-categories` → `GET /categories`

---

# Параметрическая 3D-модель — спецификация для фронтенда

## Концепция

Бэкенд хранит **шаблоны мебели** (Assembly) в виде:
- **Словарь параметров** с дефолтными значениями: `{ W: 450, H: 720, D: 560, t: 16, ... }`
- **Список деталей** (Part), каждая из которых описана **строковыми математическими выражениями**

**Бэкенд НЕ вычисляет выражения.** Вся математика и построение 3D — ответственность фронтенда.

Когда пользователь меняет параметр (например, W: 450 → 600), фронтенд:
1. Обновляет словарь параметров
2. Заново вычисляет ВСЕ выражения для каждой детали
3. Перестраивает 3D-геометрию

---

## Получение данных

### Endpoint
```
GET /api/modules/assemblies
```

Ответ — массив `AssemblyDto`, каждый содержит полный набор данных для генерации:

```typescript
interface AssemblyDto {
  id: string;
  categoryId: string;
  type: string;                                           // "base-single-door", "wall-corner-diagonal", ...
  name: string;
  parameters: Record<string, number>;                     // ← ВСЕ переменные с дефолтами
  paramConstraints: Record<string, ParamConstraint> | null; // ← ограничения для регулируемых
  isActive: boolean;
  createdAt: string;
  parts: AssemblyPartDto[];                               // ← детали для построения 3D
}

interface ParamConstraint {
  min: number;    // минимум
  max: number;    // максимум
  step: number | null;  // шаг (null = произвольный)
}

interface AssemblyPartDto {
  id: string;
  assemblyId: string;
  componentId: string;
  lengthExpr: string | null;  // выражение длины (legacy, может быть null)
  widthExpr: string | null;   // выражение ширины (legacy, может быть null)
  x: string | null;           // выражение позиции X: "0", "t", "W"
  y: string | null;           // выражение позиции Y
  z: string | null;           // выражение позиции Z
  rotationX: number;          // градусы: 90 для горизонтальных панелей
  rotationY: number;          // градусы: -90 для боковых стенок
  rotationZ: number;          // градусы (обычно 0)
  condition: string | null;   // условие включения: "W > 800" — если false, деталь пропускается
  shape: ShapeSegment[];      // 2D-контур (всегда заполнен, даже для прямоугольников)
  quantity: number;
  quantityFormula: string | null;
  sortOrder: number;
  component: ComponentDto | null;
}

type ShapeSegment = LineSegment | ArcSegment;

interface LineSegment {
  type: "line";
  x: string;   // выражение координаты X
  y: string;   // выражение координаты Y
}

interface ArcSegment {
  type: "arc";
  x: string;
  y: string;
  radius: string;       // выражение радиуса
  largeArc: boolean;
  clockwise: boolean;
}

interface ComponentDto {
  id: string;
  name: string;       // "Стенка", "Задняя стенка", "Полка", "Ручка"
  tags: string[];
  params: PanelParams | GlbParams | null;
  color: string | null; // HEX-цвет для панелей, например "#D4A574". Для GLB — null
  isActive: boolean;
  createdAt: string;
}

interface PanelParams {
  type: "panel";
  thickness: number;  // мм (16 или 4)
}

interface GlbParams {
  type: "glb";
  glbUrl: string;     // путь к .glb
  scale: number;
}
```

---

## Система координат

```
     Y (высота, вверх)
     │
     │
     │
     │
     └──────────── X (ширина, вправо)
    /
   /
  Z (глубина, к зрителю)
```

- **X** — ширина (0 = левая стенка → W = правая стенка)
- **Y** — высота (0 = дно → H = верх)
- **Z** — глубина (0 = задняя стенка → D = передний край)
- Единицы: **миллиметры**
- Начало координат (0,0,0) — **левый нижний задний** угол сборки
- Позиция детали (`x`, `y`, `z`) — **минимальный угол** (left-bottom-back corner)

---

## Параметры сборки

Каждая сборка хранит свободный словарь. Текущие сид-данные используют:

| Параметр | Назначение | Дефолт (пример) |
|----------|-----------|-----------------|
| `W` | Ширина сборки | 450 |
| `H` | Высота сборки | 720 |
| `D` | Глубина сборки | 560 |
| `t` | Толщина основных панелей (стенки, дно, крышка) | 16 |
| `tBack` | Толщина задней стенки | 4 |
| `facadeGap` | Зазор между фасадом и корпусом | 2 |
| `shelfSideGap` | Боковой зазор полки от стенок (с каждой стороны) | 2 |
| `shelfRearInset` | Отступ полки от задней стенки | 20 |
| `shelfFrontInset` | Отступ полки от переднего края | 10 |
| `shelfY` | Y-координата полки (от дна корпуса) | H/3 |
| `armDepth` | Глубина «плеча» L-образного шкафа (только L-shaped) | 304 (wall) / 544 (base) |
| `cutSize` | Размер среза диагонального шкафа (только diagonal) | 300 |

Только параметры, перечисленные в `paramConstraints`, регулируются пользователем. Остальные — конструктивные константы.

---

## Вычисление выражений

### Синтаксис

Выражения — стандартная арифметика: `+`, `-`, `*`, `/`, `(`, `)` и имена переменных из `parameters`.

### Библиотека

Рекомендуется [expr-eval](https://github.com/silentmatt/expr-eval):

```typescript
import { Parser } from 'expr-eval';

const parser = new Parser();

function evaluate(expr: string, params: Record<string, number>): number {
  return parser.evaluate(expr, params);
}

function evaluateCondition(expr: string, params: Record<string, number>): boolean {
  return !!parser.evaluate(expr, params);
}
```

### Таблица примеров (W=450, H=720, D=560, t=16)

| Выражение | Вычисление | Результат |
|-----------|-----------|-----------|
| `"H"` | 720 | 720 |
| `"D"` | 560 | 560 |
| `"0"` | 0 | 0 |
| `"-10"` | -10 | -10 |
| `"t"` | 16 | 16 |
| `"W - t"` | 450 - 16 | 434 |
| `"W - 2*t"` | 450 - 32 | 418 |
| `"H - 2*t"` | 720 - 32 | 688 |
| `"t + shelfSideGap"` | 16 + 2 | 18 |
| `"W - 2*t - 2*shelfSideGap"` | 450 - 32 - 4 | 414 |
| `"D - shelfRearInset - shelfFrontInset"` | 560 - 20 - 10 | 530 |
| `"W - 2*t - 300"` | 450 - 32 - 300 | 118 |
| `"H - 2*t - 50"` | 720 - 32 - 50 | 638 |

---

## Алгоритм генерации 3D-модели

### Общий пайплайн

```typescript
function generateAssembly3D(assembly: AssemblyDto, overrides?: Record<string, number>) {
  // 1. Параметры (дефолтные + пользовательские изменения)
  const params = { ...assembly.parameters, ...overrides };

  // 2. Для каждой детали
  const group = new THREE.Group();

  for (const part of assembly.parts.sort((a, b) => a.sortOrder - b.sortOrder)) {
    // 2a. Проверяем условие включения
    if (part.condition && !evaluateCondition(part.condition, params)) {
      continue;
    }

    // 2b. Количество
    const qty = part.quantityFormula
      ? Math.round(evaluate(part.quantityFormula, params))
      : part.quantity;

    // 2c. Геометрия — единый путь
    let mesh: THREE.Mesh;

    if (part.component?.params?.type === 'glb') {
      mesh = loadGlbModel(part.component.params.glbUrl, part.component.params.scale);
    } else {
      // Все панели: ExtrudeGeometry(shape, {depth: thickness}) + rotation + position
      mesh = createPanel(part, params);
    }

    for (let i = 0; i < qty; i++) {
      group.add(i === 0 ? mesh : mesh.clone());
    }
  }

  return group;
}
```

### Получение толщины

```typescript
function getThickness(component: ComponentDto | null): number {
  if (component?.params?.type === 'panel') return component.params.thickness;
  return 0;
}
```

Реальные данные:
- `Стенка` → 16мм
- `Задняя стенка` → 4мм
- `Полка` → 16мм

---

## Генерация панелей — единый путь через ExtrudeGeometry

**Все детали** (включая прямоугольные) теперь имеют заполненный `shape`. Для прямоугольных панелей shape содержит 4 точки. Ориентация задаётся через `rotationX`/`rotationY`/`rotationZ`.

### Принцип

`ExtrudeGeometry` строит shape в плоскости XY и экструдирует по +Z на `thickness`. Затем `rotation` поворачивает деталь в нужную ориентацию.

### Таблица ориентаций

| Ориентация | Rotation (°) | shape.x→ | shape.y→ | extrude→ |
|------------|-------------|----------|----------|----------|
| Фронтальная (back) | (0, 0, 0) | X | Y | +Z |
| Горизонтальная (top/bottom/shelf) | (90, 0, 0) | X | Z | −Y |
| Боковая (wall) | (0, −90, 0) | Z | Y | −X |

### Позиция

Позиция (`x`, `y`, `z`) — точка, **от которой экструзия идёт «внутрь» корпуса**.

### Как устроен контур (shape)

- `shape` — массив `ShapeSegment[]`, **всегда заполнен** (не null)
- Контур начинается в точке **(0, 0)**
- Каждый `LineSegment` задаёт следующую вершину `(x, y)`
- После последнего сегмента контур **автоматически замыкается** обратно в (0, 0)
- Координаты `x` и `y` в каждом сегменте — **строковые выражения**, которые нужно вычислить через `evaluate()`

### Реализация

```typescript
function createPanel(
  part: AssemblyPartDto,
  params: Record<string, number>
): THREE.Mesh {
  const thickness = getThickness(part.component);

  // 1. Строим 2D-контур из shape
  const shape = new THREE.Shape();

  for (const seg of part.shape) {
    const sx = evaluate(seg.x, params);
    const sy = evaluate(seg.y, params);

    if (seg.type === 'line') {
      shape.lineTo(sx, sy);
    } else if (seg.type === 'arc') {
      const radius = evaluate(seg.radius, params);
      addArcSegment(shape, sx, sy, radius, seg.largeArc, seg.clockwise);
    }
  }

  shape.closePath();

  // 2. Экструдируем на толщину панели
  const geometry = new THREE.ExtrudeGeometry(shape, {
    depth: thickness,
    bevelEnabled: false
  });

  const material = new THREE.MeshStandardMaterial({
    color: part.component?.color ?? '#cccccc'
  });

  // 3. Позиция и ротация из данных
  const mesh = new THREE.Mesh(geometry, material);

  const posX = part.x ? evaluate(part.x, params) : 0;
  const posY = part.y ? evaluate(part.y, params) : 0;
  const posZ = part.z ? evaluate(part.z, params) : 0;

  mesh.position.set(posX, posY, posZ);
  mesh.rotation.set(
    THREE.MathUtils.degToRad(part.rotationX),
    THREE.MathUtils.degToRad(part.rotationY),
    THREE.MathUtils.degToRad(part.rotationZ)
  );

  return mesh;
}
```

---

## Полный пример: «Нижняя база однодверный» (base-single-door)

### Параметры

```json
{
  "W": 450, "H": 720, "D": 560,
  "t": 16, "tBack": 4, "facadeGap": 2,
  "shelfSideGap": 2, "shelfRearInset": 20, "shelfFrontInset": 10,
  "shelfY": 240
}
```

### 6 деталей

#### 0. Левая стенка (wall)

```
shape: Rectangle("D", "H")  → 4 точки: (0,0)→(560,0)→(560,720)→(0,720)
rotation: (0, -90, 0)       → боковая ориентация
position: ("t", "0", "0")   → x=16, y=0, z=0
thickness: 16mm (Стенка, color: #D4A574)
→ ExtrudeGeometry(shape, depth:16) + rotateY(-90°) at (16, 0, 0)
```

#### 1. Правая стенка (wall)

```
shape: Rectangle("D", "H")  → (0,0)→(560,0)→(560,720)→(0,720)
rotation: (0, -90, 0)
position: ("W", "0", "0")   → x=450, y=0, z=0
→ ExtrudeGeometry + rotateY(-90°) at (450, 0, 0)
```

#### 2. Верхняя панель (wall)

```
shape: Rectangle("W - 2*t", "D - 2")  → (0,0)→(418,0)→(418,558)→(0,558)
rotation: (90, 0, 0)              → горизонтальная ориентация
position: ("t", "H", "0")         → x=16, y=720, z=0
→ ExtrudeGeometry + rotateX(90°) at (16, 720, 0)
```

#### 3. Нижняя панель (wall)

```
shape: Rectangle("W - 2*t", "D - 2")
rotation: (90, 0, 0)
position: ("t", "t", "0")  → x=16, y=16, z=0
```

#### 4. Задняя стенка (back) — с вырезами

```
shape: BackPanelWithNotches("W - 2*t", "H - 2*t")
  → 8 точек: (0,0)→(418,0)→(418,638)→(393,638)→(393,688)→(25,688)→(25,638)→(0,638)
rotation: (0, 0, 0)               → фронтальная ориентация
position: ("t", "t", "0")         → x=16, y=16, z=0
thickness: 4mm (Задняя стенка, color: #E8D5B7)
→ ExtrudeGeometry(shape, depth:4) at (16, 16, 0)
```

```
       25              393
       ┌────────────────┐  688 (H-2t)
       │                │
  638  ├──┐          ┌──┤  638 (H-2t-50)
       │  │          │  │
       │  │  418mm   │  │
       │  │ (W-2t)   │  │
       └──┴──────────┴──┘  0
       0                 418
```

#### 5. Полка (shelf)

```
shape: Rectangle("W-2*t-2*shelfSideGap", "D-2-shelfRearInset-shelfFrontInset")
  → (0,0)→(414,0)→(414,530)→(0,530)
rotation: (90, 0, 0)
position: ("t+shelfSideGap", "shelfY+t", "shelfFrontInset")  → x=18, y=256, z=10
thickness: 16mm (Полка, color: #B8956A)
```

### Визуализация сборки (вид спереди)

```
  0   16  18                    432  450
  ├───┼───┼─────────────────────┼────┤

  ┌───┬──────────────────────────┬───┐ 720
  │   │▓▓▓▓▓▓▓▓ top ▓▓▓▓▓▓▓▓▓▓▓│   │
  │   ├──────────────────────────┤   │
  │   │                          │   │
  │ L │                          │ R │
  │   │                          │   │
  │   │  ═══════ shelf ═══════  │   │ ← shelfY+t = 256
  │   │                          │   │
  │   ├──────────────────────────┤   │
  │   │▓▓▓▓▓▓ bottom ▓▓▓▓▓▓▓▓▓▓│   │
  └───┴──────────────────────────┴───┘ 0
```

---

## Пример: Диагональный угловой шкаф (wall-corner-diagonal)

### Параметры
```json
{ "W": 600, "H": 720, "D": 600, "t": 16, "cutSize": 300, "shelfY": 240, ... }
```

### 6 деталей

| # | component | shape | rotation (°) | position |
|---|-----------|-------|-------------|----------|
| 0 | wall | Rectangle("D", "H") | (0, −90, 0) | ("t", "0", "0") |
| 1 | wall | Rectangle("W-3*t-cutSize", "H") | (0, −90, 0) | ("W", "0", "0") |
| 2 | wall | DiagonalPentagon("W-2*t", "W-2*t-cutSize") | (90, 0, 0) | ("t", "H", "0") |
| 3 | wall | DiagonalPentagon("W-2*t", "W-2*t-cutSize") | (90, 0, 0) | ("t", "t", "0") |
| 4 | back | Rectangle("W-2*t", "H-2*t") | (0, 0, 0) | ("t", "t", "0") |
| 5 | shelf | Rectangle(...) | (90, 0, 0) | ("t+shelfSideGap", "shelfY+t", "shelfFrontInset") |

### Top/Bottom shape — пятиугольник (diagonal pentagon)

С W=600, t=16, cutSize=300 → inner=568, cut=268:

```
  (0,568)──────────(268,568)
    │                  ╲
    │                   ╲
    │                  (568,268)
    │                      │
    │        568mm         │
    │                      │
  (0,0)───────────────(568,0)
```

Shape в XY, rotation (90,0,0) → контур ложится в XZ, экструзия по −Y.

---

## Пример: L-образный угловой шкаф (wall-corner-l-shaped)

### Параметры
```json
{ "W": 600, "H": 720, "D": 600, "t": 16, "armDepth": 304, "shelfY": 240, ... }
```

### 8 деталей

| # | component | shape | rotation (°) | position |
|---|-----------|-------|-------------|----------|
| 0 | wall | Rectangle("D", "H") | (0, −90, 0) | ("t", "0", "0") |
| 1 | wall | Rectangle("armDepth", "H") | (0, −90, 0) | ("W", "0", "0") |
| 2 | wall | LShapeHexagon("W-2*t","D-2-2*t","armDepth") | (90, 0, 0) | ("t", "H", "0") |
| 3 | wall | LShapeHexagon("W-2*t","D-2-2*t","armDepth") | (90, 0, 0) | ("t", "t", "0") |
| 4 | back | BackPanelWithNotches("W-2*t","H-2*t") | (0, 0, 0) | ("t", "t", "0") |
| 5 | shelf | Rectangle(...) | (90, 0, 0) | ("t+shelfSideGap", "shelfY+t", "shelfFrontInset") |
| 6 | wall | Rectangle("W-2*t-armDepth","H") | (0, −90, 0) | ("armDepth+2*t", "0", "armDepth") |
| 7 | wall | Rectangle("D-2*t-armDepth","H") | (0, 0, 0) | ("armDepth+t", "0", "armDepth+t") |

### Top/Bottom shape — L-форма (hexagon, 6 точек)

С W=600, D=600, t=16, armDepth=304 → innerW=568, innerD=568:

```
  (0,568)────(304,568)
    │             │
    │             │
    │        (304,304)────────(568,304)
    │                              │
    │                              │
  (0,0)──────────────────────(568,0)
```

Shape в XY, rotation (90,0,0) → контур ложится в XZ, экструзия по −Y.

---

## Реакция на изменение параметров

Когда пользователь двигает слайдер (например, W: 450 → 600):

```typescript
// 1. Валидация по constraints
const constraint = assembly.paramConstraints?.['W'];
if (constraint) {
  newW = Math.max(constraint.min, Math.min(constraint.max, newW));
  if (constraint.step) {
    newW = Math.round(newW / constraint.step) * constraint.step;
  }
}

// 2. Обновляем параметры
const params = { ...assembly.parameters, W: newW };

// 3. Перегенерируем ВСЮ модель
const newModel = generateAssembly3D(assembly, { W: newW });
```

### Что меняется при W: 450 → 600

| # | Деталь | W=450 позиция | W=600 позиция | Что изменилось |
|---|--------|-------------|-------------|---------------|
| 0 | left wall | (16, 0, 0) | (16, 0, 0) | Ничего |
| 1 | right wall | (450, 0, 0) | (600, 0, 0) | Сдвинулась по X |
| 2 | top | (16, 720, 0) | (16, 720, 0) | Shape шире (418→568) |
| 3 | bottom | (16, 16, 0) | (16, 16, 0) | Shape шире |
| 4 | back | (16, 16, 0) | (16, 16, 0) | Контур пересчитан |
| 5 | shelf | (18, 256, 10) | (18, 256, 10) | Shape шире |

---

## Типы сборок — какие shape используются

| Type | Walls shape | Top/Bottom shape | Back shape | Деталей |
|------|------------|-----------------|------------|---------|
| `base-single-door` | Rectangle | Rectangle (4pt) | 8-point notched | 6 |
| `base-double-door` | Rectangle | Rectangle (4pt) | 8-point notched | 6 |
| `wall-single-door` | Rectangle | Rectangle (4pt) | 8-point notched | 6 |
| `wall-double-door` | Rectangle | Rectangle (4pt) | 8-point notched | 6 |
| `mezzanine` | Rectangle | Rectangle (4pt) | 8-point notched | 6 |
| `*-corner-straight-*` | Rectangle | Rectangle (4pt) | 8-point notched | 6 |
| `*-corner-diagonal` | Rectangle (разные размеры) | 5-point pentagon | Rectangle (4pt) | 6 |
| `*-corner-l-shaped` | Rectangle + 2 partition | 6-point L-shape | 8-point notched | 8 |
