using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RayTracingWeekend
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class RaytracingSceneManager : MonoBehaviour
    {
        static readonly List<RaytracedSphere> k_SceneSphereComponents = new List<RaytracedSphere>();
        static readonly List<RaytracedSphere> k_TempSphereComponents = new List<RaytracedSphere>();

        public Vector3 LookAtPoint = new Vector3(0f, 0f, -1f);
        
        public HitableArray<Sphere> Spheres { get; private set; }

        public CameraFrame Camera { get; protected set; }

        public event Action onSceneChanged;
        
        public JobHandle dependency { get; set; }

        Camera m_MainCamera;

        void Start()
        {
            m_MainCamera = UnityEngine.Camera.main;
            Spheres = new HitableArray<Sphere>(0);
            UpdateWorld();
        }

        Vector3 m_PreviousCameraPos;
        Quaternion m_PreviousCameraRotation;

        void Update()
        {
            var camPos = m_MainCamera.transform.position;
            var camRot = m_MainCamera.transform.rotation;
            if (camPos != m_PreviousCameraPos || camRot != m_PreviousCameraRotation)
            {
                Camera = new CameraFrame(m_MainCamera, m_MainCamera.transform, LookAtPoint);
                onSceneChanged?.Invoke();
            }

            m_PreviousCameraPos = camPos;
            m_PreviousCameraRotation = camRot;
        }

        public void UpdateWorld()
        {
            // gather all RayTracedSphere components in the scene
            k_SceneSphereComponents.Clear();
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                root.GetComponentsInChildren(k_TempSphereComponents);
                k_SceneSphereComponents.AddRange(k_TempSphereComponents);
            }

            // update the job data based on the sphere components' values
            var world = Spheres;
            Utils.ReallocateIfNeeded(ref world.Objects, k_SceneSphereComponents.Count);
            for (var i = 0; i < world.Length; i++)
            {
                world.Objects[i] = k_SceneSphereComponents[i].GetSphere();
            }
            
            dependency.Complete();
            Spheres = world;
        }
    }
}

