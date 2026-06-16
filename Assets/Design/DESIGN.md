# Digital Dog Arena - Game Design Specification

This document serves as the master design specification for the visual direction, UI layouts, asset pipelines, and game feedback systems of **Digital Dog Arena**. It provides a comprehensive visual and sensory blueprint designed to be clean, high-contrast, atmospheric, and highly tactile, focusing on a gritty, dark, concrete, and metal aesthetic.

---

## 1. Visual Identity & Art Direction (Asset Design)

### Visual Style and Atmosphere
- **Theme:** Gritty, dark underground kennel management and high-stakes arena dog fighting simulator.
- **Tone:** Mature, industrial, heavy, and serious. The environment should feel like a cold, damp, subterranean warehouse or an abandoned concrete drainage basin turned into an illicit fighting ring.
- **Key Motifs:** Rough-pitted concrete walls, rusted steel plating, chain-link fencing casting long diagonal shadows, wire-mesh grids, heavy metal rivets, weathered spray-painted stencils, and glowing neon/halogen terminal screens.
- **Visual Style:** Distressed industrial grunge with high-contrast, punchy focal points. Surfaces must look weathered, scarred, and tactile, using depth, parallax shadows, and atmospheric post-processing to create a claustrophobic, high-intensity mood.

### Color Palette System
The project's palette relies on a cold neutral base contrasted sharply with warm chemical accents and vibrant neon status indicators. Pure grays are strictly forbidden; all neutrals must be tinted with an industrial cold-blue or moss-green undertone.

| Palette Category | Swatch Name | Hex Code | Visual Application & Hierarchy |
|:---|:---|:---|:---|
| **Base Neutral** | Distressed Coal | `#111315` | Absolute background layer, screen borders, and dark vignette bases. |
| **Panel Surface** | Weathered Concrete | `#1A1C1E` | Standard structural containers, menu backdrops, and card faces. |
| **Raised Surface** | Cold Steel | `#2B2E31` | Active interactive plates, highlighted text frames, and elevated card details. |
| **Accent Primary** | Chemical Orange | `#FF6B00` | Critical buttons, active tabs, primary navigation, level-up alerts, and high-value callouts. |
| **Accent Secondary** | Rust Red | `#C43400` | Damage indicators, dangerous/high-risk choices, health-depleted segments, and retirements. |
| **Status Color** | Acid Green | `#39FF14` | Active status labels, breeding readiness, positive XP shifts, and successful traits. |
| **Status Color** | Cold Ash | `#7A8B99` | Retired or deceased state indicators, locked menus, and secondary text labels. |

#### Potential Tier Color Codes (Core)
These colors define the background glows, text highlights, and badge rings of the dog potential tiers:
- **Legendary Tier:** Toxic Gold (`#FFD700`) - Rich, glowing yellow with amber depth.
- **Apex Tier:** Neon Violet (`#D800FF`) - High-saturation magenta-purple.
- **Elite Tier:** Cyan Shock (`#00FFFF`) - Ultra-bright glowing turquoise.
- **Contender Tier:** Amber Flame (`#E65100`) - Dark warm orange.
- **Prospect Tier:** Slate Dust (`#757575`) - Medium cold-blue gray.
- **Street Tier:** Raw Concrete (`#4E5D6C`) - Low-saturation industrial blue-gray.

### Typography Hierarchy
- **Headline / Display Font (Core):** Bold, heavy-set industrial sans-serif (blocky, reminiscent of stenciled steel or condensed sans-serif) for numbers, round announcements, names, and major headings. Exaggerate size ratios between numbers and units (e.g., giant white stats, tiny orange labels).
- **Body Font (Core):** Low-contrast, clean sans-serif with wide tracking (letter spacing) to preserve readability against dark, textured backgrounds. Used for narrative text, logs, and description fields.
- **Label / Metric Font (Core):** Sharp, monospace digital-terminal style font for stats, XP counters, DNA genetics, and numeric data to reinforce the "digital terminal" overlay feeling.

---

## 2. UI Layout & Component Design

Game UI must feel built into the world—as if the player is sitting in front of a heavy, surveillance-style industrial monitor in a back-alley kennel. 

### Depth Strategy (Layout Nesting)
- **Base Background:** Distressed Coal (`#111315`) layered with concrete scratch overlays and dark corner vignettes.
- **Sub-Panels (Nesting Level 1):** Weathered Concrete (`#1A1C1E`) panels utilizing soft ambient shadows instead of hard borders.
- **Interactive Plates (Nesting Level 2):** Cold Steel (`#2B2E31`) with a bevel/emboss visual styling to convey tactile physicality. Buttons feature a lighter top gradient shifting to a darker base, with a 3px dark-shadow edge underneath representing material thickness.
- **Asymmetric Intersect:** Elements like heavy dog silhouettes or metallic badges should overlap container edges to break the flat "box" feel.

