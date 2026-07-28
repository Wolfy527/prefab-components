namespace Wolfy.PropTools.EditorUI
{
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorStyles
{
    private static GUIStyle headerTitle, headerSubtitle, moduleTitle, sectionTitle, sectionSubtitle, subHeader;
    private static GUIStyle label, mutedLabel, propertyLabel, valueLabel, statsLabel, buttonLabel, miniButtonLabel, foldoutArrow, messageTag, messageBody;
    private static GUIStyle customTextField, customPopup, objectLabel;
    private static GUIStyle itemBadge, itemTitle, itemSummary, actionTitle, actionDescription;

    public static GUIStyle HeaderTitle => headerTitle ?? (headerTitle = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Text, 16, TextAnchor.MiddleLeft, FontStyle.Bold, false));
    public static GUIStyle HeaderSubtitle => headerSubtitle ?? (headerSubtitle = Make(EditorStyles.label, PropToolsEditorTheme.TextMuted, 11, TextAnchor.UpperLeft, FontStyle.Normal, true));
    public static GUIStyle ModuleTitle => moduleTitle ?? (moduleTitle = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Text, 12, TextAnchor.MiddleLeft, FontStyle.Bold, false));
    public static GUIStyle SectionTitle => sectionTitle ?? (sectionTitle = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Text, 12, TextAnchor.MiddleLeft, FontStyle.Bold, false));
    public static GUIStyle SectionSubtitle => sectionSubtitle ?? (sectionSubtitle = Make(EditorStyles.label, PropToolsEditorTheme.TextMuted, 10, TextAnchor.MiddleLeft, FontStyle.Normal, false));
    public static GUIStyle SubHeader => subHeader ?? (subHeader = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Text, 11, TextAnchor.MiddleLeft, FontStyle.Bold, false));
    public static GUIStyle Label => label ?? (label = Make(EditorStyles.label, PropToolsEditorTheme.Text, 11, TextAnchor.MiddleLeft, FontStyle.Normal, false));
    public static GUIStyle MutedLabel => mutedLabel ?? (mutedLabel = Make(EditorStyles.label, PropToolsEditorTheme.TextMuted, 11, TextAnchor.MiddleLeft, FontStyle.Normal, false));
    public static GUIStyle PropertyLabel => propertyLabel ?? (propertyLabel = Make(EditorStyles.boldLabel, PropToolsEditorTheme.TextMuted, 11, TextAnchor.MiddleLeft, FontStyle.Bold, false));
    public static GUIStyle ValueLabel => valueLabel ?? (valueLabel = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Value, 11, TextAnchor.MiddleRight, FontStyle.Bold, false));
    public static GUIStyle StatsLabel => statsLabel ?? (statsLabel = Make(EditorStyles.boldLabel, PropToolsEditorTheme.TextMuted, 11, TextAnchor.MiddleCenter, FontStyle.Bold, true));
    public static GUIStyle ButtonLabel => buttonLabel ?? (buttonLabel = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Text, 11, TextAnchor.MiddleCenter, FontStyle.Bold, false));
    public static GUIStyle MiniButtonLabel => miniButtonLabel ?? (miniButtonLabel = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Text, 11, TextAnchor.MiddleCenter, FontStyle.Bold, false));
    public static GUIStyle FoldoutArrow => foldoutArrow ?? (foldoutArrow = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Text, 11, TextAnchor.MiddleCenter, FontStyle.Bold, false));
    public static GUIStyle MessageTag => messageTag ?? (messageTag = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Value, 10, TextAnchor.MiddleLeft, FontStyle.Bold, false));
    public static GUIStyle MessageBody => messageBody ?? (messageBody = Make(EditorStyles.label, PropToolsEditorTheme.TextMuted, 11, TextAnchor.MiddleLeft, FontStyle.Normal, true));
    public static GUIStyle ItemBadge => itemBadge ?? (itemBadge = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Value, 10, TextAnchor.MiddleCenter, FontStyle.Bold, false));
    public static GUIStyle ItemTitle => itemTitle ?? (itemTitle = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Text, 12, TextAnchor.MiddleLeft, FontStyle.Bold, false));
    public static GUIStyle ItemSummary => itemSummary ?? (itemSummary = Make(EditorStyles.label, PropToolsEditorTheme.TextMuted, 10, TextAnchor.MiddleRight, FontStyle.Normal, false));
    public static GUIStyle ActionTitle => actionTitle ?? (actionTitle = Make(EditorStyles.boldLabel, PropToolsEditorTheme.Text, 13, TextAnchor.MiddleLeft, FontStyle.Bold, false));
    public static GUIStyle ActionDescription => actionDescription ?? (actionDescription = Make(EditorStyles.label, PropToolsEditorTheme.TextMuted, 10, TextAnchor.UpperLeft, FontStyle.Normal, true));

    public static GUIStyle CustomTextField
    {
        get
        {
            if (customTextField == null)
            {
                customTextField = new GUIStyle(EditorStyles.textField)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 11,
                    border = new RectOffset(5, 5, 5, 5),
                    padding = new RectOffset(5, 5, 1, 1),
                    margin = new RectOffset(0, 0, 0, 0),
                    clipping = TextClipping.Clip
                };

                customTextField.normal.background =
                    PropToolsEditorDrawing.RoundedTexture(
                        PropToolsEditorTheme.Field,
                        PropToolsEditorTheme.FieldBorder
                    );
                customTextField.hover.background =
                    PropToolsEditorDrawing.RoundedTexture(
                        PropToolsEditorTheme.FieldHover,
                        PropToolsEditorTheme.FieldBorderHover
                    );
                customTextField.focused.background =
                    PropToolsEditorDrawing.RoundedTexture(
                        PropToolsEditorTheme.FieldFocus,
                        PropToolsEditorTheme.FieldBorderFocus
                    );
                customTextField.active.background =
                    customTextField.focused.background;
                customTextField.onNormal.background =
                    customTextField.normal.background;
                customTextField.onHover.background =
                    customTextField.hover.background;
                customTextField.onFocused.background =
                    customTextField.focused.background;
                customTextField.onActive.background =
                    customTextField.active.background;
                SetAllTextColors(customTextField, PropToolsEditorTheme.Text);
            }

            return customTextField;
        }
    }

    public static GUIStyle CustomPopup
    {
        get
        {
            if (customPopup == null)
            {
                customPopup = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 11,
                    padding = new RectOffset(5, 18, 1, 1),
                    clipping = TextClipping.Clip
                };

                SetAllTextColors(customPopup, PropToolsEditorTheme.Text);
            }

            return customPopup;
        }
    }

    public static GUIStyle ObjectLabel
    {
        get
        {
            if (objectLabel == null)
            {
                objectLabel = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 11,
                    padding = new RectOffset(5, 20, 1, 1),
                    clipping = TextClipping.Clip
                };

                SetAllTextColors(objectLabel, PropToolsEditorTheme.Text);
            }

            return objectLabel;
        }
    }

    private static GUIStyle Make(GUIStyle baseStyle, Color color, int fontSize, TextAnchor alignment, FontStyle fontStyle, bool wordWrap)
    {
        GUIStyle style = new GUIStyle(baseStyle)
        {
            fontSize = fontSize,
            alignment = alignment,
            fontStyle = fontStyle,
            wordWrap = wordWrap,
            clipping = TextClipping.Clip
        };

        SetAllTextColors(style, color);
        return style;
    }

    private static void SetAllTextColors(GUIStyle style, Color color)
    {
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.onNormal.textColor = color;
        style.onHover.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
    }
}
}
