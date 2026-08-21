# Asset source register

Record an entry here before importing an external asset into the playable game.

| Asset ID | Asset | Source/author | License | Commercial use verified | PC tier | Mobile tier | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| carvino-hatch-93 | Original prototype hatch | Carvino project | Original | Yes | placeholder source | placeholder source | In game |
| carvino-pickup-91 | Original prototype pickup | Carvino project | Original | Yes | placeholder source | placeholder source | In game |
| carvino-trackside-scoreboards-01 | Original procedural finish-line scoreboards | `Assets/Carvino/Editor/TracksideScoreboardBuilder.cs` | Original | Yes | primitive geometry, no textures | primitive geometry, no textures | In game |
| carvino-pc-garage-bay-dressing-01 | Original garage workbench, locker, lighting, and bay markers | `Assets/Carvino/Editor/GarageBayDressingBuilder.cs` | Original | Yes | 30 static primitives, no textures | can be omitted or merged into lower-detail garage tier | In game |
| carvino-starter-pavilion-01 | Original procedural starter-side timing pavilion | `Assets/Carvino/Editor/TracksideStarterPavilionBuilder.cs` | Original | Yes | 24 static primitives, no textures | can be omitted from mobile or merged into a low-detail static strip pass | In game |
| carvino-pc-prepped-asphalt-01 | Original prepped drag-strip asphalt material | `Assets/Carvino/Art/Textures/carvino_pc_prepped_asphalt_01.png` | Original | Yes | 2048px, mipmapped, aniso 8, high-quality standalone compression | downscale to 1024px / lower compression tier | In game |
| carvino-pc-garage-presentation-01 | Original garage presentation lighting and wall detail | `Assets/Carvino/Editor/GaragePresentationBuilder.cs` | Original | Yes | 19 static primitives and 3 no-shadow spot lights | omit or reduce to one light and no wall detail | In game |
| carvino-race-presentation-01 | Original procedural start/finish framing and distance beacons | `Assets/Carvino/Editor/TracksideRacePresentationBuilder.cs` | Original | Yes | 42 static primitives, no textures | omit the finish arch and reduce distance beacons | In game |

## carvino-pc-garage-presentation-01 review

- **Source / author:** Original Carvino procedural geometry and lighting, created in `GaragePresentationBuilder.cs`.
- **Rights / branding:** No external mesh, image, logo, badge, signage, text, or texture used; suitable for commercial distribution.
- **Geometry:** 19 low-poly Unity primitive renderers: a rear wall treatment, trim, emissive strips, and an inset service-bay plate. Colliders are removed because every prop is visual-only.
- **Lighting / tiers:** Three dynamic spot lights with shadows disabled for clean vehicle presentation at a minimal PC cost. The mobile tier can omit the complete pass or retain a single non-shadow fill light.
- **Unity setup:** Baked into the Garage scene via **Carvino → Art → Add PC Garage Presentation Pass**. The editor helper is excluded from player builds.
- **Review:** 2026-08-21, Carvino PC Visual-Quality worker.

## carvino-race-presentation-01 review

- **Source / author:** Original Carvino procedural geometry created in `TracksideRacePresentationBuilder.cs`.
- **Rights / branding:** No external mesh, image, badge, logo, venue mark, signage, text, or source texture used; suitable for commercial distribution.
- **Geometry:** A visual-only starting truss, starter pedestals/beacons, finish arch, and paired distance beacons. Approximately 42 low-poly Unity primitive renderers; colliders are removed, preserving race driving and timing behavior.
- **Textures / tiers:** None. Flat metal/concrete and emission materials only. PC uses the complete pass; mobile may remove the finish arch and retain only a reduced set of five distance blocks.
- **Unity setup:** Baked into the QuarterMilePrototype scene through **Carvino → Art → Add Race Presentation Pass**. The editor helper is excluded from player builds. It does not rebuild generated scenes or modify runtime code.
- **Review:** 2026-08-21, Carvino Race Presentation worker.

## carvino-pc-prepped-asphalt-01 review

- **Source / author:** Original Carvino texture created for this project; no external photo, logo, or copied track material was used.
- **Rights / branding:** Commercially usable original asset. No manufacturer or venue marks are present.
- **Resolution / tiers:** 2048 × 2048 PNG source for the PC prototype. Unity import uses repeat wrapping, mipmaps, anisotropic filtering level 8, and standalone compression quality 100. Future Ultra can use a new true 4K source; Performance/mobile uses a 1024px derivative.
- **Unity setup:** Assigned by `PrototypeSceneBuilder.cs` to the quarter-mile asphalt when explicitly rebuilding generated scenes.
- **Review:** 2026-08-21, Carvino PC Texture/Material Artist.

## carvino-starter-pavilion-01 review

- **Source / author:** Original Carvino procedural geometry created in `TracksideStarterPavilionBuilder.cs`.
- **Rights / branding:** No external asset, badge, logo, copied signage, text, source mesh, or texture used; suitable for commercial distribution.
- **Geometry:** One small trackside pavilion at the starter end with simple room, roof, windows, stairs, rails, and lights. 24 low-poly Unity primitive renderers; colliders are removed because all parts are visual-only.
- **Textures / tiers:** None. PC uses flat steel, glass-color, concrete, and emissive materials. The mobile tier can omit it or merge it into a simplified static strip dressing pass.
- **Unity setup:** Baked into the quarter-mile scene via **Carvino → Art → Add Original Starter Pavilion**. The editor helper is excluded from player builds.
- **Review:** 2026-08-21, Carvino PC Environment/Level Artist.

## carvino-pc-garage-bay-dressing-01 review

- **Source / author:** Original Carvino procedural geometry, created in `GarageBayDressingBuilder.cs`.
- **Rights / branding:** No external asset, logo, manufacturer mark, text, source mesh, or texture used; suitable for commercial distribution.
- **Geometry:** One workbench, one tool locker, three overhead fixtures, and floor bay markers. Approximately 30 low-poly Unity primitive renderers; colliders are removed because all props are visual-only.
- **Textures / tiers:** None. PC uses flat metal and emissive materials; the mobile tier can omit the dressing or batch it into a simplified static garage pass.
- **Unity setup:** Baked into the Garage scene via **Carvino → Art → Add PC Garage Bay Dressing**. The editor helper is excluded from player builds.
- **Review:** 2026-08-21, Carvino PC Art/Content worker.

## carvino-trackside-scoreboards-01 review

- **Source / author:** Original Carvino procedural geometry, created in `TracksideScoreboardBuilder.cs`.
- **Rights / branding:** No external asset, logo, manufacturer mark, text, or source texture used; safe for commercial distribution.
- **Geometry:** Two scoreboards; 17 Unity primitive renderers per board, constructed once into the quarter-mile scene. Colliders are removed because the props are visual-only.
- **Textures / tiers:** None. Uses small flat-color and emission materials, so no texture memory or compressed texture footprint on PC or mobile.
- **Unity setup:** Baked as ordinary static scene primitives through **Carvino → Art → Add Trackside Scoreboards**. The editor helper is excluded from player builds.
- **Review:** 2026-08-21, Carvino Art/Content worker.

## Required fields for each new entry

- Exact source URL or original-project location
- Author/rights holder and license
- Whether badges, branding or other protected marks were removed
- Source mesh triangle count and LOD counts
- Texture resolutions and compressed size by quality tier
- Collision setup and Unity import settings
- Review date and reviewer

No entry means no import into the release build.
