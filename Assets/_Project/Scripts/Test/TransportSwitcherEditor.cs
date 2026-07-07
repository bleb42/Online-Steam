#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransportSwitcher))]
public class TransportSwitcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var switcher = (TransportSwitcher)target;

        if (GUILayout.Button("Use Tugboat (Local)"))
            switcher.UseTugboat();

        if (GUILayout.Button("Use FishyFacepunch (Steam)"))
            switcher.UseFishyFacepunch();

        EditorGUILayout.Space();

        bool isLocal = PlayerPrefs.GetInt("UseLocalTransport", 0) == 1;

        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.normal.textColor = isLocal ? Color.green : Color.red;

        string transport = isLocal ? "Tugboat (Local)" : "FishyFacepunch (Steam)";
        EditorGUILayout.LabelField("Active: ", transport, style);
    }
}
#endif