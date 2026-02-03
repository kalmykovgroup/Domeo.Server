# Руководство по авторизации для клиента (Frontend)

## Архитектура

```
┌─────────────┐                              ┌─────────────────┐
│   Browser   │ ──── 1. Redirect ──────────▶ │  Auth Center    │
│             │ ◀─── 2. Redirect + code ──── │  :5100          │
└─────────────┘                              └─────────────────┘
       │
       │ 3. POST /api/auth/callback {code}
       ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ API Gateway │────▶│  Auth.API   │────▶│ Auth Center │
│   :5000     │◀────│  :5001      │◀────│ (token)     │
└─────────────┘     └─────────────┘     └─────────────┘
       │
       │ 4. Set-Cookie: access_token, refresh_token
       ▼
┌─────────────┐
│   Browser   │  Токены в HttpOnly cookies (недоступны из JS)
└─────────────┘
```

## Поток авторизации

### Шаг 1: Инициация входа

Frontend выполняет redirect на Auth Center:

```typescript
const AUTH_CENTER_URL = 'http://localhost:5100';
const CLIENT_ID = 'domeo';
const REDIRECT_URI = `${window.location.origin}/auth/callback`;

function login() {
  const state = crypto.randomUUID();
  sessionStorage.setItem('auth_state', state);

  const params = new URLSearchParams({
    response_type: 'code',
    client_id: CLIENT_ID,
    redirect_uri: REDIRECT_URI,
    state: state,
  });

  window.location.href = `${AUTH_CENTER_URL}/authorize?${params}`;
}
```

### Шаг 2: Авторизация в Auth Center

Пользователь видит страницу логина Auth Center, вводит credentials.

### Шаг 3: Redirect обратно на Frontend

Auth Center перенаправляет браузер на `redirect_uri`:

```
http://localhost:3000/auth/callback?code=abc123&state=xyz789
```

### Шаг 4: Обмен кода на токены

Frontend отправляет код на API:

```typescript
// Страница /auth/callback
async function handleCallback() {
  const params = new URLSearchParams(window.location.search);
  const code = params.get('code');
  const state = params.get('state');

  // Проверка state (защита от CSRF)
  if (state !== sessionStorage.getItem('auth_state')) {
    throw new Error('Invalid state');
  }
  sessionStorage.removeItem('auth_state');

  // Обмен кода на токены
  const response = await fetch('http://localhost:5000/api/auth/callback', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify({
      code: code,
      redirectUri: 'http://localhost:3000/auth/callback'
    }),
  });

  const result = await response.json();

  if (result.success) {
    // Токены установлены в cookies автоматически
    // Переход на главную
    window.location.href = '/';
  }
}
```

## Хранение токенов

Токены хранятся в **HttpOnly cookies** (недоступны из JavaScript):

| Cookie | Описание | Срок жизни |
|--------|----------|------------|
| `access_token` | JWT для API запросов | 15 минут |
| `refresh_token` | Для обновления access token | 7 дней |

**Безопасность:**
- `HttpOnly: true` — защита от XSS
- `Secure: true` — только HTTPS
- `SameSite: None` — разрешает cross-origin (для API на другом домене)

## API запросы

После авторизации cookies отправляются автоматически:

```typescript
async function api<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`http://localhost:5000/api${endpoint}`, {
    ...options,
    credentials: 'include', // ОБЯЗАТЕЛЬНО
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  });

  if (response.status === 401) {
    // Токен истёк — пробуем обновить
    const refreshed = await refreshToken();
    if (refreshed) {
      return api(endpoint, options); // Повторяем запрос
    }
    window.location.href = '/login';
    throw new Error('Session expired');
  }

  return response.json();
}

async function refreshToken(): Promise<boolean> {
  const response = await fetch('http://localhost:5000/api/auth/refresh', {
    method: 'POST',
    credentials: 'include',
  });
  return response.ok;
}
```

## Endpoints

### POST /api/auth/callback

Обмен authorization code на токены.

**Request:**
```json
{
  "code": "authorization_code_from_auth_center",
  "redirectUri": "http://localhost:3000/auth/callback"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "guid",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "admin",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z"
    },
    "token": {
      "accessToken": "eyJ...",
      "refreshToken": "abc...",
      "expiresAt": "2024-01-01T00:15:00Z"
    }
  }
}
```

**Cookies в ответе:**
```
Set-Cookie: access_token=eyJ...; HttpOnly; Secure; SameSite=None
Set-Cookie: refresh_token=abc...; HttpOnly; Secure; SameSite=None
```

> **Примечание:** `token` в JSON нужен только для WebSocket и мобильных приложений. Для браузера используйте cookies.

### POST /api/auth/refresh

Обновление access token.

**Request:** Пустой body (refresh_token берётся из cookie)

**Response:** Аналогично /callback

### GET /api/auth/me

Информация о текущем пользователе.

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "email": "user@example.com",
    "name": "John Doe",
    "role": "admin"
  }
}
```

