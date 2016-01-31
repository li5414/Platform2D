using UnityEngine;
using System.Collections;
using System;

namespace druggedcode
{
    [Serializable]
    public class HealthState
    {
        [SerializeField]
        float _hp;
        [SerializeField]
        float _hpTotal;

        public void Init(float hp, float total)
        {
            _hp = hp;
            _hpTotal = total;
        }

        public float Damaged(float dmg)
        {
            hp = _hp - dmg;
			
            return _hp;
        }

        public float Heal(float heal)
        {
            hp = _hp + heal;

            return _hp;
        }

        public float hp
        {
            get{ return _hp; }
            set
            {
                if (_hp == value)
                    return;
				
                _hp = value;
				
                if (_hp < 0)
                {
                    _hp = 0;
                }
                else if (_hp > _hpTotal)
                {
                    _hp = _hpTotal;
                }
            }
        }

        public bool IsFull
        {
            get{ return _hp == _hpTotal; }
        }

        public bool IsDead
        {
            get{ return _hp <= 0; }
        }
    }
}
