using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WeekendWindow : EditorWindow
{
    ChapterOneImage m_ChapterOne;

    [MenuItem("Window/Tracer")]
    public static void ShowWindow()
    {
        var window = GetWindow<WeekendWindow>();
        window.Show();
    }

    void OnEnable()
    {
        m_ChapterOne = new ChapterOneImage();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Draw Chapter One Test Image"))
        {
            m_ChapterOne.WriteTestImage();
        }

        var rect = EditorGUILayout.GetControlRect(GUILayout.Width(200), GUILayout.Height(100));
        EditorGUI.DrawPreviewTexture(rect, m_ChapterOne.texture);
    }
}
