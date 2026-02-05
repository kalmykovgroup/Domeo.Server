# Auth Service

**Порт:** 5001
**Gateway префикс:** `/api/auth`
**Описание:** Сервис аутентификации. Реализует OAuth 2.0 Authorization Code Flow через MockAuthCenter, управляет токенами и сессиями пользователей.

## Обзор эндпоинтов

| Метод | Путь | Описание | Авторизация |
|-------|------|----------|-------------|
| GET | `/auth/login` | Инициация OAuth-авторизации | Нет |
| GET | `/auth/callback` | OAuth callback (браузер) | Нет |
| POST | `/auth/callback` | OAuth callback (мобильный клиент) | Нет |
| POST | `/auth/refresh` | Обновление access token | Нет |
| POST | `/auth/logout` | Выход из системы | Да |
| GET | `/auth/me` | Получение текущего пользователя | Да |
| GET | `/auth/token` | Получение токена из cookie | Нет |

---

## Эндпоинты

### GET /auth/login

Инициирует OAuth-авторизацию, перенаправляя пользователя на страницу входа MockAuthCenter.

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `returnUrl` | string | Нет | URL для редиректа после успешной авторизации |

**Ответ:** HTTP redirect на MockAuthCenter `/authorize`

---

### GET /auth/callback

OAuth callback для браузерных клиентов. Получает authorization code от MockAuthCenter, обменивает на токены.

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `code` | string | Да | Authorization code от MockAuthCenter |
| `state` | string | Нет | Base64-encoded JSON с `returnUrl` |

**Ответ:** HTTP redirect на фронтенд. Устанавливает HTTP-only cookies (`access_token`, `refresh_token`).

**Побочные эффекты:**
- Создаёт refresh token в БД
- Публикует `UserLoggedInEvent` в Audit

---

### POST /auth/callback

OAuth callback для мобильных клиентов. Возвращает токены в теле ответа.

**Тело запроса:**

```json
{
  "code": "string",
  "redirectUri": "string"
}
```

**Ответ:** `ApiResponse<AuthResultDto>`

```json
{
  "success": true,
  "data": {
    "user": {
      "id": "guid",
      "role": "string",
      "email": "string",
      "name": "string"
    },
    "token": {
      "accessToken": "string",
      "refreshToken": "string",
      "expiresAt": "datetime"
    }
  },
  "message": "string",
  "errors": []
}
```

---

### POST /auth/refresh

Обновление access token с помощью refresh token. Токен берётся из тела запроса или из cookie.

**Тело запроса (опционально):**

```json
{
  "refreshToken": "string"
}
```

**Ответ:** `ApiResponse<AuthResultDto>` — аналогичен POST `/auth/callback`.

**Побочные эффекты:**
- Отзывает старый refresh token
- Создаёт новый refresh token
- Обновляет cookies для браузерных клиентов

---

### POST /auth/logout

Выход из системы. Отзывает refresh token и очищает cookies.

**Авторизация:** Требуется (`[Authorize]`)

**Тело запроса (опционально):**

```json
{
  "refreshToken": "string"
}
```

**Ответ:** `ApiResponse<string>`

```json
{
  "success": true,
  "data": null,
  "message": "Logged out successfully",
  "errors": []
}
```

**Побочные эффекты:**
- Отзывает refresh token в MockAuthCenter
- Деактивирует refresh token в БД
- Публикует `UserLoggedOutEvent` в Audit
- Очищает HTTP-only cookies

---

### GET /auth/me

Возвращает информацию о текущем аутентифицированном пользователе.

**Авторизация:** Требуется (`[Authorize]`)

**Ответ:** `ApiResponse<User>`

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "role": "string",
    "email": "string",
    "name": "string"
  },
  "message": "string",
  "errors": []
}
```

---

### GET /auth/token

Возвращает JWT access token из HTTP-only cookie. Используется для WebSocket-аутентификации.

**Ответ:** `ApiResponse<object>`

```json
{
  "success": true,
  "data": {
    "token": "string"
  },
  "message": "string",
  "errors": []
}
```

---

## Авторизация

Сервис Auth — единственный сервис, маршруты которого **не требуют** JWT-валидации на уровне API Gateway (`requireAuth: false`). Авторизация проверяется непосредственно в контроллере для эндпоинтов `logout` и `me`.

### Cookies

| Cookie | Описание | HttpOnly | Время жизни |
|--------|----------|----------|-------------|
| `access_token` | JWT access token | Да | До истечения токена |
| `refresh_token` | Refresh token | Да | 7 дней |

### Определение типа клиента

Сервис различает браузерные и мобильные клиенты:
- Заголовок `X-Client-Type: mobile` — токены возвращаются в теле ответа
- Иначе — токены устанавливаются как HTTP-only cookies

---

## Gateway маршруты

| Gateway путь | Сервис | Преобразование |
|-------------|--------|----------------|
| `/api/auth/{**catch-all}` | Auth (5001) | Удаление префикса `/api` |

Пример: `GET /api/auth/me` → `GET /auth/me` на сервисе Auth.
