namespace Wolfy.PropTools.EditorUI
{
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorMessages
{
    private static readonly Dictionary<string, bool> expandedStates =
        new Dictionary<string, bool>();
    private static GUIStyle hintTextStyle;
    private static GUIStyle hintPanelStyle;

    public static void Info(string title, string message = null) =>
        MessageBox(
            "INFO",
            title,
            message,
            PropToolsEditorTheme.InfoBar,
            PropToolsEditorTheme.InfoAccent,
            PropToolsEditorTheme.TextMuted
        );

    public static void Warning(string title, string message = null) =>
        MessageBox(
            "WARN",
            title,
            message,
            new Color(0.205f, 0.150f, 0.060f, 1f),
            PropToolsEditorTheme.Warning,
            PropToolsEditorTheme.Text
        );

    public static void Error(string title, string message = null) =>
        MessageBox(
            "ERROR",
            title,
            message,
            new Color(0.220f, 0.075f, 0.055f, 1f),
            PropToolsEditorTheme.Error,
            PropToolsEditorTheme.Text
        );

    public static void Success(string title, string message = null) =>
        MessageBox(
            "OK",
            title,
            message,
            new Color(0.065f, 0.155f, 0.075f, 1f),
            PropToolsEditorTheme.Success,
            PropToolsEditorTheme.Text
        );

    public static void Stats(string text)
    {
        Rect rect = GUILayoutUtility.GetRect(
            0,
            28f,
            GUILayout.ExpandWidth(true)
        );

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

    public static void Hint(
        string message,
        float minimumHeight = 34f)
    {
        string text = message ?? string.Empty;
        Rect panel = EditorGUILayout.BeginVertical(
            HintPanelStyle,
            GUILayout.ExpandWidth(true),
            GUILayout.MinHeight(minimumHeight)
        );

        if (Event.current.type == EventType.Repaint)
        {
            PropToolsEditorDrawing.RoundedRect(
                panel,
                PropToolsEditorTheme.PanelInset,
                PropToolsEditorTheme.BorderSoft,
                PropToolsEditorSpacing.NestedCornerRadius
            );
        }

        GUILayout.Label(
            text,
            HintTextStyle,
            GUILayout.ExpandWidth(true)
        );

        EditorGUILayout.EndVertical();
    }

    private static GUIStyle HintTextStyle =>
        hintTextStyle ?? (hintTextStyle =
            new GUIStyle(PropToolsEditorStyles.MessageBody)
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                padding = new RectOffset(0, 0, 1, 2),
                clipping = TextClipping.Clip
            });

    private static GUIStyle HintPanelStyle =>
        hintPanelStyle ?? (hintPanelStyle =
            new GUIStyle
            {
                padding = new RectOffset(9, 9, 6, 6)
            });

    private static void MessageBox(
        string tag,
        string title,
        string message,
        Color background,
        Color accent,
        Color textColor)
    {
        bool hasMessage = !string.IsNullOrWhiteSpace(message);
        string key = $"{tag}:{title}:{message}";

        if (!expandedStates.ContainsKey(key))
            expandedStates[key] = false;

        bool expanded = hasMessage && expandedStates[key];
        string headerText = hasMessage
            ? $"{(expanded ? "\u25BC" : "\u25B6")} {title}"
            : title;
        string fullText = expanded ? message : null;

        Rect widthProbe = GUILayoutUtility.GetRect(
            0f,
            0f,
            GUILayout.ExpandWidth(true)
        );
        float cardWidth = Mathf.Max(
            1f,
            widthProbe.width > 1f
                ? widthProbe.width
                : EditorGUIUtility.currentViewWidth
        );
        const float bodyLeftInset = 76f;
        const float bodyRightInset = 10f;
        const float bodyTop = 29f;
        const float bodyBottomPadding = 9f;
        float bodyWidth = Mathf.Max(
            1f,
            cardWidth - bodyLeftInset - bodyRightInset
        );

        GUIStyle body = new GUIStyle(PropToolsEditorStyles.MessageBody)
        {
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(0, 0, 1, 2),
            clipping = TextClipping.Clip
        };
        body.normal.textColor = textColor;

        float bodyHeight = expanded
            ? Mathf.Ceil(
                body.CalcHeight(new GUIContent(fullText), bodyWidth)
            )
            : 0f;
        float height = expanded
            ? Mathf.Max(
                52f,
                bodyTop + bodyHeight + bodyBottomPadding
            )
            : 32f;

        Rect rect = GUILayoutUtility.GetRect(
            0,
            height,
            GUILayout.ExpandWidth(true)
        );
        bool hover = rect.Contains(Event.current.mousePosition);
        Color border = new Color(
            accent.r,
            accent.g,
            accent.b,
            hover ? 0.78f : 0.58f
        );
        Color bg = hover
            ? new Color(
                background.r + 0.025f,
                background.g + 0.020f,
                background.b + 0.015f,
                background.a
            )
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

        Rect tagRect = new Rect(
            rect.x + 10f,
            rect.y + 7f,
            44f,
            18f
        );
        Rect titleRect = new Rect(
            tagRect.xMax + 8f,
            rect.y + 6f,
            Mathf.Max(0f, rect.width - 72f),
            20f
        );

        GUIStyle tagStyle =
            new GUIStyle(PropToolsEditorStyles.MessageTag);
        tagStyle.normal.textColor = accent;

        GUIStyle titleStyle =
            new GUIStyle(PropToolsEditorStyles.MessageBody)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
        titleStyle.normal.textColor = textColor;

        GUI.Label(tagRect, tag, tagStyle);
        GUI.Label(titleRect, headerText, titleStyle);

        if (expanded)
        {
            Rect bodyRect = new Rect(
                rect.x + bodyLeftInset,
                rect.y + bodyTop,
                Mathf.Max(
                    0f,
                    rect.width - bodyLeftInset - bodyRightInset
                ),
                Mathf.Max(
                    0f,
                    rect.height - bodyTop - bodyBottomPadding
                )
            );
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
