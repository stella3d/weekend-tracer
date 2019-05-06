using System.Linq;
using Unity.EditorCoroutines.Editor;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace RayTracingWeekend
{
    public class WeekendWindow : EditorWindow
    {
        static GUIStyle k_CompletedSamplesStyle; 
        static GUIStyle k_TotalSamplesStyle; 
        static GUILayoutOption k_LargeHeaderHeight;
        
        ChaptersOneAndTwo m_ChaptersOneAndTwo;
        ChapterThree m_ChapterThree;
        ChapterFour m_ChapterFour;
        ChapterFive m_ChapterFive;
        ChapterFiveTwo m_ChapterFiveTwo;
        ChapterSix m_ChapterSix;
        ChapterSeven m_ChapterSeven;
        
        BatchedTracer m_ChapterEight;
        BatchedTracer m_ChapterNine;
        BatchedTracer m_ChapterTen;
        BatchedTracer m_ChapterEleven;
        BatchedTracer m_ChapterTwelve;

        // default position and color same as the book
        float m_Chapter4ZPosition = -1f;
        Color32 m_Chapter4Color = Color.red;
        
        bool m_Disposed;
        Vector2 m_ScrollPosition;

        // all state for user options goes under here
        int m_SelectedScaleOption = 4;
        int m_PreviousScaleOption = 4;
        readonly int[] m_ScaleOptions = { 1, 2, 4, 6, 8, 10};
        string[] m_ScaleOptionLabels;
        string m_CanvasSizeLabel;
        
        // TODO - make it so these options can only be set to proper multiples
        int m_SamplesPerPixel = 64;
        int m_SingleSampleJobsPerBatch = 4;
        
        float m_AbsorbRateSeven = 0.5f;
        float m_PreviousAbsorbRateSeven = 0.5f;

        [MenuItem("Window/Weekend Tracer/Book")]
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
            SetupStyles();
            m_DummyHandle = new JobHandle();
            m_DummyHandle.Complete();
            m_Disposed = false;
            m_ScaleOptionLabels = m_ScaleOptions.Select((i => i.ToString())).ToArray();
            SetupChapters();
        }

        void SetupChapters()
        {
            m_ChaptersOneAndTwo = new ChaptersOneAndTwo();
            m_ChapterThree = new ChapterThree();
            m_ChapterFour = new ChapterFour();
            m_ChapterFive = new ChapterFive();
            m_ChapterFiveTwo = new ChapterFiveTwo();
            m_ChapterSix = new ChapterSix();
            m_ChapterSeven = new ChapterSeven();
            
            // from chapter 8 on, the same implementation is re-used
            m_ChapterEight = new BatchedTracer(ExampleSphereSets.ChapterEight(), CameraFrame.Default);
            m_ChapterNine = new BatchedTracer(ExampleSphereSets.FiveWithDielectric(), CameraFrame.Default);
            m_ChapterTen = new BatchedTracer(ExampleSphereSets.FiveWithDielectric(), CameraFrame.ChapterTen);
            m_ChapterEleven = new BatchedTracer(ExampleSphereSets.FiveWithDielectric(), CameraFrame.ChapterEleven);
        }
        
        void ScaleChapters()
        {
            var size = Constants.DefaultImageSize * m_SelectedScaleOption;
            m_ChaptersOneAndTwo.Resize(size);
            m_ChapterThree.Resize(size);
            m_ChapterFour.Resize(size);
            m_ChapterFive.Resize(size);
            m_ChapterFiveTwo.Resize(size);
            m_ChapterSix.Resize(size);
            m_ChapterSeven.Resize(size);

            /*
            // from chapter 8 on, the same implementation is re-used
            m_ChapterEight = new BatchedTracer(ExampleSphereSets.ChapterEight(), CameraFrame.Default);
            m_ChapterNine = new BatchedTracer(ExampleSphereSets.FiveWithDielectric(), CameraFrame.Default);
            m_ChapterTen = new BatchedTracer(ExampleSphereSets.FiveWithDielectric(), CameraFrame.ChapterTen);
            m_ChapterEleven = new BatchedTracer(ExampleSphereSets.FiveWithDielectric(), CameraFrame.ChapterEleven);
            */
        }

        void Dispose()
        {
            // TODO make the disposing work properly
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

            DrawGlobalOptions();

            DrawChapterBasic(m_ChaptersOneAndTwo, "1 & 2");
            DrawChapterBasic(m_ChapterThree, "3");
            EditorGUILayout.Space();
            DrawChapterFour();
            DrawChapterBasic(m_ChapterFive, "5.1");
            DrawChapterBasic(m_ChapterFiveTwo, "5.2");
            DrawChapterSix();
            DrawChapterSeven();
            DrawChapterEightPro();
            DrawChapterNine();
            EditorGUILayout.Space();
            DrawChapterTen();
            
            EditorGUILayout.EndScrollView();
        }

        void SetupStyles()
        {
            k_CompletedSamplesStyle = new GUIStyle(EditorStyles.boldLabel) {fontSize = 18};
            k_TotalSamplesStyle = new GUIStyle(EditorStyles.numberField)
            {
                fontSize = 18, 
                fontStyle = FontStyle.Bold, 
                fixedHeight = 36f
            };
            
            k_LargeHeaderHeight = GUILayout.Height(36);
        }

        void DrawGlobalOptions()
        {
            var maxWidth = GUILayout.MaxWidth(200);
            
            m_SamplesPerPixel = EditorGUILayout.IntField("Samples Per Pixel", m_SamplesPerPixel);
            
            EditorGUILayout.BeginHorizontal();
            
            var label = new GUIContent("Canvas Scale", "The number to multiply the book's default 200x100 canvas by");
            EditorGUILayout.LabelField(label, maxWidth);
            m_SelectedScaleOption = EditorGUILayout.IntPopup(m_SelectedScaleOption, m_ScaleOptionLabels, 
                m_ScaleOptions, maxWidth);

            if (m_PreviousScaleOption != m_SelectedScaleOption)
            {
                // resize our canvases if we've changed canvas scale
                ScaleChapters();
            }

            m_PreviousScaleOption = m_SelectedScaleOption;

            var size = Constants.DefaultImageSize * m_SelectedScaleOption;
            m_CanvasSizeLabel = $"Resolution: {size.x} x {size.y}";
            EditorGUILayout.LabelField(m_CanvasSizeLabel, GUILayout.MinWidth(180));
            
            EditorGUILayout.EndHorizontal();
        }

        void DrawChapterFour()
        {
            m_Chapter4ZPosition = EditorGUILayout.Slider("Z Position", m_Chapter4ZPosition, -5f, -0.75f);
            m_Chapter4Color = EditorGUILayout.ColorField("Color", m_Chapter4Color);

            var color = m_Chapter4Color;
            const float rgbScale = Constants.rgbMultiplier;
            m_ChapterFour.sphereColor = new float3(color.r / rgbScale, color.g / rgbScale, color.b / rgbScale);
            m_ChapterFour.spherePositionZ = m_Chapter4ZPosition;
            
            DrawChapterBasic(m_ChapterFour, "4");
        }

        void DrawChapterSix()
        {
            m_ChapterSix.numberOfSamples = m_SamplesPerPixel;
            DrawChapterBasic(m_ChapterSix, "6");
        }
        
        void DrawChapterSeven()
        {
            m_ChapterSeven.numberOfSamples = m_SamplesPerPixel;

            m_AbsorbRateSeven = EditorGUILayout.Slider("Absorb Rate", m_AbsorbRateSeven, 0.05f, 0.95f);
            m_ChapterSeven.absorbRate = m_AbsorbRateSeven;

            if (!Mathf.Approximately(m_AbsorbRateSeven, m_PreviousAbsorbRateSeven))
            {
                var handle = m_ChapterSeven.Schedule();
                handle.Complete();
            }

            m_PreviousAbsorbRateSeven = m_AbsorbRateSeven;
            DrawChapterBasic(m_ChapterSeven, "7");
        }
        
        void DrawChapterEightPro()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sample Count", EditorStyles.boldLabel); 
            EditorGUILayout.BeginHorizontal(k_LargeHeaderHeight, GUILayout.ExpandHeight(true));
            
            EditorGUILayout.LabelField("Completed: " + m_ChapterEight.CompletedSampleCount, k_CompletedSamplesStyle,
                k_LargeHeaderHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button($"Draw Chapter Eight Image"))
            {
                // TODO - better explanation for the batch coroutine
                var routineEnumerator = m_ChapterEight.BatchCoroutine(m_SamplesPerPixel, Repaint);
                m_ChapterEight.Routine = EditorCoroutineUtility.StartCoroutine(routineEnumerator, this);
            }
            
            DrawTexture(m_ChapterEight.texture);
            EditorGUILayout.Separator();
        }

        void DrawChapterNine()
        {
            if (EditorApplication.isCompiling && m_ChapterNine.texture != null)
                m_ChapterNine.Dispose();

            EditorGUILayout.Space();
            m_ChapterNine.camera = CameraFrame.Default;

            EditorGUILayout.LabelField("Sample Count", EditorStyles.boldLabel); 
            EditorGUILayout.BeginHorizontal(k_LargeHeaderHeight, GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("Completed: " + m_ChapterNine.CompletedSampleCount, 
                k_CompletedSamplesStyle, k_LargeHeaderHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button($"Draw Chapter Nine Image"))
            {
                var routineEnumerator = m_ChapterNine.BatchCoroutine(m_SamplesPerPixel, Repaint);
                m_ChapterNine.Routine = EditorCoroutineUtility.StartCoroutine(routineEnumerator, this);
            }
            
            DrawTexture(m_ChapterNine.texture);
            EditorGUILayout.Separator();
        }
        
        void DrawChapterTen()
        {
            if (EditorApplication.isCompiling && m_ChapterTen.texture != null)
                m_ChapterTen.Dispose();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sample Count", EditorStyles.boldLabel); 
            EditorGUILayout.BeginHorizontal(k_LargeHeaderHeight, GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("Completed: " + m_ChapterTen.CompletedSampleCount, 
                k_CompletedSamplesStyle, k_LargeHeaderHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button($"Draw Chapter Ten Image"))
            {
                var routineEnumerator = m_ChapterTen.BatchCoroutineNoFocus(m_SamplesPerPixel, Repaint);
                m_ChapterTen.Routine = EditorCoroutineUtility.StartCoroutine(routineEnumerator, this);
            }
            
            DrawTexture(m_ChapterTen.texture);
            EditorGUILayout.Separator();
        }

        EditorCoroutine m_Routine;
        
        float lastFovChangeTime;
        
        static void DrawChapterBasic<T>(Chapter<T> chapter, string chapterNumber) where T: struct
        {
            if (GUILayout.Button($"Draw Chapter {chapterNumber} Image"))
                CompleteAndDraw(chapter);

            DrawTexture(chapter.texture);
            EditorGUILayout.Separator();
        }

        JobHandle m_DummyHandle;

        // schedule a job, immediately complete it and update the texture
        static void CompleteAndDraw<T>(Chapter<T> chapter) where T: struct
        {
            chapter.Schedule();
            chapter.Complete();
        }

        static void DrawTexture(Texture2D texture)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(texture.width), GUILayout.Height(texture.height));
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }
    }
}