using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class CameraUtil
    {
        static public Rect GetScreenRect(Camera camera)
        {
            float depth = -camera.transform.position.z;
            Vector3 lb = camera.ScreenToWorldPoint(new Vector3(0, 0, depth ));
            Vector3 rt = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, depth ));

            return new Rect(lb.x, lb.y,
                            rt.x - lb.x, rt.y - lb.y);
        }

        static public Vector3 ConvertPosition(Vector3 pos, Camera from, Camera to)
        {
            pos = from.WorldToViewportPoint(pos);
            return to.ViewportToWorldPoint(pos);
        }
    }
}
