# Projects Service

**Порт:** 5003
**Gateway префикс:** `/api/projects`, `/api/rooms`, `/api/edges`, `/api/cabinets`, `/api/cabinet-hardware-overrides`
**Описание:** Управление проектами, комнатами, геометрией (вершины, рёбра, зоны), шкафами и переопределениями фурнитуры.

## Обзор эндпоинтов

### Projects

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/projects` | Список проектов (с пагинацией) |
| GET | `/projects/{id}` | Получение проекта |
| POST | `/projects` | Создание проекта |
| PUT | `/projects/{id}` | Обновление проекта |
| PUT | `/projects/{id}/status` | Обновление статуса |
| PUT | `/projects/{id}/questionnaire` | Обновление анкеты |
| DELETE | `/projects/{id}` | Удаление проекта |

### Rooms

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/projects/{projectId}/rooms` | Список комнат проекта |
| GET | `/projects/{projectId}/rooms/{roomId}` | Получение комнаты |
| POST | `/projects/{projectId}/rooms` | Создание комнаты |
| PUT | `/projects/{projectId}/rooms/{roomId}` | Обновление комнаты |
| DELETE | `/projects/{projectId}/rooms/{roomId}` | Удаление комнаты |

### Room Vertices

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/rooms/{roomId}/vertices` | Список вершин комнаты |
| POST | `/rooms/{roomId}/vertices` | Создание вершины |
| DELETE | `/rooms/{roomId}/vertices/{vertexId}` | Удаление вершины |

### Room Edges

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/rooms/{roomId}/edges` | Список рёбер комнаты |
| POST | `/rooms/{roomId}/edges` | Создание ребра |
| PUT | `/rooms/{roomId}/edges/{edgeId}` | Обновление ребра |
| DELETE | `/rooms/{roomId}/edges/{edgeId}` | Удаление ребра |

### Zones

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/edges/{edgeId}/zones` | Список зон ребра |
| POST | `/edges/{edgeId}/zones` | Создание зоны |
| PUT | `/edges/{edgeId}/zones/{zoneId}` | Обновление зоны |
| DELETE | `/edges/{edgeId}/zones/{zoneId}` | Удаление зоны |

### Cabinets

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/cabinets/room/{roomId}` | Список шкафов в комнате |
| GET | `/cabinets/{id}` | Получение шкафа |
| POST | `/cabinets` | Создание шкафа |
| PUT | `/cabinets/{id}` | Обновление шкафа |
| DELETE | `/cabinets/{id}` | Удаление шкафа |

### Cabinet Hardware Overrides

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/cabinet-hardware-overrides/cabinet/{cabinetId}` | Список переопределений фурнитуры |
| GET | `/cabinet-hardware-overrides/{id}` | Получение переопределения |
| POST | `/cabinet-hardware-overrides/cabinet/{cabinetId}` | Создание переопределения |
| PUT | `/cabinet-hardware-overrides/{id}` | Обновление переопределения |
| DELETE | `/cabinet-hardware-overrides/{id}` | Удаление переопределения |

---

## Эндпоинты: Projects

### GET /projects

Список проектов с фильтрацией и пагинацией.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `clientId` | Guid | Нет | Фильтр по клиенту |
| `search` | string | Нет | Поисковый запрос |
| `status` | string | Нет | Фильтр по статусу |
| `type` | string | Нет | Фильтр по типу |
| `page` | int | Нет | Номер страницы (по умолчанию 1) |
| `pageSize` | int | Нет | Размер страницы (по умолчанию 20) |
| `sortBy` | string | Нет | Поле сортировки |
| `sortOrder` | string | Нет | Направление сортировки |

**Ответ:** `ApiResponse<PaginatedResponse<ProjectDto>>`

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "name": "string",
        "type": "string",
        "status": "string",
        "clientId": "guid",
        "userId": "guid",
        "notes": "string",
        "questionnaireData": "string",
        "createdAt": "datetime",
        "updatedAt": "datetime",
        "deletedAt": "datetime"
      }
    ],
    "totalCount": 0,
    "page": 1,
    "pageSize": 20
  }
}
```

### GET /projects/{id}

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Ответ:** `ApiResponse<ProjectDto>`

### POST /projects

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Тело запроса:**

```json
{
  "name": "string",
  "type": "string",
  "clientId": "guid",
  "notes": "string"
}
```

**Ответ:** `ApiResponse<IdResponse>`

### PUT /projects/{id}

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Тело запроса:**

```json
{
  "name": "string",
  "notes": "string"
}
```

### PUT /projects/{id}/status

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Тело запроса:**

```json
{
  "status": "string"
}
```

### PUT /projects/{id}/questionnaire

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Тело запроса:**

```json
{
  "questionnaireData": "string"
}
```

### DELETE /projects/{id}

**Авторизация:** `designer`, `catalogAdmin`, `systemAdmin` (роль `sales` **не** имеет доступа)

---

## Эндпоинты: Rooms

### GET /projects/{projectId}/rooms

