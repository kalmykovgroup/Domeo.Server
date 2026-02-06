# Modules API - Cabinet Assembly System

## Overview

Modules API manages parametric kitchen cabinet assemblies. Each assembly is a **template** that describes how to build a cabinet from parts. The 3D rendering on the current UI is **hardcoded** - it does not use the API data model. This document describes the data-driven approach that should replace it.

## Data Model

```
Assembly (template: "base-single-door")
  |-- dimensions: { width: 450, depth: 560, height: 720 }
  |-- construction: { panelThickness: 16, backPanelThickness: 4, ... }
  |-- constraints: { widthMin: 300, widthMax: 600, ... }
  |
  |-- parts[] (AssemblyPart - the BINDING, not just a reference)
       |-- role: "Left" | "Right" | "Top" | "Bottom" | "Back" | "Shelf" | ...
       |-- placement: { anchorX, anchorY, anchorZ, offsetX/Y/Z, rotationX/Y/Z }
       |-- length: DynamicSize (how to calculate this part's length)
       |-- width: DynamicSize (how to calculate this part's width)
       |-- quantity: 1
       |-- quantityFormula: null | formula string
       |-- sortOrder: 0..N
       |
       |-- component (the BUILDING BLOCK)
            |-- name: "Стенка" | "Задняя стенка" | "Полка"
            |-- tags: ["panel", "wall"] | ["panel", "back"] | ["panel", "shelf"]
            |-- params: { type: "panel", thickness: 16 } | { type: "glb", glbUrl, scale }
```

### Key Principle: AssemblyPart != Component

**AssemblyPart** is the **binding** that defines WHERE and HOW a component is placed inside an assembly:
- `role` - semantic role (Left wall, Right wall, Back panel, Shelf, etc.)
- `placement` - position anchoring and offsets (3D coordinates)
- `length/width` - dynamic sizing rules (derived from parent dimensions)
- `quantity` - how many of this part
- `sortOrder` - rendering/processing order

**Component** is the **building block** itself:
- `params.type: "panel"` - a flat panel with `thickness`
- `params.type: "glb"` - a 3D model loaded from URL with `scale`
- `tags` - semantic classification for filtering

**You need BOTH** to build geometry:
- AssemblyPart tells you: where to place it, what size to make it, what role it plays
- Component tells you: what shape it is (panel/3D model), what material thickness

## API Endpoints

Base URL: `http://localhost:5008` (direct) or `http://localhost/api/modules` (via gateway)

### Auth
All endpoints require gateway headers:
```
X-User-Id: 44444444-4444-4444-4444-444444444444
X-User-Role: systemAdmin
```

### Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/assemblies` | All assemblies with parts + components |
| GET | `/assemblies/count` | Count assemblies (with optional filters) |
| GET | `/components` | List all components |
| GET | `/categories` | List all categories |
| GET | `/categories/tree` | Category tree |

### Key endpoint: GET /assemblies

Returns all assemblies with parts and embedded components in one request. Supports optional filtering:

| Param | Type | Description |
|-------|------|-------------|
| `categoryId` | string? | Filter by category |
| `activeOnly` | bool? | Only active assemblies |
| `search` | string? | Search by name |
| `page` | int? | Page number (enables pagination) |
| `limit` | int? | Page size (enables pagination) |

Without `page`/`limit` returns flat array in `data`. With pagination returns `{ total, page, pageSize, items }`.

Example response (without pagination):

