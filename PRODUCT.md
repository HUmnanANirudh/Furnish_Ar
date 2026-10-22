# Product Documentation & Architecture Guide

## 1. Product Overview
**Furnish AR** is a mobile Augmented Reality (AR) interior design and e-commerce visualization application built in **Unity 2022.3.62f3 LTS** powered entirely by the **PTC Vuforia Engine**.

The app enables users to preview true-to-scale 3D furniture models within their physical living environment or inside the Unity Editor with high-fidelity lighting and materials. Users can intuitively position, rotate, lock, remove, and switch between furniture models using an ergonomic, high-contrast control layout designed for handheld mobile use.

The application features a comprehensive **9-Category Catalog Suite** comprising **75 curated 3D furniture models** (Sofas, Chairs, Beds, Tables, Closets, Drawers, Bathroom, Kitchen, Cushions) accessible via an intuitive **1/3 Sidebar and 2/3 Grid** modal drawer.

---

## 2. Core Capabilities & User Flow

```mermaid
flowchart TD
    Start["Launch App / Open SampleScene"] --> Splash["SplashScreenPanel<br>(Monogram, Highlights & 'START AR EXPERIENCE')"]
    Splash --> DismissSplash["Automatic 2.4s Timer or Tap 'START AR'"]
    DismissSplash --> ARView["Full-Screen AR Camera View<br>(AROverlayPanel Active)"]
    ARView --> ShowcaseInit["InitialShowcaseFurniture Loaded on Floor<br>(Real 3D White Sofa with Lock Badge)"]
    ARView --> PlaneTrack["Vuforia Ground Plane Tracking<br>(Android Camera Feed / Editor View)"]
    PlaneTrack --> TapPlacement["Tap Floor to Place Selected Piece"]
    TapPlacement --> SpawnObject["Spawn 3D Model & Attach 3D World Lock Badge"]
    SpawnObject --> Interact["On-Screen Manipulation<br>(Rotate -15 deg, Rotate +15 deg, Drag)"]
    ShowcaseInit --> Interact
    Interact --> LockCheck{"Lock Position Button"}
    LockCheck -- "Lock" --> LockedState["Furniture Anchored & Locked<br>(Badge turns GREEN 'LOCKED')"]
    LockCheck -- "Unlock" --> UnlockedState["Furniture Moveable<br>(Badge turns AMBER 'UNLOCKED')"]
    Interact --> OpenCatalog["Tap 'FURNITURE CATALOG'<br>(Opens 1/3 - 2/3 Drawer)"]
    OpenCatalog --> SelectCat["Select Category in 1/3 Sidebar<br>(Sofas, Chairs, Beds, Tables, etc.)"]
    SelectCat --> BrowseGrid["Browse 2/3 Grid with 256x256 Thumbnails"]
    BrowseGrid --> SelectPiece["Tap Any Furniture Card"]
    SelectPiece --> HotSwap["Hot-Swap In-Place on Floor<br>(Preserves Position & Rotation)"]
    HotSwap --> CloseModal["Catalog Closes Automatically<br>(Returns to Full-Screen AR)"]
    Interact --> DelAction["DEL Button<br>(Removes current furniture)"]
    Interact --> ResetAction["RESET Button<br>(Clears all placed models)"]
```