Список всех комнат проекта.

**Авторизация:** Политика `Permission:projects:read`

**Ответ:** `ApiResponse<List<RoomDto>>`

```json
{
  "id": "guid",
  "projectId": "guid",
  "name": "string",
  "ceilingHeight": 2700,
  "orderIndex": 0,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET /projects/{projectId}/rooms/{roomId}

**Авторизация:** Политика `Permission:projects:read`

**Ответ:** `ApiResponse<RoomDto>`

### POST /projects/{projectId}/rooms

**Авторизация:** Политика `Permission:projects:write`

**Тело запроса:**

```json
{
  "name": "string",
  "ceilingHeight": 2700,
  "orderIndex": 0
}
```

**Ответ:** `ApiResponse<IdResponse>`

### PUT /projects/{projectId}/rooms/{roomId}

**Авторизация:** Политика `Permission:projects:write`

**Тело запроса:**

```json
{
  "name": "string",
  "ceilingHeight": 2700,
  "orderIndex": 0
}
```

### DELETE /projects/{projectId}/rooms/{roomId}

**Авторизация:** Политика `Permission:projects:delete`

---

## Эндпоинты: Room Vertices

### GET /rooms/{roomId}/vertices

Список вершин геометрии комнаты.

**Авторизация:** Политика `Permission:projects:read`

**Ответ:** `ApiResponse<List<RoomVertexDto>>`

```json
{
  "id": "guid",
  "roomId": "guid",
  "x": 0.0,
  "y": 0.0,
  "orderIndex": 0
}
```

### POST /rooms/{roomId}/vertices

**Авторизация:** Политика `Permission:projects:write`

**Тело запроса:**

```json
{
  "x": 0.0,
  "y": 0.0,
  "orderIndex": 0
}
```

**Ответ:** `ApiResponse<IdResponse>`

### DELETE /rooms/{roomId}/vertices/{vertexId}

**Авторизация:** Политика `Permission:projects:delete`

---

## Эндпоинты: Room Edges

### GET /rooms/{roomId}/edges

Список рёбер (стен) комнаты.

**Авторизация:** Политика `Permission:projects:read`

**Ответ:** `ApiResponse<List<RoomEdgeDto>>`

```json
{
  "id": "guid",
  "roomId": "guid",
  "startVertexId": "guid",
  "endVertexId": "guid",
  "wallHeight": 2700,
  "hasWindow": false,
  "hasDoor": false,
  "orderIndex": 0
}
```

### POST /rooms/{roomId}/edges

**Авторизация:** Политика `Permission:projects:write`

**Тело запроса:**

```json
{
  "startVertexId": "guid",
  "endVertexId": "guid",
  "wallHeight": 2700,
  "hasWindow": false,
  "hasDoor": false,
  "orderIndex": 0
}
```

**Ответ:** `ApiResponse<IdResponse>`

### PUT /rooms/{roomId}/edges/{edgeId}

**Авторизация:** Политика `Permission:projects:write`

**Тело запроса:**

```json
{
  "wallHeight": 2700,
  "hasWindow": false,
  "hasDoor": false,
  "orderIndex": 0
}
```

### DELETE /rooms/{roomId}/edges/{edgeId}

**Авторизация:** Политика `Permission:projects:delete`

---

## Эндпоинты: Zones

### GET /edges/{edgeId}/zones

Список зон на ребре (стене).

**Авторизация:** Политика `Permission:projects:read`

**Ответ:** `ApiResponse<List<ZoneDto>>`

```json
{
  "id": "guid",
  "edgeId": "guid",
  "name": "string",
  "type": "string",
  "startX": 0.0,
  "endX": 0.0
}
```

### POST /edges/{edgeId}/zones

**Авторизация:** Политика `Permission:projects:write`

**Тело запроса:**

```json
{
  "type": "string",
  "startX": 0.0,
  "endX": 0.0,
  "name": "string"
}
```

**Ответ:** `ApiResponse<IdResponse>`

### PUT /edges/{edgeId}/zones/{zoneId}

**Авторизация:** Политика `Permission:projects:write`

**Тело запроса:**

```json
{
  "name": "string",
  "startX": 0.0,
  "endX": 0.0
}
```

### DELETE /edges/{edgeId}/zones/{zoneId}

**Авторизация:** Политика `Permission:projects:delete`

---

## Эндпоинты: Cabinets

### GET /cabinets/room/{roomId}

Список всех шкафов в комнате.

**Авторизация:** Политика `Permission:cabinets:read`

**Ответ:** `ApiResponse<List<CabinetDto>>`

```json
{
  "id": "guid",
  "roomId": "guid",
  "edgeId": "guid",
  "zoneId": "guid",
  "assemblyId": "guid",
  "name": "string",
  "placementType": "string",
  "facadeType": "string",
  "positionX": 0.0,
  "positionY": 0.0,
  "rotation": 0.0,
  "width": 0.0,
  "height": 0.0,
  "depth": 0.0,
  "calculatedPrice": 0.00,
  "createdAt": "datetime"
}
```

### GET /cabinets/{id}

**Авторизация:** Политика `Permission:cabinets:read`

**Ответ:** `ApiResponse<CabinetDto>`

### POST /cabinets

**Авторизация:** Политика `Permission:cabinets:write`

**Тело запроса:**

```json
{
  "roomId": "guid",
  "placementType": "string",
  "positionX": 0.0,
  "positionY": 0.0,
  "width": 0.0,
  "height": 0.0,
  "depth": 0.0,
  "name": "string",
  "edgeId": "guid",
  "zoneId": "guid",
  "assemblyId": "guid",
  "facadeType": "string",
  "rotation": 0.0
}
```

**Ответ:** `ApiResponse<IdResponse>`

### PUT /cabinets/{id}

**Авторизация:** Политика `Permission:cabinets:write`

**Тело запроса:**

```json
{
  "positionX": 0.0,
  "positionY": 0.0,
  "rotation": 0.0,
  "width": 0.0,
  "height": 0.0,
  "depth": 0.0,
  "name": "string",
  "edgeId": "guid",
  "zoneId": "guid",
  "assemblyId": "guid",
  "facadeType": "string",
  "calculatedPrice": 0.00
}
```

### DELETE /cabinets/{id}

**Авторизация:** Политика `Permission:cabinets:delete`

---

## Эндпоинты: Cabinet Hardware Overrides

### GET /cabinet-hardware-overrides/cabinet/{cabinetId}

Список переопределений фурнитуры для шкафа.

**Авторизация:** Политика `Permission:cabinets:read`

**Ответ:** `ApiResponse<List<CabinetHardwareOverrideDto>>`

```json
{
  "id": "guid",
  "cabinetId": "guid",
  "assemblyPartId": "guid",
  "componentId": "guid",
  "role": "string",
  "quantityFormula": "string",
  "positionXFormula": "string",
  "positionYFormula": "string",
  "positionZFormula": "string",
  "isEnabled": true,
  "materialId": "string",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET /cabinet-hardware-overrides/{id}

**Авторизация:** Политика `Permission:cabinets:read`

**Ответ:** `ApiResponse<CabinetHardwareOverrideDto>`

### POST /cabinet-hardware-overrides/cabinet/{cabinetId}

**Авторизация:** Политика `Permission:cabinets:write`

**Тело запроса:**

```json
{
  "assemblyPartId": "guid",
  "isEnabled": true,
  "componentId": "guid",
  "role": "string",
  "quantityFormula": "string",
  "positionXFormula": "string",
  "positionYFormula": "string",
  "positionZFormula": "string",
  "materialId": "string"
}
```

**Ответ:** `ApiResponse<IdResponse>`

### PUT /cabinet-hardware-overrides/{id}

**Авторизация:** Политика `Permission:cabinets:write`

**Тело запроса:**

```json
{
  "isEnabled": true,
  "componentId": "guid",
  "role": "string",
  "quantityFormula": "string",
  "positionXFormula": "string",
  "positionYFormula": "string",
  "positionZFormula": "string",
  "materialId": "string"
}
```

### DELETE /cabinet-hardware-overrides/{id}

**Авторизация:** Политика `Permission:cabinets:delete`

---

## Авторизация

### Ролевая авторизация (Projects)

| Действие | Роли |
|----------|------|
| Чтение, создание, обновление | `sales`, `designer`, `catalogAdmin`, `systemAdmin` |
| Удаление | `designer`, `catalogAdmin`, `systemAdmin` |

### Авторизация на основе политик (остальные контроллеры)

| Политика | Действие |
|----------|----------|
| `Permission:projects:read` | GET-запросы (Rooms, Vertices, Edges, Zones) |
| `Permission:projects:write` | POST/PUT-запросы (Rooms, Vertices, Edges, Zones) |
| `Permission:projects:delete` | DELETE-запросы (Rooms, Vertices, Edges, Zones) |
| `Permission:cabinets:read` | GET-запросы (Cabinets, Hardware Overrides) |
| `Permission:cabinets:write` | POST/PUT-запросы (Cabinets, Hardware Overrides) |
| `Permission:cabinets:delete` | DELETE-запросы (Cabinets, Hardware Overrides) |

---

## Gateway маршруты

| Gateway путь | Сервис | Преобразование |
|-------------|--------|----------------|
| `/api/projects/{**catch-all}` | Projects (5003) | Удаление префикса `/api` |
| `/api/rooms/{**catch-all}` | Projects (5003) | Удаление префикса `/api` |
| `/api/edges/{**catch-all}` | Projects (5003) | Удаление префикса `/api` |
| `/api/cabinets/{**catch-all}` | Projects (5003) | Удаление префикса `/api` |
| `/api/cabinet-hardware-overrides/{**catch-all}` | Projects (5003) | Удаление префикса `/api` |

Пример: `GET /api/cabinets/room/{roomId}` → `GET /cabinets/room/{roomId}` на сервисе Projects.
