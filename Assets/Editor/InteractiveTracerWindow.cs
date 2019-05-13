using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RayTracingWeekend
{
    public class InteractiveTracerWindow : EditorWindow
    {
        Texture2D m_TracerRenderTexture;
        RaytracingSceneManager m_SceneManager;

        BatchedTracer m_RayTracer;

        void OnEnable()
        {
            minSize = new Vector2(200, 100);
            maxSize = new Vector2(1600, 800);
            EnsureRaySceneManager();
            
            m_RayTracer = new BatchedTracer(m_SceneManager.Spheres, CameraFrame.Default, 400, 200) {ClearOnDraw = true};
            m_TracerRenderTexture = m_RayTracer.texture;
            m_SceneManager = FindObjectOfType<RaytracingSceneManager>();
            m_SceneManager.UpdateWorld();
            m_SceneManager.onSceneChanged += OnSceneChange;

            if (FindObjectOfType<RaytracingSceneManager>() == null)
                EditorSceneManager.LoadScene("ChapterEight");
            
            m_SceneManager.UpdateWorld();
            OnSceneChange();
        }

        void OnDisable()
        {
            m_SceneManager.onSceneChanged -= OnSceneChange;
        }

        const string k_HintBoxText = "This camera will move with the main Unity camera, but always look at the " +
                                     "center sphere, so it won't show exactly what is in the Game View.\n" + 
                                     "Load the ChapterEight scene and move the camera to see something.";
        
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

            EditorGUILayout.HelpBox(k_HintBoxText, MessageType.Info);

            var tex = m_TracerRenderTexture;
            if (tex == null) 
                return;
            
            var rect = GUILayoutUtility.GetRect(tex.width, tex.height);
            EditorGUI.DrawPreviewTexture(rect, m_TracerRenderTexture, null, ScaleMode.ScaleToFit);
        }

        void OnSceneChange()
        {
            m_RayTracer.camera = m_SceneManager.Camera;
            m_RayTracer.Spheres = m_SceneManager.Spheres;
            m_RayTracer.DrawToTextureWithoutFocus();
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