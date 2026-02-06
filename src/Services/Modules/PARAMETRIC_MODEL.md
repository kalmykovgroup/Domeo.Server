# Parametric 3D Model Generation — Full Specification

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
  role: string;               // "left" | "right" | "top" | "bottom" | "back" | "shelf" | "facade" | "hinge" | "handle" | etc.
  lengthExpr: string | null;  // выражение длины: "H", "W - 2*t"
  widthExpr: string | null;   // выражение ширины: "D", "D - shelfRearInset - shelfFrontInset"
  x: string | null;           // выражение позиции X: "0", "t", "W - t"
  y: string | null;           // выражение позиции Y
  z: string | null;           // выражение позиции Z
  rotationX: number;          // градусы (обычно 0)
  rotationY: number;
  rotationZ: number;
  condition: string | null;   // условие включения: "W > 800" — если false, деталь пропускается
  shape: ShapeSegment[] | null; // 2D-контур (null = прямоугольник)
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
      continue; // пропускаем деталь
    }

    // 2b. Количество
    const qty = part.quantityFormula
      ? Math.round(evaluate(part.quantityFormula, params))
      : part.quantity;

    // 2c. Размеры
    const length    = part.lengthExpr ? evaluate(part.lengthExpr, params) : 0;
    const width     = part.widthExpr  ? evaluate(part.widthExpr, params)  : 0;
    const thickness = getThickness(part.component);

    // 2d. Позиция
    const posX = part.x ? evaluate(part.x, params) : 0;
    const posY = part.y ? evaluate(part.y, params) : 0;
    const posZ = part.z ? evaluate(part.z, params) : 0;

    // 2e. Геометрия
    let mesh: THREE.Mesh;

    if (part.component?.params?.type === 'glb') {
      mesh = loadGlbModel(part.component.params.glbUrl, part.component.params.scale);
    } else if (part.shape) {
      mesh = createExtrudedPanel(part.shape, params, thickness, part.role);
    } else {
      mesh = createRectPanel(length, width, thickness, part.role);
    }

    // 2f. Позиционирование
    mesh.position.set(posX, posY, posZ);
    mesh.rotation.set(
      THREE.MathUtils.degToRad(part.rotationX),
      THREE.MathUtils.degToRad(part.rotationY),
      THREE.MathUtils.degToRad(part.rotationZ)
    );

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

## Генерация прямоугольных панелей (shape = null)

Когда `shape` не задан, деталь — прямоугольная панель (Box).

### Ключевая идея: что такое length, width и thickness

- **`lengthExpr`** — первый размер 2D-грани панели
- **`widthExpr`** — второй размер 2D-грани панели
- **`thickness`** (из component.params) — толщина панели (3-е измерение)

**Ориентация панели зависит от `role`:**

### Вертикальные панели: left, right, divider

Боковые стенки стоят вертикально. Их 2D-грань — это профиль «высота × глубина».

```
length = высота панели (по оси Y)
width  = глубина панели (по оси Z)
thickness = толщина (по оси X)

THREE.BoxGeometry(thickness, length, width)
```

```
Вид спереди:            Вид сверху:

  ┌──┐ ← thickness       ──────── ← width (Z)
  │  │     (X)           │      │
  │  │                   ────────
  │  │ ← length (Y)     ↑ thickness (X)
  │  │
  └──┘
```

**Пример — левая стенка:**
```
role="left", lengthExpr="H" → 720, widthExpr="D" → 560, thickness=16
→ BoxGeometry(16, 720, 560) at position (0, 0, 0)
```

**Пример — правая стенка:**
```
role="right", lengthExpr="H" → 720, widthExpr="D" → 560, thickness=16
→ BoxGeometry(16, 720, 560) at position (434, 0, 0)   [x = "W - t" = 434]
```

### Горизонтальные панели: top, bottom, shelf

Лежат горизонтально. Их 2D-грань — это «ширина × глубина».

```
length = ширина панели (по оси X)
width  = глубина панели (по оси Z)
thickness = толщина (по оси Y)

THREE.BoxGeometry(length, thickness, width)
```

```
Вид сверху:               Вид спереди:

  ┌────────────────┐       ════════ ← thickness (Y)
  │                │         ↕
  │                │       ← length (X) →
  └────────────────┘
  ↑ width (Z)
```

**Пример — нижняя панель:**
```
role="bottom", lengthExpr="W - 2*t" → 418, widthExpr="D" → 560, thickness=16
→ BoxGeometry(418, 16, 560) at position (16, 0, 0)   [x = "t" = 16]
```

**Пример — верхняя панель:**
```
role="top", lengthExpr="W - 2*t" → 418, widthExpr="D" → 560, thickness=16
→ BoxGeometry(418, 16, 560) at position (16, 560, 0)  [x = "t", y = "D"]
```

