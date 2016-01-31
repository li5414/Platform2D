using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class ItemCoin : Item
    {
        /// 획득 했을 때 추가될 점수
        public int PointsToAdd = 10;

        override protected void Getted()
        {
            base.Getted();

            Debug.Log("[ Add " + PointsToAdd + " POINT!]");
        }
    }
}
