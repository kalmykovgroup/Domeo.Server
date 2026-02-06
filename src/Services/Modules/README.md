# Modules Service

**Порт:** 5008
**Gateway префикс:** `/api/modules`, `/api/assemblies`, `/api/module-categories`
**Описание:** Каталог модулей мебели — категории, сборочные единицы (assemblies), компоненты, их части и файловое хранилище GLB.

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

---

## Типы данных (Value Objects)

### ComponentParams (полиморфный, дискриминатор `type`)

**PanelParams:**
```json
{
  "type": "panel",
  "thickness": 16.0
}
```

**GlbParams:**
```json
{
  "type": "glb",
  "glbUrl": "https://bucket.storage.yandexcloud.net/glb/file.glb",
  "scale": 1.0
}
```

### DynamicSize
```json
{
  "source": "parentWidth | parentDepth | parentHeight | fixed",
  "offset": -32.0,
  "fixedValue": 100.0
}
```

### Placement
```json
{
  "anchorX": "start | center | end",
  "anchorY": "start | center | end",
  "anchorZ": "start | center | end",
  "offsetX": 0.0,
  "offsetY": 0.0,
  "offsetZ": 0.0,
  "rotationX": 0.0,
  "rotationY": 0.0,
  "rotationZ": 0.0
}
```

### Cutout
```json
{
  "anchor": "topLeft | topRight | bottomLeft | bottomRight",
  "width": 50.0,
  "height": 30.0
}
```

### Dimensions
```json
{
  "width": 600.0,
  "depth": 560.0,
  "height": 720.0
}
```

### Constraints
```json
{
  "widthMin": 300.0,
  "widthMax": 900.0,
  "heightMin": 600.0,
  "heightMax": 900.0,
  "depthMin": 400.0,
  "depthMax": 600.0
}
```

### Construction
```json
{
  "panelThickness": 16.0,
  "backPanelThickness": 4.0,
  "facadeThickness": 18.0,
  "facadeGap": 2.0,
  "facadeOffset": 0.0,
  "shelfSideGap": 0.0,
  "shelfRearInset": 0.0,
  "shelfFrontInset": 0.0
}
```

### Перечисления (enum, передаются как строки)

**PartRole:** `left`, `right`, `top`, `bottom`, `back`, `shelf`, `divider`, `facade`, `hinge`, `handle`, `leg`, `drawerSlide`

**DimensionSource:** `parentWidth`, `parentDepth`, `parentHeight`, `fixed`

**AnchorOrigin:** `start`, `center`, `end`

**CutoutAnchor:** `topLeft`, `topRight`, `bottomLeft`, `bottomRight`

---

## Categories

### GET /categories

Список категорий модулей (дерево с вложенными children).

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Query-параметры:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|:------------:|----------|
| `activeOnly` | bool | - | Только активные |

**Ответ:** `ApiResponse<List<ModuleCategoryDto>>`

```json
{
  "success": true,
  "data": [
    {
      "id": "base",
      "parentId": null,
      "name": "Нижние модули",
      "description": "Напольные кухонные модули",
      "orderIndex": 1,
      "isActive": true,
      "children": [
        {
          "id": "base_single_door",
          "parentId": "base",
          "name": "Однодверный",
          "description": null,
          "orderIndex": 1,
          "isActive": true,
          "children": []
        }
      ]
    }
  ]
}
```

---

## Components

### GET /components

