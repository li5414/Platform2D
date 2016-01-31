using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 획득하면 플레이어가 hp 회복
    /// </summary>
    public class ItemRedDrug : Item
    {
        // 획득 했을 때 플레이어에게 전달 될 hp
        public int HealthToGive = 5;

        override protected void Getted()
        {
            base.Getted();

            Debug.Log("[ " + HealthToGive + " HEAL! ]");
        }
    }
}
