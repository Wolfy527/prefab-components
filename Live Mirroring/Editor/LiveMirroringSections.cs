namespace Wolfy.PropTools.Customer.LiveMirroring.Editor
{
using Wolfy.PropTools.Customer.LiveMirroring;
using Wolfy.PropTools.EditorUI;

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class LiveMirroringSections
{
    public static void DrawCore(SerializedObject serializedObject)
    {
        SerializedProperty liveMirror = serializedObject.FindProperty("liveMirror");
        SerializedProperty center = serializedObject.FindProperty("mirrorCenter");

        if (liveMirror != null && !liveMirror.boolValue)
        {
            PropToolsEditor.Info(
                "Live Mirroring Disabled",
                "Setup targets will not update automatically until Live Mirroring is enabled."
            );
        }

        if (center != null && center.objectReferenceValue == null)
        {
            PropToolsEditor.Info(
                "Mirror Root Empty",
                "The system will fall back to its parent transform as the mirror center."
            );
        }

        PropToolsEditor.Toggle(liveMirror, "Enable Live Mirroring");
        PropToolsEditor.ObjectField(center, typeof(Transform), "Mirror Root");
    }

    public static void DrawScaleReference(SerializedObject serializedObject)
    {
        SerializedProperty enabled = serializedObject.FindProperty("applyScaleReference");
        SerializedProperty reference = serializedObject.FindProperty("scaleReference");

        if (enabled != null && enabled.boolValue &&
            reference != null && reference.objectReferenceValue == null)
        {
            PropToolsEditor.Warning(
                "Scale Reference Missing",
                "Apply Scale Reference is enabled, but no Scale Reference transform is assigned."
            );
        }

        PropToolsEditor.Toggle(enabled, "Apply Scale Reference");

        if (enabled == null || !enabled.boolValue)
            return;

        PropToolsEditor.ObjectField(reference, typeof(Transform), "Scale Reference");
    }

    public static void DrawPairs(SerializedObject serializedObject)
    {
        SerializedProperty pairs = serializedObject.FindProperty("pairs");

        if (pairs == null)
        {
            PropToolsEditor.Warning("Mirror Pairs Missing");
            return;
        }

        DrawPairValidation(serializedObject, pairs);
        if (pairs.arraySize > 64)
        {
            PropToolsEditor.Warning(
                "Large Scene Preview",
                "Scene Preview displays at most 128 unique target ghosts to " +
                "keep the Unity editor responsive."
            );
        }
        DrawAddPairButton(pairs);

        if (pairs.arraySize == 0)
            return;

        PropToolsEditor.SpaceSmall();

        for (int i = 0; i < pairs.arraySize; i++)
        {
            if (DrawMirrorPair(pairs, i))
                i--;
        }
    }

    public static void DrawPreview(SerializedObject serializedObject)
    {
        SerializedProperty enabled = serializedObject.FindProperty("showScenePreview");
        SerializedProperty source = serializedObject.FindProperty("previewSource");
        SerializedProperty material = serializedObject.FindProperty("previewMaterial");

        if (enabled != null && enabled.boolValue &&
            source != null && source.objectReferenceValue == null)
        {
            PropToolsEditor.Warning(
                "Preview Source Missing",
                "Scene Preview Ghosts are enabled, but Preview Source is not assigned."
            );
        }

        if (enabled != null && enabled.boolValue &&
            material != null && material.objectReferenceValue == null)
        {
            PropToolsEditor.Info(
                "Preview Material Optional",
                "A dedicated ghost material usually gives a cleaner setup preview."
            );
        }

        PropToolsEditor.Toggle(enabled, "Show Scene Preview");

        if (enabled == null || !enabled.boolValue)
            return;

        PropToolsEditor.ObjectField(source, typeof(GameObject), "Preview Source");
        PropToolsEditor.ObjectField(material, typeof(Material), "Preview Material", false);
    }

    public static void DrawAdvanced(SerializedObject serializedObject)
    {
        SerializedProperty options = serializedObject.FindProperty("mirrorOptions");

        if (options == null)
        {
            PropToolsEditor.Warning("Mirror Options Missing");
            return;
        }

        PropToolsEditor.Toggle(options.FindPropertyRelative("mirrorPosition"), "Mirror Position");
        PropToolsEditor.Toggle(options.FindPropertyRelative("mirrorRotation"), "Mirror Rotation");
        PropToolsEditor.Toggle(options.FindPropertyRelative("mirrorScale"), "Mirror Scale");
        PropToolsEditor.EnumField(options.FindPropertyRelative("mirrorAxis"), "Mirror Axis");
    }

    private static void DrawPairValidation(
        SerializedObject serializedObject,
        SerializedProperty pairs)
    {
        if (pairs.arraySize == 0)
        {
            PropToolsEditor.Info(
                "No Mirror Pairs",
                "Nothing will mirror until at least one source and mirrored target pair exists."
            );
            return;
        }

        LiveMirroringSystem system = serializedObject.targetObject as LiveMirroringSystem;
        Transform root = LiveMirroringService.ResolveRoot(system);
        HashSet<Transform> controlledMirroredTargets = new HashSet<Transform>();
        Dictionary<Transform, List<Transform>> edges =
            new Dictionary<Transform, List<Transform>>();

        for (int i = 0; i < pairs.arraySize; i++)
        {
            SerializedProperty pair = pairs.GetArrayElementAtIndex(i);
            SerializedProperty enabled = pair.FindPropertyRelative("mirrorEnabled");
            string label = GetPairLabel(pair, i);

            if (enabled != null && !enabled.boolValue)
            {
                PropToolsEditor.Info(
                    $"{label} Disabled",
                    "This pair is assigned but will not update while disabled."
                );
                continue;
            }

            Transform source = GetTransform(pair, "sourceTarget");
            Transform mirrored = GetTransform(pair, "mirroredTarget");

            if (source == null || mirrored == null)
            {
                PropToolsEditor.Warning(
                    $"{label} Incomplete",
                    "Assign both Source Target and Mirrored Target."
                );
                continue;
            }

            if (source == mirrored)
            {
                PropToolsEditor.Error(
                    $"{label} Uses One Object Twice",
                    "Source Target and Mirrored Target cannot be the same object."
                );
                continue;
            }

            if (!controlledMirroredTargets.Add(mirrored))
            {
                PropToolsEditor.Error(
                    $"{label} Reuses A Mirrored Target",
                    $"Another enabled pair already controls '{mirrored.name}'. The first pair wins."
                );
            }
            else if (WouldCreateCycle(source, mirrored, edges))
            {
                controlledMirroredTargets.Remove(mirrored);
                PropToolsEditor.Error(
                    $"{label} Creates A Mirror Cycle",
                    "This pair feeds back into an earlier pair and will be ignored."
                );
            }
            else
            {
                AddEdge(edges, source, mirrored);
            }

            if (root != null && (!source.IsChildOf(root) || !mirrored.IsChildOf(root)))
            {
                PropToolsEditor.Warning(
                    $"{label} Is Outside The Mirror Root",
                    "One or both targets are outside the Mirror Root hierarchy."
                );
            }
        }
    }

    private static bool WouldCreateCycle(
        Transform source,
        Transform mirrored,
        IReadOnlyDictionary<Transform, List<Transform>> edges)
    {
        HashSet<Transform> visited = new HashSet<Transform>();
        Stack<Transform> pending = new Stack<Transform>();
        pending.Push(mirrored);

        while (pending.Count > 0)
        {
            Transform current = pending.Pop();
            if (current == null || !visited.Add(current))
                continue;
            if (current == source)
                return true;

            if (!edges.TryGetValue(
                    current,
                    out List<Transform> next))
                continue;

            foreach (Transform target in next)
                pending.Push(target);
        }

        return false;
    }

    private static void AddEdge(
        IDictionary<Transform, List<Transform>> edges,
        Transform source,
        Transform mirrored)
    {
        if (!edges.TryGetValue(
                source,
                out List<Transform> targets))
        {
            targets = new List<Transform>();
            edges.Add(source, targets);
        }

        targets.Add(mirrored);
    }

    private static void DrawAddPairButton(SerializedProperty pairs)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        Rect addRect = GUILayoutUtility.GetRect(
            100f,
            22f,
            GUILayout.Width(100f),
            GUILayout.Height(22f)
        );

        if (PropToolsEditor.MiniButton(addRect, "+ Add Pair"))
            AddMirrorPair(pairs);

        EditorGUILayout.EndHorizontal();
    }

    private static void AddMirrorPair(SerializedProperty pairs)
    {
        int index = pairs.arraySize;
        pairs.arraySize++;

        SerializedProperty pair = pairs.GetArrayElementAtIndex(index);
        pair.FindPropertyRelative("mirrorEnabled").boolValue = true;
        pair.FindPropertyRelative("pairName").stringValue = "Mirror Pair";
        pair.FindPropertyRelative("sourceTarget").objectReferenceValue = null;
        pair.FindPropertyRelative("mirroredTarget").objectReferenceValue = null;
        pair.FindPropertyRelative("mirroredRotationOffset").vector3Value = Vector3.zero;
    }

    private static bool DrawMirrorPair(SerializedProperty pairs, int index)
    {
        SerializedProperty pair = pairs.GetArrayElementAtIndex(index);
        SerializedProperty enabled = pair.FindPropertyRelative("mirrorEnabled");
        Transform source = GetTransform(pair, "sourceTarget");
        Transform mirrored = GetTransform(pair, "mirroredTarget");
        string summary = enabled != null && !enabled.boolValue
            ? "Disabled"
            : source != null && mirrored != null
                ? "Ready"
                : "Needs Targets";
        string stateKey = $"{pairs.serializedObject.targetObject.GetInstanceID()}:{pair.propertyPath}:card";

        bool remove = PropToolsEditor.ItemCard(
            stateKey,
            "PAIR",
            GetPairLabel(pair, index),
            summary,
            index == 0,
            () =>
        {
            PropToolsEditor.Toggle(pair.FindPropertyRelative("mirrorEnabled"), "Mirror Enabled");
            PropToolsEditor.Property(pair.FindPropertyRelative("pairName"), "Pair Name");
            PropToolsEditor.ObjectField(
                pair.FindPropertyRelative("sourceTarget"),
                typeof(Transform),
                "Source Target"
            );
            PropToolsEditor.ObjectField(
                pair.FindPropertyRelative("mirroredTarget"),
                typeof(Transform),
                "Mirrored Target"
            );
            PropToolsEditor.Property(
                pair.FindPropertyRelative("mirroredRotationOffset"),
                "Rotation Offset"
            );
        },
            "Name of this Live Mirroring pair. Expand the card to edit its targets and offset.",
            "A source and mirrored target that are managed together."
        );

        if (!remove)
            return false;

        pairs.DeleteArrayElementAtIndex(index);
        return true;
    }

    private static Transform GetTransform(SerializedProperty pair, string propertyName)
    {
        SerializedProperty property = pair.FindPropertyRelative(propertyName);
        return property != null ? property.objectReferenceValue as Transform : null;
    }

    private static string GetPairLabel(SerializedProperty pair, int index)
    {
        SerializedProperty pairName = pair.FindPropertyRelative("pairName");

        return pairName != null && !string.IsNullOrWhiteSpace(pairName.stringValue)
            ? pairName.stringValue
            : $"Pair {index + 1}";
    }
}
}
