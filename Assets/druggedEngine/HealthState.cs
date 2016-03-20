using UnityEngine;
using System.Collections;
using System;

namespace druggedcode
{
    [Serializable]
    public class HealthState
    {
        [SerializeField]
		float mHp;

		float mHpTotal;

		public HealthState( float total )
		{
			mHpTotal = total;
			mHp = mHpTotal;
		}

        public float Damaged(float dmg)
        {
            hp = mHp - dmg;
			
            return mHp;
        }

        public float Heal(float heal)
        {
            hp = mHp + heal;

            return mHp;
        }

        public float hp
        {
            get{ return mHp; }
            set
            {
                if (mHp == value)
                    return;
				
                mHp = value;
				
                if (mHp < 0)
                {
                    mHp = 0;
                }
                else if (mHp > mHpTotal)
                {
                    mHp = mHpTotal;
                }
            }
        }

        public bool IsFull
        {
            get{ return mHp == mHpTotal; }
        }

        public bool IsDead
        {
            get{ return mHp <= 0; }
        }

		public string ToString()
		{
			return mHp + " / " + mHpTotal;
		}
    }
}
