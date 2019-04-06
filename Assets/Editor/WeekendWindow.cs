using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace RayTracingWeekend
{
    public class WeekendWindow : EditorWindow
    {
        ChapterOne m_ChapterOne;
        ChapterTwo m_ChapterTwo;
        ChapterThree m_ChapterThree;
        ChapterFour m_ChapterFour;
        ChapterFive m_ChapterFive;
        ChapterFiveTwo m_ChapterFiveTwo;

        // default position and color same as the book
        float m_Chapter4ZPosition = -1f;
        Color32 m_Chapter4Color = Color.red;

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
            m_ChapterFour = new ChapterFour();
            m_ChapterFive = new ChapterFive();
            m_ChapterFiveTwo = new ChapterFiveTwo();
        }

        void OnGUI()
        {
            DrawChapterBasic(m_ChapterOne, "1");
            DrawChapterBasic(m_ChapterTwo, "2");
            DrawChapterBasic(m_ChapterThree, "3");
            
            EditorGUILayout.Space();
            DrawChapterFour();
            
            DrawChapterBasic(m_ChapterFive, "5.1");
            DrawChapterBasic(m_ChapterFiveTwo, "5.2");
        }

        void DrawChapterFour()
        {
            m_Chapter4ZPosition = EditorGUILayout.Slider("Z Pos", m_Chapter4ZPosition, -5f, -0.75f);
            m_Chapter4Color = EditorGUILayout.ColorField("Color", m_Chapter4Color);

            var color = m_Chapter4Color;
            const float rgbScale = Constants.rgbMultiplier;
            m_ChapterFour.sphereColor = new float3(color.r / rgbScale, color.g / rgbScale, color.b / rgbScale);
            m_ChapterFour.spherePositionZ = m_Chapter4ZPosition;
            
            if (GUILayout.Button($"Draw Chapter 4 Image"))
                m_ChapterFour.DrawToTexture();

            DrawTexture(m_ChapterFour.texture);
            EditorGUILayout.Separator();
        }

        static void DrawChapterBasic<T>(Chapter<T> chapter, string chapterNumber) where T: struct
        {
            if (GUILayout.Button($"Draw Chapter {chapterNumber} Image"))
                chapter.DrawToTexture();

            DrawTexture(chapter.texture);
            EditorGUILayout.Separator();
        }

        static void DrawTexture(Texture2D texture)
        {
            var size = Constants.ImageSize;
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(size.x), GUILayout.Height(size.y));
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }
    }
}