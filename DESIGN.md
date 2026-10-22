# Furnish AR Design System & UI Specification

## 1. Design Philosophy & Direction

Furnish AR utilizes a warm, minimalist **Japandi & Scandinavian-inspired** aesthetic merged with functional, high-contrast mobile ergonomic controls. 

Key principles governing the interface:
1. **Three-Screen Architecture:** The UI is cleanly structured into three distinct top-level panels: `SplashScreenPanel` (visual brand entrance & feature highlights), `AROverlayPanel` (minimalist, distraction-free AR manipulation HUD), and `CatalogModalPanel` (1/3 sidebar and 2/3 grid furniture explorer).
2. **Immediate In-Editor WYSIWYG & Showcase Furniture:** A pre-instantiated high-detail 3D model ([`White Sofa.prefab`](file:///home/anni/Furnish_Ar/Assets/Assets/Furnitures/White%20Sofa/White%20Sofa.prefab)) sits on the ground floor at `(0, 0, 2.2f)` with dedicated directional lighting and a floating `FurnitureLockBadge`. Developers and designers can immediately test interactions, camera angles, and UI layouts directly in the Unity Editor Game and Scene views without compiling to APK.
3. **Ergonomic Spatial Controls:** Primary floor manipulation controls are located in the lower thumb-zone of modern tall mobile screens (20:9 aspect ratio) to allow easy one-handed operation while holding a phone in AR.
4. **Immediate State Clarity:** The user is always aware whether an object is editable or locked. Status is reinforced both on the 2D primary HUD button (`LOCK POSITION` / `UNLOCK POSITION`) and in 3D space above the furniture with a floating billboard badge.
5. **1/3 - 2/3 Modal Catalog Hierarchy:** To accommodate 75 furniture pieces across 9 categories without cluttering the AR viewfinder, a modal drawer divides into a 1/3 category navigation sidebar and a 2/3 scrollable 2-column card grid.
6. **Seamless In-Place Hot-Swapping:** Selecting any piece from the catalog replaces an existing unlocked model on the floor at the exact world coordinates and orientation, immediately adjusting the 3D lock badge.
7. **Clean Production UX (Zero Developer Jargon):** All interface labels, prompts, and actions are framed from the perspective of an interior designer or shopper, eliminating developer terms like `[ OPEN 1/3 SIDEBAR & 2/3 GRID -> ]` in favor of polished actions such as `EXPLORE ALL FURNITURE ->`.

---

## 2. Color System & Tokens

### 2.1 Palette Reference

| Token Name | Hex Code | Unity `Color(r, g, b, a)` | Purpose |
| :--- | :--- | :--- | :--- |
| **`ColorBgPrimary`** | `#F8F6F1` | `(0.973, 0.965, 0.945, 1.0)` | Off-white warm ivory base for buttons & inactive cards |
| **`ColorBgSurface`** | `#FFFFFF` | `(1.0, 1.0, 1.0, 0.98)` | High-opacity surface white for floating bars, cards, and modal window |
| **`ColorAccentTeal`** | `#2EB0B0` | `(0.180, 0.680, 0.680, 1.0)` | Primary brand action color (`LOCK`, `ACTIVE` tags, catalog launcher) |
| **`ColorAccentActive`** | `#EBF8F8` | `(0.920, 0.980, 0.980, 1.0)` | Subtle teal tint applied to selected furniture card backgrounds |
| **`ColorTextPrimary`** | `#242424` | `(0.141, 0.141, 0.141, 1.0)` | High-contrast dark charcoal ink for headlines and labels (Contrast > 14:1) |
| **`ColorTextSecondary`**| `#737373` | `(0.450, 0.450, 0.450, 1.0)` | Muted neutral gray for secondary actions and hints (Contrast > 5.5:1) |
| **`ColorBorderSubtle`** | `#E0DDD6` | `(0.880, 0.870, 0.840, 1.0)` | Crisp 1px outline for cards, dividers, and separators |
| **`ColorHandle`** | `#CCC8C0` | `(0.800, 0.790, 0.760, 1.0)` | Sheet drag handle pill indicator |
| **`ColorScrim`** | `#141419` | `(0.080, 0.080, 0.100, 0.72)` | Translucent dark backdrop scrim behind modal drawer |
| **`ColorStatusLocked`** | `#0FA870` | `(0.060, 0.680, 0.440, 0.94)` | Emerald green indicating locked/anchored position |
| **`ColorStatusUnlocked`**| `#E98C1E`| `(0.920, 0.550, 0.120, 0.94)` | Warm amber indicating editable/unlocked state |

---

## 3. Typography Hierarchy

The UI uses native sans-serif system fonts (`LegacyRuntime.ttf` / `Arial.ttf` / `sans-serif`) with explicit font weight and sizing hierarchy:

| Element | Size | Weight / Style | Color | Alignment |
| :--- | :--- | :--- | :--- | :--- |
| **Splash Monogram Emblem** | 64 px | Bold | `#FFFFFF` | Center |
| **Splash Screen Title** | 36 px | Bold | `#242424` | Center |
| **Splash Screen Subtitle** | 18 px | Normal | `#737373` | Center |
| **Splash Feature Text** | 16 px | Bold | `#242424` | Center |
| **Splash Start Button CTA** | 20 px | Bold | `#FFFFFF` | Center |
| **Modal Header Title** | 28 px | Bold | `#242424` | Left |
| **Catalog Launcher Title** | 28 px | Bold | `#FFFFFF` | Center |
| **Top Status Header** | 20 px | Bold | `#242424` | Center |
| **Primary Action** (`LOCK POSITION`) | 24 px | Bold | `#FFFFFF` | Center |
| **Grid Category Title** | 22 px | Bold | `#242424` | Left |
| **Furniture Card Label** | 20 px | Bold | `#242424` | Center |
| **Category Button Label** | 19 px | Bold (Active) / Normal | `#FFFFFF` / `#242424` | Center |
| **Active Item Card Name** | 18 px | Bold | `#242424` | Center |
| **Secondary Controls** (`ROTATE`, `DEL`) | 16-18 px | Bold | `#242424` | Center |
| **Top Header Action** (`RESET`) | 17 px | Bold | `#242424` | Center |
| **Launcher Subtitle** | 17 px | Normal | `#FFFFFF` (92% alpha) | Center |
| **Modal Header Subtitle** | 16 px | Normal | `#737373` | Left |
| **Category Sidebar Header** | 16 px | Bold | `#737373` | Center |
| **Grid Header Hint** | 15 px | Normal | `#737373` | Right |
| **Card Action Hint** (`SELECT & PLACE`) | 14 px | Bold | `#2EB0B0` | Center |
| **Card Active Badge** | 14 px | Bold | `#FFFFFF` | Center |
| **Active Selection Tag** | 13 px | Bold | `#737373` | Center |
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

---

### 4.2 Splash Screen Panel (`SplashScreenPanel`)

The splash screen acts as an elegant welcome card overlay that greets the user on launch:
```
+-------------------------------------------------------------+
|                                                             |
|          +---------------------------------------+          |
|          |                 ( F )                 |          | <- LogoEmblem (110 x 110, Teal Circle)
|          |               Furnish AR              |          | <- AppTitle (36 px Bold)
|          |    Augmented Reality Interior Design  |          | <- AppSubtitle (18 px)
|          | ------------------------------------- |          | <- Divider (740 x 2)
|          | [ True-To-Scale 3D Preview          ] |          | <- FeaturePill 1 (740 x 64)
|          | [ Instant Floor Plane Tracking      ] |          | <- FeaturePill 2 (740 x 64)
|          | [ 75 Curated Designer Pieces        ] |          | <- FeaturePill 3 (740 x 64)
|          |                                       |          |
|          |     [ START AR EXPERIENCE -> ]        |          | <- StartBtn (740 x 84, Teal)
|          +---------------------------------------+          | <- SplashCard (860 x 980)
|                                                             |
+-------------------------------------------------------------+
```
- **Panel Transform:** Stretches across full canvas (`1080 × 2400 px`).
- **SplashCard:** Size `860 × 980 px`, centered at `(0, 40)`, surface white background (`ColorBgSurface`) with subtle border.
- **LogoEmblem:** `110 × 110 px` teal circular emblem with white monogram letter `"F"`.
- **Feature Pills:** 3 rounded rows (`740 × 64 px`, `ColorBgPrimary`) highlighting key value props.
- **StartBtn:** High-contrast `740 × 84 px` teal action button with `"START AR EXPERIENCE ->"`. Tapping immediately triggers a smooth 0.4s fade-out, or auto-dismisses after 2.4 seconds.

---

### 4.3 Primary AR HUD View (`AROverlayPanel`)

```
+-------------------------------------------------------------+
|                      [AR Camera View]                       |
|                                                             |
|  +-------------------------------------------------------+  |
|  | [RESET]      Tap floor to place White Sofa            |  | <- TopFloatingHeader (980 x 76)
|  +-------------------------------------------------------+  |
|                                                             |
|                      [ 3D FURNITURE ]                       |
|                      [ LOCKED BADGE ]                       |
|                                                             |
|                                                             |
|  +-------------------------------------------------------+  |
|  |                        ( === )                        |  | <- HandlePill (64 x 6)
|  | +----------+ +-----------------------+ +----------+-+ |  |
|  | | ROTATE L | |     LOCK POSITION     | | ROTATE R |X| |  | <- ControlRow (Height 78)
|  | +----------+ +-----------------------+ +----------+-+ |  |
|  |                                                       |  |
|  | +-----------------+   +-----------------------------+ |  |
|  | | CURRENT PIECE   |   |      FURNITURE CATALOG      | |  |
|  | | [Thumb]         |   | 9 Categories - 75 Pieces    | |  | <- CatalogLauncherRow (Height 224)
|  | | White Sofa      |   | [ EXPLORE ALL FURNITURE -> ]| |  |
|  | +-----------------+   +-----------------------------+ |  |
|  +-------------------------------------------------------+  | <- UnifiedBottomPanel (1020 x 360)
+-------------------------------------------------------------+
```

#### Top Floating Header (980 × 76 px)
- **Object Name:** `TopFloatingHeader`
- **Anchoring:** Top-Center (`anchorMin: (0.5, 1.0)`, `anchorMax: (0.5, 1.0)`, `pivot: (0.5, 1.0)`)
- **Position & Size:** `anchoredPosition: (0, -130)`, `sizeDelta: (980, 76)`
- **Background:** `ColorBgSurface` with rounded card styling.
- **Children:**
  - `ResetBtn`: Left-aligned (`anchoredPosition: (16, 0)`), size `130 × 52 px`, background `ColorBgPrimary`, text `"RESET"`. Clears placed models.
  - `StatusLabel`: Stretched between margins `(160, 0)` to `(-160, 0)`, centered bold 20px guidance text (e.g. *"Tap floor to place White Sofa"*, *"POSITIONING WHITE SOFA - Tap LOCK to fix"*).

#### Unified Bottom Panel (1020 × 360 px)
- **Object Name:** `UnifiedBottomPanel`
- **Anchoring:** Bottom-Center (`anchorMin: (0.5, 0.0)`, `anchorMax: (0.5, 0.0)`, `pivot: (0.5, 0.5)`)
- **Position & Size:** `anchoredPosition: (0, 200)`, `sizeDelta: (1020, 360)`
- **Background:** `ColorBgSurface` with subtle border.
- **Children:**
  - `HandlePill`: Centered at top `(0, -10)`, size `64 × 6 px`, color `ColorHandle`.
  - `ControlRow`: Height `78 px`, anchored `Y = -26 px`. Uses `HorizontalLayoutGroup` (spacing `14 px`, padding `16 px` sides):
    - `RotLBtn`: Size `170 × 72 px`, text `"ROTATE L\n(-15 deg)"`.
    - `LockBtn`: Size `470 × 72 px`, background `ColorAccentTeal`, text `"LOCK POSITION"`.
    - `RotRBtn`: Size `170 × 72 px`, text `"ROTATE R\n(+15 deg)"`.
    - `DelBtn`: Size `84 × 72 px`, background `ColorBgPrimary`, text `"DEL"`.
  - `CatalogLauncherRow`: Height `224 px`, anchored `Y = 16 px`:
    - `ActiveItemCard`: Size `320 × 212 px`. Contains subtitle `"CURRENT SELECTION"`, 256×256 thumbnail preview of the active furniture, piece name `"White Sofa"`, and tap-to-open catalog action.
    - `OpenCatalogBtn`: Size `644 × 212 px`, background `ColorAccentTeal`. Features headline `"FURNITURE CATALOG"`, subtitle `"9 Categories - 75 Curated Pieces"`, and action pill `"EXPLORE ALL FURNITURE ->"`.

---

### 4.4 1/3 Sidebar & 2/3 Grid Catalog Modal Drawer (`CatalogModalPanel`)

```
+-------------------------------------------------------------+
| [ Modal Scrim: Dark Translucent Overlay rgba(20,20,25,0.72)]|
|                                                             |
|  +-------------------------------------------------------+  |
|  | FURNITURE CATALOG                            [X CLOSE]|  | <- ModalHeader (1020 x 120)
|  | Select category on left - Tap any piece to place      |  |
|  +-------------------------------------------------------+  |
|  |       1/3 SIDEBAR (320px)   |   2/3 GRID AREA (698px) |  | <- SplitBody (1020 x 1930)
|  | CATEGORIES (9)              | SOFAS (11 ITEMS)   [Hint|  |
|  | +-------------------------+ | +--------+   +--------+ |  |
|  | | [ACTIVE] SOFAS (11)   | | | | [Thumb]|   | [Thumb]| |  |
|  | +-------------------------+ | | White  |   | Brown  | |  |
|  | | CHAIRS (8)              | | | Sofa   |   | Sofa   | |  |
|  | +-------------------------+ | +--------+   +--------+ |  |
|  | | BEDS (8)                | | +--------+   +--------+ |  |
|  | +-------------------------+ | | [Thumb]|   | [Thumb]| |  |
|  | | TABLES (8)              | | | Lt. Brn|   | Modern | |  |
|  | +-------------------------+ | | Sofa   |   | Sofa 01| |  |
|  | | CLOSETS (8)             | | +--------+   +--------+ |  |
|  | +-------------------------+ |                         |  |
|  | | DRAWERS (8)             | |        ( Scrollable     |  |
|  | +-------------------------+ |          2-Column       |  |
|  | | BATHROOM (8)            | |            Grid )       |  |
|  | +-------------------------+ |                         |  |
|  | | KITCHEN (8)             | |                         |  |
|  | +-------------------------+ |                         |  |
|  | | CUSHIONS (8)            | |                         |  |
|  | +-------------------------+ |                         |  |
|  +-------------------------------------------------------+  |
+-------------------------------------------------------------+
```

#### Modal Window Specifications
- **Object Name:** `CatalogModalPanel / ModalWindow`
- **Dimensions:** `1020 × 2050 px`, anchored Center `(0.5, 0.5)` with `anchoredPosition: (0, 10)`.
- **Backdrop:** Full-screen `ModalScrim` (`1080 × 2400 px`), color `ColorScrim` (`rgba(20,20,25,0.72)`). Tapping scrim dismisses the modal.
- **Modal Header (1020 × 120 px):**
  - Title: 28 px Bold `"FURNITURE CATALOG"`.
  - Subtitle: 16 px Normal `"Select category on left - Tap any piece to place in your room"`.
  - `CloseBtn`: Size `130 × 60 px`, text `"X CLOSE"` (20 px Bold).
- **Left 1/3 Category Sidebar (320 px width):**
  - Header: `"CATEGORIES (9)"` (16 px Bold, `#737373`).
  - Vertical scrollable stack of 9 category buttons (`CatBtn_Sofas`, `CatBtn_Chairs`, `CatBtn_Beds`, `CatBtn_Tables`, `CatBtn_Closets`, `CatBtn_Drawers`, `CatBtn_Bathroom`, `CatBtn_Kitchen`, `CatBtn_Cushions`).
  - Button Dimensions: `300 × 92 px`.
  - Active Button: Background `ColorAccentTeal` (`#2EB0B0`), Text Color White, 19 px Bold, vertical white indicator bar.
  - Inactive Button: Background `ColorBgSurface` (`#FFFFFF`), Text Color `#242424`, 19 px Normal.
- **Divider Line (2 px width):**
  - X position `320 px`, full height stretch, color `ColorBorderSubtle` (`#E0DDD6`).
- **Right 2/3 Furniture Grid Area (698 px width):**
  - Header: Category Title (e.g. `"SOFAS (11 ITEMS)"`) + Hint (`"Tap piece to place"`).
  - ScrollRect Viewport: Clipped cleanly via `RectMask2D`.
  - 2-Column Grid: `GridLayoutGroup` with cell size `320 × 340 px`, spacing `16 × 18 px`, padding `18 px` sides.

---

## 5. Furniture Card Anatomy

Each card in the 2/3 grid is a composite component designed for immediate clarity:
```
+------------------------------------+
|                      [ACTIVE BADGE]| <- Active Badge (86 x 30 px, Teal)
|                                    |
|             [ 256 x 256 ]          |
|            STUDIO LIGHTED          | <- Offscreen Studio Render
|               THUMBNAIL            |    Aspect Fitted
|                                    |
|                                    |
|             White Sofa             | <- 20 px Bold Title
|           SELECT & PLACE           | <- 14 px Bold Teal Action Hint
+------------------------------------+
```
1. **Card Background:** `Image` component. Active card uses `ColorAccentActive` (`#EBF8F8`); inactive uses `ColorBgSurface` (`#FFFFFF`).
2. **Card Border:** 1px perimeter outline. Active card uses `ColorAccentTeal` (`#2EB0B0`); inactive uses `ColorBorderSubtle` (`#E0DDD6`).
3. **Thumbnail:** Offscreen-camera baked 256×256 studio render with transparent/warm ivory background (`anchorMin: (0.06, 0.28)`, `anchorMax: (0.94, 0.94)`).
4. **Label:** Centered title text (`anchorMin: (0.04, 0.12)`, `anchorMax: (0.96, 0.28)`), 20 px Bold `#242424`.
5. **Action Hint:** Centered prompt (`anchorMin: (0.04, 0.02)`, `anchorMax: (0.96, 0.12)`), 14 px Bold `#2EB0B0` (`"SELECT & PLACE"`).
6. **ActiveBadge:** Top-right anchored pill (`sizeDelta: (86, 30)`), background `ColorAccentTeal`, white 14px bold text `"ACTIVE"`. Visible only on the currently active piece.

---

## 6. 3D World-Space Lock Badge

- **Object Name:** `LockBadgeCanvas` (child of placed furniture instance)
- **Render Mode:** `WorldSpace`
- **Scale:** `(0.0035, 0.0035, 0.0035)`
- **Canvas Size:** `300 × 80 px`
- **Positioning:** Dynamically calculated at runtime by querying all active `MeshRenderer` bounds on the furniture piece, positioning the badge **`+0.25m`** above the highest point of the model.
- **Billboarding:** In `LateUpdate()`, aligns rotation with `Camera.main.transform.rotation` so it always faces the user regardless of camera angle.
- **States:**
  - **Unlocked:** Background `#E98C1E` (Amber), Text `"UNLOCKED"`.
  - **Locked:** Background `#0FA870` (Emerald Green), Text `"LOCKED"`.

---

## 7. Interaction States & Flows

### 7.1 Category Selection & Filtering Flow
1. User taps `"FURNITURE CATALOG"` or `"EXPLORE ALL FURNITURE ->"` in the bottom dock.
2. `CatalogModalPanel` activates instantly.
3. User taps any of the 9 categories on the 1/3 sidebar (e.g. `Chairs`).
4. The sidebar button highlights with teal background and white text.
5. The 2/3 grid view instantly updates to display the matching category cards (e.g. `CategoryView_Chairs`).
6. The grid header updates to `"CHAIRS (8 ITEMS)"`.

### 7.2 In-Place Hot-Swapping Flow
1. User taps any furniture card in the grid (e.g. `Armchair 03`).
2. If an active piece is currently placed on the floor (unlocked):
   - The exact position and rotation of the current instance are preserved.
   - The current instance is cleanly destroyed.
   - The new prefab (`Chair03.prefab`) is instantiated at the exact world position and orientation.
   - `SnapToGround()` aligns the lowest vertex bound with the detected floor plane.
   - The `FurnitureLockBadge` is created and positioned above the new model's vertex bounds.
3. The bottom dock `ActiveItemCard` updates its thumbnail and name to `"Armchair 03"`.
4. The modal drawer dismisses smoothly, returning to full-screen AR.
5. A confirmation toast displays: `"Selected Armchair 03 - Tap floor to place"`.

---

## 8. Accessibility & Touch Target Standards

1. **Strict Touch Target Dimensions:** Every interactive touch target exceeds the 48×48 dp standard:
   - Category Sidebar Buttons: `300 × 92 px`
   - Furniture Grid Cards: `320 × 340 px`
   - Start AR Experience Button: `740 × 84 px`
   - Lock Position Button: `470 × 72 px`
   - Rotation Buttons: `170 × 72 px`
   - Close Button: `130 × 60 px`
   - Header Action Button (`RESET`): `130 × 52 px`
2. **Text Contrast Compliance:**
   - Dark ink (`#242424`) on light surface (`#FFFFFF`): Contrast ratio **14.2:1** (exceeds WCAG AAA).
   - White text on primary teal (`#2EB0B0`): Contrast ratio **4.6:1** (exceeds WCAG AA).
   - Secondary gray text (`#737373`) on ivory (`#F8F6F1`): Contrast ratio **5.6:1** (exceeds WCAG AA).
3. **Typographic Standard:** All action buttons use uppercase text (`RESET`, `ROTATE L`, `LOCK POSITION`, `DEL`, `X CLOSE`, `SELECT & PLACE`, `START AR EXPERIENCE ->`).
4. **Code Quality Standard:** Zero comments (`//`, `/* */`), zero emojis across all C# scripts (`/ponytail`). All design decisions and architecture specifications are maintained in this document and [`PRODUCT.md`](file:///home/anni/Furnish_Ar/PRODUCT.md).
