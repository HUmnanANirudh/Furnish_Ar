# Furnish AR Design System & UI Specification

## 1. Design Philosophy & Direction

Furnish AR utilizes a warm, minimalist **Japandi & Scandinavian-inspired** aesthetic merged with functional, high-contrast mobile ergonomic controls. 

Key principles governing the interface:
1. **Frictionless Spatial Interaction:** Controls are oversized, high-contrast, and placed along the lower thumb-zone of modern tall mobile screens (20:9 aspect ratio) to allow easy one-handed operation while holding a phone in AR.
2. **Immediate State Clarity:** The user must never wonder whether a 3D object is moveable or locked. Status is reinforced both on the 2D primary HUD button (`LOCK POSITION`) and in 3D space above the furniture with a dynamic floating billboard badge.
3. **In-Editor WYSIWYG Transparency:** All UI components exist as real baked uGUI hierarchies in the scene canvas, styled with clean flat cards, subtle borders, and clear typographic hierarchy that render accurately in both Editor and Device viewports.

---

## 2. Color System & Tokens

### 2.1 Palette Reference

| Token Name | Hex Code | Unity `Color(r, g, b, a)` | Purpose |
| :--- | :--- | :--- | :--- |
| **`ColorBgPrimary`** | `#F8F6F1` | `(0.973, 0.965, 0.945, 1.0)` | Off-white warm ivory base for buttons & inactive cards |
| **`ColorBgSurface`** | `#FFFFFF` | `(1.0, 1.0, 1.0, 0.98)` | High-opacity surface white for floating bars and panels |
| **`ColorAccentTeal`** | `#39B6B6` | `(0.224, 0.714, 0.714, 1.0)` | Primary brand action color (`LOCK`, `ACTIVE` tags, highlights) |
| **`ColorAccentActive`** | `#EBF8F8` | `(0.920, 0.980, 0.980, 1.0)` | Subtle teal tint applied to selected furniture card backgrounds |
| **`ColorTextPrimary`** | `#242424` | `(0.141, 0.141, 0.141, 1.0)` | High-contrast dark charcoal ink for headlines and labels |
| **`ColorTextSecondary`**| `#858585` | `(0.522, 0.522, 0.522, 1.0)` | Muted neutral gray for secondary actions and hints |
| **`ColorBorderSubtle`** | `#E0DDD6` | `(0.880, 0.870, 0.840, 1.0)` | Crisp 1px outline for inactive cards and separators |
| **`ColorHandle`** | `#CCC8C0` | `(0.800, 0.790, 0.760, 1.0)` | Sheet drag handle pill indicator |
| **`ColorStatusLocked`** | `#0FA870` | `(0.060, 0.680, 0.440, 0.94)` | Emerald green indicating locked/anchored position |
| **`ColorStatusUnlocked`**| `#E98C1E`| `(0.920, 0.550, 0.120, 0.94)` | Warm amber indicating editable/unlocked state |

---

## 3. Typography Hierarchy

The UI uses native sans-serif system fonts (`LegacyRuntime.ttf` / `Arial.ttf` / `sans-serif`) with explicit font weight and sizing hierarchy:

| Element | Size | Weight / Style | Color | Alignment |
| :--- | :--- | :--- | :--- | :--- |
| **Top Status Header** | 20 px | Bold | `#242424` | Center |
| **Top Header Actions** (`RESET`, `PHOTO`) | 17 px | Bold | `#242424` / `#858585` | Center |
| **Primary Action** (`LOCK POSITION`) | 24 px | Bold | `#FFFFFF` | Center |
| **Secondary Controls** (`ROTATE`, `DEL`) | 16-18 px | Bold | `#242424` | Center |
| **Sofa Card Label** | 20 px | Bold (Active) / Normal | `#39B6B6` / `#242424` | Center |
| **Card Active Badge** | 14 px | Bold | `#FFFFFF` | Center |
| **3D World Lock Badge** | 36 px | Bold | `#FFFFFF` | Center |
| **Toast Notifications** | 22 px | Normal | `#FFFFFF` | Center |

---

## 4. Layout Architecture & Dimensions

### 4.1 Canvas Configuration
- **Render Mode:** `Screen Space - Overlay`
- **Canvas Scaler:** `Scale With Screen Size`
  - Reference Resolution: **1080 × 2400** (Modern smartphone portrait)
  - Screen Match Mode: `Match Width Or Height`
  - Match: **`0.5`** (Equal weighting for width and height scaling)

