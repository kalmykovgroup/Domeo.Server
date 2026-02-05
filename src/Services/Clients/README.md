# Clients Service

**Порт:** 5006
**Gateway префикс:** `/api/clients`
**Описание:** Управление клиентами — создание, редактирование, удаление и восстановление карточек клиентов.

## Обзор эндпоинтов

| Метод | Путь | Описание | Авторизация |
|-------|------|----------|-------------|
| GET | `/clients` | Список клиентов (с пагинацией) | `sales`, `designer`, `catalogAdmin`, `systemAdmin` |
| GET | `/clients/{id}` | Получение клиента | `sales`, `designer`, `catalogAdmin`, `systemAdmin` |
| POST | `/clients` | Создание клиента | `sales`, `designer`, `catalogAdmin`, `systemAdmin` |
| PUT | `/clients/{id}` | Обновление клиента | `sales`, `designer`, `catalogAdmin`, `systemAdmin` |
| POST | `/clients/{id}/restore` | Восстановление клиента | `sales`, `designer`, `catalogAdmin`, `systemAdmin` |
| DELETE | `/clients/{id}` | Удаление клиента | `designer`, `catalogAdmin`, `systemAdmin` |

---

## Эндпоинты

### GET /clients

Список клиентов с поиском и пагинацией.

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `search` | string | Нет | Поисковый запрос |
| `page` | int | Нет | Номер страницы |
| `pageSize` | int | Нет | Размер страницы |
| `sortBy` | string | Нет | Поле сортировки |
| `sortOrder` | string | Нет | Направление сортировки (asc/desc) |

**Ответ:** `ApiResponse<PaginatedResponse<ClientDto>>`

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "name": "string",
        "phone": "string",
        "email": "string",
        "address": "string",
        "notes": "string",
        "userId": "guid",
        "createdAt": "datetime",
        "deletedAt": "datetime"
      }
    ],
    "totalCount": 0,
    "page": 1,
    "pageSize": 20
  }
}
```

---

### GET /clients/{id}

Получение клиента по ID.

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | Guid | ID клиента |

**Ответ:** `ApiResponse<ClientDto>`

---

### POST /clients

Создание нового клиента.

**Тело запроса:**

```json
{
  "name": "string",
  "phone": "string",
  "email": "string",
  "address": "string",
  "notes": "string"
}
```

| Поле | Тип | Обязательный | Описание |
|------|-----|--------------|----------|
| `name` | string | Да | Имя клиента |
| `phone` | string | Нет | Телефон |
| `email` | string | Нет | Email |
| `address` | string | Нет | Адрес |
| `notes` | string | Нет | Заметки |

**Ответ:** `ApiResponse<ClientDto>` с сообщением "Client created successfully"

---

### PUT /clients/{id}

Обновление клиента.

**Тело запроса:**

```json
{
  "name": "string",
  "phone": "string",
  "email": "string",
  "address": "string",
  "notes": "string"
}
```

**Ответ:** `ApiResponse<ClientDto>` с сообщением "Client updated successfully"

---

### POST /clients/{id}/restore

Восстановление ранее удалённого клиента.

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | Guid | ID клиента |

**Ответ:** `ApiResponse<ClientDto>` с сообщением "Client restored successfully"

---

### DELETE /clients/{id}

Мягкое удаление клиента.

**Авторизация:** `designer`, `catalogAdmin`, `systemAdmin` (роль `sales` **не** имеет доступа)

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | Guid | ID клиента |

**Ответ:** `ApiResponse` с сообщением "Client deleted successfully"

---

## Авторизация

| Действие | Роли |
|----------|------|
| Чтение, создание, обновление, восстановление | `sales`, `designer`, `catalogAdmin`, `systemAdmin` |
| Удаление | `designer`, `catalogAdmin`, `systemAdmin` |

Аутентификация через заголовки `X-User-Id` и `X-User-Role`, передаваемые API Gateway.

---

## Gateway маршруты

| Gateway путь | Сервис | Преобразование |
|-------------|--------|----------------|
| `/api/clients/{**catch-all}` | Clients (5006) | Удаление префикса `/api` |

Пример: `GET /api/clients` → `GET /clients` на сервисе Clients.
