using UnityEngine;

namespace druggedcode.engine
{
    public interface IDamageable
    {
		void Hit( float Damage );
		void Hit( HitData hitdata );
    }
}

