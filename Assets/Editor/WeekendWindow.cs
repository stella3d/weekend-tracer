using System.Collections;
using TMPro;
using Unity.EditorCoroutines.Editor;
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
        ChapterSevenAlternate m_ChapterSevenAlt;
        ChapterEight m_ChapterEight;
        ChapterEightProgressive m_ChapterEightPro;

        // default position and color same as the book
        float m_Chapter4ZPosition = -1f;
        Color32 m_Chapter4Color = Color.red;

        Vector2 m_ScrollPosition;

        static int s_CanvasScaling = 8;

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
            m_ChapterSevenAlt = new ChapterSevenAlternate();
            m_ChapterEight = new ChapterEight();
            m_ChapterEightPro = new ChapterEightProgressive();
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
            DrawChapterSevenAlt();
            DrawChapterEight();
            DrawChapterEightPro();
            
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
        
        
        
        void DrawChapterSevenAlt()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            var texture = m_ChapterSevenAlt.texture;
            var vec = new Vector2Int(texture.width, texture.height);
            EditorGUILayout.Vector2IntField("Canvas Size", vec);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            m_SampleCountSeven = EditorGUILayout.IntField("Sample Count", m_SampleCountSeven);
            m_ChapterSevenAlt.numberOfSamples = m_SampleCountSeven;

            m_AbsorbRateSeven = EditorGUILayout.Slider("Absorb Rate", m_AbsorbRateSeven, 0.05f, 0.95f);
            m_ChapterSevenAlt.absorbRate = m_AbsorbRateSeven;
            
            if (!Mathf.Approximately(m_AbsorbRateSeven, m_PreviousAbsorbRateSeven))
            {
                m_ChapterSevenAlt.canvasScale = s_CanvasScaling;
                m_ChapterSevenAlt.DrawToTexture();
            }

            m_PreviousAbsorbRateSeven = m_AbsorbRateSeven;
            DrawChapterBasic(m_ChapterSevenAlt, "7");
        }
        
        void DrawChapterEight()
        {
            m_ChapterEight.canvasScale = s_CanvasScaling;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            var texture = m_ChapterEight.texture;
            var vec = new Vector2Int(texture.width, texture.height);
            EditorGUILayout.Vector2IntField("Canvas Size", vec);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            m_SampleCountEight = EditorGUILayout.IntField("Sample Count", m_SampleCountEight);
            m_ChapterEight.numberOfSamples = m_SampleCountEight;
            DrawChapterBasic(m_ChapterEight, "8");
        }
        
        void DrawChapterEightPro()
        {
            if (m_ChapterEightPro == null)
            {
                m_ChapterEightPro = new ChapterEightProgressive();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            var completedStyle = new GUIStyle(EditorStyles.boldLabel);
            completedStyle.fontSize = 18;
            var totalStyle = new GUIStyle(EditorStyles.numberField);
            totalStyle.fontSize = 18;
            totalStyle.fontStyle = FontStyle.Bold;
            
            var forceHeight = GUILayout.Height(36);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            string label = "";
            if ( m_ChapterEightPro.texture != null)
            {
                var texture = m_ChapterEightPro.texture;
                label = $"Canvas Size:   {new Vector2Int(texture.width, texture.height)}";
            }

            EditorGUILayout.LabelField(label, completedStyle, forceHeight);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Sample Count", EditorStyles.boldLabel); 
            EditorGUILayout.BeginHorizontal(forceHeight, GUILayout.ExpandHeight(true));

            m_SampleCountEight = EditorGUILayout.IntField("Total", m_SampleCountEight, totalStyle, forceHeight);
            
            EditorGUILayout.LabelField("Completed: " + m_ChapterEightPro.CompletedSampleCount, completedStyle,
                forceHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button($"Draw Progressive Image"))
            {
                lastTime = Time.time - 0.64f;
                EditorCoroutineUtility.StartCoroutine(ProgressiveRoutine(), this);
            }
            
            DrawTexture(m_ChapterEightPro.texture);
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
        }

        float lastTime;
        
        IEnumerator ProgressiveRoutine()
        {
            if (Time.time - lastTime < 0.64f)
                yield return null;
            
            for (int i = 0; i < m_SampleCountEight / 8; i++)
            {
                m_ChapterEightPro.DrawToTexture();
                Repaint();
                lastTime = Time.time;
                yield return null;
            }
        }

        void DrawExperimental()
        {
            
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
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(size.x * s_CanvasScaling), GUILayout.Height(size.y * s_CanvasScaling));
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }
    }
}