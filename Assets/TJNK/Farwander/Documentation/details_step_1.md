Great—here’s exactly what I’ll implement for **Step 1 – Dungeon Generation**, following the **Overall Implementation Prompt (v2)** and the **Step 1 prompt** you’ve got in the sidebar. No gameplay code beyond gen logic; pure C# services + events, deterministic, and test-first.

# What I’ll build

## Overview

* A **headless dungeon generator** (`IDungeonGenerator`) that produces a small, connected grid map made of `Wall|Floor`, plus room rects and suggested spawn points.
* **GameController** will, at **tick 1**, post `Gen_Request` (Action). The generator runs and immediately `Dispatch`es `Gen_Complete` with the `DungeonMap` result.
* Everything is **deterministic** via `DungeonConfigSO.Seed`. No scenes/Monos needed beyond the existing GameController; logic is pure C# registered in `QueryRegistry`.

## Files & Locations

* `Assets/TJNK/Farwander/Modules/Generation/`

  * `IDungeonGenerator.cs` (interface)
  * `RectDungeonGenerator.cs` (impl; rooms + L-corridors)
  * `DungeonMap.cs` (DTO: tiles/rooms/spawn points)
  * `MapTile.cs` (enum `Wall|Floor`)
  * `IMapQuery.cs` + `MapQuery.cs` (read‐only lookups/bounds/pathability)
* `Assets/TJNK/Farwander/Modules/Game/`

  * Wire-up only (in `GameControllerModuleProvider.Bind`): register the generator + map query in `QueryRegistry`, schedule `Gen_Request` at tick 1, handle `Gen_Complete`. (No entity spawning yet—out of scope for Step 1.)
* Tests (EditMode): `Assets/TJNK/Farwander/Tests/Generation/*.cs`

## Data & Types

* `enum MapTile { Wall=0, Floor=1 }`
* `sealed class DungeonMap`

  * `MapTile[,] Tiles` (size `Width×Height`)
  * `List<RectInt> Rooms` (non-overlapping, within bounds)
  * `List<Vector2Int> SpawnPoints` (suggestions: room centers; at least one for player)
* `interface IMapQuery`

  * `Vector2Int Size { get; }`
  * `bool InBounds(Vector2Int p)`
  * `MapTile GetTile(Vector2Int p)`
  * `bool IsWalkable(Vector2Int p)` (Floor only)

## Interfaces / Queries

* `interface IDungeonGenerator`

  * `DungeonMap Generate(int seed, Vector2Int size, Vector2Int roomMin, Vector2Int roomMax, int roomCount)`
* `MapQuery` wraps a produced `DungeonMap` and exposes `IMapQuery`.
* Both `IDungeonGenerator` and `IMapQuery` instances are **registered in `QueryRegistry`** by GameController after generation (so other systems can read the map).

## Events & Scheduling (frozen-core rules)

* **Action @ tick 1:**
  `Gen_Request { int Seed; Vector2Int Size; Vector2Int RoomMin; Vector2Int RoomMax; int RoomCount }`
* **Dispatch @ Now (same tick):**
  `Gen_Complete { DungeonMap Map }`
* GameController subscribes to `Gen_Complete` to stash the `DungeonMap` in `QueryRegistry` (as `IMapQuery`) and to be available for later steps (player/enemy spawns, FOV).

## Algorithm (small, fast, deterministic)

1. **Grid init:** Fill with `Wall`.
2. **Room placement:** Rejection sampling:

   * Randomly sample a room size within `[RoomMin,RoomMax]`, random top-left within bounds (keeping a 1-tile border optional).
   * Accept only if **no overlap** with existing rooms (simple AABB test).
   * Repeat until `RoomCount` placed or attempts exceeded (fall back to fewer rooms if config is too dense).
3. **Carve rooms:** Set tiles inside each rect to `Floor`.
4. **Corridors:** Connect rooms in their sorted order (e.g., by room center X):

   * For each adjacent pair, carve an **L-shaped** corridor: first horizontal, then vertical (or vice versa; pick deterministically), turning at the target’s coordinate.
   * Set corridor tiles to `Floor`. Ensure bounds checks.
5. **Spawn points:** Use **room centers** (rounded) as spawn suggestions; first center reserved for player in later steps.
6. **Determinism:** Use a `System.Random(seed)` instance throughout. No `Time` or global RNG.

## Validation (audit only, via `ValidationPipeline`)

* **Room overlap check:** assert no intersections.
* **Bounds check:** all rooms within map size.
* **Connectivity check:** BFS from first room floor to ensure all floors are reachable (rooms + corridors).
* On fail: record audit entries; generation still returns a map (but tests expect audit **PASS**).

## Atlas usage (data only here)

* Tiles **name mapping** for later renderers:

  * `Wall` → atlas slice `wall`
  * `Floor` → atlas slice `floor`
* If `UIConfigSO.Atlas` is absent, later UI can fall back to simple generated colors (already planned).

## Registration flow (GameController)

* On `Bind(core)`:

  * Read `Config.Dungeon` → extract size/rooms/seed.
  * **Schedule** `Gen_Request` at tick 1 (**Action**).
* On `Gen_Complete`:

  * Register `IMapQuery` (new `MapQuery(map)`) in `QueryRegistry`.
  * Keep the `DungeonMap` available for subsequent steps (spawns, FOV).
* No entity/UI work yet.

## EditMode Tests (deterministic, no scenes)

* `Generation_ProducesConnectedWalkableGraph`

  * Build from known seed; BFS over `Floor` covers all floor tiles.
* `Generation_NoRoomOverlap`

  * Assert pairwise non-overlap of `Rooms`.
* `Generation_Deterministic_GivenSeed`

  * Same inputs produce same hash of tiles (or room centers sequence).
* `Generation_TilesMatchConfigSize`

  * `Tiles.GetLength(0/1)` match `Width/Height`.
* Optional: `Generation_CorridorsReachAllRooms`, `Generation_SpawnPointsInsideRooms`.

## Edge cases & safeguards

* If `RoomCount` is too high for the map area, accept **fewer** rooms after a fixed attempt budget (log via ValidationPipeline).
* If `RoomMin > RoomMax` in config, **swap** at runtime (and audit log).
* Corridor carving clamps to bounds.
* We keep the operation **O(Width×Height + RoomCount)**; hard-capped attempt loops to keep tests sub-millisecond.

## Acceptance criteria

* Given the default config (e.g., 48×32, 8 rooms, seed=1337), tests pass and `Gen_Complete` fires within the first scheduler advance.
* The produced map is connected, rooms don’t overlap, and spawn points exist.
* No schedules violate lane rules (no Action to Now; Dispatch uses Now correctly).

---

If you want, I can also sketch the method signatures and the test file names before I start coding—just say the word.