---

### UI Screen Specifications

#### A. Stable Page Dog Cards (Core)
Each dog is represented by a highly structured physical card that visually reports their attributes, growth, and survival status.
- **Container:** Rectangular vertical frame (`#1A1C1E`) with rounded corners (8px) and a rusted steel texture strip along the top banner. Subtly backlit with a potential tier color glow.
- **Top Bar:** 
  - Monospace text showing breed (e.g., "PIT BULL") on the left and the dog's Level (e.g., "Lvl 12") in Chemical Orange on the right.
- **Visual Frame:**
  - A centered, square frame housing the dog's high-contrast gritty portrait. Background is a dark concrete mesh pattern with diagonal light shadows casting through chain-link wire.
- **Identity & Life Stage:**
  - Name rendered in display font (e.g., "DIESEL"). Directly below, a smaller bracketed label shows their life stage (e.g., `[ PRIME ]` in Acid Green, `[ VETERAN ]` in Slate Dust).
- **Stats & Potential Bars:**
  - Strength, Agility, and Stamina represented as compact horizontal blocks.
  - Active stats are filled with solid Chemical Orange segments inside inset, pitch-black slot tracks.
  - Potential ceilings are indicated as ghosted, semi-transparent grey markers further down the track, visually showing remaining growth room.
- **Record & Fight Style:**
  - Standard monospaced record stamp: `W: 14 | L: 2` highlighted in white.
  - Fight Style label (e.g., "RUSHDOWN") printed on a weathered metal band icon.
- **Status Overlay Stamp:**
  - If a dog's status changes (`isDead` or `isRetired`), a heavy, semi-transparent stamped block slams across the center of the card.
  - **Deceased Stamp:** Bold red (`#C43400`) reading `[ DECEASED ]` tilted at -15 degrees with simulated paint-flecks.
  - **Retired Stamp:** Cold ash (`#7A8B99`) reading `[ RETIRED ]` tilted at 15 degrees.

#### B. Fight Page Layout (Core)
Designed to represent an underground cage-side scoring monitor, split cleanly into control, feedback, and active stat panels.
- **Split-View Structure:**
  - **Top 45% (Fighter Cards & Face-off):** Two massive, high-contrast fighter cards (Player vs Opponent/Rival) facing each other across a central round indicator. Under each fighter is an oversized health bar: segmented orange bars for HP, dropping to red chunks when damaged, overlaid with giant numeric HP values (e.g., `HP: 180 / 220` in monospace).
  - **Middle 40% (Fight Log Terminal):** A prominent, inset digital monitor box (`#08090A`). Text scrolls upward with a classic digital typewriter effect. Standard details are light grey; round headings are Chemical Orange; critical impact events and come-backs are highlighted in neon flashes. A vertical scrollbar styled like a rusty threaded screw tracks reading progress.
  - **Bottom 15% (Control Deck):** Flat, heavy metal control buttons. 
    - **"Start Fight" Button (Core):** Highly prominent Chemical Orange gradient, glowing border, turning inactive when clicked.
    - **"Next Round" Button (Core):** Weathered metal plate button that lights up with bright hazard lines when a round is ready to progress.
    - **Strategy Selectors (Core):** Small, inset dropdown selectors under each fighter's panel, displaying options such as `[ Rush Early ]` or `[ Wear Down ]` in a green monospaced digital font.

#### C. League Ladder Badges (Core)
The league progression requires five unique badge assets representing the competitive rungs of Digital Dog Arena.
- **Visual Progression States:**
  - **Locked State (Core):** Heavy, dark iron silhouette (`#1F2224`) wrapped in rusty chain-link overlays at 45% opacity. Standard padlock icon sits over the center.
  - **Unlocked State (Core):** Sharp, high-contrast concrete badge with raw metal plate borders. An energetic rim light corresponding to the league's mood borders the shape.
  - **Completed State (Core):** Polished chrome-and-steel finish, wrapped in glowing orange rim-lighting with a stenciled "COMPLETED" overlay stamp.
