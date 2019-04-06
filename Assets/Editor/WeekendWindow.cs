using UnityEditor;
using UnityEngine;

namespace RayTracingWeekend
{
    public class WeekendWindow : EditorWindow
    {
        ChapterOne m_ChapterOne;
        ChapterTwo m_ChapterTwo;

        [MenuItem("Window/Tracer")]
        public static void ShowWindow()
        {
            var window = GetWindow<WeekendWindow>();
            window.Show();
        }

        void OnEnable()
        {
            m_ChapterOne = new ChapterOne();
            m_ChapterTwo = new ChapterTwo();
        }

        void OnGUI()
        {
            if (GUILayout.Button("Draw Chapter One Image"))
                m_ChapterOne.WriteTestImage();

            DrawTexture(m_ChapterOne.texture);
            
            EditorGUILayout.Separator();
            
            if (GUILayout.Button("Draw Chapter Two Image"))
                m_ChapterTwo.WriteTestImage();

            DrawTexture(m_ChapterTwo.texture);
        }


        void DrawTexture(Texture2D texture)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(200), GUILayout.Height(100));
            EditorGUI.DrawPreviewTexture(rect, texture);
        }

    }
}