```json
{
  "success": true,
  "data": [
    {
      "id": "e18b3383-a905-41c9-a8f6-07acd3cc833c",
      "categoryId": "base_single_door",
      "type": "base-single-door",
      "name": "Нижняя база однодверный",
      "dimensions": { "width": 450, "depth": 560, "height": 720 },
      "constraints": {
        "widthMin": 300, "widthMax": 600,
        "heightMin": 720, "heightMax": 720,
        "depthMin": 500, "depthMax": 600
      },
      "construction": {
        "panelThickness": 16,
        "backPanelThickness": 4,
        "facadeThickness": 18,
        "facadeGap": 2,
        "facadeOffset": 0,
        "shelfSideGap": 2,
        "shelfRearInset": 20,
        "shelfFrontInset": 10
      },
      "isActive": true,
      "createdAt": "2026-02-06T...",
      "parts": [
        {
          "id": "7d41975f-...",
          "assemblyId": "e18b3383-...",
          "componentId": "0c934475-...",
          "role": "Left",
          "placement": {
            "anchorX": "Start", "anchorY": "Start", "anchorZ": "Start",
            "offsetX": 0, "offsetY": 0, "offsetZ": 0,
            "rotationX": 0, "rotationY": 0, "rotationZ": 0
          },
          "length": { "source": "ParentHeight", "offset": 0, "fixedValue": null },
          "width": { "source": "ParentDepth", "offset": 0, "fixedValue": null },
          "quantity": 1,
          "quantityFormula": null,
          "sortOrder": 0,
          "component": {
            "id": "0c934475-...",
            "name": "Стенка",
            "tags": ["panel", "wall"],
            "params": { "type": "panel", "thickness": 16 },
            "isActive": true,
            "createdAt": "2026-02-06T..."
          }
        },
        {
          "role": "Right",
          "placement": {
            "anchorX": "End", "anchorY": "Start", "anchorZ": "Start",
            "offsetX": 0, "offsetY": 0, "offsetZ": 0
          },
          "length": { "source": "ParentHeight", "offset": 0, "fixedValue": null },
          "width": { "source": "ParentDepth", "offset": 0, "fixedValue": null },
          "component": { "name": "Стенка", "params": { "type": "panel", "thickness": 16 } }
        },
        {
          "role": "Top",
          "placement": { "anchorX": "Start", "anchorY": "End", "anchorZ": "Start", "offsetX": 16 },
          "length": { "source": "ParentWidth", "offset": -32, "fixedValue": null },
          "width": { "source": "ParentDepth", "offset": 0, "fixedValue": null },
          "component": { "name": "Стенка", "params": { "type": "panel", "thickness": 16 } }
        },
        {
          "role": "Bottom",
          "placement": { "anchorX": "Start", "anchorY": "Start", "anchorZ": "Start", "offsetX": 16 },
          "length": { "source": "ParentWidth", "offset": -32, "fixedValue": null },
          "width": { "source": "ParentDepth", "offset": 0, "fixedValue": null },
          "component": { "name": "Стенка", "params": { "type": "panel", "thickness": 16 } }
        },
        {
          "role": "Back",
          "placement": { "anchorX": "Start", "anchorY": "Start", "anchorZ": "End", "offsetX": 16 },
          "length": { "source": "ParentWidth", "offset": -32, "fixedValue": null },
          "width": { "source": "ParentHeight", "offset": 0, "fixedValue": null },
          "component": { "name": "Задняя стенка", "params": { "type": "panel", "thickness": 4 } }
        },
        {
          "role": "Shelf",
          "placement": { "anchorX": "Start", "anchorY": "Center", "anchorZ": "Start", "offsetX": 18, "offsetZ": 10 },
          "length": { "source": "ParentWidth", "offset": -36, "fixedValue": null },
          "width": { "source": "ParentDepth", "offset": -30, "fixedValue": null },
          "component": { "name": "Полка", "params": { "type": "panel", "thickness": 16 } }
        }
      ]
    }
  ]
}
```

## How to Build 3D Geometry (JSCAD / Three.js)

### Current State (HARDCODED - must be replaced)
The current UI hardcodes cabinet geometry: it knows "base-single-door" means 4 walls + back + shelf and manually creates boxes. This ignores the API data.

### Target Approach (DATA-DRIVEN)
The renderer should iterate over `parts[]` and build geometry from each part's data:

```
for each part in assembly.parts (sorted by sortOrder):
  1. Determine SHAPE from component.params.type:
     - "panel" -> flat box (cuboid)
     - "glb"   -> load 3D model from glbUrl

  2. Determine SIZE from part + assembly:
     - part.length and part.width are always DynamicSize objects (never null)
     - Calculate: resolvedValue = parentDimension[source] + offset
       - source: "ParentWidth" | "ParentDepth" | "ParentHeight" | "Fixed"
       - offset: adjustment in mm (e.g., -32 for double panel thickness)
       - fixedValue: absolute value when source is Fixed (3)
     - Thickness comes from component.params.thickness

  3. Determine POSITION from part.placement:
     - anchorX/Y/Z: "Start" | "Center" | "End" - anchor point
     - offsetX/Y/Z: offset from anchor in mm
     - rotationX/Y/Z: rotation in degrees

  4. Apply quantity (part.quantity) - repeat with formula if quantityFormula set
```

### Role-Based DynamicSize Values (always populated by server)

