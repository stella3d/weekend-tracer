using UnityEditor;
using UnityEngine;

namespace RayTracingWeekend
{
    public class WeekendWindow : EditorWindow
    {
        ChapterOne m_ChapterOne;
        ChapterTwo m_ChapterTwo;
        ChapterThree m_ChapterThree;

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
            m_ChapterThree = new ChapterThree();
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
            
            if (GUILayout.Button("Draw Chapter Three Image"))
                m_ChapterThree.WriteTestImage();

            DrawTexture(m_ChapterThree.texture);
        }

        static void DrawTexture(Texture2D texture)
        {
            var size = Constants.ImageSize;
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(size.x), GUILayout.Height(size.y));
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }
    }
}