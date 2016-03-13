
using UnityEngine;

namespace druggedcode.engine
{
    public struct HitData
    {
        public float damage;
        public Vector2 origin;
        public Vector2 point;
        public Vector2 velocity;
        public string tag;

        public HitData(float damage, Vector2 origin, Vector2 point, Vector2 velocity)
        {
            this.damage = damage;
            this.origin = origin;
            this.point = point;
            this.velocity = velocity;
            this.tag = "";
        }

        public HitData(float damage, Vector2 origin, Vector2 point, Vector2 velocity, string tag)
        {
            this.damage = damage;
            this.origin = origin;
            this.point = point;
            this.velocity = velocity;
            this.tag = tag;
        }
    }
}