Список компонентов с фильтрацией.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Query-параметры:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|:------------:|----------|
| `tag` | string | - | Фильтр по тегу |
| `activeOnly` | bool | - | Только активные |

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
      "isActive": true,
      "createdAt": "2026-01-15T10:30:00Z"
    }
  ]
}
```

---

### GET /components/{id}

Получение компонента по ID.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | Guid | ID компонента |

**Ответ:** `ApiResponse<ComponentDto>`

```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Стенка",
    "tags": ["panel", "wall"],
    "params": { "type": "panel", "thickness": 16.0 },
    "isActive": true,
    "createdAt": "2026-01-15T10:30:00Z"
  }
}
```

**404:**
```json
{ "success": false, "errors": ["Component 550e8400-... not found"] }
```

---

### POST /components

Создать новый компонент.

**Авторизация:** `catalogAdmin`, `systemAdmin`

**Тело запроса:** `CreateComponentRequest` (JSON)

```json
{
  "name": "Новый компонент",
  "params": { "type": "panel", "thickness": 16.0 },
  "tags": ["panel", "custom"]
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `name` | string | + | Название |
| `params` | ComponentParams | - | Параметры (PanelParams или GlbParams) |
| `tags` | string[] | - | Теги для фильтрации |

**Ответ (201):** `ApiResponse<ComponentDto>`

```json
{
  "success": true,
  "data": {
    "id": "новый-guid",
    "name": "Новый компонент",
    "tags": ["panel", "custom"],
    "params": { "type": "panel", "thickness": 16.0 },
    "isActive": true,
    "createdAt": "2026-02-06T12:00:00Z"
  }
}
```

---

### PUT /components/{id}

Обновить компонент.

**Авторизация:** `catalogAdmin`, `systemAdmin`

**Тело запроса:** `UpdateComponentRequest` (JSON)

```json
{
  "name": "Обновлённое название",
  "params": { "type": "glb", "glbUrl": "https://...", "scale": 1.0 },
  "tags": ["glb", "model"]
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `name` | string | + | Название |
| `params` | ComponentParams | - | Параметры |
| `tags` | string[] | - | Теги (если null — не меняются) |

**Ответ:** `ApiResponse<ComponentDto>`

---

### DELETE /components/{id}

Удалить компонент.

**Авторизация:** `catalogAdmin`, `systemAdmin`

**Ответ:** `ApiResponse`

```json
{ "success": true, "message": "Component deleted" }
```

---

### POST /components/{id}/glb

Загрузить GLB-файл для компонента. Автоматически обновляет `params` на `GlbParams`.

**Авторизация:** `catalogAdmin`, `systemAdmin`

**Тело запроса:** `multipart/form-data`

| Поле | Тип | Описание |
|------|-----|----------|
| `file` | IFormFile | Файл `.glb` |

**Ответ:** `ApiResponse<ComponentDto>`

```json
{
  "success": true,
  "data": {
    "id": "550e8400-...",
    "name": "3D Ручка",
    "tags": ["glb", "handle"],
    "params": {
      "type": "glb",
      "glbUrl": "https://bucket.storage.yandexcloud.net/glb/550e8400-..._handle.glb",
      "scale": 1.0
    },
    "isActive": true,
    "createdAt": "2026-01-15T10:30:00Z"
  }
}
```

**Ошибки:**
- `400` — файл пустой или не `.glb`
- `404` — компонент не найден

---

## Assemblies

### GET /assemblies

Список сборочных единиц. Если указаны `page`+`limit` — пагинация, иначе полный список.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Query-параметры:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|:------------:|----------|
| `categoryId` | string | - | Фильтр по категории |
| `activeOnly` | bool | - | Только активные |
| `search` | string | - | Поиск по названию |
| `page` | int | - | Номер страницы (с 1) |
| `limit` | int | - | Размер страницы |

**Ответ без пагинации:** `ApiResponse<List<AssemblyDto>>`

**Ответ с пагинацией:** `ApiResponse<PaginatedResponse<AssemblyDto>>`

```json
{
  "success": true,
  "data": {
    "total": 42,
    "page": 1,
    "pageSize": 10,
    "items": [
      {
        "id": "guid",
        "categoryId": "base_single_door",
        "type": "base-600-single",
        "name": "Нижний 600 однодверный",
        "dimensions": { "width": 600.0, "depth": 560.0, "height": 720.0 },
        "constraints": {
          "widthMin": 300.0, "widthMax": 600.0,
          "heightMin": 720.0, "heightMax": 720.0,
          "depthMin": 560.0, "depthMax": 560.0
        },
        "construction": {
          "panelThickness": 16.0,
          "backPanelThickness": 4.0,
          "facadeThickness": 18.0,
          "facadeGap": 2.0,
          "facadeOffset": 0.0,
          "shelfSideGap": 0.0,
          "shelfRearInset": 0.0,
          "shelfFrontInset": 0.0
        },
        "isActive": true,
        "createdAt": "2026-01-15T10:30:00Z",
        "parts": [
          {
            "id": "guid",
            "assemblyId": "guid",
            "componentId": "guid",
            "role": "left",
            "length": { "source": "parentHeight", "offset": 0.0, "fixedValue": null },
            "width": { "source": "parentDepth", "offset": 0.0, "fixedValue": null },
            "placement": {
              "anchorX": "start", "anchorY": "start", "anchorZ": "start",
              "offsetX": 0.0, "offsetY": 0.0, "offsetZ": 0.0,
              "rotationX": 0.0, "rotationY": 0.0, "rotationZ": 0.0
            },
            "cutouts": [
              { "anchor": "topLeft", "width": 50.0, "height": 30.0 }
            ],
            "quantity": 1,
            "quantityFormula": null,
            "sortOrder": 0,
            "component": {
              "id": "guid",
              "name": "Стенка",
              "tags": ["panel", "wall"],
              "params": { "type": "panel", "thickness": 16.0 },
              "isActive": true,
              "createdAt": "2026-01-15T10:30:00Z"
            }
          }
        ]
      }
    ]
  }
}
```

---

## Assembly Parts

### POST /assemblies/{assemblyId}/parts

Добавить часть к сборке.

**Авторизация:** `catalogAdmin`, `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `assemblyId` | Guid | ID сборки |

**Тело запроса:** `CreateAssemblyPartRequest` (JSON)

```json
{
  "componentId": "550e8400-e29b-41d4-a716-446655440000",
  "role": "shelf",
  "placement": {
    "anchorX": "start",
    "anchorY": "center",
    "anchorZ": "start",
    "offsetX": 16.0,
    "offsetY": 0.0,
    "offsetZ": 0.0,
    "rotationX": 0.0,
    "rotationY": 0.0,
    "rotationZ": 0.0
  },
  "length": { "source": "parentWidth", "offset": -32.0, "fixedValue": null },
  "width": { "source": "parentDepth", "offset": 0.0, "fixedValue": null },
  "cutouts": null,
  "quantity": 1,
  "quantityFormula": null,
  "sortOrder": 5
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `componentId` | Guid | + | ID компонента |
| `role` | PartRole | + | Роль детали в сборке |
| `placement` | Placement | + | Позиционирование |
| `length` | DynamicSize | - | Длина (параметрическая) |
| `width` | DynamicSize | - | Ширина (параметрическая) |
| `cutouts` | Cutout[] | - | Вырезы в детали |
| `quantity` | int | + | Количество |
| `quantityFormula` | string | - | Формула количества |
| `sortOrder` | int | + | Порядок сортировки |

**Ответ (201):** `ApiResponse<AssemblyPartDto>`

```json
{
  "success": true,
  "data": {
    "id": "новый-guid",
    "assemblyId": "guid-сборки",
    "componentId": "guid-компонента",
    "role": "shelf",
    "length": { "source": "parentWidth", "offset": -32.0, "fixedValue": null },
    "width": { "source": "parentDepth", "offset": 0.0, "fixedValue": null },
    "placement": { "anchorX": "start", "...": "..." },
    "cutouts": null,
    "quantity": 1,
    "quantityFormula": null,
    "sortOrder": 5,
    "component": {
      "id": "guid",
      "name": "Полка",
      "tags": ["panel", "shelf"],
      "params": { "type": "panel", "thickness": 16.0 },
      "isActive": true,
      "createdAt": "2026-01-15T10:30:00Z"
    }
  }
}
```

---

### PUT /parts/{id}

Обновить часть сборки.

**Авторизация:** `catalogAdmin`, `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | Guid | ID части |

**Тело запроса:** `UpdateAssemblyPartRequest` (JSON)

```json
{
  "componentId": "550e8400-e29b-41d4-a716-446655440000",
  "role": "shelf",
  "placement": { "anchorX": "start", "anchorY": "center", "anchorZ": "start", "offsetX": 16.0, "offsetY": 0.0, "offsetZ": 0.0, "rotationX": 0.0, "rotationY": 0.0, "rotationZ": 0.0 },
  "length": { "source": "parentWidth", "offset": -32.0, "fixedValue": null },
  "width": { "source": "parentDepth", "offset": 0.0, "fixedValue": null },
  "cutouts": [{ "anchor": "topLeft", "width": 50.0, "height": 30.0 }],
  "quantity": 2,
  "quantityFormula": null,
  "sortOrder": 5
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `componentId` | Guid | + | ID компонента |
| `role` | PartRole | + | Роль детали |
| `placement` | Placement | + | Позиционирование |
| `length` | DynamicSize | - | Длина |
| `width` | DynamicSize | - | Ширина |
| `cutouts` | Cutout[] | - | Вырезы |
| `quantity` | int | + | Количество |
| `quantityFormula` | string | - | Формула количества |
| `sortOrder` | int | + | Порядок сортировки |

**Ответ:** `ApiResponse<AssemblyPartDto>`

---

### DELETE /parts/{id}

Удалить часть сборки.

**Авторизация:** `catalogAdmin`, `systemAdmin`

**Ответ:** `ApiResponse`

```json
{ "success": true, "message": "Part deleted" }
```

---

## Storage

Управление подключениями к файловому хранилищу. Поддерживается S3-совместимое хранилище (Yandex Cloud).

### Логика хранения GLB-файлов

1. Если есть активное S3-подключение (`isActive: true`) — файлы загружаются в S3
2. Если нет — файлы сохраняются локально в `wwwroot/uploads/glb/`
3. Эндпоинт `/storage/migrate` переносит локальные файлы в S3 и обновляет URL в компонентах

---

### GET /storage/connections

Список всех подключений к хранилищу.

**Авторизация:** `systemAdmin`

**Ответ:** `ApiResponse<List<StorageConnectionDto>>`

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

---

### POST /storage/connections

Создать подключение к хранилищу. Создаётся неактивным (`isActive: false`).

**Авторизация:** `systemAdmin`

**Тело запроса:** `CreateStorageConnectionRequest` (JSON)

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

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `name` | string | + | Название подключения |
| `endpoint` | string | + | S3 endpoint |
| `bucket` | string | + | Имя бакета |
| `region` | string | + | Регион |
| `accessKey` | string | + | Ключ доступа |
| `secretKey` | string | + | Секретный ключ |

**Ответ (201):** `ApiResponse<StorageConnectionDto>`

```json
{
  "success": true,
  "data": {
    "id": "новый-guid",
    "name": "Yandex Cloud Production",
    "type": "s3",
    "endpoint": "storage.yandexcloud.net",
    "bucket": "domeo-glb",
    "region": "ru-central1",
    "isActive": false,
    "createdAt": "2026-02-06T12:00:00Z",
    "updatedAt": null
  }
}
```

---

### PUT /storage/connections/{id}

Обновить подключение. Можно активировать/деактивировать.

**Авторизация:** `systemAdmin`

**Тело запроса:** `UpdateStorageConnectionRequest` (JSON)

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

| Поле | Тип | Обязательный | Описание |
|------|-----|:------------:|----------|
| `name` | string | + | Название |
| `endpoint` | string | + | S3 endpoint |
| `bucket` | string | + | Имя бакета |
| `region` | string | + | Регион |
| `accessKey` | string | + | Ключ доступа |
| `secretKey` | string | + | Секретный ключ |
| `isActive` | bool | + | Активно ли подключение |

**Ответ:** `ApiResponse<StorageConnectionDto>`

---

### DELETE /storage/connections/{id}

Удалить подключение.

**Авторизация:** `systemAdmin`

**Ответ:** `ApiResponse`

```json
{ "success": true, "message": "Storage connection deleted" }
```

---

### POST /storage/connections/{id}/test

Проверить подключение (загружает и удаляет тестовый файл).

**Авторизация:** `systemAdmin`

**Тело запроса:** нет

**Ответ:** `ApiResponse<bool>`

```json
{ "success": true, "data": true, "message": "Connection successful" }
```

```json
{ "success": true, "data": false, "message": "Connection failed" }
```

---

### POST /storage/migrate

Перенести локальные GLB-файлы из `wwwroot/uploads/glb/` в активное S3-хранилище. Обновляет URL в компонентах.

**Авторизация:** `systemAdmin`

**Тело запроса:** нет

**Ответ:** `ApiResponse<int>` — количество перенесённых файлов

```json
{ "success": true, "data": 5, "message": "Migrated 5 files" }
```

**Ошибка (если нет активного S3):**
```json
{ "success": false, "errors": ["No active S3 storage connection found"] }
```

---

## Gateway маршруты

| Gateway путь | Сервис | Преобразование |
|-------------|--------|----------------|
| `/api/modules/{**catch-all}` | Modules (5008) | Удаление префикса `/api/modules` |
| `/api/assemblies/{**catch-all}` | Modules (5008) | Перезапись на `/assemblies/{**catch-all}` |
| `/api/module-categories/{**catch-all}` | Modules (5008) | Перезапись на `/categories/{**catch-all}` |

**Примеры через Gateway:**
- `GET /api/modules/components` → `GET /components`
- `POST /api/modules/components` → `POST /components`
- `GET /api/assemblies?categoryId=base` → `GET /assemblies?categoryId=base`
- `POST /api/assemblies/{id}/parts` → `POST /assemblies/{id}/parts`
- `PUT /api/modules/parts/{id}` → `PUT /parts/{id}`
- `POST /api/modules/storage/connections` → `POST /storage/connections`
- `GET /api/module-categories` → `GET /categories`
