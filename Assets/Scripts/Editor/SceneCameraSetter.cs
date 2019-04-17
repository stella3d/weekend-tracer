﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SceneCameraSetter : EditorWindow
{
    static Camera s_SceneCamera;
    static Vector3 s_SceneCamPosition;
    static Vector3 s_PreviousSceneCamPosition;

    UnityEngine.Object m_LookAtObject;
    Transform m_LookAtTrans;
    
    void OnGUI()
    {
        s_SceneCamPosition = EditorGUILayout.Vector3Field("scene camera position", s_SceneCamPosition);
        if (s_SceneCamPosition != s_PreviousSceneCamPosition)
        {
            SetSceneCameraPosition(s_SceneCamPosition);
            if (m_LookAtTrans != null)
            {
                SetSceneCameraLookAt(m_LookAtTrans.position);
            }
        }

        m_LookAtTrans = (Transform) EditorGUILayout.ObjectField(m_LookAtTrans, typeof(Transform));

        s_PreviousSceneCamPosition = s_SceneCamPosition;
    }

    [MenuItem("Window/Weekend Tracer/Scene Camera Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneCameraSetter>();
        window.Show();
    }

    public static void SetSceneCameraPosition(Vector3 position)
    {
        var view = SceneView.lastActiveSceneView;
        view.pivot = position;
        view.Repaint();
    }
    
    public static void SetSceneCameraLookAt(Vector3 lookAt)
    {
        var view = SceneView.lastActiveSceneView;
        var cam = view.camera;
        cam.transform.LookAt(lookAt);
        view.Repaint();
    }
}