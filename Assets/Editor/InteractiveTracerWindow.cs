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

        BatchedTracer m_RayTracer;

        int DrawRepeater = 0;
        int DrawRepeaterCounter;
        
        void OnEnable()
        {
            minSize = new Vector2(200, 100);
            maxSize = new Vector2(1600, 800);
            EnsureRaySceneManager();
            
            // TODO - make this constructor work on just texture size instead of scaling
            m_RayTracer = new BatchedTracer(m_SceneManager.Spheres, CameraFrame.Default, 400, 200);
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

            if (GUILayout.Button("update world"))
            {
                m_SceneManager.UpdateWorld();
                OnSceneChange();
            }

            var tex = m_TracerRenderTexture;
            var rect = GUILayoutUtility.GetRect(tex.width, tex.height);
            EditorGUI.DrawPreviewTexture(rect, m_TracerRenderTexture, null, ScaleMode.ScaleToFit);
        }

        void OnSceneChange()
        {
            m_RayTracer.camera = m_SceneManager.Camera;
            m_RayTracer.Spheres = m_SceneManager.Spheres;

            DrawRepeaterCounter = 0;
            DrawAndRepaint();
            if (DrawRepeater > 0)
            {
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
        }
        
        void DrawAndRepaint()
        {
            m_RayTracer.DrawToTexture();
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