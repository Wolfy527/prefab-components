namespace Wolfy.PropTools.EditorUI
{
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorHighlight
{
    private sealed class HoverState
    {
        public int targetId;
        public readonly HashSet<string> properties =
            new HashSet<string>();
    }

    public readonly struct HighlightScope : IDisposable
    {
        private readonly int previousScopeId;

        internal HighlightScope(int previousScopeId)
        {
            this.previousScopeId = previousScopeId;
        }

        public void Dispose()
        {
            currentScopeId = previousScopeId;
        }
    }

    private const string ModulePrefix = "@module:";
    private const string FeaturePrefix = "@feature:";
    private const string ItemPrefix = "@item:";
    private static int currentScopeId;
    private static readonly HashSet<string> comparisonProperties = new HashSet<string>();
    private static readonly Dictionary<int, HoverState> hoverByScope =
        new Dictionary<int, HoverState>();

    public static HighlightScope PushScope(UnityEngine.Object owner)
    {
        int previous = currentScopeId;
        currentScopeId = owner != null ? owner.GetInstanceID() : 0;
        return new HighlightScope(previous);
    }

    public static bool Hover(UnityEngine.Object target, IEnumerable<string> propertyPaths)
    {
        int targetId = target != null ? target.GetInstanceID() : 0;
        HoverState state = GetHoverState();

        if (targetId == state.targetId &&
            Matches(state.properties, propertyPaths))
        {
            return false;
        }

        state.targetId = targetId;
        Replace(state.properties, propertyPaths);
        return true;
    }

    public static void Draw(SerializedProperty property, Rect rect)
    {
        if (property == null || property.serializedObject?.targetObject == null)
            return;

        int targetId = property.serializedObject.targetObject.GetInstanceID();
        string path = property.propertyPath;
        HoverState state = GetHoverState();
        bool highlighted =
            targetId == state.targetId &&
            state.properties.Contains(path);

        if (!highlighted || Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(rect, PropToolsEditorTheme.HighlightFill);
        PropToolsEditorDrawing.Border(
            rect,
            PropToolsEditorTheme.HighlightBorder
        );
        EditorGUI.DrawRect(
            new Rect(
                rect.x,
                rect.y + 2f,
                4f,
                Mathf.Max(0f, rect.height - 4f)
            ),
            PropToolsEditorTheme.HighlightAccent
        );
    }

    public static string Module(string moduleId) =>
        string.IsNullOrWhiteSpace(moduleId) ? null : ModulePrefix + moduleId;

    public static bool IsModuleHovered(UnityEngine.Object target, string moduleId)
    {
        HoverState state = GetHoverState();
        return target != null &&
               target.GetInstanceID() == state.targetId &&
               state.properties.Contains(Module(moduleId));
    }

    public static string Feature(string generationModuleId) =>
        string.IsNullOrWhiteSpace(generationModuleId)
            ? null
            : FeaturePrefix + generationModuleId;

    public static bool IsFeatureHovered(UnityEngine.Object target, string generationModuleId)
    {
        HoverState state = GetHoverState();
        return target != null &&
               target.GetInstanceID() == state.targetId &&
               state.properties.Contains(Feature(generationModuleId));
    }

    public static string Item(string itemId) =>
        string.IsNullOrWhiteSpace(itemId)
            ? null
            : ItemPrefix + itemId;

    public static bool IsItemHovered(
        UnityEngine.Object target,
        string itemId)
    {
        HoverState state = GetHoverState();
        return target != null &&
               target.GetInstanceID() == state.targetId &&
               state.properties.Contains(Item(itemId));
    }

    private static HoverState GetHoverState()
    {
        if (!hoverByScope.TryGetValue(
                currentScopeId,
                out HoverState state))
        {
            state = new HoverState();
            hoverByScope[currentScopeId] = state;
        }

        return state;
    }

    private static void Replace(HashSet<string> destination, IEnumerable<string> values)
    {
        destination.Clear();

        if (values == null)
            return;

        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                destination.Add(value);
        }
    }

    private static bool Matches(HashSet<string> current, IEnumerable<string> values)
    {
        if (values == null)
            return current.Count == 0;

        comparisonProperties.Clear();

        foreach (string value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            comparisonProperties.Add(value);
        }

        return current.SetEquals(comparisonProperties);
    }
}
}
