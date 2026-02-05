# Audit Service

**Порт:** 5007
**Gateway префикс:** `/api/audit`
**Описание:** Сервис аудита — хранение и просмотр сессий входа, журналов аудита и логов приложений.

## Обзор эндпоинтов

### Login Sessions

| Метод | Путь | Описание | Авторизация |
|-------|------|----------|-------------|
| POST | `/audit/login-sessions` | Создание сессии входа | InternalApi |
| PUT | `/audit/login-sessions/{id}/logout` | Завершение сессии | InternalApi |
| GET | `/audit/login-sessions` | Список сессий | `systemAdmin` |
| GET | `/audit/login-sessions/{id}` | Получение сессии | `systemAdmin` |
| GET | `/audit/login-sessions/user/{userId}` | Сессии пользователя | `systemAdmin` |

### Audit Logs

| Метод | Путь | Описание | Авторизация |
|-------|------|----------|-------------|
| GET | `/audit/logs` | Список записей аудита | `systemAdmin` |
| GET | `/audit/logs/entity/{entityType}/{entityId}` | История сущности | `systemAdmin` |

### Application Logs

| Метод | Путь | Описание | Авторизация |
|-------|------|----------|-------------|
| GET | `/audit/application-logs` | Список логов приложений | `systemAdmin` |
| GET | `/audit/application-logs/{id}` | Получение лога | `systemAdmin` |
| GET | `/audit/application-logs/stats` | Статистика логов | `systemAdmin` |

---

## Эндпоинты: Login Sessions

### POST /audit/login-sessions

Создание записи о входе пользователя. Вызывается внутренними сервисами.

**Авторизация:** Политика `InternalApi` (заголовок `X-Internal-Api-Key`)

**Тело запроса:**

```json
{
  "userId": "guid",
  "userRole": "string",
  "ipAddress": "string",
  "userAgent": "string"
}
```

**Ответ:** `LoginSessionDto`

```json
{
  "id": "guid",
  "userId": "guid",
  "userRole": "string",
  "ipAddress": "string",
  "userAgent": "string",
  "loggedInAt": "datetime",
  "loggedOutAt": "datetime",
  "isActive": true
}
```

---

### PUT /audit/login-sessions/{id}/logout

Завершение сессии входа.

**Авторизация:** Политика `InternalApi`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | Guid | ID сессии |

**Ответ:** 204 No Content

---

### GET /audit/login-sessions

Список сессий входа с фильтрацией и пагинацией.

**Авторизация:** Роль `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `userId` | Guid | Нет | Фильтр по пользователю |
| `activeOnly` | bool | Нет | Только активные сессии |
| `from` | DateTime | Нет | Начало периода |
| `to` | DateTime | Нет | Конец периода |
| `page` | int | Нет | Номер страницы |
| `pageSize` | int | Нет | Размер страницы |

**Ответ:** Пагинированный список `LoginSessionDto`

---

### GET /audit/login-sessions/{id}

Получение сессии по ID.

**Авторизация:** Роль `systemAdmin`

**Ответ:** `LoginSessionDto`

---

### GET /audit/login-sessions/user/{userId}

Список сессий конкретного пользователя.

**Авторизация:** Роль `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `userId` | Guid | ID пользователя |

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `activeOnly` | bool | Нет | Только активные сессии |
| `page` | int | Нет | Номер страницы |
| `pageSize` | int | Нет | Размер страницы |

**Ответ:** Пагинированный список `LoginSessionDto`

---

## Эндпоинты: Audit Logs

### GET /audit/logs

Список записей аудита с фильтрацией.

**Авторизация:** Роль `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `userId` | Guid | Нет | Фильтр по пользователю |
| `action` | string | Нет | Тип действия |
| `entityType` | string | Нет | Тип сущности |
| `serviceName` | string | Нет | Имя сервиса |
| `from` | DateTime | Нет | Начало периода |
| `to` | DateTime | Нет | Конец периода |
| `page` | int | Нет | Номер страницы |
| `pageSize` | int | Нет | Размер страницы |

**Ответ:** Пагинированный список `AuditLogDto`

```json
{
  "id": "guid",
  "userId": "guid",
  "action": "string",
  "entityType": "string",
  "entityId": "string",
  "serviceName": "string",
  "oldValue": "string",
  "newValue": "string",
  "ipAddress": "string",
  "createdAt": "datetime"
}
```

---

### GET /audit/logs/entity/{entityType}/{entityId}

История изменений конкретной сущности.

**Авторизация:** Роль `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `entityType` | string | Тип сущности |
| `entityId` | string | ID сущности |

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `page` | int | Нет | Номер страницы |
| `pageSize` | int | Нет | Размер страницы |

**Ответ:** Пагинированный список `AuditLogDto`

---

## Эндпоинты: Application Logs

### GET /audit/application-logs

Список логов приложений.

**Авторизация:** Роль `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `level` | string | Нет | Уровень лога (Information, Warning, Error) |
| `serviceName` | string | Нет | Имя сервиса |
| `userId` | Guid | Нет | Фильтр по пользователю |
| `from` | DateTime | Нет | Начало периода |
| `to` | DateTime | Нет | Конец периода |
| `page` | int | Нет | Номер страницы |
| `pageSize` | int | Нет | Размер страницы |

**Ответ:** Пагинированный список `ApplicationLogDto`

```json
{
  "id": "guid",
  "serviceName": "string",
  "level": "string",
  "message": "string",
  "exception": "string",
  "exceptionType": "string",
  "properties": "string",
  "requestPath": "string",
  "userId": "guid",
  "correlationId": "string",
  "createdAt": "datetime"
}
```

---

### GET /audit/application-logs/{id}

Получение лога по ID.

**Авторизация:** Роль `systemAdmin`

**Ответ:** `ApplicationLogDto`

---

### GET /audit/application-logs/stats

Статистика логов (группировка по сервису, уровню).

**Авторизация:** Роль `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `groupBy` | string | Нет | Поле группировки |
| `from` | DateTime | Нет | Начало периода |
| `to` | DateTime | Нет | Конец периода |

**Ответ:** `List<LogStatsDto>`

```json
[
  {
    "serviceName": "string",
    "level": "string",
    "count": 0
  }
]
```

С общим итогом `LogStatsTotalDto`:

```json
{
  "total": 0
}
```

---

## Авторизация

| Тип | Эндпоинты | Описание |
|-----|-----------|----------|
| `InternalApi` | POST login-sessions, PUT logout | Внутренние вызовы между сервисами (заголовок `X-Internal-Api-Key`) |
| `systemAdmin` | Все GET-эндпоинты | Только системные администраторы |

---

## Gateway маршруты

| Gateway путь | Сервис | Преобразование |
|-------------|--------|----------------|
| `/api/audit/{**catch-all}` | Audit (5007) | Удаление префикса `/api` |

Пример: `GET /api/audit/logs` → `GET /audit/logs` на сервисе Audit.
