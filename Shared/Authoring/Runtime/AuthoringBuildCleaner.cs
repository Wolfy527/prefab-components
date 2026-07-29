#if UNITY_EDITOR
namespace Wolfy.PropTools.Customer.Authoring
{
using System.Collections.Generic;
using UnityEngine;

public static class AuthoringBuildCleaner
{
    public struct CleanupReport
    {
        public int ComponentsRemoved;
        public int GameObjectsRemoved;

        public bool HasChanges =>
            ComponentsRemoved > 0 || GameObjectsRemoved > 0;
    }

    public static CleanupReport StripAuthoringComponentsFrom(GameObject root)
    {
#if UNITY_EDITOR
        if (root == null)
            return default;
        if (!root.scene.IsValid())
        {
            Debug.LogError(
                "Prefab Components refused to strip authoring data directly " +
                "from an asset. Run cleanup on a scene or upload copy instead.",
                root);
            return default;
        }

        int componentCount;
        HashSet<GameObject> generatedObjectsToRemove;
        StripComponents(
            root,
            out generatedObjectsToRemove,
            out componentCount);
        RemoveGeneratedObjects(generatedObjectsToRemove);

        return new CleanupReport
        {
            ComponentsRemoved = componentCount,
            GameObjectsRemoved = generatedObjectsToRemove.Count
        };
#else
        return default;
#endif
    }

#if UNITY_EDITOR
    public static void StripAuthoringComponent(
        AuthoringOnlyComponent component)
    {
        if (component == null)
            return;

        if (component.RemoveGameObjectWithComponent)
            UnityEngine.Object.DestroyImmediate(component.gameObject, true);
        else
            UnityEngine.Object.DestroyImmediate(component, true);
    }

    private static void StripComponents(
        GameObject root,
        out HashSet<GameObject> generatedObjectsToRemove,
        out int componentCount)
    {
        generatedObjectsToRemove = new HashSet<GameObject>();
        AuthoringOnlyComponent[] components =
            root.GetComponentsInChildren<AuthoringOnlyComponent>(true);
        componentCount = components.Length;

        foreach (AuthoringOnlyComponent component in components)
        {
            if (component == null)
                continue;

            if (component.RemoveGameObjectWithComponent)
            {
                generatedObjectsToRemove.Add(component.gameObject);
                continue;
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

#endif
}
}
#endif
