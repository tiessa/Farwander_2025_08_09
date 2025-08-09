using UnityEngine;
using TJNK.Farwander.Actors;
using TJNK.Farwander.Systems.UI;

namespace TJNK.Farwander.Systems
{
    public static class CombatResolver
    {
        public static bool TryMelee(Actor attacker, Actor defender, int baseDamage = 3)
        {
            if (attacker == null || defender == null) return false;
            var hp = defender.GetComponent<Health>();
            if (!hp) return false;

            hp.TakeDamage(baseDamage);

            // pop the number at defender’s position
            DamagePopup.Spawn(defender.transform.position, baseDamage);

            CombatLog.Instance?.Log($"{attacker.name} hits {defender.name} for {baseDamage}.");
            if (hp.IsDead)
                CombatLog.Instance?.Log($"{defender.name} dies!");

            return true;
        }
    }
}