```
+-------------------------------------------------------------+
|                      [AR Camera View]                       |
|                                                             |
|  +-------------------------------------------------------+  |
|  | [RESET]      Tap floor to place White Sofa     [PHOTO]|  | <- TopFloatingHeader (980 x 76)
|  +-------------------------------------------------------+  |
|                                                             |
|                         [ 3D SOFA ]                         |
|                      [ LOCKED BADGE ]                       |
|                                                             |
|                                                             |
|  +-------------------------------------------------------+  |
|  |                        ( === )                        |  | <- HandlePill (64 x 6)
|  | +----------+ +-----------------------+ +----------+-+ |  |
|  | | ROTATE L | |     LOCK POSITION     | | ROTATE R |X| |  | <- ControlRow (Height 78)
|  | +----------+ +-----------------------+ +----------+-+ |  |
|  |                                                       |  |
|  | +-------------+   +-------------+   +---------------+ |  |
|  | | [ACTIVE]    |   |             |   |               | |  |
|  | | White Sofa  |   | Brown Sofa  |   |Lt. Brown Sofa | |  | <- SofaSelectionRow (Height 220)
|  | +-------------+   +-------------+   +---------------+ |  |
|  +-------------------------------------------------------+  | <- UnifiedBottomPanel (1020 x 360)
+-------------------------------------------------------------+
```

### 4.2 Top Floating Header
- **Object Name:** `TopFloatingHeader`
- **Anchoring:** Top-Center (`anchorMin: (0.5, 1.0)`, `anchorMax: (0.5, 1.0)`, `pivot: (0.5, 1.0)`)
- **Position & Size:** `anchoredPosition: (0, -130)`, `sizeDelta: (980, 76)`
- **Background:** `ColorBgSurface` with rounded card styling.
- **Children:**
  - `ResetBtn`: Left-aligned (`anchoredPosition: (16, 0)`), size `130 × 52 px`, background `ColorBgPrimary`, text `"RESET"`.
  - `StatusLabel`: Stretched between margins `(160, 0)` to `(-160, 0)`, centered bold 20px guidance text.
  - `PhotoBtn`: Right-aligned (`anchoredPosition: (-16, 0)`), size `130 × 52 px`, background `ColorBgPrimary`, text `"PHOTO"`.

### 4.3 Unified Bottom Panel
- **Object Name:** `UnifiedBottomPanel`
- **Anchoring:** Bottom-Center (`anchorMin: (0.5, 0.0)`, `anchorMax: (0.5, 0.0)`, `pivot: (0.5, 0.0)`)
- **Position & Size:** `anchoredPosition: (0, 40)`, `sizeDelta: (1020, 360)`
- **Background:** `ColorBgSurface` with rounded card styling.
- **Children:**
  - `HandlePill`: Centered at top `(0, -10)`, size `64 × 6 px`, color `ColorHandle`.
  - `ControlRow`: Height `78 px`, anchored `Y = -28 px`. Uses `HorizontalLayoutGroup` (spacing `14 px`, padding `16 px` sides):
    - `RotLBtn`: Size `170 × 72 px`, text `"ROTATE L\n(-15°)"`.
    - `LockBtn`: Size `470 × 72 px`, background `ColorAccentTeal`, text `"LOCK POSITION"` (changes to `"UNLOCK POSITION"` when locked).
    - `RotRBtn`: Size `170 × 72 px`, text `"ROTATE R\n(+15°)"`.
    - `DelBtn`: Size `84 × 72 px`, background `ColorBgPrimary`, text `"DEL"`.
  - `SofaSelectionRow`: Height `220 px`, anchored `Y = 16 px`. Uses `HorizontalLayoutGroup` (spacing `16 px`, padding `16 px` sides):
    - `Card_white_sofa`: Size `310 × 210 px`.
    - `Card_brown_sofa`: Size `310 × 210 px`.
    - `Card_light_brown_sofa`: Size `310 × 210 px`.

