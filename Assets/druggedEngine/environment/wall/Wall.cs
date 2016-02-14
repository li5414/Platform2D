using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    [RequireComponent(typeof(Collider2D))]
    [ExecuteInEditMode]
    public class Wall : MonoBehaviour
    {
        public WallSlideWay slideWay;

        void Start()
        {
            LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_PLATFORM);
        }
    }

}
