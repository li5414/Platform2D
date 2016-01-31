using UnityEngine;

namespace druggedcode.engine
{
    public interface IDamageable
    {
        void TakeDamage(int damage, GameObject attacker);
    }
}

