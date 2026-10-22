# Product Context

## Vision
A premium, mobile-first AR furniture shopping and interior design application that closely reproduces the reference video's aesthetic and interaction architecture. The application bridges e-commerce and augmented reality, allowing users to discover curated furniture collections, visualize high-fidelity 3D furniture models within an authentic room environment or real-world floor, customize color variations in real-time, manipulate placement, and swap similar styles seamlessly.

## Target User & Situation
- **User:** Interior design enthusiasts, homeowners, and premium furniture shoppers exploring furniture fit and aesthetics before purchasing.
- **Situation:** Users scanning their living room floor or evaluating room arrangements on mobile (iOS/Android AR).

## Core Capabilities
1. **Minimalist Warm Splash Screen:** Elegant ivory background (#F8F6F1), minimalist chair silhouette logo, subtle fade-in, "Visualize your space" tagline, smooth transition to AR.
2. **Immersive AR Camera / Room Visualization:**
   - Full-screen AR view with real-time horizontal plane tracking.
   - Floor scanning guidance modal ("Now move your device around the room aim at the floor ✨") with tracking indicators.
   - True-to-scale virtual furniture placement with realistic perspective, contact shadows, and anchor stability.
3. **Top Navigation & Color Variations:**
   - Warm translucent header with back/profile button, selected furniture name ("Armchair"), price ("$99"), and favorite heart icon.
   - Horizontal color swatch bar with 4 variations (Cream, Warm Tan, Teal, White) that crossfade/re-texture the furniture in place.
4. **AR Placement Ring & Object Manipulation:**
   - Reticle placement ring on the floor beneath the active furniture.
   - Manipulation controls: Move arrows (Forward, Backward, Left, Right), Rotate CCW/CW, dismiss reticle.
   - Side utility triggers: 3D inspect view and dimension/AR measure mode.
5. **Floating Bottom Action Bar:**
   - Left: White circular Delete/Trash button to reset/remove furniture.
   - Center: Accent turquoise (#39B6B6) '+' button to open the Furniture Collection & Discovery screen.
   - Right: White circular Shutter button to capture a scene snapshot with haptic/flash feedback.
6. **Furniture Collection & Discovery Screen:**
   - Clean white modal with top-left 'X' close button.
   - "Best Collection" section with search icon and horizontal carousel of styled editorial room photography banners (e.g., "OFFICE COLLECTION").
   - "Product for you" section with category icon filter strip (Desk, Chair, TV Stand, Sofa, Drawer, Bed).
   - Two-column product grid with "New" badges, 3D AR cube badges, product cutout imagery, dimensions (Size: 3ft / 2ft), and pricing.
7. **"Replace Similar Furniture" Bottom Sheet:**
   - Slide-up bottom sheet with grab handle, close icon, and "Replace Similar Furniture" title.
   - Two-column grid of curated alternatives (Corner Sofa, Backrest Chair, Stock Range Chairs, Recliner).
   - Selecting any alternative replaces the furniture model in the exact same AR placement position without resetting the room context.

## Stack & Platform
- **Platform:** Native Unity AR Application (Android / iOS ARFoundation)
- **Engine:** Unity 2022.3.62f3 LTS
- **AR Stack:** ARFoundation 5.2.2, ARCore / ARKit XR Subsystems, XR Simulation
- **UI Framework:** Unity UI (uGUI / Canvas) styled to modern Apple HIG & reference video design tokens

## Design Tokens & Palette
- Primary Background: Warm Ivory (`#F8F6F1`)
- Catalog Background: Pure White (`#FFFFFF`)
- Primary Text: Deep Charcoal (`#242424`)
- Secondary Text: Muted Gray (`#858585`)
- Warm Beige: `#C9AD88`
- Light Sand: `#E8DCCB`
- Accent Action (Turquoise): `#39B6B6`
- Feature Badge (Coral Orange): `#F28B63`
- Subtle Borders: `#ECEAE5`
