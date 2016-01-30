using UnityEngine;
using System.Collections;
namespace druggedcode.extensions.transform
{
    static public class TransformExtensions
    {   
        public static void ResetTransformation( this Transform trans )
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }
    }
}
