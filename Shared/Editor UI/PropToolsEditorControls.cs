namespace Wolfy.PropTools.EditorUI
{
using System;
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorControls
{
    public const float ToggleWidth = 30f;
    public const float ToggleHeight = 16f;
    private const int ToggleTextureScale = 4;

    private static Texture2D toggleTrackOff;
    private static Texture2D toggleTrackOffHover;
    private static Texture2D toggleTrackOn;
    private static Texture2D toggleTrackOnHover;
    private static Texture2D toggleThumbOff;
    private static Texture2D toggleThumbOn;
    private static GUIStyle quietDangerLabel;
    private static GUIStyle quietDangerHoverLabel;

    public static bool Toggle(SerializedObject so, string propertyName, string label)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        return Toggle(property, label);
    }

    public static bool Toggle(SerializedProperty property, string label)
    {
        if (property == null)
            return false;

        property.boolValue = Toggle(property.boolValue, label, property);
        return property.boolValue;
    }

    public static bool Toggle(bool value, string label)
    {
        return Toggle(value, label, null);
    }

    private static bool Toggle(
        bool value,
        string label,
        SerializedProperty property)
    {
        Rect rect = GUILayoutUtility.GetRect(0, PropToolsEditorSpacing.RowHeight, GUILayout.ExpandWidth(true));
        bool hover = rect.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint && hover)
            EditorGUI.DrawRect(rect, new Color(PropToolsEditorTheme.Accent.r, PropToolsEditorTheme.Accent.g, PropToolsEditorTheme.Accent.b, 0.045f));

        PropToolsEditorHighlight.Draw(property, rect);

        Rect box = new Rect(rect.x + 5f, rect.y + 4f, ToggleWidth, ToggleHeight);
        Rect labelRect = new Rect(box.xMax + 8f, rect.y + 3f, rect.width - ToggleWidth - 18f, 18f);

        bool newValue = ToggleBox(box, value);
        GUI.Label(labelRect, PropToolsEditorTooltips.Content(label, property), PropToolsEditorStyles.MutedLabel);
        PropToolsEditorTooltips.Track(labelRect, label, property);

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition) && !box.Contains(Event.current.mousePosition))
        {
            Event.current.Use();
            newValue = !value;
        }

        return newValue;
    }

    public static bool ToggleBox(Rect rect, bool value)
    {
        rect = PropToolsEditorDrawing.PixelAlign(rect);
        bool hover = rect.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint)
        {
            Texture2D track = value
                ? hover ? ToggleTrackOnHover : ToggleTrackOn
                : hover ? ToggleTrackOffHover : ToggleTrackOff;

            GUI.DrawTexture(rect, track, ScaleMode.StretchToFill, true);

            float thumbSize = Mathf.Max(8f, rect.height - 4f);
            float thumbX = value
                ? rect.xMax - thumbSize - 2f
                : rect.x + 2f;
            Rect thumbRect = new Rect(
                thumbX,
                rect.y + (rect.height - thumbSize) * 0.5f,
                thumbSize,
                thumbSize
            );
            GUI.DrawTexture(thumbRect, value ? ToggleThumbOn : ToggleThumbOff, ScaleMode.StretchToFill, true);
        }

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
        {
            Event.current.Use();
            return !value;
        }

        return value;
    }

    private static Texture2D ToggleTrackOff =>
        toggleTrackOff != null
            ? toggleTrackOff
            : toggleTrackOff = CreateRoundedTexture(
                30 * ToggleTextureScale,
                16 * ToggleTextureScale,
                8f * ToggleTextureScale,
                PropToolsEditorTheme.ToggleOff,
                new Color(1f, 0.62f, 0.30f, 0.18f),
                ToggleTextureScale
            );

    private static Texture2D ToggleTrackOffHover =>
        toggleTrackOffHover != null
            ? toggleTrackOffHover
            : toggleTrackOffHover = CreateRoundedTexture(
                30 * ToggleTextureScale,
                16 * ToggleTextureScale,
                8f * ToggleTextureScale,
                PropToolsEditorTheme.ToggleOffHover,
                new Color(1f, 0.62f, 0.30f, 0.30f),
                ToggleTextureScale
            );

    private static Texture2D ToggleTrackOn =>
        toggleTrackOn != null
            ? toggleTrackOn
            : toggleTrackOn = CreateRoundedTexture(
                30 * ToggleTextureScale,
                16 * ToggleTextureScale,
                8f * ToggleTextureScale,
                PropToolsEditorTheme.ToggleOn,
                new Color(1f, 0.63f, 0.30f, 0.48f),
                ToggleTextureScale
            );

    private static Texture2D ToggleTrackOnHover =>
        toggleTrackOnHover != null
            ? toggleTrackOnHover
            : toggleTrackOnHover = CreateRoundedTexture(
                30 * ToggleTextureScale,
                16 * ToggleTextureScale,
                8f * ToggleTextureScale,
                PropToolsEditorTheme.ToggleOnHover,
                new Color(1f, 0.72f, 0.38f, 0.64f),
                ToggleTextureScale
            );

    private static Texture2D ToggleThumbOff =>
        toggleThumbOff != null
            ? toggleThumbOff
            : toggleThumbOff = CreateRoundedTexture(
                12 * ToggleTextureScale,
                12 * ToggleTextureScale,
                6f * ToggleTextureScale,
                PropToolsEditorTheme.ToggleThumbOff,
                new Color(0.05f, 0.045f, 0.04f, 0.72f),
                ToggleTextureScale
            );

    private static Texture2D ToggleThumbOn =>
        toggleThumbOn != null
            ? toggleThumbOn
            : toggleThumbOn = CreateRoundedTexture(
                12 * ToggleTextureScale,
                12 * ToggleTextureScale,
                6f * ToggleTextureScale,
                PropToolsEditorTheme.ToggleThumbOn,
                new Color(0.24f, 0.11f, 0.035f, 0.72f),
                ToggleTextureScale
            );

    private static Texture2D CreateRoundedTexture(
        int width,
        int height,
        float radius,
        Color fill,
        Color border,
        int borderWidth)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = "Prop Tools Toggle",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color32[] pixels = new Color32[width * height];
        Color32 transparent = new Color32(0, 0, 0, 0);
        Color32 fill32 = fill;
        Color32 border32 = border;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool insideOuter = IsInsideRoundedRect(x, y, width, height, radius);
                bool insideInner = IsInsideRoundedRect(
                    x - borderWidth,
                    y - borderWidth,
                    width - borderWidth * 2,
                    height - borderWidth * 2,
                    Mathf.Max(0f, radius - borderWidth)
                );
                pixels[y * width + x] = !insideOuter
                    ? transparent
                    : insideInner ? fill32 : border32;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static bool IsInsideRoundedRect(
        float x,
        float y,
        float width,
        float height,
        float radius)
    {
        if (x < 0f || y < 0f || x >= width || y >= height)
            return false;

        float centerX = Mathf.Clamp(x, radius - 0.5f, width - radius - 0.5f);
        float centerY = Mathf.Clamp(y, radius - 0.5f, height - radius - 0.5f);
        float deltaX = x - centerX;
        float deltaY = y - centerY;
        return deltaX * deltaX + deltaY * deltaY <= radius * radius;
    }

    public static void Stepper(SerializedObject so, string propertyName, string label, int min, int max, Func<int, string> displayFormatter = null)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null) Stepper(property, label, min, max, displayFormatter);
    }

    public static void Stepper(SerializedProperty property, string label, int min, int max, Func<int, string> displayFormatter = null)
    {
        if (property == null) return;

        property.intValue = Mathf.Clamp(property.intValue, min, max);

        Rect rect = GUILayoutUtility.GetRect(0, 26f, GUILayout.ExpandWidth(true));
        bool hover = rect.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint)
        {
            PropToolsEditorDrawing.RoundedRect(
                rect,
                hover
                    ? PropToolsEditorTheme.FieldHover
                    : PropToolsEditorTheme.Field,
                hover
                    ? PropToolsEditorTheme.FieldBorderHover
                    : PropToolsEditorTheme.FieldBorder,
                PropToolsEditorSpacing.NestedCornerRadius
            );
        }

        PropToolsEditorHighlight.Draw(property, rect);

        float labelW = 160f;
        float buttonW = 34f;
        float numberW = 48f;
        float valueW = 115f;
        float gap = 4f;

        Rect labelRect = new Rect(rect.x + 7f, rect.y + 4f, labelW, 18f);
        Rect valueRect = new Rect(rect.xMax - valueW - buttonW * 2f - numberW - gap * 4f - 4f, rect.y + 4f, valueW, 18f);
        Rect minusRect = new Rect(valueRect.xMax + gap, rect.y + 3f, buttonW, 20f);
        Rect numberRect = new Rect(minusRect.xMax + gap, rect.y + 3f, numberW, 20f);
        Rect plusRect = new Rect(numberRect.xMax + gap, rect.y + 3f, buttonW, 20f);

        PropToolsEditorDrawing.LabelDivider(
            rect,
            valueRect.x - 7f
        );
        GUI.Label(labelRect, PropToolsEditorTooltips.Content(label, property), PropToolsEditorStyles.PropertyLabel);
        PropToolsEditorTooltips.Track(labelRect, label, property);
        GUI.Label(valueRect, displayFormatter?.Invoke(property.intValue) ?? property.intValue.ToString(), PropToolsEditorStyles.ValueLabel);

        if (MiniButton(minusRect, "-")) property.intValue = Mathf.Clamp(property.intValue - 1, min, max);

        string current = property.intValue.ToString();
        int targetId =
            property.serializedObject?.targetObject != null
                ? property.serializedObject.targetObject.GetInstanceID()
                : 0;
        string next = PropToolsEditorFields.TextInput(
            numberRect,
            current,
            $"Stepper.{targetId}.{property.propertyPath}"
        );
        if (next != current && int.TryParse(next, out int parsed))
            property.intValue = Mathf.Clamp(parsed, min, max);

        if (MiniButton(plusRect, "+")) property.intValue = Mathf.Clamp(property.intValue + 1, min, max);
    }

    public static bool PrimaryButton(string text, float height = 30f) => ButtonInternal(text, height, PropToolsEditorTheme.ButtonPrimaryLeft, PropToolsEditorTheme.ButtonPrimaryRight, PropToolsEditorTheme.AccentBright, PropToolsEditorTheme.BorderStrong);
    public static bool SecondaryButton(string text, float height = 26f) => ButtonInternal(text, height, PropToolsEditorTheme.ButtonSecondaryLeft, PropToolsEditorTheme.ButtonSecondaryRight, PropToolsEditorTheme.AccentDark, PropToolsEditorTheme.Border);
    public static bool DangerButton(string text, float height = 26f) => ButtonInternal(text, height, PropToolsEditorTheme.ButtonDangerLeft, PropToolsEditorTheme.ButtonDangerRight, PropToolsEditorTheme.Error, new Color(PropToolsEditorTheme.Error.r, PropToolsEditorTheme.Error.g, PropToolsEditorTheme.Error.b, 0.75f));

    public static bool MiniButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);

        Color fill = hover
            ? Color.Lerp(PropToolsEditorTheme.ButtonPrimaryLeft, PropToolsEditorTheme.ButtonPrimaryRight, 0.42f)
            : Color.Lerp(PropToolsEditorTheme.ButtonSecondaryLeft, PropToolsEditorTheme.ButtonSecondaryRight, 0.50f);
        PropToolsEditorDrawing.RoundedRect(
            rect,
            fill,
            hover ? PropToolsEditorTheme.BorderStrong : PropToolsEditorTheme.BorderSoft,
            3f,
            hover
        );
        GUI.Label(rect, PropToolsEditorTooltips.Content(text), PropToolsEditorStyles.MiniButtonLabel);
        PropToolsEditorTooltips.Track(rect, text);

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
        {
            Event.current.Use();
            return true;
        }

        return false;
    }

    public static bool MiniButton(string text, float width = 72f, float height = 22f)
    {
        Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));
        return MiniButton(rect, text);
    }

    public static bool QuietDangerButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint)
        {
            PropToolsEditorDrawing.RoundedRect(
                rect,
                hover
                    ? new Color(PropToolsEditorTheme.Error.r, PropToolsEditorTheme.Error.g, PropToolsEditorTheme.Error.b, 0.13f)
                    : new Color(0.155f, 0.145f, 0.134f, 1f),
                hover
                    ? new Color(PropToolsEditorTheme.Error.r, PropToolsEditorTheme.Error.g, PropToolsEditorTheme.Error.b, 0.48f)
                    : PropToolsEditorTheme.Border,
                3f,
                hover
            );
        }

        GUI.Label(rect, PropToolsEditorTooltips.Content(text), hover ? QuietDangerHoverLabel : QuietDangerLabel);
        PropToolsEditorTooltips.Track(rect, text);
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&
            rect.Contains(Event.current.mousePosition))
        {
            Event.current.Use();
            return true;
        }

        return false;
    }

    private static GUIStyle QuietDangerLabel =>
        quietDangerLabel ?? (quietDangerLabel = CreateQuietDangerLabel(PropToolsEditorTheme.TextMuted));

    private static GUIStyle QuietDangerHoverLabel =>
        quietDangerHoverLabel ?? (quietDangerHoverLabel = CreateQuietDangerLabel(PropToolsEditorTheme.Error));

    private static GUIStyle CreateQuietDangerLabel(Color color)
    {
        GUIStyle style = new GUIStyle(PropToolsEditorStyles.MiniButtonLabel);
        PropToolsEditorDrawing.SetTextColor(style, color);
        return style;
    }

    private static bool ButtonInternal(string text, float height, Color left, Color right, Color hoverLeft, Color border)
    {
        Rect rect = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
        bool hover = rect.Contains(Event.current.mousePosition);

        Color fill = hover ? hoverLeft : Color.Lerp(left, right, 0.38f);
        PropToolsEditorDrawing.RoundedRect(
            rect,
            fill,
            hover ? PropToolsEditorTheme.BorderStrong : border,
            4f,
            true
        );
        GUI.Label(rect, PropToolsEditorTooltips.Content(text), PropToolsEditorStyles.ButtonLabel);
        PropToolsEditorTooltips.Track(rect, text);

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
        {
            Event.current.Use();
            return true;
        }

        return false;
    }
}
}
