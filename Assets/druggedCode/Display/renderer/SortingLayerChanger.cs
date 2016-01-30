using UnityEngine;


namespace druggedcode
{
    /// <summary>
    /// 자신 혹은 자식들의 sortingLayer를 변경한다. 파티클이 포함되어 있다면 파티클의 Sortinglayer도 변경한다.
    /// </summary>
    [ExecuteInEditMode]
    public class SortingLayerChanger : MonoBehaviour
    {
        public bool applyChildren;
        [HideInInspector]
        public int sortingLayerID;
        public void Excute()
        {
            UpdateParticleLayer(GetComponentsInChildren<ParticleSystem>());

            if (applyChildren)
            {
                UpdateLayer(GetComponentsInChildren<Renderer>());
            }
            else
            {
                UpdateLayer(GetComponents<Renderer>());
            }
        }

        void UpdateParticleLayer(ParticleSystem[] ptcs)
        {
            foreach (var ptc in ptcs)
            {
                ptc.GetComponent<Renderer>().sortingLayerID = sortingLayerID;
            }
        }

        void UpdateLayer(Renderer[] renderers)
        {
            foreach (var ren in renderers)
            {
                ren.sortingLayerID = sortingLayerID;
            }
        }
    }
}

