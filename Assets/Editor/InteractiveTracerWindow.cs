using UnityEditor;
using UnityEngine;

namespace RayTracingWeekend
{
    public class InteractiveTracerWindow : EditorWindow
    {
        Texture2D m_TracerRenderTexture;

        void OnEnable()
        {
            minSize = new Vector2(200, 100);
            maxSize = new Vector2(1600, 800);
            Debug.Log("on enable interactive tracer");
            m_TracerRenderTexture = new Texture2D(200, 100, TextureFormat.RGBAFloat, false);
        }

        public void OnGUI()
        {
            EditorGUILayout.LabelField("tracer render texture");
            var rect = GUILayoutUtility.GetRect(200, 100);
            EditorGUI.DrawPreviewTexture(rect, m_TracerRenderTexture, null, ScaleMode.ScaleToFit);
        }

        [MenuItem("Window/Weekend Tracer/Interactive")]
        public static void ShowWindow()
        {
            Debug.Log("show ineractive???");
            var window = GetWindow<InteractiveTracerWindow>();
            window.ShowAuxWindow();
        }
    }
}