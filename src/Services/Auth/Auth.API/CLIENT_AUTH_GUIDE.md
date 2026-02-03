# Руководство по авторизации для клиента (Frontend)

## Обзор

Система авторизации использует OAuth 2.0 Authorization Code Flow с внешним центром авторизации (Auth Center). Токены хранятся в HttpOnly cookies, что обеспечивает защиту от XSS-атак.

## Архитектура

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Frontend  │────▶│  API Gateway │────▶│  Auth.API   │
│  (Browser)  │◀────│  :5000      │◀────│  :5001      │
└─────────────┘     └─────────────┘     └─────────────┘
       │                                       │
       │         ┌─────────────────┐           │
       └────────▶│  Auth Center    │◀──────────┘
                 │  :5100 (dev)    │
                 └─────────────────┘
```

## Endpoints

### Базовый URL
- **Development:** `http://localhost:5000/api`
- **Auth Center (dev):** `http://localhost:5100`

---

## Поток авторизации

### 1. Инициация входа

Клиент должен инициировать редирект на Auth Center:

```typescript
const AUTH_CENTER_URL = 'http://localhost:5100';
const CLIENT_ID = 'domeo';
const REDIRECT_URI = 'http://localhost:3000/auth/callback';

function login() {
  const state = generateRandomState(); // Сохранить в sessionStorage
  sessionStorage.setItem('auth_state', state);

  const params = new URLSearchParams({
    response_type: 'code',
    client_id: CLIENT_ID,
    redirect_uri: REDIRECT_URI,
    state: state,
  });

  window.location.href = `${AUTH_CENTER_URL}/authorize?${params}`;
}

function generateRandomState(): string {
  return crypto.randomUUID();
}
```

### 2. Обработка callback

После успешной авторизации, Auth Center перенаправит пользователя на `redirect_uri` с параметрами `code` и `state`:

```
http://localhost:3000/auth/callback?code=abc123&state=xyz789
```

```typescript
// pages/auth/callback.tsx или routes/auth/callback.ts

async function handleCallback() {
  const params = new URLSearchParams(window.location.search);
  const code = params.get('code');
  const state = params.get('state');

  // Проверка state для защиты от CSRF
  const savedState = sessionStorage.getItem('auth_state');
  if (state !== savedState) {
    throw new Error('Invalid state parameter');
  }
  sessionStorage.removeItem('auth_state');

  if (!code) {
    throw new Error('No authorization code received');
  }

  // Обмен кода на токены через API Gateway
  const response = await fetch('http://localhost:5000/api/auth/callback', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include', // ВАЖНО: для получения cookies
    body: JSON.stringify({
      code,
      redirectUri: REDIRECT_URI,
    }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Authentication failed');
  }

  // Токены установлены в cookies автоматически
  // Перенаправляем на главную страницу
  window.location.href = '/';
}
```

### 3. Выполнение API запросов

После авторизации все запросы автоматически включают cookies:

```typescript
async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const response = await fetch(`http://localhost:5000/api${endpoint}`, {
    ...options,
    credentials: 'include', // ВАЖНО: всегда включать
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  });

  if (response.status === 401) {
    // Токен истёк, пробуем обновить
    const refreshed = await refreshToken();
    if (refreshed) {
      // Повторяем запрос
      return apiRequest(endpoint, options);
    } else {
      // Refresh token тоже истёк - редирект на логин
      window.location.href = '/login';
      throw new Error('Session expired');
    }
  }

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Request failed');
  }

  return response.json();
}
```

### 4. Обновление токена

```typescript
async function refreshToken(): Promise<boolean> {
  try {
    const response = await fetch('http://localhost:5000/api/auth/refresh', {
      method: 'POST',
      credentials: 'include',
    });

    return response.ok;
  } catch {
    return false;
  }
}
```

### 5. Получение информации о пользователе

```typescript
interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

