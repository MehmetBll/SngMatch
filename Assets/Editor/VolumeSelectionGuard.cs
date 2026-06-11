using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Clears selected Volume components before Unity unloads or reloads scenes in the Editor.</summary>
[InitializeOnLoad]
public static class VolumeSelectionGuard
{
    static VolumeSelectionGuard()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorSceneManager.sceneClosing += OnSceneClosing;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            ClearVolumeSelection();
    }

    private static void OnSceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene)
    {
        ClearVolumeSelection();
    }

    private static void ClearVolumeSelection()
    {
        Object activeObject = Selection.activeObject;
        if (activeObject == null)
            return;

        if (activeObject is Volume || activeObject is GameObject gameObject && gameObject.GetComponent<Volume>() != null)
            Selection.activeObject = null;
    }
}