**Пример — полка:**
```
role="shelf", lengthExpr="W - 2*t - 2*shelfSideGap" → 414, widthExpr="D - shelfRearInset - shelfFrontInset" → 530, thickness=16
→ BoxGeometry(414, 16, 530) at position (18, 0, 10)   [x = "t + shelfSideGap", z = "shelfFrontInset"]
```

### Задняя стенка: back

Стоит вертикально, параллельно фронту. Её 2D-грань — «ширина × высота».

```
length = ширина панели (по оси X)
width  = высота панели (по оси Y)
thickness = толщина (по оси Z)

THREE.BoxGeometry(length, width, thickness)
```

**Пример — задняя стенка (если shape = null):**
```
role="back", lengthExpr="W - 2*t" → 418, widthExpr="H - 2*t" → 688, thickness=4
→ BoxGeometry(418, 688, 4) at position (16, 16, -10)   [x = "t", y = "t", z = "-10"]
```

### Сводная таблица ориентации

| Role | length по оси | width по оси | thickness по оси | BoxGeometry(a, b, c) |
|------|--------------|-------------|-----------------|---------------------|
| left, right, divider | Y (высота) | Z (глубина) | X | (thickness, length, width) |
| top, bottom, shelf | X (ширина) | Z (глубина) | Y | (length, thickness, width) |
| back | X (ширина) | Y (высота) | Z | (length, width, thickness) |

### Реализация

```typescript
function createRectPanel(length: number, width: number, thickness: number, role: string): THREE.Mesh {
  let geometry: THREE.BoxGeometry;

  switch (role) {
    case 'left':
    case 'right':
    case 'divider':
      geometry = new THREE.BoxGeometry(thickness, length, width);
      break;
    case 'top':
    case 'bottom':
    case 'shelf':
      geometry = new THREE.BoxGeometry(length, thickness, width);
      break;
    case 'back':
      geometry = new THREE.BoxGeometry(length, width, thickness);
      break;
    default:
      // Для неизвестных ролей — горизонтальная ориентация по умолчанию
      geometry = new THREE.BoxGeometry(length, thickness, width);
  }

  return new THREE.Mesh(geometry, getPanelMaterial(role));
}
```

---

## Генерация панелей с контуром (shape != null)

Когда `shape` задан, деталь имеет непрямоугольную форму. Нужно:
1. Вычислить координаты всех точек контура
2. Построить `THREE.Shape` (2D-полигон)
3. Экструдировать на толщину панели

### Как устроен контур

- Контур начинается в точке **(0, 0)**
- Каждый `LineSegment` задаёт следующую вершину `(x, y)`
- После последнего сегмента контур **автоматически замыкается** обратно в (0, 0)
- Координаты `x` и `y` в каждом сегменте — **строковые выражения**, которые нужно вычислить через `evaluate()`

### 2D-плоскость контура зависит от role

Контур определяет форму 2D-грани панели. В какой 3D-плоскости эта грань находится — зависит от ориентации:

| Role | 2D контур в плоскости | Экструзия по оси | Что значат x/y в shape |
|------|----------------------|-------------------|----------------------|
| top, bottom, shelf | XZ (горизонтальная) | Y (толщина вверх) | shape.x → мировая X, shape.y → мировая Z |
| back | XY (вертикальная, фронтальная) | Z (толщина вглубь) | shape.x → мировая X, shape.y → мировая Y |
| left, right, divider | YZ (вертикальная, боковая) | X (толщина вбок) | shape.x → мировая Y, shape.y → мировая Z |

### Реализация

```typescript
function createExtrudedPanel(
  segments: ShapeSegment[],
  params: Record<string, number>,
  thickness: number,
  role: string
): THREE.Mesh {
  // 1. Строим 2D-контур
  const shape = new THREE.Shape();
  // Начальная точка (0, 0) уже задана по умолчанию

  for (const seg of segments) {
    const sx = evaluate(seg.x, params);
    const sy = evaluate(seg.y, params);

    if (seg.type === 'line') {
      shape.lineTo(sx, sy);
    } else if (seg.type === 'arc') {
      // Для дугового сегмента — конвертировать SVG-arc в bezier
      // (пока в данных дуг нет, все контуры состоят из линий)
      const radius = evaluate(seg.radius, params);
      addArcSegment(shape, sx, sy, radius, seg.largeArc, seg.clockwise);
    }
  }

  shape.closePath();

  // 2. Экструдируем
  const extrudeSettings: THREE.ExtrudeGeometryOptions = {
    depth: thickness,
    bevelEnabled: false
  };

  const geometry = new THREE.ExtrudeGeometry(shape, extrudeSettings);

  // 3. Поворачиваем геометрию чтобы экструзия шла по правильной оси
  //    THREE.ExtrudeGeometry экструдирует по локальной Z
  //    Нужно повернуть чтобы контур лежал в правильной плоскости
  switch (role) {
    case 'top':
    case 'bottom':
    case 'shelf':
      // Контур в XZ, экструзия по Y
      geometry.rotateX(-Math.PI / 2);
      break;
    case 'back':
      // Контур в XY, экструзия по Z — дефолт ExtrudeGeometry, ничего не делаем
      break;
    case 'left':
    case 'right':
    case 'divider':
      // Контур в YZ, экструзия по X
      geometry.rotateY(Math.PI / 2);
      break;
  }

  return new THREE.Mesh(geometry, getPanelMaterial(role));
}
```

