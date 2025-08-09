using UnityEngine;
using System;
using TJNK.Farwander.Systems;

namespace TJNK.Farwander.Actors
{
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour
    {
        public int maxHp = 10;
        public int hp = 10;

        public event Action<Health> OnDeath;
        public event System.Action<Health> OnHealthChanged;

        public bool IsDead => hp <= 0;

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            hp -= Mathf.Max(1, amount);
            OnHealthChanged?.Invoke(this);          // <-- notify bars
            if (hp <= 0) Die();
        }
        
        private void Die()
        {
            OnDeath?.Invoke(this);

            // If player: pause game; otherwise destroy enemy
            if (GetComponent<PlayerController>())
            {
                Debug.Log("GAME OVER");
                Time.timeScale = 0f;
                // You can later show a UI overlay here
                enabled = false;
            }
            else
            {
                var actor = GetComponent<Actor>();
                if (actor && actor.HasGrid && ActorIndex.Instance)
                    ActorIndex.Instance.Unregister(actor, actor.Pos);
                Destroy(gameObject);
            }
        }
    }
}