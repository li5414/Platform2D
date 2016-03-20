
using UnityEngine;

namespace druggedcode.engine
{
    public class HitData
    {
        public float damage;
		public Vector2 force;

        public HitData(float damage, Vector2 force)
        {
            this.damage = damage;
            this.force = force;
        }
    }
}