### POST /api/auth/logout

Выход из системы.

**Response:**
```json
{
  "success": true,
  "data": "Logged out successfully"
}
```

Cookies удаляются автоматически.

### GET /api/auth/token

Получение токена для WebSocket.

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJ..."
  }
}
```

## WebSocket авторизация

WebSocket не поддерживает cookies, поэтому токен передаётся явно:

```typescript
async function connectWebSocket() {
  // Получаем токен из cookie через API
  const response = await fetch('http://localhost:5000/api/auth/token', {
    credentials: 'include',
  });
  const { data } = await response.json();

  // Подключаемся с токеном в query
  const ws = new WebSocket(
    `ws://localhost:5000/ws?access_token=${encodeURIComponent(data.token)}`
  );

  return ws;
}
```

## React пример

### AuthProvider

```typescript
import { createContext, useContext, useEffect, useState, ReactNode } from 'react';

interface User {
  id: string;
  email: string;
  name: string;
  role: string;
}

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: () => void;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    checkAuth();
  }, []);

  async function checkAuth() {
    try {
      const response = await fetch('http://localhost:5000/api/auth/me', {
        credentials: 'include',
      });
      if (response.ok) {
        const { data } = await response.json();
        setUser(data);
      }
    } catch {
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  }

  function login() {
    const state = crypto.randomUUID();
    sessionStorage.setItem('auth_state', state);

    const params = new URLSearchParams({
      response_type: 'code',
      client_id: 'domeo',
      redirect_uri: `${window.location.origin}/auth/callback`,
      state,
    });

    window.location.href = `http://localhost:5100/authorize?${params}`;
  }

  async function logout() {
    await fetch('http://localhost:5000/api/auth/logout', {
      method: 'POST',
      credentials: 'include',
    });
    setUser(null);
    window.location.href = '/login';
  }

  return (
    <AuthContext.Provider value={{
      user,
      isLoading,
      isAuthenticated: !!user,
      login,
      logout,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
```

### Callback страница

```typescript
// pages/auth/callback.tsx
import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export function AuthCallback() {
  const navigate = useNavigate();

  useEffect(() => {
    handleCallback();
  }, []);

  async function handleCallback() {
    const params = new URLSearchParams(window.location.search);
    const code = params.get('code');
    const state = params.get('state');

    if (state !== sessionStorage.getItem('auth_state')) {
      navigate('/login?error=invalid_state');
      return;
    }
    sessionStorage.removeItem('auth_state');

    const response = await fetch('http://localhost:5000/api/auth/callback', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({
        code,
        redirectUri: `${window.location.origin}/auth/callback`,
      }),
    });

    if (response.ok) {
      navigate('/');
    } else {
      navigate('/login?error=auth_failed');
    }
  }

  return <div>Авторизация...</div>;
}
```

## Конфигурация

```typescript
// config.ts
export const config = {
  apiUrl: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  authCenterUrl: import.meta.env.VITE_AUTH_CENTER_URL || 'http://localhost:5100',
  clientId: 'domeo',
};
```

```env
# .env.development
VITE_API_URL=http://localhost:5000/api
VITE_AUTH_CENTER_URL=http://localhost:5100
```

## Тестовые пользователи

| Email | Password | Role |
|-------|----------|------|
| admin@test.com | admin123 | admin |
| manager@test.com | manager123 | manager |
| viewer@test.com | viewer123 | viewer |

## Чеклист реализации

- [ ] Создать страницу `/auth/callback`
- [ ] Добавить `credentials: 'include'` ко всем fetch запросам
- [ ] Реализовать `AuthProvider` с проверкой `/auth/me`
- [ ] Добавить автоматический refresh при 401
- [ ] Настроить WebSocket с токеном
