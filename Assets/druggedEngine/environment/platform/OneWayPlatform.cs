using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    [RequireComponent( typeof(EdgeCollider2D) )]
    public class OneWayPlatform : Platform
    {
        override protected void UpdateLayer()
        {
            LayerUtil.ChangeLayer(gameObject, DruggedEngine.OneWayPlatform);
        }
    }
}
