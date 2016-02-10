using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    [RequireComponent(typeof(Collider2D))]
    [ExecuteInEditMode]
    public class Wall : MonoBehaviour
    {
        public enum WallSlideWay
        {
            NOTHING,
            LEFT,
            RIGHT,
            BOTH
        }

        public WallSlideWay slideWay;

        void Start()
        {
            if (Application.isPlaying == false) return;

            LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_PLATFORM);
            LayerUtil.ChanageSortingLayer(gameObject, DruggedEngine.SORTING_LAYER_ENVIRONMENT);
        }
    }

}
