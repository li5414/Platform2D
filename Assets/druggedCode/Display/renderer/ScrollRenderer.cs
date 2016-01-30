using UnityEngine;

namespace druggedcode
{
    public class ScrollRenderer : MonoBehaviour
    {
        public float speed = 0.1f;
        
        void Update()
        {
            float y = Mathf.Repeat(Time.time * speed, 1);
            Vector2 offset = new Vector2(0, y);
            GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset);
        }
    }
}
