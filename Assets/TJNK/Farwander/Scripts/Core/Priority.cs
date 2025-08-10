using System;

namespace TJNK.Farwander.Core
{
    /// <summary>Event/service priority classes. Lower is earlier.</summary>
    public enum EventPriority : int { System = 0, World = 1, Actor = 2, UI = 3 }

    /// <summary>Logical lanes for scheduling.</summary>
    public enum EventLane : int { Tick = 0, Action = 1, Dispatch = 2 }
}