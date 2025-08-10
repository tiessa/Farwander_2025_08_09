namespace TJNK.Farwander.Modules.Game
{
    // Generation
    public struct Gen_Request { public int Seed; public UnityEngine.Vector2Int Size; public UnityEngine.Vector2Int RoomMin; public UnityEngine.Vector2Int RoomMax; public int RoomCount; }
    public struct Gen_Complete { public object DungeonMap; }

    // Input & Movement
    public enum Direction8 { Left, Right, Up, Down, UpLeft, UpRight, DownLeft, DownRight }
    public struct Input_Move { public Direction8 Dir; }
    public struct Input_Wait { }
    public struct Move_Request { public int EntityId; public Direction8 Dir; }
    public struct Move_Resolved { public int EntityId; public UnityEngine.Vector2Int From; public UnityEngine.Vector2Int To; public bool Succeeded; }

    // Spawning / Entities (for views)
    public struct Entity_Spawned { public int EntityId; public UnityEngine.Vector2Int Pos; public bool IsPlayer; public string SpriteName; }

    // AI
    public struct AI_Tick { }
    public struct AI_MoveIntent { public int EntityId; }

    // FOV
    public struct Fov_Recompute { public int ViewerId; public int Radius; }
    public struct Fov_Updated { }

    // Items/Inventory
    public struct Item_Spawned { public int ItemId; public UnityEngine.Vector2Int Pos; }
    public struct Item_PickupRequest { public int EntityId; public int ItemId; }
    public struct Item_PickedUp { public int EntityId; public int ItemId; }
    public struct Item_DropRequest { public int EntityId; public int ItemId; }
    public struct Item_Dropped { public int EntityId; public int ItemId; public UnityEngine.Vector2Int Pos; }

    // Equipment
    public enum EquipSlot { Head, Body, MainHand }
    public struct Equip_Request { public int EntityId; public int ItemId; public EquipSlot Slot; }
    public struct Equip_Changed { public int EntityId; public EquipSlot Slot; public int ItemId; public bool Equipped; }

    // Character view
    public struct CharView_Request { public int EntityId; }
    public struct CharView_DataReady { }

    // Combat
    public struct Combat_MeleeRequest { public int Attacker; public int Defender; }
    public struct Combat_Resolved { public int Attacker; public int Defender; public int Damage; public bool DefenderDied; }
    public struct Health_Changed { public int EntityId; public int HP; public int MaxHP; }
    public struct Entity_Died { public int EntityId; }
    public struct Entity_Respawned { public int EntityId; public UnityEngine.Vector2Int Pos; }

    // Targeting
    public struct Targeting_Begin { public int EntityId; }
    public struct Targeting_Move { public Direction8 Dir; }
    public struct Targeting_Confirm { public UnityEngine.Vector2Int TargetCell; }
    public struct Targeting_Cancel { }

    // Ranged
    public struct Ranged_AttackRequest { public int Attacker; public int Target; }
    public struct Ranged_Resolved { public int Attacker; public int Target; public int Damage; public bool Hit; }

    // Spells/Mana
    public struct Spell_CastRequest { public int Caster; public SpellKind Spell; public int Target; }
    public struct Spell_Resolved { public int Caster; public SpellKind Spell; public int Target; public int Amount; }
    public struct Mana_Changed { public int EntityId; public int Mana; public int MaxMana; }
}
