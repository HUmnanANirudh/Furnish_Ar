# Design Authority & Visual Tokens

## Core Identity
A warm, serene, Scandinavian/Japandi-inspired interior visualization aesthetic. Natural light, tactile upholstery, muted wood, organic greenery, and minimalist Swiss-style UI typography with rounded micro-controls that float effortlessly over photorealistic 3D room environments.

## Color System
```css
:root {
  /* Surface colors */
  --bg-primary: #F8F6F1;          /* Warm Ivory (Splash & Canvas base) */
  --bg-surface: #FFFFFF;          /* Pure White (Catalog, discovery & sheets) */
  --bg-translucent-header: rgba(248, 246, 241, 0.45);
  --bg-translucent-card: rgba(255, 255, 255, 0.94);
  --bg-sheet-scrim: rgba(26, 24, 22, 0.45);

  /* Typography & Ink */
  --text-primary: #242424;        /* Deep Charcoal ink */
  --text-secondary: #858585;      /* Warm Muted Gray */
  --text-muted: #A39E93;          /* Soft Sand Gray */
  --text-white: #FFFFFF;

  /* Accent & Action */
  --accent-teal: #39B6B6;         /* Primary Action Turquoise/Teal */
  --accent-teal-dark: #2A9191;
  --accent-teal-subtle: #E8F7F7;
  --accent-badge: #F28B63;        /* Warm Coral/Orange for "New" tags */

  /* Neutral tones & Warmth */
  --sand-warm: #C9AD88;
  --sand-light: #E8DCCB;
  --border-subtle: #ECEAE5;
  --border-light: #F2EFE9;

  /* Furniture Colorway Variations */
  --colorway-cream: #EAE6DF;
  --colorway-tan: #CBA576;
  --colorway-teal: #4A9E9E;
  --colorway-white: #F9F9F8;
}
```

## Typography Hierarchy
Font family: Apple SF Pro Display / Inter / Plus Jakarta Sans (-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif)
- **Splash Title:** 24px, Medium, letter-spacing -0.02em, color `#242424`
- **Splash Tagline:** 13px, Regular, color `#858585`
- **Header Title (AR):** 18px, SemiBold, color `#FFFFFF`, shadow: `0 1px 3px rgba(0,0,0,0.3)`
- **Header Subtitle (AR):** 13px, Regular, color `rgba(255,255,255,0.85)`
- **Section Headings:** 17px, Bold, letter-spacing -0.01em, color `#242424`
- **Product Card Title:** 13px, SemiBold, color `#242424`
- **Product Card Meta:** 11px, Regular, color `#858585`
- **Product Card Price:** 12px, SemiBold, color `#242424`
- **Badge Text:** 10px, Bold, color `#FFFFFF`

## Iconography & Controls
- **Header Back/Profile:** 40px circle, `#FFFFFF`, 1px border `#ECEAE5`, teal left arrow icon.
- **Header Favorite:** Minimalist white thin outline heart (turns solid crimson `#E05353` when active).
- **Variation Swatches:** 4 × 22px circles centered with 12px gap, 2px ring around active swatch.
- **Scanning Card:** Rounded rectangular toast with subtle 3D shadow `0 8px 24px rgba(0,0,0,0.12)`, compact prompt, animated device scan icon.
- **Placement Reticle:** Concentric circular floor guides with direction nudge arrows and CCW/CW rotation indicators.
- **Bottom Floating Action Bar:**
  - Left: 44px white circle, trash/delete icon.
  - Center: 54px teal circle (`#39B6B6`), white bold plus icon, elevation `0 8px 20px rgba(57,182,182,0.35)`.
  - Right: 44px white circle, shutter camera / measure ruler icon.

## Screen Layouts & Modals
1. **Splash Screen:** Centered minimalist chair logo, brand name, subtle pulse/fade loading indicator.
2. **Main AR View:** Full viewport living-room photographic environment or live device camera with plane detection, 3D placed furniture, contact drop-shadow, floor tracking dots.
3. **Collection & Product Discovery Modal:** Full-height slide-over white canvas with top-left close 'X', "Best Collection" carousel with horizontal room photos, "Product for you" category line icons (Desk, Chair, TV Stand, Sofa, Drawer, Bed), and responsive 2-column product grid.
4. **Replace Similar Furniture Sheet:** Bottom drawer modal (65vh) with drag pill, close button, title, and 2-column grid of styled alternatives that hot-swap the AR model on click.
