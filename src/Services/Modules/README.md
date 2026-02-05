# Modules Service

**Порт:** 5008
**Gateway префикс:** `/api/modules`, `/api/assemblies`, `/api/module-categories`
**Описание:** Каталог модулей мебели — категории, сборочные единицы (assemblies), компоненты и их части. Все эндпоинты только для чтения.

## Обзор эндпоинтов

### Categories

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/categories` | Список категорий |
| GET | `/categories/tree` | Дерево категорий |

### Assemblies

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/assemblies` | Список сборок (с пагинацией) |
| GET | `/assemblies/count` | Количество сборок |
| GET | `/assemblies/{id}` | Получение сборки с деталями |
| GET | `/assemblies/{id}/parts` | Список частей сборки |

### Components

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/components` | Список компонентов |
| GET | `/components/{id}` | Получение компонента |

---

## Эндпоинты: Categories

### GET /categories

Плоский список категорий модулей.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `activeOnly` | bool | Нет | Только активные категории |

**Ответ:** `ApiResponse<List<ModuleCategoryDto>>`

```json
{
  "id": "string",
  "parentId": "string",
  "name": "string",
  "description": "string",
  "orderIndex": 0,
  "isActive": true
}
```

---

### GET /categories/tree

Иерархическое дерево категорий с вложенными подкатегориями.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `activeOnly` | bool | Нет | Только активные категории |

**Ответ:** `ApiResponse<List<ModuleCategoryTreeDto>>`

```json
{
  "id": "string",
  "parentId": "string",
  "name": "string",
  "description": "string",
  "orderIndex": 0,
  "isActive": true,
  "children": [
    {
      "id": "string",
      "name": "string",
      "children": []
    }
  ]
}
```

---

## Эндпоинты: Assemblies

### GET /assemblies

Список сборочных единиц с фильтрацией. Поддерживает пагинацию.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `categoryId` | string | Нет | Фильтр по категории |
| `activeOnly` | bool | Нет | Только активные |
| `search` | string | Нет | Поисковый запрос |
| `page` | int | Нет | Номер страницы |
| `limit` | int | Нет | Размер страницы |

**Ответ:** `ApiResponse<PaginatedResponse<AssemblyDto>>` или `ApiResponse<List<AssemblyDto>>` (если пагинация не указана)

```json
{
  "id": "guid",
  "categoryId": "string",
  "type": "string",
  "name": "string",
  "dimensions": {
    "width": 0.0,
    "depth": 0.0,
    "height": 0.0
  },
  "constraints": {
    "widthMin": 0.0,
    "widthMax": 0.0,
    "heightMin": 0.0,
    "heightMax": 0.0,
    "depthMin": 0.0,
    "depthMax": 0.0
  },
  "construction": {
    "panelThickness": 0.0,
    "backPanelThickness": 0.0,
    "facadeThickness": 0.0,
    "facadeGap": 0.0,
    "facadeOffset": 0.0,
    "shelfSideGap": 0.0,
    "shelfRearInset": 0.0,
    "shelfFrontInset": 0.0
  },
  "isActive": true,
  "createdAt": "datetime"
}
```

---

### GET /assemblies/count

Количество сборок по заданным фильтрам.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `categoryId` | string | Нет | Фильтр по категории |
| `activeOnly` | bool | Нет | Только активные |
| `search` | string | Нет | Поисковый запрос |

**Ответ:** `ApiResponse<object>`

```json
{
  "success": true,
  "data": {
    "count": 42
  }
}
```

---

### GET /assemblies/{id}

Получение сборки с полным описанием и списком частей.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | Guid | ID сборки |

**Ответ:** `ApiResponse<AssemblyDetailDto>`

Включает все поля `AssemblyDto` плюс:

```json
{
  "parts": [
    {
      "id": "guid",
      "assemblyId": "guid",
      "componentId": "guid",
      "role": "string",
      "length": {
        "source": "ParentWidth|ParentDepth|ParentHeight|Fixed",
        "offset": 0.0,
        "fixedValue": 0.0
      },
      "width": { "source": "...", "offset": 0.0, "fixedValue": 0.0 },
      "placement": {
        "anchorX": "Start|Center|End",
        "anchorY": "Start|Center|End",
        "anchorZ": "Start|Center|End",
        "offsetX": 0.0,
        "offsetY": 0.0,
        "offsetZ": 0.0,
        "rotationX": 0.0,
        "rotationY": 0.0,
        "rotationZ": 0.0
      },
      "quantity": 1,
      "quantityFormula": "string",
      "sortOrder": 0,
      "component": { "id": "guid", "name": "string", "tags": [], "params": null, "isActive": true }
    }
  ]
}
```

---

### GET /assemblies/{id}/parts

Список частей сборки.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Ответ:** `ApiResponse<List<AssemblyPartDto>>`

---

## Эндпоинты: Components

### GET /components

Список компонентов с фильтрацией.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `tag` | string | Нет | Фильтр по тегу |
| `activeOnly` | bool | Нет | Только активные |

**Ответ:** `ApiResponse<List<ComponentDto>>`

```json
{
  "id": "guid",
  "name": "string",
  "tags": ["string"],
  "params": {
    "type": "panel",
    "thickness": 0.0
  },
  "isActive": true,
  "createdAt": "datetime"
}
```

Поле `params` — полиморфный тип:
- **PanelParams:** `{ "type": "panel", "thickness": 16.0 }`
- **GlbParams:** `{ "type": "glb", "glbUrl": "string", "scale": 1.0 }`

---

### GET /components/{id}

Получение компонента по ID.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Ответ:** `ApiResponse<ComponentDto>`

---

## Авторизация

Все эндпоинты требуют одну из ролей: `sales`, `designer`, `catalogAdmin`, `systemAdmin`.

Аутентификация через заголовки `X-User-Id` и `X-User-Role`, передаваемые API Gateway.

---

## Gateway маршруты

| Gateway путь | Сервис | Преобразование |
|-------------|--------|----------------|
| `/api/modules/{**catch-all}` | Modules (5008) | Удаление префикса `/api/modules` |
| `/api/assemblies/{**catch-all}` | Modules (5008) | Перезапись на `/assemblies/{**catch-all}` |
| `/api/module-categories/{**catch-all}` | Modules (5008) | Перезапись на `/categories/{**catch-all}` |

Примеры:
- `GET /api/modules/components` → `GET /components`
- `GET /api/assemblies/count` → `GET /assemblies/count`
- `GET /api/module-categories/tree` → `GET /categories/tree`