Server always returns `length` and `width` as DynamicSize objects. The client calculates real dimensions:

`t` = panelThickness (16), `sg` = shelfSideGap (2), `ri` = shelfRearInset (20), `fi` = shelfFrontInset (10)

| Role | Length | Width | Thickness |
|------|--------|-------|-----------|
| Left | ParentHeight, offset: 0 | ParentDepth, offset: 0 | 16 |
| Right | ParentHeight, offset: 0 | ParentDepth, offset: 0 | 16 |
| Top | ParentWidth, offset: -2t (-32) | ParentDepth, offset: 0 | 16 |
| Bottom | ParentWidth, offset: -2t (-32) | ParentDepth, offset: 0 | 16 |
| Back | ParentWidth, offset: -2t (-32) | ParentHeight, offset: 0 | 4 |
| Shelf | ParentWidth, offset: -2t-2sg (-36) | ParentDepth, offset: -ri-fi (-30) | 16 |

### Placement per Role

| Role | AnchorX | AnchorY | AnchorZ | OffsetX | OffsetY | OffsetZ |
|------|---------|---------|---------|---------|---------|---------|
| Left | Start | Start | Start | 0 | 0 | 0 |
| Right | End | Start | Start | 0 | 0 | 0 |
| Top | Start | End | Start | t (16) | 0 | 0 |
| Bottom | Start | Start | Start | t (16) | 0 | 0 |
| Back | Start | Start | End | t (16) | 0 | 0 |
| Shelf | Start | Center | Start | t+sg (18) | 0 | fi (10) |

### DynamicSize Object
```json
{
  "source": "ParentWidth",  // "ParentWidth" | "ParentDepth" | "ParentHeight" | "Fixed"
  "offset": -32,            // adjustment in mm (e.g., minus two panel thicknesses)
  "fixedValue": null         // used only when source is "Fixed"
}
```

## Component Types

### PanelParams (type: "panel")
```json
{ "type": "panel", "thickness": 16 }
```
Render as a cuboid/box. The thickness is the "thin" dimension.

### GlbParams (type: "glb")
```json
{ "type": "glb", "glbUrl": "/models/handle.glb", "scale": 1.0 }
```
Load a GLTF/GLB 3D model and apply scale.

## Available Assembly Types (seed data)

| Type | Category | Parts |
|------|----------|-------|
| base-single-door | base_single_door | Left, Right, Top, Bottom, Back, Shelf |
| base-double-door | base_double_door | Left, Right, Top, Bottom, Back, Shelf |
| base-corner-straight-left | base_corner | Left, Right, Top, Bottom, Back, Shelf |
| base-corner-straight-right | base_corner | Left, Right, Top, Bottom, Back, Shelf |
| base-corner-diagonal | base_corner | Left, Right, Top, Bottom, Back, Shelf |
| base-corner-l-shaped | base_corner | Left, Right, Top, Bottom, Back, Shelf |
| wall-single-door | wall_single_door | Left, Right, Top, Bottom, Back, Shelf |
| wall-double-door | wall_double_door | Left, Right, Top, Bottom, Back, Shelf |
| wall-corner-straight-left | wall_corner | Left, Right, Top, Bottom, Back, Shelf |
| wall-corner-straight-right | wall_corner | Left, Right, Top, Bottom, Back, Shelf |
| wall-corner-diagonal | wall_corner | Left, Right, Top, Bottom, Back, Shelf |
| wall-corner-l-shaped | wall_corner | Left, Right, Top, Bottom, Back, Shelf |
| mezzanine | mezzanine | Left, Right, Top, Bottom, Back, Shelf |

## Part Roles (enum)

| Role | Description |
|------|-------------|
| Left | Left side panel |
| Right | Right side panel |
| Top | Top panel (horizontal) |
| Bottom | Bottom panel (horizontal) |
| Back | Back panel (thin, 4mm) |
| Shelf | Interior shelf |
| Divider | Vertical interior divider |
| Facade | Door/drawer front panel |
| Hinge | Door hinge (glb model) |
| Handle | Door/drawer handle (glb model) |
| Leg | Cabinet leg/support (glb model) |
| DrawerSlide | Drawer slide mechanism (glb model) |

## Anchor Origins

```
Start  = left/bottom/front edge
Center = center of dimension
End    = right/top/back edge
```

Serialized as strings: "Start", "Center", "End"