### 2.1 Vuforia Augmented Reality Tracking & Camera Pipeline
- **AR Engine:** PTC Vuforia Engine 11.4.4.
- **Tracking Mechanism:** `PlaneFinderBehaviour` with `HitTestMode.AUTOMATIC` and device ground plane tracking.
- **Floor Hit Testing:** `VuforiaBehaviour.Instance.World.HitTest(screenPosition, 1.4f, out HitTestResult hitResult)` calculates the real-world metric surface position and normal.
- **Android Camera Permissions:**
  - Configured in [`Assets/Plugins/Android/AndroidManifest.xml`](file:///home/anni/Furnish_Ar/Assets/Plugins/Android/AndroidManifest.xml) with `<uses-permission android:name="android.permission.CAMERA" />` and `<uses-feature android:name="android.hardware.camera" android:required="true" />`.
  - Explicit runtime authorization requested on start via `UnityEngine.Android.Permission.RequestUserPermission(Permission.Camera)` in [`ARFurnitureAppManager.cs`](file:///home/anni/Furnish_Ar/Assets/Scripts/ARFurnitureAppManager.cs), ensuring zero black-screen issues on Android 14 (API 34).
- **Natural Standing Perspective:** The `ARCamera` is initialized at standing eye-level `(0, 1.4, -0.5)` with an `18°` downward pitch to ensure raycasts intersect the floor plane reliably.
- **Zero ARCore / Zero AR Foundation:** ARCore and AR Foundation have been completely removed from packages, scene hierarchy, and build settings in favor of Vuforia's cross-platform engine.

### 2.2 WYSIWYG In-Editor Scene Visibility & 3D Showcase Asset
- Unlike standard ARFoundation setups that render a blank gray screen inside the Unity Editor, Furnish AR bakes all UI screens and an initial 3D showcase asset directly into [`Assets/Scenes/SampleScene.unity`](file:///home/anni/Furnish_Ar/Assets/Scenes/SampleScene.unity).
- **Real 3D Showcase Furniture:** A high-detail [`White Sofa.prefab`](file:///home/anni/Furnish_Ar/Assets/Assets/Furnitures/White%20Sofa/White%20Sofa.prefab) is pre-spawned at `(0, 0, 2.2f)` with soft directional lighting (`intensity 1.15`, warm white tint) and the [`FurnitureLockBadge`](file:///home/anni/Furnish_Ar/Assets/Scripts/FurnitureLockBadge.cs) attached.
- [`furniturePlacementManager.cs`](file:///home/anni/Furnish_Ar/Assets/Scripts/furniturePlacementManager.cs) automatically detects and binds `InitialShowcaseFurniture` upon startup, allowing instant manipulation, rotation, locking, and hot-swapping directly in the Editor Game view without requiring device deployment.

### 2.3 Distinct Three-Screen Architecture
The user interface is structured into three clean, dedicated canvas panels:

1. **`SplashScreenPanel`:**
   - Visual onboarding screen displaying the Furnish AR monogram emblem (`"F"`), headline, subtitle (*"Augmented Reality Interior Design"*), and feature highlight pills (*"True-To-Scale 3D Preview"*, *"Instant Floor Plane Tracking"*, *"75 Curated Designer Pieces"*).
   - Includes a prominent `"START AR EXPERIENCE ->"` action button for immediate dismissal or auto-fades over 2.4 seconds.
2. **`AROverlayPanel`:**
   - The primary transparent HUD layer active during AR tracking.
   - **Top Header (`TopFloatingHeader`):** Features the `RESET` button on the left to clear placed models and the centered `StatusLabel` delivering real-time contextual guidance (e.g. *"Tap floor to place White Sofa"*, *"POSITIONING WHITE SOFA - Tap LOCK to fix"*). The legacy photo snapshot button has been removed for a clean, distraction-free viewfinder.
   - **Unified Bottom Dock (`UnifiedBottomPanel`):**
     - **Control Row:** `ROTATE L (-15 deg)`, teal `LOCK POSITION` / `UNLOCK POSITION`, `ROTATE R (+15 deg)`, and `DEL`.
     - **Catalog Launcher Row:** `ActiveItemCard` (displaying active piece thumbnail and name) and `OpenCatalogBtn` (bold teal button with `"FURNITURE CATALOG"`, `"9 Categories - 75 Curated Pieces"`, and `"EXPLORE ALL FURNITURE ->"`).
3. **`CatalogModalPanel`:**
   - Full-screen modal drawer divided into a 1/3 category navigation sidebar and 2/3 scrollable furniture grid.

### 2.4 1/3 Sidebar and 2/3 Grid Catalog Suite
- When the user taps `"FURNITURE CATALOG"` or `"EXPLORE ALL FURNITURE ->"`, the modal drawer opens over a dark translucent backdrop (`rgba(20,20,25,0.72)`).
- **1/3 Category Sidebar (320 px width):**
   - Features 9 category filter buttons under the header `"CATEGORIES (9)"`:
     1. **Sofas** (11 curated pieces: White Sofa, Brown Sofa, Light Brown Sofa, Modern Sofa 01-08)
     2. **Chairs** (8 curated armchairs: Armchair 01-08)
     3. **Beds** (8 curated beds: Comfort Bed 01-08)
     4. **Tables** (8 curated tables: Dining Table 01-08)
     5. **Closets** (8 curated wardrobes: Wardrobe 01-08)
     6. **Drawers** (8 curated drawers: Storage Drawer 01-08)
     7. **Bathroom** (8 curated fixtures: Vanity Unit 01-07, Soaking Tub 01)
     8. **Kitchen** (8 curated units: Kitchen Cabinet 01-07, Kitchen Sink Unit)
     9. **Cushions** (8 curated accents: Accent Cushion 01-08)
   - Selecting any category highlights the button with teal accent (`#2EB0B0`), white text, and right indicator bar, while instantly displaying the matching items in the 2/3 grid.
- **2/3 Furniture Grid (698 px width):**
   - 2-column scrollable grid of tactile cards (320 × 340 px).
   - Each card features:
     - High-resolution 256×256 studio thumbnail with transparent/warm background.
     - Clean bold item name label (e.g., *"White Sofa"*, *"Armchair 01"*, *"Kitchen Sink Unit"*).
     - Bold teal `"SELECT & PLACE"` action hint.
     - `"ACTIVE"` teal badge if this is the currently selected piece.
   - Tapping any card immediately selects the item, performs an in-place hot swap if an unlocked piece is on the floor, closes the modal, and returns to full AR view with a confirmation toast.

### 2.5 In-Place Hot-Swapping
- If a furniture model is already placed on the floor (unlocked), selecting a new model from the catalog immediately replaces it at the exact same floor coordinate and rotation angle.
- The 3D World Lock Badge automatically updates to match the newly swapped model's height bounds.
- If no model is currently on the floor, selecting a piece primes it for immediate floor placement upon the next tap.

### 2.6 3D World-Space Lock Badge
- When a furniture piece is placed in the scene, [`FurnitureLockBadge`](file:///home/anni/Furnish_Ar/Assets/Scripts/FurnitureLockBadge.cs) instantiates a world-space billboard badge directly above the highest vertex bound of the model.
- **Unlocked State:** Amber badge (`#E98C1E`) displaying `UNLOCKED`.
- **Locked State:** Emerald green badge (`#0FA870`) displaying `LOCKED`.
- Automatically faces the active camera at all times with smooth billboarding.

---

## 3. Technology Stack & Target Specifications

| Component | Specification |
| :--- | :--- |
| **Unity Version** | `2022.3.62f3 LTS` |
| **AR Framework** | **PTC Vuforia Engine `11.4.4`** |
| **Target OS** | **Android 14 (API Level 34)** |
| **Minimum OS** | Android 7.0 Nougat (API Level 24) |
| **Scripting Backend** | **IL2CPP** |
| **Target Architecture**| **ARM64 (64-bit)** |
| **UI System** | Unity UI (uGUI Canvas) with Scaler set to 1080×2400 |
| **Catalog Scale** | **9 Categories, 75 Curated 3D Models, 75 Studio Sprites** |
| **Asset Footprint** | **~390 MB (Pruned 1,273 unused files / 1.39 GB removed)** |
| **Code Standard** | Zero comments (`//`, `/* */`), zero emojis across all C# scripts |

---

## 4. Project Directory Structure

```
Furnish_Ar/
├── Assets/
│   ├── Assets/
│   │   └── Furnitures/                     # Original 3D Models & Prefabs
│   │       ├── White Sofa/                 # White Sofa prefab, materials, textures, meshes
│   │       ├── Brown Sofa/                 # Brown Sofa prefab, materials, textures, meshes
│   │       └── Light Brown Sofa/           # Light Brown Sofa prefab, materials, textures, meshes
│   ├── Furniture Mega Pack/                # Curated 3D Furniture Assets Suite (Pruned to Active 72 Models)
│   │   ├── FBX/                            # Active model geometry
│   │   ├── Materials/                      # Active PBR surface materials
│   │   ├── Prefabs/                        # Categorized Prefabs (8 per category)
│   │   │   ├── Bathroom/                   # Bathroom vanities and tub
│   │   │   ├── Beds/                       # Beds 01-08
│   │   │   ├── Chairs/                     # Armchairs 01-08
│   │   │   ├── Closets/                    # Wardrobes 01-08
│   │   │   ├── Cushioins/                  # Accent cushions 01-08
│   │   │   ├── Drawers/                    # Storage drawers 01-08
│   │   │   ├── Kitchen/                    # Kitchen cabinets and sink
│   │   │   ├── Sofas/                      # Modern sofas 01-08
│   │   │   └── Tables/                     # Dining tables 01-08
│   │   └── Textures/                       # Active PBR surface maps
│   ├── Editor/
│   │   ├── BuildScript.cs                  # Headless & batchmode Android build automation
│   │   ├── CatalogThumbnailBaker.cs        # Studio camera thumbnail generation automation
│   │   └── SceneUIBuilder.cs               # One-click scene UI baker & 1/3-2/3 catalog suite generator
│   ├── Plugins/
│   │   └── Android/
│   │       └── AndroidManifest.xml         # Camera hardware & permission declarations
│   ├── Resources/
│   │   └── Textures/
│   │       └── Catalog/                    # 75 Baked 256x256 Studio Sprites (692 KB total)
│   │           ├── Bathroom/               # 8 bathroom fixture sprites
│   │           ├── Beds/                   # 8 bed sprites
│   │           ├── Chairs/                 # 8 chair sprites
│   │           ├── Closets/                # 8 closet sprites
│   │           ├── Cushions/               # 8 cushion sprites
│   │           ├── Drawers/                # 8 drawer sprites
│   │           ├── Kitchen/                # 8 kitchen unit sprites
│   │           ├── Sofas/                  # 11 sofa sprites (including originals)
│   │           └── Tables/                 # 8 table sprites
│   ├── Scenes/
│   │   └── SampleScene.unity               # Fully baked scene with Splash, AR & Catalog screens, and showcase asset
│   └── Scripts/
│       ├── ARFurnitureAppManager.cs        # UI controller, 3-screen manager, camera permissions & catalog
│       ├── furniturePlacementManager.cs    # Vuforia hit-test placement, showcase binding, rotation & lock manager
│       └── FurnitureLockBadge.cs           # 3D world-space floating lock/unlock status badge
├── Packages/
│   ├── manifest.json                       # Vuforia scoped registry & Unity dependencies
│   └── packages-lock.json                  # Resolved dependency tree
├── ProjectSettings/
│   ├── ProjectSettings.asset               # Target API 34, ARM64, IL2CPP
│   └── EditorBuildSettings.asset           # Active scene list (SampleScene only)
├── DESIGN.md                               # UI visual tokens, 1/3-2/3 layout specifications & component system
└── PRODUCT.md                              # This document
```

---

## 5. Developer Guide: Building & Extending

### 5.1 Opening the Project in Unity
1. Ensure **Unity 2022.3.62f3** is installed via Unity Hub with **Android Build Support** (Android SDK & NDK tools, OpenJDK).
2. Open the project directory (`Furnish_Ar`).
3. Unity Package Manager automatically resolves `com.ptc.vuforia.engine` version `11.4.4` via the Vuforia scoped registry specified in [`Packages/manifest.json`](file:///home/anni/Furnish_Ar/Packages/manifest.json).

### 5.2 Rebaking or Updating the UI
If you modify UI dimensions, add categories, or change styling in [`Assets/Editor/SceneUIBuilder.cs`](file:///home/anni/Furnish_Ar/Assets/Editor/SceneUIBuilder.cs):
1. In the Unity Editor menu bar, click: **`Furnish AR -> Bake UI To Scene`**.
2. The script cleans any stale hierarchy and generates the modern canvas layout directly in `SampleScene.unity`.
3. Press **Ctrl+S** to save the scene.

### 5.3 Regenerating Catalog Thumbnails
If new 3D prefabs are added to `Assets/Furniture Mega Pack/`:
1. In the Unity Editor menu bar, click: **`Furnish AR -> Bake All Catalog Thumbnails`**.
2. The offscreen studio lighting camera renders high-resolution 256×256 PNGs directly into `Assets/Resources/Textures/Catalog/<Category>/`.
3. The assets are automatically configured with `Sprite (2D and UI)` import settings.

### 5.4 Building the Android APK
From the Unity Editor:
- Go to **File -> Build Settings -> Android -> Build**.

Or via Command Line / Headless Batchmode:
```bash
/home/anni/Unity/Hub/Editor/2022.3.62f3/Editor/Unity -quit -batchmode -projectPath /home/anni/Furnish_Ar -executeMethod BuildScript.BuildAndroid -logFile build.log
```
The output APK will be generated at `Builds/Android/FurnishAR.apk`.
