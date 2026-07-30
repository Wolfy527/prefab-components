namespace Wolfy.PropTools.EditorUI
{
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorFoldouts
{
    private static readonly System.Collections.Generic.Dictionary<string, FoldoutStateKeys>
        stateKeyCache = new System.Collections.Generic.Dictionary<string, FoldoutStateKeys>();
    private static GUIStyle enabledTitleStyle;
    private static GUIStyle disabledTitleStyle;
    private static GUIStyle compactSummaryStyle;

    public static bool Foldout(
        ref bool expanded,
        string title,
        string subtitle = null,
        bool enabled = true,
        bool selected = false,
        SerializedProperty toggleProperty = null,
        string compactSummary = null)
    {
        expanded = FoldoutHeader(title, expanded, enabled, subtitle, selected, toggleProperty, compactSummary);
        return expanded;
    }

    public static bool FoldoutHeader(
        string title,
        bool expanded,
        bool enabled = true,
        string subtitle = null,
        bool selected = false,
        SerializedProperty toggleProperty = null,
        string compactSummary = null)
    {
        FoldoutStateKeys keys = BuildStateKeys(title, subtitle, toggleProperty);
        expanded = LoadState(keys, expanded);

        Rect rect = GUILayoutUtility.GetRect(
            0,
            string.IsNullOrWhiteSpace(subtitle)
                ? PropToolsEditorSpacing.FoldoutHeight
                : PropToolsEditorSpacing.FoldoutHeightWithSubtitle,
            GUILayout.ExpandWidth(true)
        );

        bool hover = rect.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint)
            EditorGUI.DrawRect(rect, PropToolsEditorTheme.Background);

        Color left = enabled
            ? selected ? PropToolsEditorTheme.AccentDark
              : expanded ? new Color(0.340f, 0.158f, 0.067f, 1f)
              : hover ? new Color(0.300f, 0.145f, 0.067f, 1f)
              : PropToolsEditorTheme.ModuleLeft
            : new Color(0.125f, 0.116f, 0.108f, 1f);

        Color mid = enabled
            ? PropToolsEditorTheme.ModuleMid
            : PropToolsEditorTheme.BackgroundDark;

        Color right = PropToolsEditorTheme.ModuleRight;

        Color fill = Color.Lerp(
            Color.Lerp(left, mid, 0.50f),
            right,
            0.34f
        );

        if (selected)
        {
            fill = Color.Lerp(
                fill,
                PropToolsEditorTheme.AccentDark,
                0.32f
            );
        }

        PropToolsEditorDrawing.RoundedRect(
            rect,
            fill,
            selected
                ? PropToolsEditorTheme.HighlightBorder
                : hover
                    ? PropToolsEditorTheme.Border
                    : PropToolsEditorTheme.BorderSoft,
            PropToolsEditorSpacing.ModuleCornerRadius,
            selected || hover
        );
        PropToolsEditorDrawing.RoundedAccentBar(
            rect,
            selected
                ? PropToolsEditorTheme.HighlightAccent
                : enabled
                    ? PropToolsEditorTheme.Accent
                    : PropToolsEditorTheme.TextDim,
            selected ? 6f : 4f,
            PropToolsEditorSpacing.AccentVerticalInset
        );

        Rect arrowRect = new Rect(rect.x + 9f, rect.y + 8f, 18f, 18f);
        Rect toggleRect = new Rect(
            rect.x + 33f,
            rect.y + 9f,
            PropToolsEditorControls.ToggleWidth,
            PropToolsEditorControls.ToggleHeight
        );
        Rect labelRect = new Rect(
            rect.x + (toggleProperty != null ? 71f : 33f),
            rect.y + 7f,
            rect.width - (toggleProperty != null ? 82f : 44f),
            20f
        );

        GUI.Label(arrowRect, expanded ? "▼" : "▶", PropToolsEditorStyles.FoldoutArrow);

        if (toggleProperty != null)
            toggleProperty.boolValue = PropToolsEditorControls.ToggleBox(toggleRect, toggleProperty.boolValue);

        GUIStyle titleStyle = enabled ? EnabledTitleStyle : DisabledTitleStyle;

        GUI.Label(labelRect, PropToolsEditorTooltips.Content(title), titleStyle);
        PropToolsEditorTooltips.Track(labelRect, title);

        if (!expanded && !string.IsNullOrWhiteSpace(compactSummary))
        {
            float titleWidth = Mathf.Min(
                labelRect.width * 0.55f,
                titleStyle.CalcSize(new GUIContent(title)).x + 12f
            );
            Rect summaryRect = new Rect(
                labelRect.x + titleWidth,
                labelRect.y,
                Mathf.Max(0f, labelRect.width - titleWidth),
                labelRect.height
            );

            GUI.Label(
                summaryRect,
                PropToolsEditorTooltips.Content(
                    compactSummary,
                    compactSummary
                ),
                CompactSummaryStyle
            );
            PropToolsEditorTooltips.Track(
                summaryRect,
                compactSummary
            );
        }

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            Rect subtitleRect = new Rect(
                labelRect.x,
                rect.y + 27f,
                labelRect.width,
                14f
            );
            GUI.Label(
                subtitleRect,
                PropToolsEditorTooltips.Content(subtitle, subtitle),
                PropToolsEditorStyles.SectionSubtitle
            );
            PropToolsEditorTooltips.Track(subtitleRect, subtitle);
        }

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&
            rect.Contains(Event.current.mousePosition) &&
            (toggleProperty == null || !toggleRect.Contains(Event.current.mousePosition)))
        {
            expanded = !expanded;
            SaveState(keys, expanded);

            Event.current.Use();
            GUI.changed = true;

            return expanded;
        }

        return expanded;
    }

    private static bool LoadState(FoldoutStateKeys keys, bool fallback)
    {
        if (HasState(keys.ObjectHasValueKey))
            return GetState(keys.ObjectValueKey, fallback);

        if (HasState(keys.GlobalHasValueKey))
            return GetState(keys.GlobalValueKey, fallback);

        return fallback;
    }

    private static GUIStyle EnabledTitleStyle
    {
        get
        {
            if (enabledTitleStyle == null)
            {
                enabledTitleStyle = new GUIStyle(PropToolsEditorStyles.ModuleTitle);
                enabledTitleStyle.normal.textColor = PropToolsEditorTheme.Text;
            }

            return enabledTitleStyle;
        }
    }

    private static GUIStyle DisabledTitleStyle
    {
        get
        {
            if (disabledTitleStyle == null)
            {
                disabledTitleStyle = new GUIStyle(PropToolsEditorStyles.ModuleTitle);
                disabledTitleStyle.normal.textColor = PropToolsEditorTheme.TextMuted;
            }

            return disabledTitleStyle;
        }
    }

    private static GUIStyle CompactSummaryStyle
    {
        get
        {
            if (compactSummaryStyle == null)
            {
                compactSummaryStyle = new GUIStyle(PropToolsEditorStyles.SectionSubtitle)
                {
                    alignment = TextAnchor.MiddleRight,
                    clipping = TextClipping.Clip
                };
            }

            return compactSummaryStyle;
        }
    }

    private static void SaveState(FoldoutStateKeys keys, bool value)
    {
        SetState(keys.GlobalHasValueKey, true);
        SetState(keys.GlobalValueKey, value);

        SetState(keys.ObjectHasValueKey, true);
        SetState(keys.ObjectValueKey, value);
    }

    private static bool HasState(string key)
    {
        if (SessionState.GetBool(key + ".sessionKnown", false))
            return SessionState.GetBool(key, false);

        bool value = EditorPrefs.GetBool(key, false);

        SessionState.SetBool(key + ".sessionKnown", true);
        SessionState.SetBool(key, value);

        return value;
    }

    private static bool GetState(string key, bool fallback)
    {
        if (SessionState.GetBool(key + ".sessionKnown", false))
            return SessionState.GetBool(key, fallback);

        bool value = EditorPrefs.GetBool(key, fallback);

        SessionState.SetBool(key + ".sessionKnown", true);
        SessionState.SetBool(key, value);

        return value;
    }

    private static void SetState(string key, bool value)
    {
        SessionState.SetBool(key + ".sessionKnown", true);
        SessionState.SetBool(key, value);

        EditorPrefs.SetBool(key, value);
    }

    private static FoldoutStateKeys BuildStateKeys(string title, string subtitle, SerializedProperty toggleProperty)
    {
        Object context = null;
        string propertyPath = "NoProperty";

        if (toggleProperty != null)
        {
            propertyPath = toggleProperty.propertyPath;

            if (toggleProperty.serializedObject != null)
                context = toggleProperty.serializedObject.targetObject;
        }

        if (context == null)
            context = Selection.activeObject;

        int contextId = context != null ? context.GetInstanceID() : 0;
        string cacheKey = $"{contextId}:{propertyPath}:{title}:{subtitle}";

        if (stateKeyCache.TryGetValue(cacheKey, out FoldoutStateKeys cached))
            return cached;

        string objectKey = BuildObjectKey(context);
        string cleanTitle = CleanKeyPart(title, "Untitled");
        string cleanSubtitle = CleanKeyPart(subtitle, "NoSubtitle");
        string cleanPropertyPath = CleanKeyPart(propertyPath, "NoProperty");

        string globalBase = $"PropToolsEditor.Foldout.Global.{cleanPropertyPath}.{cleanTitle}.{cleanSubtitle}";
        string objectBase = $"PropToolsEditor.Foldout.Object.{objectKey}.{cleanPropertyPath}.{cleanTitle}.{cleanSubtitle}";

        FoldoutStateKeys keys = new FoldoutStateKeys
        {
            GlobalHasValueKey = globalBase + ".hasValue",
            GlobalValueKey = globalBase + ".value",
            ObjectHasValueKey = objectBase + ".hasValue",
            ObjectValueKey = objectBase + ".value"
        };

        stateKeyCache[cacheKey] = keys;
        return keys;
    }

    private static string BuildObjectKey(Object context)
    {
        if (context == null)
            return "NoObject";

        try
        {
            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(context);

            if (!string.IsNullOrWhiteSpace(id.ToString()))
                return CleanKeyPart(id.ToString(), "Object");
        }
        catch
        {
        }

        return CleanKeyPart($"{context.GetType().FullName}.{context.GetInstanceID()}", "Object");
    }

    private static string CleanKeyPart(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        return value
            .Replace(" ", "_")
            .Replace("/", "_")
            .Replace("\\", "_")
            .Replace(".", "_")
            .Replace(":", "_")
            .Replace(";", "_")
            .Replace(",", "_")
            .Replace("(", "_")
            .Replace(")", "_")
            .Replace("[", "_")
            .Replace("]", "_")
            .Replace("{", "_")
            .Replace("}", "_");
    }

    private struct FoldoutStateKeys
    {
        public string GlobalHasValueKey;
        public string GlobalValueKey;
        public string ObjectHasValueKey;
        public string ObjectValueKey;
    }
}
}