- **Badge Art Direction:**
  1. **Street League Badge (Core):** A gritty, spray-painted asphalt stencil pattern featuring crossed chains and a raw chain link silhouette.
  2. **Local Circuit Badge (Core):** A heavy iron spike-collar symbol set against concrete pitting.
  3. **Underground Circuit Badge (Core):** A steel wire mesh screen backdrop with a neon industrial warning sign emblem.
  4. **Elite Circuit Badge (Core):** Distressed bronze shield overlaid with twin double-bladed street axes.
  5. **Apex League Badge (Core):** Weathered dark steel crown welded to spiked chain-link fences.

#### D. Story Page Panel (Core)
The narrative portal where risk, reward, and underground faction relationships are forged.
- **Layout:** Built to resemble a steel clip-board or a heavy folder containing classified underground files.
- **Background Panel:** Thick, cracked concrete slab texture (`#1B1E20`) framing the text area.
- **Narrative Frame:** Left-aligned column containing high-contrast narrative text in a bright white/grey readable sans-serif font.
- **Choice Deck (Core):** A stacked vertical list of tactile cards representing player choices.
  - Hovering over a choice slightly increases its scale (1.02x) and overlays a thin Chemical Orange hazard outline.
  - Each choice card displays its risk and reputation impact on the bottom margin in tiny monospaced digital readouts (e.g., `REP: +2 | UNDERGROUND: +0 | RISK: +0.00`).

#### E. Narrator Panel (Core)
The ambient feed that constantly updates the player on the state of the kennel, fight results, and world interactions.
- **Interface Structure:** Split into two tabs along a heavy steel top bar.
  - **Tab 1: FEED (Core):** Shows a rolling digital feed of compressed, one-line event announcements (typewriter scroll, classic cyber-terminal mood).
  - **Tab 2: DETAILS (Core):** Accesses full, deep-dive historical combat logs or event breakdowns.
- **Visuals:** Framed inside an old cathode-ray tube (CRT) monitor bezel with horizontal scanning scanlines and a flickering green or orange phosphorescent glow.

---

## 3. Prototype Graphics Pipeline (Asset Design)

Prototype graphics will be systematically generated to match the gritty, high-contrast industrial style of Digital Dog Arena.

### Asset Inventory Checklist

| Asset Name | Tier | Asset Type | Concept & Style Prompt | Dominant / Accent Colors | Target In-Scene Dimensions |
|:---|:---|:---|:---|:---|:---|
| **bg_concrete_seamless** | Core | Texture2D | Distressed seamless concrete wall texture, heavy pits, hairline cracks, gritty industrial grunge, dark vignette, highly tileable. | `#1A1C1E` / `#111315` | 1024x1024 (seamless) |
| **bg_rusted_metal_seamless** | Core | Texture2D | Distressed rusty steel plate texture, oxidized metal, dark corrosion, heavy industrial rivets, tileable texture. | `#2B2E31` / `#C43400` | 1024x1024 (seamless) |
| **dog_portrait_diesel** | Core | Sprite | Gritty fighter pitbull portrait, scarred muzzle, intense red-glowing rim light, dark underground chain-link fence background, high contrast shadows. | `#1A1C1E` / `#FF6B00` | 1024x1024 (framed sprite) |
| **dog_portrait_phantom** | Optional | Sprite | Sleek black doberman portrait, wire muzzle, cold neon-blue rim light, damp concrete drainage wall backdrop, high contrast shadows. | `#111315` / `#00FFFF` | 1024x1024 (framed sprite) |
| **badge_street_league** | Core | Sprite | Spray-painted asphalt stencil icon, crossed steel chains, gritty urban spray texture, high-contrast badge shape. | `#3A3F42` / `#FF6B00` | 512x512 |
| **badge_local_circuit** | Core | Sprite | Industrial steel badge, thick rusted spiked collar symbol, weathered concrete pitting backdrop. | `#2B2E31` / `#C43400` | 512x512 |
| **badge_underground_circuit** | Core | Sprite | Distressed warning plate badge, wire-mesh pattern, vibrant glowing orange neon warning bars. | `#1A1C1E` / `#FF6B00` | 512x512 |
| **badge_elite_circuit** | Optional | Sprite | Heavy bronze shield, twin crossed street axes, scratched metal texture, high contrast shadow. | `#2B2E31` / `#D800FF` | 512x512 |
| **badge_apex_league** | Optional | Sprite | Dark steel crown welded to spiked chain-link fences, toxic gold rim-glow, high-contrast graphic symbol. | `#111315` / `#FFD700` | 512x512 |

---

## 4. Sensory & Game Feedback Design (Polishment)

To ground the tactical management gameplay, every action must receive an exaggerated sensory response that feels punchy and satisfyingly violent.

