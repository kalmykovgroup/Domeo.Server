# Materials Service

**Порт:** 5009
**Gateway префикс:** `/api/materials`, `/api/suppliers`, `/api/material-categories`
**Описание:** Прокси-сервис для работы с материалами, поставщиками и брендами. Проксирует запросы к MockSupplier (порт 5102). Все эндпоинты только для чтения, собственная БД отсутствует.

## Обзор эндпоинтов

### Categories

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/categories/tree` | Дерево категорий материалов |
| GET | `/categories/{id}/attributes` | Атрибуты категории |

### Suppliers

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/suppliers` | Список поставщиков |
| GET | `/suppliers/{id}` | Получение поставщика |

### Brands

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/brands` | Список брендов |

### Materials

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/items` | Список материалов |
| GET | `/items/{id}` | Получение материала |
| GET | `/items/suggest` | Поисковые подсказки |
| GET | `/items/{id}/offers` | Предложения поставщиков |

---

## Эндпоинты: Categories

### GET /categories/tree

Иерархическое дерево категорий материалов.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `activeOnly` | bool | Нет | Только активные категории |

**Ответ:** `ApiResponse<List<CategoryTreeNodeDto>>`

```json
{
  "id": "string",
  "parentId": "string",
  "name": "string",
  "level": 0,
  "orderIndex": 0,
  "isActive": true,
  "supplierIds": ["string"],
  "children": []
}
```

---

### GET /categories/{id}/attributes

Список атрибутов категории (для фильтрации материалов).

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | string | ID категории |

**Ответ:** `ApiResponse<List<CategoryAttributeDto>>`

```json
{
  "id": "string",
  "categoryId": "string",
  "name": "string",
  "type": "string",
  "unit": "string",
  "enumValues": ["string"]
}
```

---

## Эндпоинты: Suppliers

### GET /suppliers

Список поставщиков.

**Авторизация:** `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `activeOnly` | bool | Нет | Только активные поставщики |

**Ответ:** `ApiResponse<List<SupplierDto>>`

```json
{
  "id": "string",
  "company": "string",
  "contactName": "string",
  "email": "string",
  "phone": "string",
  "address": "string",
  "website": "string",
  "rating": 0.0,
  "isActive": true
}
```

---

### GET /suppliers/{id}

Получение поставщика по ID.

**Авторизация:** `catalogAdmin`, `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | string | ID поставщика |

**Ответ:** `ApiResponse<SupplierDto>`

---

## Эндпоинты: Brands

### GET /brands

Список брендов материалов.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `activeOnly` | bool | Нет | Только активные бренды |

**Ответ:** `ApiResponse<List<BrandDto>>`

```json
{
  "id": "string",
  "name": "string",
  "logoUrl": "string",
  "isActive": true
}
```

---

## Эндпоинты: Materials

### GET /items

Список материалов с фильтрацией.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `categoryId` | string | Нет | Фильтр по категории |
| `brandId` | string | Нет | Фильтр по бренду |
| `supplierId` | string | Нет | Фильтр по поставщику |
| `activeOnly` | bool | Нет | Только активные |
| `attr_{name}` | string | Нет | Динамические фильтры по атрибутам категории |

**Ответ:** `ApiResponse<List<MaterialDto>>`

```json
{
  "id": "string",
  "categoryId": "string",
  "brandId": "string",
  "brandName": "string",
  "name": "string",
  "description": "string",
  "unit": "string",
  "color": "string",
  "textureUrl": "string",
  "isActive": true
}
```

> Динамические атрибуты передаются как `attr_thickness=16`, `attr_color=white` и т.д.

---

### GET /items/{id}

Получение материала по ID.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | string | ID материала |

**Ответ:** `ApiResponse<MaterialDto>`

---

### GET /items/suggest

Поисковые подсказки по материалам, категориям, брендам и атрибутам.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры запроса:**

| Параметр | Тип | Обязательный | Описание |
|----------|-----|--------------|----------|
| `query` | string | Да | Поисковый запрос |
| `limit` | int | Нет | Максимум результатов (по умолчанию 10) |

**Ответ:** `ApiResponse<List<SearchSuggestionDto>>`

```json
{
  "type": "material|category|brand|attribute",
  "text": "string",
  "score": 0.0,
  "materialId": "string",
  "categoryId": "string",
  "brandName": "string",
  "textureUrl": "string",
  "brandId": "string",
  "logoUrl": "string",
  "parentId": "string",
  "level": 0,
  "categoryPath": "string",
  "attributeName": "string",
  "attributeValue": "string"
}
```

---

### GET /items/{id}/offers

Список предложений поставщиков для материала.

**Авторизация:** `sales`, `designer`, `catalogAdmin`, `systemAdmin`

**Параметры пути:**

| Параметр | Тип | Описание |
|----------|-----|----------|
| `id` | string | ID материала |

**Ответ:** `ApiResponse<MaterialOffersDto>`

```json
{
  "material": {
    "id": "string",
    "name": "string",
    "unit": "string",
    "description": "string"
  },
  "offers": [
    {
      "offerId": "string",
      "materialId": "string",
      "price": 0.00,
      "currency": "string",
      "minOrderQty": 1,
      "leadTimeDays": 0,
      "inStock": true,
      "sku": "string",
      "notes": "string",
      "updatedAt": "datetime",
      "supplier": {
        "id": "string",
        "company": "string",
        "contactName": "string",
        "phone": "string",
        "email": "string",
        "rating": 0.0
      }
    }
  ],
  "totalOffers": 0
}
```

---

## Авторизация

| Эндпоинты | Роли |
|-----------|------|
| Categories, Brands, Materials | `sales`, `designer`, `catalogAdmin`, `systemAdmin` |
| Suppliers | `catalogAdmin`, `systemAdmin` |

Аутентификация через заголовки `X-User-Id` и `X-User-Role`, передаваемые API Gateway.

---

## Архитектура

Materials Service — прокси-сервис без собственной базы данных. Все данные получаются от MockSupplier (порт 5102) через HTTP-клиент `SupplierApiClient`.

```
Frontend → API Gateway (5000) → Materials.API (5009) → MockSupplier (5102)
```

---

## Gateway маршруты

| Gateway путь | Сервис | Преобразование |
|-------------|--------|----------------|
| `/api/materials/{**catch-all}` | Materials (5009) | Удаление префикса `/api/materials` |
| `/api/suppliers/{**catch-all}` | Materials (5009) | Перезапись на `/suppliers/{**catch-all}` |
| `/api/material-categories/{**catch-all}` | Materials (5009) | Перезапись на `/categories/{**catch-all}` |

Примеры:
- `GET /api/materials/items` → `GET /items`
- `GET /api/suppliers` → `GET /suppliers`
- `GET /api/material-categories/tree` → `GET /categories/tree`
