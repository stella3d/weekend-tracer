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
        ChapterSix m_ChapterSix;
        ChapterSeven m_ChapterSeven;
        ChapterEight m_ChapterEight;

        // default position and color same as the book
        float m_Chapter4ZPosition = -1f;
        Color32 m_Chapter4Color = Color.red;

        Vector2 m_ScrollPosition;

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
            m_ChapterSix = new ChapterSix();
            m_ChapterSeven = new ChapterSeven();
            m_ChapterEight = new ChapterEight();
        }

        void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            
            DrawChapterBasic(m_ChapterOne, "1");
            DrawChapterBasic(m_ChapterTwo, "2");
            DrawChapterBasic(m_ChapterThree, "3");
            
            EditorGUILayout.Space();
            DrawChapterFour();
            
            DrawChapterBasic(m_ChapterFive, "5.1");
            DrawChapterBasic(m_ChapterFiveTwo, "5.2");

            DrawChapterSix();
            DrawChapterSeven();
            DrawChapterEight();
            
            EditorGUILayout.EndScrollView();
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

        int m_SampleCountSix = 16;
        int m_SampleCountSeven = 16;

        float m_AbsorbRateSeven = 0.5f;
        float m_PreviousAbsorbRateSeven = 0.5f;
        
        int m_SampleCountEight = 16;
        float m_PreviousAbsorbRateEight = 0.5f;

        
        void DrawChapterSix()
        {
            m_SampleCountSix = EditorGUILayout.IntField("Sample Count", m_SampleCountSix);
            m_ChapterSix.numberOfSamples = m_SampleCountSix;
            DrawChapterBasic(m_ChapterSix, "6");
        }
        
        void DrawChapterSeven()
        {
            m_SampleCountSeven = EditorGUILayout.IntField("Sample Count", m_SampleCountSeven);
            m_ChapterSeven.numberOfSamples = m_SampleCountSeven;

            m_AbsorbRateSeven = EditorGUILayout.Slider("Absorb Rate", m_AbsorbRateSeven, 0.05f, 0.95f);
            m_ChapterSeven.absorbRate = m_AbsorbRateSeven;

            if (!Mathf.Approximately(m_AbsorbRateSeven, m_PreviousAbsorbRateSeven))
            {
                m_ChapterSeven.DrawToTexture();
            }

            m_PreviousAbsorbRateSeven = m_AbsorbRateSeven;
            DrawChapterBasic(m_ChapterSeven, "7");
        }
        
        void DrawChapterEight()
        {
            m_SampleCountEight = EditorGUILayout.IntField("Sample Count", m_SampleCountEight);
            m_ChapterEight.numberOfSamples = m_SampleCountEight;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("fuzziness left");
            m_ChapterEight.fuzzinessOne = Mathf.Clamp01(EditorGUILayout.DelayedFloatField(m_ChapterEight.fuzzinessOne));
            EditorGUILayout.PrefixLabel("fuzziness right");
            m_ChapterEight.fuzzinessTwo = Mathf.Clamp01(EditorGUILayout.DelayedFloatField(m_ChapterEight.fuzzinessTwo));
            EditorGUILayout.EndHorizontal();
            
            DrawChapterBasic(m_ChapterEight, "8");
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
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(size.x * 4), GUILayout.Height(size.y * 4));
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }
    }
}