### Interaction Feedback Matrix

| Moment | Tier | Importance | Camera / Screen FX | Time Settings | Transform Anim | Visual FX | Audio Sound FX | Rationale |
|:---|:---|:---|:---|:---|:---|:---|:---|:---|
| **Round Fight Impact** | Core | Heavy | Directional shake scaled to damage, heavy chromatic aberration flash | Hitstop pause of 0.04s on heavy hits | Card squash & stretch (12% vertical squish, snap settle) | Burst of rust-colored concrete dust and bright spark particles | Heavy hollow metallic crunch, high pitch randomization (±12%) | Conveys immediate physicality, weight, and violent impact. |
| **Strategy Toggle** | Core | Minor | — | — | Subtle push-down scale (0.96x) on click | Small neon green indicator dot pulse | Sharp mechanical relay click | Confirms player input was registered with high-fidelity feedback. |
| **Dog Level Up** | Core | Medium | Brief fullscreen light vignette pulse | Slow-down ramp (0.4x speed for 0.5s) | Vertical card lift with springy overshoot settle | Chemical orange particle spray radiating outward | Rising industrial siren sound, low bass thud | Elevates the victory progression, rewarding breeding and training effort. |
| **Fighter Death** | Core | Critical | Radial screen shake, screen desaturation except status stamp | Dramatic slow-mo ramp to 0.15s for 1.2s | Card collapse (tilt 90 degrees and sink) | Large blood/oil mist burst, crackling sparks | Falling iron clang, low-frequency haptic hum | Punctuates the tragedy and heavy permanent loss of a bloodline dog. |
| **Tab Transition** | Core | Minor | — | — | Lateral page sliding bounce | Soft transition wipe of screen overlays | Distressed electronic hum | Fluid, responsive menu traversal. |

---

### Particle Systems & Visual FX

- **Concrete Impact Sparks (Core):** Activated during combat hits. Emits small, sharp concrete fragments and bright orange electric sparks. Particles must fall and bounce using light physics simulation to feel anchored in the scene.
- **Heavy Dust Smoke (Core):** Slow-moving, semi-transparent grey-brown smoke puffs billowing from card edges during heavy rounds, representing cage dust kicked up by the struggle.
- **Rival Challenge Glow (Optional):** A pulsing, unstable red neon aura surrounding the Rival fight panel on the Fight Page, visually building anticipation.

---

### Screen Post-Processing Specifications (URP Volume)

The gritty, dark underground mood is heavily supported by a high-contrast Post-Processing stack configured via a global URP Volume.

1. **Vignette (Core):**
   - *Intensity:* `0.45`
   - *Smoothness:* `0.35`
   - *Color:* Pure black (`#000000`)
   - *Rationale:* Pulls focus heavily to the center of the UI, simulating a dark, claustrophobic viewing terminal in a damp cellar.
2. **Bloom (Core):**
   - *Threshold:* `0.82`
   - *Intensity:* `1.3`
   - *Scatter:* `0.65`
   - *Rationale:* Essential for making active health segments, Chemical Orange alerts, and acid green indicators bleed light into the surrounding concrete-grey shadows, creating a striking "neon in the dark" contrast.
3. **Color Adjustments (Core):**
   - *Post Exposure:* `-0.15` (Slightly dims the scene to enrich shadow depth)
   - *Contrast:* `25.0` (Enforces high-contrast readable elements)
   - *Saturation:* `-12.0` (Desaturates cold greys and steels, allowing the warm orange and rust accents to command attention)
4. **Film Grain (Optional):**
   - *Type:* `Medium 1`
   - *Intensity:* `0.18`
   - *Rationale:* Adds a fine, dynamic texture across flat dark areas, breaking up clean digital vectors and mimicking old surveillance monitor feeds.

---

### Key Event Sequences

#### A. The Round Hit Impact Sequence
`Round Play Action (0ms) → Base Card Squash (12% volume-conserving, 10ms) → Full-Screen Camera Shake & Chromatic Aberration Peak (20ms) → Hitstop Pause (30ms - 70ms) → Card Overshoot Stretch (10% vertical stretch, 90ms) → Concrete Dust Burst and Spark Emission (110ms) → Card Settle to Idle Rest (180ms)`

#### B. The Story Choice Click Sequence
`Choice Button Pressed (0ms) → Scale down to 0.95x with industrial click SFX (15ms) → Button border flashes Acid Green or Rust Red depending on risk impact (40ms) → Card springs outward 1.05x with radial particle vapor (150ms) → Transition wipe of narrative text container (300ms)`
