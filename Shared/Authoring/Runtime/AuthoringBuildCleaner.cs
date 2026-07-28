namespace Wolfy.PropTools.Customer.Authoring
{
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class AuthoringBuildCleaner
{
    // Kept independent of individual authoring tools so customer modules remain separable.
    private static readonly string[] EditorOnlyObjectNames =
    {
        "EDITOR ONLY - Live Mirroring"
    };

    public static void StripAuthoringComponentsFrom(GameObject root)
    {
#if UNITY_EDITOR
        if (root == null)
            return;

        HashSet<GameObject> generatedEditorOnlyObjects;
        HashSet<GameObject> generatedObjectsToRemove;
        StripComponents(root, out generatedEditorOnlyObjects, out generatedObjectsToRemove);
        RemoveGeneratedObjects(generatedObjectsToRemove);

        if (root == null)
            return;

        RemoveEmptyEditorOnlyObjects(root.transform, generatedEditorOnlyObjects);

        EditorUtility.SetDirty(root);
#endif
    }

#if UNITY_EDITOR
    private static void StripComponents(
        GameObject root,
        out HashSet<GameObject> generatedEditorOnlyObjects,
        out HashSet<GameObject> generatedObjectsToRemove)
    {
        generatedEditorOnlyObjects = new HashSet<GameObject>();
        generatedObjectsToRemove = new HashSet<GameObject>();
        AuthoringOnlyComponent[] components =
            root.GetComponentsInChildren<AuthoringOnlyComponent>(true);

        foreach (AuthoringOnlyComponent component in components)
        {
            if (component == null)
                continue;

            if (component.RemoveGameObjectWithComponent)
            {
                generatedEditorOnlyObjects.Add(component.gameObject);
                generatedObjectsToRemove.Add(component.gameObject);
            }

            UnityEngine.Object.DestroyImmediate(component, true);
        }
    }

    private static void RemoveGeneratedObjects(HashSet<GameObject> generatedObjectsToRemove)
    {
        if (generatedObjectsToRemove == null)
            return;

        foreach (GameObject generatedObject in generatedObjectsToRemove)
        {
            if (generatedObject != null)
                UnityEngine.Object.DestroyImmediate(generatedObject, true);
        }
    }

    private static void RemoveEmptyEditorOnlyObjects(
        Transform root,
        HashSet<GameObject> generatedEditorOnlyObjects)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
            RemoveEmptyEditorOnlyObjects(root.GetChild(i), generatedEditorOnlyObjects);

        bool isGeneratedEditorOnlyObject =
            generatedEditorOnlyObjects != null && generatedEditorOnlyObjects.Contains(root.gameObject);

        if (!isGeneratedEditorOnlyObject && !IsEditorOnlyObjectName(root.name))
            return;

        if (root.childCount > 0)
            return;

        Component[] components = root.GetComponents<Component>();

        // Transform is always present. If only Transform remains, the object is safe to remove.
        if (components.Length > 1)
            return;

        UnityEngine.Object.DestroyImmediate(root.gameObject, true);
    }

    private static bool IsEditorOnlyObjectName(string objectName)
    {
        foreach (string editorOnlyName in EditorOnlyObjectNames)
        {
            if (objectName == editorOnlyName)
                return true;
        }

        return false;
    }

#endif
}
}
