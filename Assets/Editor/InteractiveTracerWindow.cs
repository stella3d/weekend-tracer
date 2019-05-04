using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace RayTracingWeekend
{
    public class InteractiveTracerWindow : EditorWindow
    {
        Texture2D m_TracerRenderTexture;
        RaytracingSceneManager m_SceneManager;

        DebugTracerWithoutFocus m_RayTracer;

        DebugRayVisualizer m_RayVisualizer = new DebugRayVisualizer();

        bool m_ClearOnDraw;
        
        void OnEnable()
        {
            minSize = new Vector2(200, 100);
            maxSize = new Vector2(1600, 800);
            EnsureRaySceneManager();
            
            // TODO - make this constructor work on just texture size instead of scaling
            m_RayTracer = new DebugTracerWithoutFocus(m_SceneManager.Spheres, CameraFrame.Default, 2);
            m_TracerRenderTexture = m_RayTracer.texture;
            m_SceneManager = FindObjectOfType<RaytracingSceneManager>();
            m_SceneManager.UpdateWorld();
            m_SceneManager.onSceneChanged += OnSceneChange;
        }

        void OnDisable()
        {
            m_SceneManager.onSceneChanged -= OnSceneChange;
        }

        public void OnGUI()
        {
            if (m_SceneManager == null)
            {
                m_SceneManager = FindObjectOfType<RaytracingSceneManager>();
                m_SceneManager.UpdateWorld();
                m_SceneManager.onSceneChanged += OnSceneChange;
            }

            if(m_TracerRenderTexture == null)
                m_TracerRenderTexture = m_RayTracer.texture;

            m_ClearOnDraw = GUILayout.Toggle(m_ClearOnDraw, "clear on re-draw");
            
            if (GUILayout.Button("update world"))
            {
                m_SceneManager.UpdateWorld();
                OnSceneChange();
            }
            
            if (GUILayout.Button("draw debug rays"))
            {
                m_RayVisualizer.PixelIndex += 16;
                m_RayVisualizer.DrawCurrent(Random.ColorHSV());
            }

            var tex = m_TracerRenderTexture;
            var rect = GUILayoutUtility.GetRect(tex.width, tex.height);
            EditorGUI.DrawPreviewTexture(rect, m_TracerRenderTexture, null, ScaleMode.ScaleToFit);
        }

        int DrawRepeater = 0;
        int DrawRepeaterCounter;

        void OnSceneChange()
        {
            m_RayTracer.clearOnDraw = m_ClearOnDraw;
            m_RayTracer.camera = m_SceneManager.Camera;
            m_RayTracer.Spheres = m_SceneManager.Spheres;

            DrawRepeaterCounter = 0;
            DrawAndRepaint();
            if (DrawRepeater > 0)
            {
                m_RayTracer.clearOnDraw = false;
                EditorCoroutineUtility.StartCoroutineOwnerless(DrawAndRepaintRoutine());
            }
            else
            {
                DrawAndRepaint();
            }
        }

        IEnumerator DrawAndRepaintRoutine()
        {
            do
            {
                DrawRepeaterCounter++;
            
                m_RayTracer.DrawToTexture();
                m_SceneManager.dependency = m_RayTracer.m_Handle;
                Repaint();
            
                yield return default;
            } 
            while (DrawRepeaterCounter < DrawRepeater);

            m_RayTracer.clearOnDraw = m_ClearOnDraw;
        }
        
        void DrawAndRepaint()
        {
            m_RayTracer.DrawToTexture();

            m_RayVisualizer.RaySegments = m_RayTracer.m_RaySegments;
            m_RayVisualizer.PixelRaySegmentCount = m_RayTracer.m_PerPixelRaySegmentCount;
            
            Repaint();
        }

        void EnsureRaySceneManager()
        {
            var manager = FindObjectOfType<RaytracingSceneManager>();
            if (manager == null)
            {
                var managerObj = new GameObject("Ray Tracing Scene Manager");
                manager = managerObj.AddComponent<RaytracingSceneManager>();
            }

            m_SceneManager = manager;
        }

        [MenuItem("Window/Weekend Tracer/Interactive")]
        public static void ShowWindow()
        {
            var window = GetWindow<InteractiveTracerWindow>();
            window.ShowAuxWindow();
        }
    }
}