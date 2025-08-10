# Farwander – Frozen Core Architecture (Design Doc)

*Last updated: 2025‑08‑10*

## 1) Purpose

Reboot Farwander on a tiny, **typed, scheduler‑first frozen core** that stays mostly immutable. Gameplay lives in modules that publish/subscribe events and schedule future work. We minimize `Update()` usage, keep execution **deterministic and predictable**, and make features easy to add without touching core.

---

## 2) Non‑goals

* No stringly‑typed events or generic bags of state.
* No global static singletons that survive scene reloads.
* No sprawling Update loops; avoid per‑frame polling unless strictly for rendering.

---

## 3) Core principles

1. **One clock** (ticks) drives everything.
2. **Typed events** and **typed queries**; modules own their event catalogs.
3. **Scheduler first**: actions and world changes occur by scheduled events.
4. **Non‑preemptive** pipelines: finish the current top‑level dispatch before starting another.
5. **Modules over monoliths**: gameplay code belongs to modules with clear boundaries.
6. **Validation as audit** initially; can be made blocking later.

---

## 4) Time & ticks

* **Clock type:** `ulong Now` (tick counter).
* **Default rate:** **30 ticks/sec** (configurable; never changed at runtime).
* **Real‑time → ticks:** floor to integer ticks; if any time elapses, **advance ≥ 1 tick**.

---

## 5) Event lanes & rules

We distinguish three event classes with explicit scheduling policy:

### 5.1 Tick events (world progression)

* Examples: `Dawn`, `PoisonTick`, optional `TurnTick`.
* **Must not** schedule to `Now`.

### 5.2 Action events (choices that advance state)

* Examples: `ActRequested`, `MoveCommand`, `AttackCommand`, `UseItemCommand`.
* Originates from player input or AI.
* **Must not** schedule to `Now` (schedule at `Now + ≥1`).

### 5.3 Dispatch / Effect events (immediate sub‑steps)

* Examples: `ApplyDamage`, `SpawnProjectile`, `AnimationPhase`, `LogLine`.
* **May** schedule to `Now` (same‑tick) to sequence an action’s pipeline.
* Same‑tick guardrails (below) apply.

**No interleaving of multiple top‑level dispatches.**

---

## 6) Ordering, budgets, and safety

* **Within‑tick order:** `(Priority ASC, Sequence ASC)`.
* **Priorities:** `System=0`, `World=1`, `Actor=2`, `UI=3` (lower = earlier).
* **Per‑cycle dispatch budget:** default **256** events; log on exceed.
* **Same‑tick per‑owner cap (Dispatch lane):** **8** events.
* **Handles & cancellation:** each schedule returns a handle. `Cancel(handle)` marks dead (no queue surgery). `CancelOwned(owner)` removes all future work for a given owner.
* **Ownership required** for all schedules (e.g., `Guid` actor/system id). Owner must be alive/active when handling.

---

## 7) Pause & save/load

* **Pause = hard stop**: don’t advance `Now`; don’t dispatch.
* **Save/Load restores**: `Now` and the entire queue (tick, priority, sequence, owner, payload DTO). **No history replay.**

---

## 8) Core components (frozen API surface)

### 8.1 Event bus (typed)

```csharp
public interface IEventBus {
  void Publish<T>(T evt);
  void Subscribe<T>(System.Action<T> handler);
  void Unsubscribe<T>(System.Action<T> handler);
}
```

### 8.2 Query registry (typed)

```csharp
public interface IQueryRegistry {
  void Register<T>(System.Func<T> factory);
  T Get<T>() where T : class;
}
```

### 8.3 Timed scheduler

```csharp
public enum Priority { System=0, World=1, Actor=2, UI=3 }

public readonly struct ScheduleHandle {
  public readonly ulong Tick; public readonly uint Sequence; public readonly System.Guid? Owner;
  public ScheduleHandle(ulong tick, uint seq, System.Guid? owner) { Tick=tick; Sequence=seq; Owner=owner; }
}

public interface ITimedScheduler {
  ulong Now { get; }
  ScheduleHandle ScheduleAt<T>(T evt, ulong atTick, Priority prio = Priority.Actor, System.Guid? owner = null);
  ScheduleHandle ScheduleAfter<T>(T evt, ulong delayTicks, Priority prio = Priority.Actor, System.Guid? owner = null);
  bool Cancel(in ScheduleHandle handle);
  int  CancelOwned(System.Guid owner);
  int  DispatchDue(ulong upToTick, int budget = 256); // returns processed count
  void AdvanceTo(ulong tick, int budget = 256);
}
```

### 8.4 Game core (scene‑scoped Mono)

* Holds instances of `IEventBus`, `IQueryRegistry`, `ITimedScheduler`.
* Serialized array/list of **ModuleConfig** (ScriptableObjects) that reference providers.
* On `Awake`: initialize core, instantiate providers (children GOs), call `Initialize(queries, bus, scheduler)`.
* Minimal `Update()`: convert deltaTime→ticks (floor, min=1 when >0), `scheduler.AdvanceTo(Now+advance)` unless paused.

### 8.5 Module contract

```csharp
public interface IGameModule {
  void Initialize(IQueryRegistry queries, IEventBus events, ITimedScheduler scheduler);
}
```

---

## 9) Modules & event catalog location

* **Events live with their modules**, not in core.
* Shared cross‑module contracts can live in `Modules/SharedEvents/` (still outside core).

---

## 10) Folder layout

```
Assets/TJNK/Farwander/
  Core/
  Modules/
  Content/
  World/
  Actors/
  Editor/
  Tests/                  // Unity Test Runner compatible
    Farwander.Core.Tests.asmdef
    EventBusTests.cs
    SchedulerTests.cs
    Tests_README.md
```

---

## 11) Validation pipeline (audit mode)

* Non‑blocking initially; logs warnings.
* Can be upgraded to blocking later.

---

## 12) Input & animation

* **Input** → **Action events** at `Now+1`.
* **Animation** phases as **Dispatch events**.

---

## 13) Determinism & RNG

* Stable ordering ensures consistent dispatch within a tick.
* Centralize RNG per domain.

---

## 14) Guardrails

1. Tick & Action events **cannot** schedule to `Now`.
2. Dispatch events **may** schedule to `Now`, capped per owner (8) and per cycle (budget).
3. Non‑preemptive pipelines.
4. All schedules have **owners**.
5. Keep handlers short.
6. Monitor queue lengths lightly.
7. Pause stops time; save/load restores state.

---

## 15) Rollout plan

1. Core skeleton + Unity Test Runner setup.
2. Publishers → Subscribers migration.
3. Scheduling migration.
4. Validation registration.
5. Save/load queue persistence.

---

## 16) Open knobs

* Ticks/sec: **30** (configurable at startup; no runtime change).
* Budget per dispatch cycle: **256**.
* Same‑tick cap per owner (Dispatch lane): **8**.
* Priorities: `System, World, Actor, UI`.
* Clock type: `ulong`.