### 4.4 Furniture Selection Card Anatomy
Each sofa card is a composite component containing:
1. **Card Background:** `Image` component. Active card uses `ColorAccentActive` (`#EBF8F8`); inactive uses `ColorBgPrimary` (`#F8F6F1`).
2. **Card Border:** 1px perimeter overlay image. Active card uses `ColorAccentTeal` (`#39B6B6`); inactive uses `ColorBorderSubtle` (`#E0DDD6`).
3. **Thumbnail:** Aspect-fitted `Image` occupying top 70% of the card area (`anchorMin: (0.08, 0.28)`, `anchorMax: (0.92, 0.94)`).
4. **Label:** Centered text occupying bottom 25% (`anchorMin: (0.04, 0.04)`, `anchorMax: (0.96, 0.28)`).
5. **ActiveBadge:** Top-right anchored pill (`sizeDelta: (80, 28)`), background `ColorAccentTeal`, white 14px bold text `"ACTIVE"`. Visible only on the currently active sofa.

### 4.5 3D World-Space Lock Badge
- **Object Name:** `LockBadgeCanvas` (child of placed furniture instance)
- **Render Mode:** `WorldSpace`
- **Scale:** `(0.0035, 0.0035, 0.0035)`
- **Canvas Size:** `300 × 80 px`
- **Positioning:** Dynamically calculated at runtime by querying all active `MeshRenderer` bounds on the sofa, positioning the badge **`+0.25m`** above the highest point of the model.
- **Billboarding:** In `LateUpdate()`, aligns rotation with `Camera.main.transform.rotation` so it always faces the user regardless of camera angle.
- **States:**
  - **Unlocked:** Background `#E98C1E` (Amber), Text `"UNLOCKED"`.
  - **Locked:** Background `#0FA870` (Emerald Green), Text `"LOCKED"`.

---

## 5. Interaction States & Animations

### 5.1 Locking Flow
```mermaid
stateDiagram-v2
    [*] --> Unlocked: Furniture Placed
    Unlocked --> Locked: Tap 'LOCK POSITION'
    Locked --> Unlocked: Tap 'UNLOCK POSITION'
    
    state Unlocked {
        LockBtn: Teal Background / "LOCK POSITION"
        WorldBadge: Amber Background / "UNLOCKED"
        FloorHitTest: Active (Object drags with touch)
    }
    
    state Locked {
        LockBtn: Coral or Muted Gray / "UNLOCK POSITION"
        WorldBadge: Green Background / "LOCKED"
        FloorHitTest: Ignored (Object anchored in world)
    }
```

### 5.2 Hot-Swapping Furniture Models
When a user taps an inactive sofa card while an active furniture instance is on screen:
1. The position and Y-rotation of the current instance are preserved.
2. The current instance is cleanly destroyed.
3. The newly selected sofa prefab is spawned at the exact same world coordinate and rotation angle.
4. The 3D World Lock Badge is immediately attached and recalculated to match the new model's height.
5. The bottom selection card states and top status label update instantly.

### 5.3 Snapshot Capture & Flash Effect
1. User taps the `"PHOTO"` button in the top header.
2. The UI momentarily hides controls or activates `SnapshotFlashOverlay`.
3. `SnapshotFlashOverlay` rapidly ramps `CanvasGroup.alpha` to `1.0` (white flash) and smoothly fades to `0.0` over 350ms.
4. A full-resolution screenshot is written to device storage with timestamp.
5. A dark floating `ToastNotification` (`"Snapshot saved to gallery!"`) appears at `Y = 44%` and auto-dismisses after 2.5 seconds.

---

## 6. Guidelines for Extending the UI

1. **Maintain Unified Bottom Placement:** Do not scatter icon-only buttons around random screen corners. Group manipulation actions in `ControlRow` and catalog items in `SofaSelectionRow`.
2. **Hit Target Sizes:** Every interactive touch target must be at least **48 × 48 dp** (at 1080×2400 reference resolution, minimum button height is `65 px`).
3. **Typography Standard:** Always use clean, uppercase typographic labels for actions (`RESET`, `PHOTO`, `ROTATE L`, `LOCK POSITION`, `DEL`).
4. **Code Quality Rule:** Do not add code comments or emojis to C# scripts in this repository. All architectural and functional decisions must be documented here in [`DESIGN.md`](file:///home/anni/Furnish_Ar/DESIGN.md) and [`PRODUCT.md`](file:///home/anni/Furnish_Ar/PRODUCT.md).
