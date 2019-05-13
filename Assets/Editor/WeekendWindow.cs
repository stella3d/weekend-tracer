using System;
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
        const string k_ChapterFourText = "Try changing the color and/or Z position of the sphere on this one. " +
                                         "Click the button again to re-draw after changes.";
                                   

        const string k_LaterChaptersText = "From here on all chapters use a shared implementation, which varies " +
                                           "from the book in a few ways.\n1) It runs multiple serial, single-" +
                                           "threaded jobs at once.  Each one of these jobs does one sample of the " +
                                           "whole image, and then they are combined.\n2) We use the RGBA32 texture " +
                                           "format, which is 4 32-bit floats per pixel, as opposed to converting " +
                                           "the results of our color calculation back to 8-bit RGB color.\n\n" +
                                           "You can set how many total samples to do when you click each button below.";
        
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

        bool m_Disposed;
        Vector2 m_ScrollPosition;

        // all state for user options goes under here
        int m_SelectedScaleOption = 8;
        int m_PreviousScaleOption = 8;
        readonly int[] m_ScaleOptions = { 1, 2, 4, 6, 8, 10};
        string[] m_ScaleOptionLabels;
        string m_CanvasSizeLabel;
        
        // default position and color same as the book
        float m_Chapter4ZPosition = -1f;
        Color32 m_Chapter4Color = Color.red;
        
        // TODO - make it so these options can only be set to proper multiples
        int m_SamplesPerPixel = 64;
        
        EditorCoroutine m_Routine;
        JobHandle m_DummyHandle;
        
        [MenuItem("Window/Weekend Tracer/Book")]
        public static void ShowWindow()
        {
            var window = GetWindow<WeekendWindow>();
            window.Show();
        }

        void OnDisable()
        {
            Dispose();
        }

        void OnEnable()
        {
            Debug.Log("processor count : " + Environment.ProcessorCount);
            SetupStyles();
            m_DummyHandle = new JobHandle();
            m_DummyHandle.Complete();
            m_Disposed = false;
            m_ScaleOptionLabels = m_ScaleOptions.Select((i => i.ToString())).ToArray();
            SetupChapters();
        }

        void SetupChapters()
        {
            var size = Constants.DefaultImageSize * m_SelectedScaleOption;
            m_ChaptersOneAndTwo = new ChaptersOneAndTwo(size.x, size.y);
            m_ChapterThree = new ChapterThree(size.x, size.y);
            m_ChapterFour = new ChapterFour(size.x, size.y);
            m_ChapterFive = new ChapterFive(size.x, size.y);
            m_ChapterFiveTwo = new ChapterFiveTwo(size.x, size.y);
            m_ChapterSix = new ChapterSix(size.x, size.y);
            m_ChapterSeven = new ChapterSeven(size.x, size.y);
            
            // from chapter 8 on, the same implementation is re-used
            m_ChapterEight = new BatchedTracer(ExampleSphereSets.ChapterEight(), 
                CameraFrame.Default, size.x, size.y);
            m_ChapterNine = new BatchedTracer(ExampleSphereSets.FiveWithDielectric(),
                CameraFrame.Default, size.x, size.y);
            m_ChapterTen = new BatchedTracer(ExampleSphereSets.FiveWithDielectric(), 
                CameraFrame.ChapterTen, size.x, size.y);
            m_ChapterEleven = new BatchedTracer(ExampleSphereSets.FiveWithDielectric(), 
                CameraFrame.ChapterEleven, size.x, size.y);
            
            // make sure we get a random seed for our random scene
            var tempRng = new Unity.Mathematics.Random();
            tempRng.InitState((uint)UnityEngine.Random.Range(1, 1000));
            m_ChapterTwelve = new BatchedTracer(ExampleSphereSets.RandomScene(500, tempRng.NextUInt()), 
                CameraFrame.ChapterTwelve, size.x, size.y);
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
            m_ChapterEight.Resize(size);
            m_ChapterNine.Resize(size);
            m_ChapterTen.Resize(size);
            m_ChapterEleven.Resize(size);
            
            m_ChapterTwelve.Dispose();
            var tempRng = new Unity.Mathematics.Random();
            tempRng.InitState((uint)UnityEngine.Random.Range(1, 1000));
            m_ChapterTwelve = new BatchedTracer(ExampleSphereSets.RandomScene(500, tempRng.NextUInt()), 
                CameraFrame.ChapterTwelve, size.x, size.y);
        }

        void Dispose()
        {
            m_ChaptersOneAndTwo.Dispose();
            m_ChapterThree.Dispose();
            m_ChapterFour.Dispose();
            m_ChapterFive.Dispose();
            m_ChapterFiveTwo.Dispose();
            m_ChapterSix.Dispose();
            m_ChapterSeven.Dispose();
            m_ChapterEight.Dispose();
            m_ChapterNine.Dispose();
            m_ChapterTen.Dispose();
            m_ChapterEleven.Dispose();
            m_ChapterTwelve.Dispose();
            m_Disposed = true;
        }

        void OnGUI()
        {
            if (m_Disposed)
                return;
            if (k_TotalSamplesStyle == null)
                SetupStyles();

            DrawScaleOptions();
            
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            
            DrawChapterBasic(m_ChaptersOneAndTwo, "1 & 2");
            DrawChapterBasic(m_ChapterThree, "3");
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(k_ChapterFourText, MessageType.Info);
            DrawChapterFour();
            DrawChapterBasic(m_ChapterFive, "5.1");
            DrawChapterBasic(m_ChapterFiveTwo, "5.2");
            DrawChapterSix();
            DrawChapterSeven();
            
            EditorGUILayout.HelpBox(k_LaterChaptersText, MessageType.Info);
            m_SamplesPerPixel = EditorGUILayout.IntField("Samples Per Pixel", m_SamplesPerPixel);
            DrawLaterChapter(m_ChapterEight, "Eight");
            DrawLaterChapter(m_ChapterNine, "Nine");
            DrawLaterChapter(m_ChapterTen, "Ten");
            DrawChapterWithFocusSupport(m_ChapterEleven, "Eleven");
            DrawChapterWithFocusSupport(m_ChapterTwelve, "Twelve");
            
            EditorGUILayout.EndScrollView();
        }

        void SetupStyles()
        {
            try
            {
                k_CompletedSamplesStyle = new GUIStyle(EditorStyles.boldLabel) {fontSize = 18};
            }
#pragma warning disable 0168
            catch (Exception e)
            {
                return;
            }
#pragma warning restore 0168
            k_TotalSamplesStyle = new GUIStyle(EditorStyles.numberField)
            {
                fontSize = 18, 
                fontStyle = FontStyle.Bold, 
                fixedHeight = 36f
            };
            
            k_LargeHeaderHeight = GUILayout.Height(36);
        }

        void DrawScaleOptions()
        {
            var maxWidth = GUILayout.MaxWidth(200);
            EditorGUILayout.BeginHorizontal();
            
            var label = new GUIContent("Canvas Scale", "The number to multiply the book's default 200x100 canvas by");
            EditorGUILayout.LabelField(label, maxWidth);
            m_SelectedScaleOption = EditorGUILayout.IntPopup(m_SelectedScaleOption, m_ScaleOptionLabels, 
                m_ScaleOptions, maxWidth);

            if (m_PreviousScaleOption != m_SelectedScaleOption)
                ScaleChapters();

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
            DrawChapterBasic(m_ChapterSeven, "7");
        }
        
        static void DrawChapterBasic<T>(Chapter<T> chapter, string chapterNumber) where T: struct
        {
            if (GUILayout.Button($"Draw Chapter {chapterNumber} Image"))
                CompleteAndDraw(chapter);

            DrawTexture(chapter.texture);
            EditorGUILayout.Separator();
        }
        
        void DrawLaterChapter(BatchedTracer chapter, string chapterNumber)
        {
            DrawLaterChapter(chapter, chapterNumber, (tracer) =>
            {
                var routineEnumerator = tracer.BatchCoroutineNoFocus(m_SamplesPerPixel, Repaint);
                tracer.Routine = EditorCoroutineUtility.StartCoroutine(routineEnumerator, this);
            });
        }
        
        // used for chapter 11 & 12 - the job that supports focus is different from the one that doesn't,
        // so we need to start a different coroutine on click
        void DrawChapterWithFocusSupport(BatchedTracer chapter, string chapterNumber)
        {
            DrawLaterChapter(chapter, chapterNumber, (tracer) =>
            {
                var routineEnumerator = tracer.BatchCoroutine(m_SamplesPerPixel, Repaint);
                tracer.Routine = EditorCoroutineUtility.StartCoroutine(routineEnumerator, this);
            });
        }
        
        void DrawLaterChapter(BatchedTracer chapter, string chapterNumber, Action<BatchedTracer> onClick)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sample Count", EditorStyles.boldLabel); 
            EditorGUILayout.BeginHorizontal(k_LargeHeaderHeight, GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("Completed: " + chapter.CompletedSampleCount, 
                k_CompletedSamplesStyle, k_LargeHeaderHeight);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button($"Draw Chapter {chapterNumber} Image"))
                onClick(chapter);

            DrawTexture(chapter.texture);
            EditorGUILayout.Separator();
            EditorGUILayout.Space();
        }

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