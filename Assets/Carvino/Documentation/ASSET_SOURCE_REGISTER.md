# Asset source register

Record an entry here before importing an external asset into the playable game.

| Asset ID | Asset | Source/author | License | Commercial use verified | PC tier | Mobile tier | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| carvino-hatch-93 | Original prototype hatch | Carvino project | Original | Yes | placeholder source | placeholder source | In game |
| carvino-pickup-91 | Original prototype pickup | Carvino project | Original | Yes | placeholder source | placeholder source | In game |
| carvino-trackside-scoreboards-01 | Original procedural finish-line scoreboards | `Assets/Carvino/Editor/TracksideScoreboardBuilder.cs` | Original | Yes | primitive geometry, no textures | primitive geometry, no textures | In game |
| carvino-pc-garage-bay-dressing-01 | Original garage workbench, locker, lighting, and bay markers | `Assets/Carvino/Editor/GarageBayDressingBuilder.cs` | Original | Yes | 30 static primitives, no textures | can be omitted or merged into lower-detail garage tier | In game |

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
