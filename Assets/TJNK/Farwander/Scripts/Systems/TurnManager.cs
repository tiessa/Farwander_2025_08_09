using System.Collections.Generic;
using UnityEngine;

namespace TJNK.Farwander.Systems
{
    public enum TurnPhase { Player, Enemies, Busy }

    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }
        public TurnPhase Phase { get; private set; } = TurnPhase.Player;

        private readonly List<IEnemyActor> enemies = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void RegisterEnemy(IEnemyActor enemy)
        {
            if (!enemies.Contains(enemy)) enemies.Add(enemy);
        }

        public void UnregisterEnemy(IEnemyActor enemy)
        {
            enemies.Remove(enemy);
        }

        public void EndPlayerTurn()
        {
            Phase = TurnPhase.Enemies;
            StartCoroutine(RunEnemies());
        }

        private System.Collections.IEnumerator RunEnemies()
        {
            foreach (var e in enemies)
                yield return e.TakeTurn();

            Phase = TurnPhase.Player;
        }

        public bool IsPlayerTurn() => Phase == TurnPhase.Player;
    }

    public interface IEnemyActor
    {
        System.Collections.IEnumerator TakeTurn();
    }
}