using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class Fps : MonoBehaviour
    {
        private int m_frames = 0;
        private float m_time_now;
        private float m_update_interval = 0.25f;
        private float m_last_interval;
        private float m_fps;
        private string m_fps_text;

        void Start()
        {
            //Application.targetFrameRate = 60;
            m_last_interval = Time.realtimeSinceStartup;
            m_frames = 0;
        }

        void Update()
        {
            m_frames++;
            m_time_now = Time.realtimeSinceStartup;

            if (m_time_now > m_last_interval + m_update_interval)
            {
                //fps = (int)Mathf.Round(frames / (timeNow - lastInterval));
                m_fps = m_frames / (m_time_now - m_last_interval);
                float ms = 1000.0f / Mathf.Max(m_fps, 0.00001f);
                m_fps_text = ms.ToString("0.####") + " ms\n" + m_fps.ToString("0.#") + " fps";
                m_frames = 0;
                m_last_interval = m_time_now;
            }
        }

        void OnGUI()
        {
            GUIStyle labelLeft = new GUIStyle("label");
            labelLeft.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(10, 10, 400, 40), m_fps_text);
        }
    }
}
