using TJNK.Farwander.Actors;

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
            return true;
        }
    }
}