---

## Полный пример: «Нижняя база однодверный» (base-single-door)

### Параметры

```json
{
  "W": 450, "H": 720, "D": 560,
  "t": 16, "tBack": 4, "facadeGap": 2,
  "shelfSideGap": 2, "shelfRearInset": 20, "shelfFrontInset": 10
}
```

### 6 деталей

#### 0. Левая стенка (left)

```
lengthExpr: "H"      → 720    (высота)
widthExpr:  "D"      → 560    (глубина)
x: "0"  y: "0"  z: "0"
thickness: 16mm (Стенка)
→ BoxGeometry(16, 720, 560) at (0, 0, 0)
```

```
Вид спереди:
  ┌──┐
  │  │ 720mm
  │  │
  └──┘
  16mm
```

#### 1. Правая стенка (right)

```
lengthExpr: "H"      → 720
widthExpr:  "D"      → 560
x: "W - t" → 434    y: "0"  z: "0"
thickness: 16mm
→ BoxGeometry(16, 720, 560) at (434, 0, 0)
```

#### 2. Верхняя панель (top)

```
lengthExpr: "W - 2*t"  → 418   (ширина минус две стенки)
widthExpr:  "D"         → 560
x: "t" → 16   y: "D" → 560   z: "0"
thickness: 16mm
→ BoxGeometry(418, 16, 560) at (16, 560, 0)
```

#### 3. Нижняя панель (bottom)

```
lengthExpr: "W - 2*t"  → 418
widthExpr:  "D"         → 560
x: "t" → 16   y: "0"   z: "0"
thickness: 16mm
→ BoxGeometry(418, 16, 560) at (16, 0, 0)
```

#### 4. Задняя стенка (back) — с контуром!

```
lengthExpr: "W - 2*t"  → 418
widthExpr:  "H - 2*t"  → 688
x: "t" → 16   y: "t" → 16   z: "-10"
thickness: 4mm (Задняя стенка)
```

Shape (8 точек — прямоугольник с вырезами 25×50 в верхних углах):
```json
[
  {"type":"line", "x":"0",           "y":"0"},
  {"type":"line", "x":"W - 2*t",     "y":"0"},
  {"type":"line", "x":"W - 2*t",     "y":"H - 2*t - 50"},
  {"type":"line", "x":"W - 2*t - 25","y":"H - 2*t - 50"},
  {"type":"line", "x":"W - 2*t - 25","y":"H - 2*t"},
  {"type":"line", "x":"25",          "y":"H - 2*t"},
  {"type":"line", "x":"25",          "y":"H - 2*t - 50"},
  {"type":"line", "x":"0",           "y":"H - 2*t - 50"}
]
```

Вычисленные координаты:
```
(0,0) → (418,0) → (418,638) → (393,638) → (393,688) → (25,688) → (25,638) → (0,638)
```

```
       25              393
       ┌────────────────┐  688 (H-2t)
       │                │
  638  ├──┐          ┌──┤  638 (H-2t-50)
       │  │          │  │
       │  │          │  │
       │  │  418mm   │  │
       │  │ (W-2t)   │  │
       └──┴──────────┴──┘  0
       0                 418
```

→ `ExtrudeGeometry` этого контура с `depth: 4`, role="back" → контур в XY, экструзия по Z.

#### 5. Полка (shelf)

```
lengthExpr: "W - 2*t - 2*shelfSideGap"          → 450 - 32 - 4 = 414
widthExpr:  "D - shelfRearInset - shelfFrontInset" → 560 - 20 - 10 = 530
x: "t + shelfSideGap" → 18   y: "0"   z: "shelfFrontInset" → 10
thickness: 16mm (Полка)
→ BoxGeometry(414, 16, 530) at (18, 0, 10)
```

### Визуализация сборки (вид спереди)

