using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 획득 하면 플레이어의 무기가 변화
    /// </summary>
    public class ItemWeapon : Item
    {
        ///플레이어에 전달할 무기 프리팹
        //        public Weapon WeaponToGive;

        override protected void Getted()
        {
            base.Getted();
//            collider.GetComponent<CharacterShoot>().ChangeWeapon(WeaponToGive);

        }
    }
}