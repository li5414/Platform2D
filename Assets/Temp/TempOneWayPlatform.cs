using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    [RequireComponent( typeof(EdgeCollider2D) )]
    public class TempOneWayPlatform : TempPlatform
    {
        override protected void UpdateLayer()
        {
            LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_ONEWAY);
        }
    }
}