```
  0    16  18                    432 434  450
  ├────┼───┼────────────────────┼───┼────┤

  ┌────┬───────────────────────────┬────┐ 720
  │    │▓▓▓▓▓▓▓▓▓ top ▓▓▓▓▓▓▓▓▓▓▓│    │
  │    ├───────────────────────────┤    │
  │    │                           │    │
  │ L  │                           │ R  │
  │ E  │     (внутреннее          │ I  │
  │ F  │      пространство)       │ G  │
  │ T  │                           │ H  │
  │    │  ═══════ shelf ════════   │ T  │
  │    │                           │    │
  │    ├───────────────────────────┤    │
  │    │▓▓▓▓▓▓▓ bottom ▓▓▓▓▓▓▓▓▓▓│    │
  └────┴───────────────────────────┴────┘ 0
```

---

## Пример: Диагональный угловой шкаф (wall-corner-diagonal)

### Параметры
```json
{ "W": 600, "H": 720, "D": 600, "t": 16, ... }
```

### Top/Bottom shape — пятиугольник (diagonal pentagon)

```json
[
  {"type":"line", "x":"0",             "y":"0"},
  {"type":"line", "x":"W - 2*t",       "y":"0"},
  {"type":"line", "x":"W - 2*t",       "y":"W - 2*t - 300"},
  {"type":"line", "x":"W - 2*t - 300", "y":"W - 2*t"},
  {"type":"line", "x":"0",             "y":"W - 2*t"}
]
```

С W=600, t=16 → inner=568, cut=268:

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

Это **вид сверху** (плоскость XZ). Role = "top"/"bottom" → контур экструдируется по Y на 16mm.

---

## Пример: L-образный угловой шкаф (wall-corner-l-shaped)

### Параметры
```json
{ "W": 600, "H": 720, "D": 600, "t": 16, ... }
```

### Top/Bottom shape — L-форма (hexagon, 6 точек)

```json
[
  {"type":"line", "x":"0",       "y":"0"},
  {"type":"line", "x":"W - 2*t", "y":"0"},
  {"type":"line", "x":"W - 2*t", "y":"304"},
  {"type":"line", "x":"304",     "y":"304"},
  {"type":"line", "x":"304",     "y":"D - 2*t"},
  {"type":"line", "x":"0",       "y":"D - 2*t"}
]
```

С W=600, D=600, t=16 → innerW=568, innerD=568:

```
  (0,568)────(304,568)
    │             │
    │             │
    │        (304,304)────────(568,304)
    │                              │
    │                              │
  (0,0)──────────────────────(568,0)
```

Это тоже **вид сверху** (XZ). Экструзия по Y.

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

| Деталь | Старый размер | Новый размер | Что изменилось |
|--------|-------------|-------------|---------------|
| left | 16×720×560 at (0,0,0) | 16×720×560 at (0,0,0) | Ничего |
| right | 16×720×560 at (434,0,0) | 16×720×560 at (584,0,0) | Сдвинулась по X |
| top | 418×16×560 at (16,560,0) | 568×16×560 at (16,560,0) | Стала шире |
| bottom | 418×16×560 at (16,0,0) | 568×16×560 at (16,0,0) | Стала шире |
| back | контур 418×688 | контур 568×688 | Контур пересчитан |
| shelf | 414×16×530 at (18,0,10) | 564×16×530 at (18,0,10) | Стала шире |

---

## Типы сборок — какие shape используются

| Type | Top/Bottom shape | Back shape | Описание |
|------|-----------------|------------|----------|
| `base-single-door` | null (прямоугольник) | 8-point notched | Стандартный однодверный |
| `base-double-door` | null | 8-point notched | Стандартный двудверный |
| `wall-single-door` | null | 8-point notched | Верхний однодверный |
| `wall-double-door` | null | 8-point notched | Верхний двудверный |
| `mezzanine` | null | 8-point notched | Антресоль |
| `*-corner-straight-*` | null | 8-point notched | Прямые угловые |
| `*-corner-diagonal` | 5-point pentagon | null (прямоугольник) | Диагональные угловые |
| `*-corner-l-shaped` | 6-point L-shape | 8-point notched | L-образные угловые |

---

## Материалы / цвета (рекомендация)

Разные роли деталей можно визуально различать:

| Role | Предлагаемый цвет | Описание |
|------|-------------------|----------|
| left, right | #D4A574 | Боковые стенки — дерево |
| top, bottom | #C89660 | Горизонтальные панели — чуть темнее |
| back | #E8D5B7 | Задняя стенка — светлее, тоньше |
| shelf | #B8956A | Полки — отдельный оттенок |
| facade | #F5E6D0 | Фасады — самый светлый |

---

## Auth для запросов

```
X-User-Id: 44444444-4444-4444-4444-444444444444
X-User-Role: systemAdmin
```
