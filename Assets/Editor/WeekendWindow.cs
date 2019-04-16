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
        ChapterEightParallel m_ChapterEightParallel;
        
        BatchedTracer m_ChapterEight;
        BatchedTracer m_ChapterNine;
        BatchedTracer m_ChapterTen;
        BatchedTracer m_ChapterEleven;
        BatchedTracer m_ChapterTwelve;

        // default position and color same as the book
        float m_Chapter4ZPosition = -1f;
        Color32 m_Chapter4Color = Color.red;

        Vector2 m_ScrollPosition;

        static int s_CanvasScaling = 6;
        int m_PreviousCanvasScaling;

        [MenuItem("Window/Weekend Tracer")]
        public static void ShowWindow()
        {
            var window = GetWindow<WeekendWindow>();
            window.Show();
        }

        ~WeekendWindow()
        {
            Dispose();
        }

        void OnEnable()
        {
            m_Disposed = false;
            m_ChapterOne = new ChapterOne();
            m_ChapterTwo = new ChapterTwo();
            m_ChapterThree = new ChapterThree();
            m_ChapterFour = new ChapterFour();
            m_ChapterFive = new ChapterFive();
            m_ChapterFiveTwo = new ChapterFiveTwo();
            m_ChapterSix = new ChapterSix();
            m_ChapterSeven = new ChapterSeven();
            m_ChapterSevenAlt = new ChapterSevenAlternate();
            m_ChapterEightParallel = new ChapterEightParallel();
            m_ChapterEleven = new BatchedTracer();
        }

        bool m_Disposed;
        
        void Dispose()
        {
            Debug.Log("disposing all chapters....");
            m_ChapterEleven.Dispose();
            m_Disposed = true;
        }

        void OnGUI()
        {
            if (m_Disposed)
                return;
            
            if (EditorApplication.isCompiling)
            {
                Dispose();
            }
            
            

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
            
            EditorGUILayout.Space();

            DrawChapterTen();
            
            EditorGUILayout.EndScrollView();
        }

        void DrawChapterFour()
        {
            m_Chapter4ZPosition = EditorGUILayout.Slider("Z Position", m_Chapter4ZPosition, -5f, -0.75f);
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
        int m_SampleCountNine = 16;

        
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
            m_ChapterEightParallel.canvasScale = s_CanvasScaling;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            var texture = m_ChapterEightParallel.texture;
            var vec = new Vector2Int(texture.width, texture.height);
            EditorGUILayout.Vector2IntField("Canvas Size", vec);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            m_SampleCountEight = EditorGUILayout.IntField("Sample Count", m_SampleCountEight);
            m_ChapterEightParallel.numberOfSamples = m_SampleCountEight;
            DrawChapterBasic(m_ChapterEightParallel, "8");
        }
        
        void DrawChapterEightPro()
        {
            if (m_ChapterEleven == null)
            {
                m_ChapterEleven = new BatchedTracer();
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
            if ( m_ChapterEleven.texture != null)
            {
                var texture = m_ChapterEleven.texture;
                label = $"Canvas Size:   {new Vector2Int(texture.width, texture.height)}";
            }

            EditorGUILayout.LabelField(label, completedStyle, forceHeight);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Sample Count", EditorStyles.boldLabel); 
            EditorGUILayout.BeginHorizontal(forceHeight, GUILayout.ExpandHeight(true));

            m_SampleCountEight = EditorGUILayout.IntField("Total", m_SampleCountEight, totalStyle, forceHeight);
            
            EditorGUILayout.LabelField("Completed: " + m_ChapterEleven.CompletedSampleCount, completedStyle,
                forceHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button($"Draw Progressive Image"))
            {
                lastTime = Time.time - 0.64f;
                EditorCoroutineUtility.StartCoroutine(ChapterEightBatchRoutine(), this);
            }
            
            DrawTexture(m_ChapterEleven.texture);
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
        }

        float m_ChapterTenFov = 90;
        float m_PreviousTenFov = 90;
        
        void DrawChapterTen()
        {
            if (EditorApplication.isCompiling && m_ChapterEleven.texture != null)
            {
                m_ChapterEleven.Dispose();
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
            if (m_ChapterEleven.texture != null)
            {
                var texture = m_ChapterEleven.texture;
                label = $"Canvas Size:   {new Vector2Int(texture.width, texture.height)}";
            }

            EditorGUILayout.LabelField(label, completedStyle, forceHeight);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            /*
            m_ChapterTenFov = EditorGUILayout.Slider("Field of View", m_ChapterTenFov, 10f, 180f);
            if (math.abs(m_ChapterTenFov - m_PreviousTenFov) > 1f || lastFovChangeTime > 1f)
            {
                Debug.Log("schedule");
                m_PreviousTenFov = m_ChapterTenFov;
                lastFovChangeTime = Time.time;
                m_Pro.fieldOfView = m_ChapterTenFov;
                lastTime = Time.time - 0.64f;
                m_Routine = EditorCoroutineUtility.StartCoroutine(ProgressiveRoutine(8), this);
            }
            */

            EditorGUILayout.LabelField("Sample Count", EditorStyles.boldLabel); 
            EditorGUILayout.BeginHorizontal(forceHeight, GUILayout.ExpandHeight(true));
            m_SampleCountEight = EditorGUILayout.IntField("Total", m_SampleCountEight, totalStyle, forceHeight);
            EditorGUILayout.LabelField("Completed: " + m_ChapterEleven.CompletedSampleCount, completedStyle, forceHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button($"Draw Image"))
            {
                lastTime = Time.time - 0.64f;
                m_Routine = EditorCoroutineUtility.StartCoroutine(ChapterEightBatchRoutine(), this);
            }
            
            DrawTexture(m_ChapterEleven.texture);
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
        }

        EditorCoroutine m_Routine;
        
        float lastTime;
        float lastFovChangeTime;
        
        IEnumerator ChapterEightBatchRoutine(int count = 0)
        {
            // TODO - name the magic number
            if (Time.time - lastTime < 0.64f)
                yield return null;

            if (count == 0)
                count = m_SampleCountEight;
            
            for (int i = 0; i < count / 8; i++)
            {
                m_ChapterEleven.DrawToTexture();
                Repaint();
                lastTime = Time.time;
                yield return null;
            }
            
            EditorCoroutineUtility.StopCoroutine(m_Routine);
        }
        
        IEnumerator ChapterNineBatchRoutine(int count = 0)
        {
            // TODO - name the magic number
            if (Time.time - lastTime < 0.64f)
                yield return null;

            if (count == 0)
                count = m_SampleCountNine;
            
            for (int i = 0; i < count / 8; i++)
            {
                m_ChapterEleven.DrawToTexture();
                Repaint();
                lastTime = Time.time;
                yield return null;
            }
            
            EditorCoroutineUtility.StopCoroutine(m_Routine);
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