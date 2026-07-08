using System.Reflection;
using FishNet.Object;
using UnityEngine;

public class SceneIdDebugger : MonoBehaviour
{
    private void Start()
    {
        var allObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);

        var sceneIdField = typeof(NetworkObject).GetField("SceneId",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach (var no in allObjects)
        {
            if (!no.gameObject.name.ToLower().Contains("bomb"))
                continue;

            object sceneIdValue = sceneIdField != null ? sceneIdField.GetValue(no) : "N/A (field not found)";

            Debug.Log($"[SceneIdDebugger] Name='{no.gameObject.name}' " +
                       $"SceneId={sceneIdValue} " +
                       $"IsSceneObject={no.IsSceneObject} " +
                       $"ObjectId={no.ObjectId} " +
                       $"ActiveInHierarchy={no.gameObject.activeInHierarchy}");
        }
    }
}