async function getCurrentUser(): Promise<User | null> {
  try {
    return await apiRequest<User>('/auth/me');
  } catch {
    return null;
  }
}
```

### 6. Выход из системы

```typescript
async function logout() {
  try {
    await fetch('http://localhost:5000/api/auth/logout', {
      method: 'POST',
      credentials: 'include',
    });
  } finally {
    // Редирект на страницу логина
    window.location.href = '/login';
  }
}
```

---

## WebSocket авторизация

Для WebSocket соединений токен нужно получить явно и передать в query параметре:

```typescript
async function connectWebSocket(): Promise<WebSocket> {
  // Получаем токен для WebSocket
  const tokenResponse = await fetch('http://localhost:5000/api/auth/token', {
    credentials: 'include',
  });

  if (!tokenResponse.ok) {
    throw new Error('Failed to get WebSocket token');
  }

  const { accessToken } = await tokenResponse.json();

  // Подключаемся с токеном в query
  const ws = new WebSocket(
    `ws://localhost:5000/ws?access_token=${encodeURIComponent(accessToken)}`
  );

  return ws;
}
```

---

## React-хуки

### useAuth

```typescript
import { createContext, useContext, useEffect, useState, ReactNode } from 'react';

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: () => void;
  logout: () => Promise<void>;
  refresh: () => Promise<void>;
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
      const user = await getCurrentUser();
      setUser(user);
    } catch {
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  }

  const login = () => {
    const state = crypto.randomUUID();
    sessionStorage.setItem('auth_state', state);

    const params = new URLSearchParams({
      response_type: 'code',
      client_id: 'domeo',
      redirect_uri: `${window.location.origin}/auth/callback`,
      state,
    });

    window.location.href = `http://localhost:5100/authorize?${params}`;
  };

  const logout = async () => {
    await fetch('http://localhost:5000/api/auth/logout', {
      method: 'POST',
      credentials: 'include',
    });
    setUser(null);
    window.location.href = '/login';
  };

  const refresh = async () => {
    await checkAuth();
  };

  return (
    <AuthContext.Provider value={{
      user,
      isLoading,
      isAuthenticated: !!user,
      login,
      logout,
      refresh,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
```

### Защищённый маршрут

```typescript
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from './useAuth';

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}
```

---

## API Client (axios пример)

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  withCredentials: true, // ВАЖНО: включить cookies
});

// Интерцептор для обновления токена
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        await api.post('/auth/refresh');
        return api(originalRequest);
      } catch (refreshError) {
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export { api };
```

---

## Конфигурация

### Environment Variables

```env
# .env.development
VITE_API_URL=http://localhost:5000/api
VITE_AUTH_CENTER_URL=http://localhost:5100
VITE_CLIENT_ID=domeo
```

### Config файл

```typescript
// config/auth.ts
export const authConfig = {
  apiUrl: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  authCenterUrl: import.meta.env.VITE_AUTH_CENTER_URL || 'http://localhost:5100',
  clientId: import.meta.env.VITE_CLIENT_ID || 'domeo',

  get redirectUri() {
    return `${window.location.origin}/auth/callback`;
  },
};
```

---

## Тестовые пользователи (Development)

| Email | Password | Role |
|-------|----------|------|
| admin@test.com | admin123 | admin |
| manager@test.com | manager123 | manager |
| viewer@test.com | viewer123 | viewer |

---

## Обработка ошибок

### Типичные коды ошибок

| Код | Описание | Действие |
|-----|----------|----------|
| 401 | Не авторизован | Попробовать refresh, иначе редирект на логин |
| 403 | Доступ запрещён | Показать сообщение о недостаточных правах |
| 400 | Неверный запрос | Показать ошибку валидации |

### Пример обработки

```typescript
async function handleApiError(error: any) {
  if (error.response) {
    switch (error.response.status) {
      case 401:
        // Сессия истекла
        window.location.href = '/login';
        break;
      case 403:
        showNotification('У вас нет прав для этого действия', 'error');
        break;
      case 400:
        const errors = error.response.data.errors;
        if (errors) {
          // Показать ошибки валидации
          Object.values(errors).flat().forEach((msg: string) => {
            showNotification(msg, 'error');
          });
        }
        break;
      default:
        showNotification('Произошла ошибка', 'error');
    }
  }
}
```

---

## Checklist для реализации

- [ ] Я 
- [ ] Создать страницу `/auth/callback` для обработки редиректа
- [ ] Настроить `credentials: 'include'` для всех fetch запросов
- [ ] Реализовать автоматическое обновление токена при 401
- [ ] Добавить `AuthProvider` в корень приложения
- [ ] Защитить маршруты с помощью `ProtectedRoute`
- [ ] Настроить WebSocket с токеном в query параметре
- [ ] Добавить обработку ошибок авторизации

---

## Миграция с предыдущей версии

Если ранее использовался прямой логин с email/password:

1. **Удалить**: форму логина с полями email/password
2. **Удалить**: localStorage хранение токенов
3. **Добавить**: редирект на Auth Center
4. **Добавить**: callback страницу
5. **Изменить**: все fetch запросы добавить `credentials: 'include'`
6. **Удалить**: заголовок `Authorization: Bearer ...` (cookies передаются автоматически)
