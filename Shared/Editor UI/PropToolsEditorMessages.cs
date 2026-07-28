namespace Wolfy.PropTools.EditorUI
{
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorMessages
{
    private static readonly Dictionary<string, bool> expandedStates = new Dictionary<string, bool>();

    public static void Info(string title, string message = null) =>
        MessageBox("INFO", title, message, PropToolsEditorTheme.InfoBar, PropToolsEditorTheme.InfoAccent, PropToolsEditorTheme.TextMuted);

    public static void Warning(string title, string message = null) =>
        MessageBox("WARN", title, message, new Color(0.205f, 0.150f, 0.060f, 1f), PropToolsEditorTheme.Warning, PropToolsEditorTheme.Text);

    public static void Error(string title, string message = null) =>
        MessageBox("ERROR", title, message, new Color(0.220f, 0.075f, 0.055f, 1f), PropToolsEditorTheme.Error, PropToolsEditorTheme.Text);

    public static void Success(string title, string message = null) =>
        MessageBox("OK", title, message, new Color(0.065f, 0.155f, 0.075f, 1f), PropToolsEditorTheme.Success, PropToolsEditorTheme.Text);

    public static void Stats(string text)
    {
        Rect rect = GUILayoutUtility.GetRect(0, 28f, GUILayout.ExpandWidth(true));

        PropToolsEditorDrawing.RoundedRect(
            rect,
            Color.Lerp(
                PropToolsEditorTheme.PanelHeader,
                PropToolsEditorTheme.BackgroundDark,
                0.48f
            ),
            PropToolsEditorTheme.BorderSoft,
            PropToolsEditorSpacing.NestedCornerRadius
        );

        GUI.Label(rect, text, PropToolsEditorStyles.StatsLabel);
    }

    private static void MessageBox(string tag, string title, string message, Color background, Color accent, Color textColor)
    {
        bool hasMessage = !string.IsNullOrWhiteSpace(message);
        string key = $"{tag}:{title}:{message}";

        if (!expandedStates.ContainsKey(key))
            expandedStates[key] = false;

        bool expanded = hasMessage && expandedStates[key];

        string headerText = hasMessage
            ? $"{(expanded ? "▼" : "▶")} {title}"
            : title;

        string fullText = expanded ? message : null;

        float width = Mathf.Max(120f, EditorGUIUtility.currentViewWidth - 104f);

        GUIStyle body = new GUIStyle(PropToolsEditorStyles.MessageBody);
        body.normal.textColor = textColor;

        float bodyHeight = expanded
            ? body.CalcHeight(new GUIContent(fullText), width)
            : 0f;

        float height = expanded
            ? Mathf.Clamp(bodyHeight + 34f, 50f, 120f)
            : 31f;

        Rect rect = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
        bool hover = rect.Contains(Event.current.mousePosition);

        Color border = new Color(accent.r, accent.g, accent.b, hover ? 0.78f : 0.58f);
        Color bg = hover
            ? new Color(background.r + 0.025f, background.g + 0.020f, background.b + 0.015f, background.a)
            : background;

        PropToolsEditorDrawing.RoundedRect(
            rect,
            bg,
            border,
            PropToolsEditorSpacing.NestedCornerRadius,
            hover
        );
        PropToolsEditorDrawing.RoundedAccentBar(
            rect,
            border,
            3f,
            PropToolsEditorSpacing.AccentVerticalInset
        );

        Rect tagRect = new Rect(rect.x + 10f, rect.y + 8f, 44f, 16f);
        Rect titleRect = new Rect(tagRect.xMax + 8f, rect.y + 7f, rect.width - 68f, 17f);

        GUIStyle tagStyle = new GUIStyle(PropToolsEditorStyles.MessageTag);
        tagStyle.normal.textColor = accent;

        GUIStyle titleStyle = new GUIStyle(PropToolsEditorStyles.MessageBody)
        {
            fontStyle = FontStyle.Bold,
            wordWrap = false,
            clipping = TextClipping.Clip
        };

        titleStyle.normal.textColor = textColor;

        GUI.Label(tagRect, tag, tagStyle);
        GUI.Label(titleRect, headerText, titleStyle);

        if (expanded)
        {
            Rect bodyRect = new Rect(titleRect.x + 14f, rect.y + 27f, rect.width - 82f, rect.height - 32f);
            GUI.Label(bodyRect, fullText, body);
        }

        if (hasMessage)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0 &&
                rect.Contains(Event.current.mousePosition))
            {
                expandedStates[key] = !expandedStates[key];
                Event.current.Use();
            }
        }
    }
}
}
