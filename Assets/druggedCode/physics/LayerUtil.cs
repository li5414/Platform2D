using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class LayerUtil
    {
        /// <summary>
        /// 지정한 레이어마스크에 특정 레이어가 포함되었는지 검사
        /// </summary>
        static public bool Contains(LayerMask mask, int layer)
        {
            if ((mask.value & (1 << layer)) == 0)
            {
                return false;
            }

            return true;
        }


        //-- get layerMask

        static public LayerMask GetLayerMask(string layerName)
        {
            int i = LayerMask.NameToLayer(layerName);
            return GetLayerMask(i);
        }

        static public LayerMask GetLayerMask(int layerID)
        {
            return 1 << layerID;
        }

        //-- change Layer

        static public void ChangeLayer(GameObject obj, string layerName, bool changeChild = true, System.Type ignoreType = null)
        {
            int layerid = LayerMask.NameToLayer(layerName);

            ChangeLayer(obj, layerid, changeChild, ignoreType);
        }

        static public void ChangeLayer(GameObject obj, LayerMask msk, bool changeChild = true, System.Type ignoreType = null)
        {
            int layerid = GetLayerIdFromLayerMask(msk.value);

            ChangeLayer(obj, layerid, changeChild, ignoreType);
        }

        static private void ChangeLayer(GameObject obj, int layerId, bool changeChild = true, System.Type ignoreType = null)
        {
            ChangeLayer(obj, layerId, changeChild, ignoreType != null ? ignoreType.ToString() : "");
        }

        static private void ChangeLayer(GameObject obj, int layerid, bool changeChild = true, string ignoreTypeString = "")
        {
            if (obj.GetComponent(ignoreTypeString) != null) return;

            obj.layer = layerid;

            if (changeChild == false)
                return;

            foreach (Transform child in obj.transform)
            {
                if (child.GetComponent(ignoreTypeString) != null)
                    continue;

                ChangeLayer(child.gameObject, layerid, changeChild, ignoreTypeString);
            }
        }

        static public int GetLayerIdFromLayerMask(int maskValue)
        {
            if (maskValue < 0)
                return -1;

            if (maskValue == 1)
                return 1;

            if (maskValue % 2 != 0)
                return -1;

            int count = 0;
            do
            {
                maskValue /= 2;
                count++;
            } while (maskValue != 1);

            return count;
        }

        //-- change sortingLayer

        static public void ChanageSortingLayer(GameObject obj, string layername)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer ren in renderers)
            {
                ren.sortingLayerName = layername;
            }
        }
    }
}

