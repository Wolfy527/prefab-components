namespace Wolfy.PropTools.EditorUI
{
using System;
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorLayout
{
    private static GUIStyle moduleShellStyle;
    private static GUIStyle moduleBodyStyle;
    private static GUIStyle nestedBodyStyle;
    private static GUIStyle itemShellStyle;
    private static GUIStyle itemBodyStyle;
    private static GUIStyle actionPanelStyle;
    private static GUIStyle actionFooterStyle;
    private static GUIStyle smallFoldoutLabelStyle;
    private static Texture2D headerLogo;

    public static void Header(
        string title,
        string subtitle = null,
        float availableWidthOverride = 0f)
    {
        Space(PropToolsEditorSpacing.Small);

        bool hasSubtitle = !string.IsNullOrWhiteSpace(subtitle);
        bool hasLogo = HeaderLogo != null;
        const float titleHeight = 22f;
        const float descriptionPaddingX = 7f;
        const float descriptionPaddingY = 4f;
        Rect widthProbe = GUILayoutUtility.GetRect(
            0f,
            0f,
            GUILayout.ExpandWidth(true)
        );
        float availableWidth = Mathf.Max(
            120f,
            availableWidthOverride > 0f
                ? availableWidthOverride
                : widthProbe.width > 1f
                ? widthProbe.width
                : EditorGUIUtility.currentViewWidth
        );
        float estimatedTextWidth = Mathf.Max(
            80f,
            availableWidth - (hasLogo ? 84f : 30f)
        );
        float subtitleHeight = hasSubtitle
            ? PropToolsEditorStyles.HeaderSubtitle.CalcHeight(
                new GUIContent(subtitle),
                Mathf.Max(80f, estimatedTextWidth - descriptionPaddingX * 2f)
            )
            : 0f;
        float descriptionHeight = hasSubtitle
            ? subtitleHeight + descriptionPaddingY * 2f
            : 0f;
        float textBlockHeight = titleHeight + (hasSubtitle ? 5f + descriptionHeight : 0f);
        float logoSize = hasSubtitle ? 44f : 32f;
        float headerHeight = hasSubtitle
            ? Mathf.Max(
                PropToolsEditorSpacing.HeaderHeight,
                Mathf.Max(textBlockHeight, hasLogo ? logoSize : 0f) + 16f
            )
            : 50f;

        Rect rect = GUILayoutUtility.GetRect(
            0,
            headerHeight,
            GUILayout.ExpandWidth(true)
        );

        DrawBackground(rect);

        PropToolsEditorDrawing.HorizontalGradient(
            rect,
            PropToolsEditorTheme.HeaderLeft,
            PropToolsEditorTheme.HeaderMid,
            PropToolsEditorTheme.HeaderRight
        );

        PropToolsEditorDrawing.Border(rect, PropToolsEditorTheme.BorderStrong);
        PropToolsEditorDrawing.AccentBar(rect, PropToolsEditorTheme.Accent, 5f);

        float textX = rect.x + 16f;
        Texture2D logo = HeaderLogo;

        if (logo != null)
        {
            Rect logoRect = new Rect(
                rect.x + 14f,
                rect.y + (rect.height - logoSize) * 0.5f,
                logoSize,
                logoSize
            );

            GUI.DrawTexture(logoRect, logo, ScaleMode.ScaleToFit, true);
            textX = logoRect.xMax + 12f;
        }

        float actualTextWidth = Mathf.Max(0f, rect.xMax - textX - 14f);
        float textTop = rect.y + (rect.height - textBlockHeight) * 0.5f;

        GUI.Label(
            new Rect(textX, textTop, actualTextWidth, titleHeight),
            title,
            PropToolsEditorStyles.HeaderTitle
        );

        if (hasSubtitle)
        {
            float actualSubtitleHeight = Mathf.Max(
                subtitleHeight,
                PropToolsEditorStyles.HeaderSubtitle.CalcHeight(
                    new GUIContent(subtitle),
                    Mathf.Max(0f, actualTextWidth - descriptionPaddingX * 2f)
                )
            );
            Rect descriptionRect = new Rect(
                textX,
                textTop + titleHeight + 5f,
                actualTextWidth,
                actualSubtitleHeight + descriptionPaddingY * 2f
            );

            if (Event.current.type == EventType.Repaint)
            {
                PropToolsEditorDrawing.RoundedRect(
                    descriptionRect,
                    PropToolsEditorTheme.HeaderDescription,
                    PropToolsEditorTheme.BorderSoft,
                    3f
                );
            }

            GUI.Label(
                new Rect(
                    descriptionRect.x + descriptionPaddingX,
                    descriptionRect.y + descriptionPaddingY,
                    Mathf.Max(0f, descriptionRect.width - descriptionPaddingX * 2f),
                    actualSubtitleHeight
                ),
                new GUIContent(subtitle, subtitle),
                PropToolsEditorStyles.HeaderSubtitle
            );
        }

        Space(PropToolsEditorSpacing.Medium);
    }

    private static Texture2D HeaderLogo
    {
        get
        {
            if (headerLogo != null)
                return headerLogo;

            string assetPath = AssetDatabase.GUIDToAssetPath(PropToolsEditorTheme.HeaderLogoGuid);
            if (string.IsNullOrEmpty(assetPath))
                assetPath = PropToolsEditorTheme.HeaderLogoPath;

            headerLogo = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            return headerLogo;
        }
    }

    public static void Module(
        ref bool expanded,
        string title,
        bool enabled,
        string subtitle,
        bool selected,
        Action content)
    {
        BeginModuleShell();

        expanded = PropToolsEditorFoldouts.FoldoutHeader(
            title,
            expanded,
            enabled,
            subtitle,
            selected,
            toggleProperty: null
        );

        if (expanded && enabled)
            ModuleBody(content);

        EndModuleShell();
    }

    public static void Module(
        ref bool expanded,
        string title,
        SerializedProperty enabledProperty,
        string subtitle,
        bool selected,
        Action content)
    {
        BeginModuleShell();

        bool enabled = enabledProperty == null || enabledProperty.boolValue;

        expanded = PropToolsEditorFoldouts.FoldoutHeader(
            title,
            expanded,
            enabled,
            subtitle,
            selected,
            enabledProperty
        );

        if (expanded && (enabledProperty == null || enabledProperty.boolValue))
            ModuleBody(content);

        EndModuleShell();
    }

    public static void Group(string title, Action content)
    {
        bool expanded = true;
        expanded = SmallFoldout(title, fallback: true);

        if (!expanded)
            return;

        NestedBody(content);
    }

    public static void GroupCard(string title, Action content)
    {
        bool expanded = true;
        expanded = SmallFoldout(title, fallback: true);

        if (!expanded)
            return;

        NestedBody(content);
    }

    public static void Section(string title, string subtitle = null)
    {
        Space(PropToolsEditorSpacing.Large);

        Rect rect = GUILayoutUtility.GetRect(
            0,
            string.IsNullOrWhiteSpace(subtitle) ? 34f : 42f,
            GUILayout.ExpandWidth(true)
        );

        PropToolsEditorDrawing.RoundedRect(
            rect,
            Color.Lerp(
                PropToolsEditorTheme.AccentDark,
                PropToolsEditorTheme.ModuleRight,
                0.58f
            ),
            PropToolsEditorTheme.BorderStrong,
            PropToolsEditorSpacing.ModuleCornerRadius,
            true
        );
        PropToolsEditorDrawing.RoundedAccentBar(
            rect,
            PropToolsEditorTheme.Accent,
            5f,
            PropToolsEditorSpacing.AccentVerticalInset
        );

        GUI.Label(
            new Rect(rect.x + 13f, rect.y + 5f, rect.width - 26f, 18f),
            title,
            PropToolsEditorStyles.SectionTitle
        );

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            GUI.Label(
                new Rect(rect.x + 13f, rect.y + 24f, rect.width - 26f, 14f),
                subtitle,
                PropToolsEditorStyles.SectionSubtitle
            );
        }
    }

    public static void Card(Action content)
    {
        PanelBody(content, ModuleBodyStyle, drawBorder: true);
    }

    public static bool ItemCard(
        string stateKey,
        string badge,
        string title,
        string summary,
        bool defaultExpanded,
        Action content,
        string titleTooltip = null,
        string badgeTooltip = null)
    {
        string key = BuildGroupStateKey("ItemCard." + stateKey);
        bool expanded = LoadGroupState(key, defaultExpanded);
        bool removeRequested = false;

        EditorGUILayout.BeginVertical(ItemShellStyle);

        Rect header = GUILayoutUtility.GetRect(0, 34f, GUILayout.ExpandWidth(true));
        bool hover = header.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint)
        {
            PropToolsEditorDrawing.RoundedRect(
                header,
                hover ? PropToolsEditorTheme.ItemHeaderHover : PropToolsEditorTheme.ItemHeader,
                expanded || hover ? PropToolsEditorTheme.Border : PropToolsEditorTheme.BorderSoft,
                4f,
                true
            );
            PropToolsEditorDrawing.RoundedAccentBar(
                header,
                expanded ? PropToolsEditorTheme.Accent : PropToolsEditorTheme.TextDim,
                3f,
                PropToolsEditorSpacing.AccentVerticalInset
            );
        }

        Rect arrowRect = new Rect(header.x + 8f, header.y + 10f, 12f, 12f);
        PropToolsEditorDrawing.Triangle(
            arrowRect,
            expanded,
            hover ? PropToolsEditorTheme.Text : PropToolsEditorTheme.TextMuted
        );

        float badgeWidth = Mathf.Clamp(
            PropToolsEditorStyles.ItemBadge.CalcSize(new GUIContent(badge)).x + 14f,
            52f,
            82f
        );
        Rect badgeRect = new Rect(header.x + 27f, header.y + 8f, badgeWidth, 18f);

        if (Event.current.type == EventType.Repaint)
        {
            PropToolsEditorDrawing.RoundedRect(
                badgeRect,
                PropToolsEditorTheme.ItemBadge,
                PropToolsEditorTheme.ItemBadgeBorder,
                9f
            );
        }

        GUI.Label(
            badgeRect,
            new GUIContent(
                badge,
                string.IsNullOrWhiteSpace(badgeTooltip)
                    ? "Identifies the kind of generated object represented by this card."
                    : badgeTooltip
            ),
            PropToolsEditorStyles.ItemBadge
        );

        Rect removeRect = new Rect(header.xMax - 66f, header.y + 7f, 58f, 20f);
        removeRequested = PropToolsEditorControls.QuietDangerButton(removeRect, "Remove");

        float titleX = badgeRect.xMax + 9f;
        float contentRight = removeRect.x - 8f;
        float available = Mathf.Max(0f, contentRight - titleX);
        float summaryWidth = available >= 220f
            ? Mathf.Min(120f, available * 0.42f)
            : 0f;
        Rect titleRect = new Rect(
            titleX,
            header.y + 7f,
            Mathf.Max(0f, available - summaryWidth - (summaryWidth > 0f ? 8f : 0f)),
            20f
        );
        GUI.Label(
            titleRect,
            new GUIContent(
                title,
                string.IsNullOrWhiteSpace(titleTooltip)
                    ? "Name of this generated object. Expand the card to edit its settings."
                    : titleTooltip
            ),
            PropToolsEditorStyles.ItemTitle
        );

        if (summaryWidth > 0f && !string.IsNullOrWhiteSpace(summary))
        {
            Rect summaryRect = new Rect(
                contentRight - summaryWidth,
                header.y + 8f,
                summaryWidth,
                18f
            );
            GUI.Label(summaryRect, new GUIContent(summary, summary), PropToolsEditorStyles.ItemSummary);
        }

        EditorGUIUtility.AddCursorRect(header, MouseCursor.Link);

        if (!removeRequested &&
            Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&
            header.Contains(Event.current.mousePosition) &&
            !removeRect.Contains(Event.current.mousePosition))
        {
            expanded = !expanded;
            SaveGroupState(key, expanded);
            Event.current.Use();
            GUI.changed = true;
        }

        if (expanded && !removeRequested)
        {
            Rect body = EditorGUILayout.BeginVertical(ItemBodyStyle);

            if (Event.current.type == EventType.Repaint)
            {
                PropToolsEditorDrawing.RoundedRect(
                    body,
                    PropToolsEditorTheme.ItemBody,
                    PropToolsEditorTheme.BorderSoft,
                    3f
                );
            }

            content?.Invoke();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
        Space(PropToolsEditorSpacing.Tiny);
        return removeRequested;
    }

    public static bool FeatureCard(
        string title,
        string tooltip,
        bool added,
        bool highlighted = false)
    {
        Rect row = GUILayoutUtility.GetRect(0, 32f, GUILayout.ExpandWidth(true));
        bool hover = row.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint)
        {
            Color fill = highlighted
                ? new Color(
                    PropToolsEditorTheme.Accent.r,
                    PropToolsEditorTheme.Accent.g,
                    PropToolsEditorTheme.Accent.b,
                    0.12f
                )
                : hover
                    ? PropToolsEditorTheme.ItemHeaderHover
                    : PropToolsEditorTheme.ItemHeader;

            PropToolsEditorDrawing.RoundedRect(
                row,
                fill,
                highlighted ? PropToolsEditorTheme.BorderStrong : PropToolsEditorTheme.BorderSoft,
                4f
            );
            PropToolsEditorDrawing.RoundedAccentBar(
                row,
                added ? PropToolsEditorTheme.Accent : PropToolsEditorTheme.TextDim,
                3f,
                PropToolsEditorSpacing.AccentVerticalInset
            );
        }

        Rect buttonRect = new Rect(row.xMax - 66f, row.y + 5f, 58f, 22f);
        Rect labelRect = new Rect(
            row.x + 12f,
            row.y + 6f,
            Mathf.Max(0f, buttonRect.x - row.x - 20f),
            20f
        );

        GUI.Label(
            labelRect,
            new GUIContent(title, tooltip),
            PropToolsEditorStyles.ItemTitle
        );

        bool clicked = PropToolsEditorControls.MiniButton(
            buttonRect,
            added ? "Remove" : "+ Add"
        );

        Space(PropToolsEditorSpacing.Tiny);
        return clicked;
    }

    public static bool ActionPanel(string title, string description, string buttonText)
    {
        Space(PropToolsEditorSpacing.Medium);
        Rect panel = EditorGUILayout.BeginVertical(ActionPanelStyle);

        if (Event.current.type == EventType.Repaint)
        {
            PropToolsEditorDrawing.RoundedRect(
                panel,
                PropToolsEditorTheme.PanelTop,
                PropToolsEditorTheme.BorderStrong,
                4f,
                true
            );
            PropToolsEditorDrawing.RoundedAccentBar(
                panel,
                PropToolsEditorTheme.Accent,
                4f,
                PropToolsEditorSpacing.AccentVerticalInset
            );
        }

        GUILayout.Label(title, PropToolsEditorStyles.ActionTitle);
        GUILayout.Label(description, PropToolsEditorStyles.ActionDescription);
        GUILayout.Space(PropToolsEditorSpacing.Small);
        bool clicked = PropToolsEditorControls.PrimaryButton(buttonText, 34f);

        EditorGUILayout.EndVertical();
        return clicked;
    }

    public static bool ActionFooter(
        string title,
        string status,
        string buttonText,
        MessageType statusType = MessageType.None)
    {
        Rect panel = EditorGUILayout.BeginHorizontal(
            ActionFooterStyle,
            GUILayout.Height(48f),
            GUILayout.ExpandWidth(true)
        );

        if (Event.current.type == EventType.Repaint)
        {
            PropToolsEditorDrawing.RoundedRect(
                panel,
                PropToolsEditorTheme.FloatingDock,
                PropToolsEditorTheme.FloatingDockBorder,
                8f,
                true
            );
            PropToolsEditorDrawing.RoundedAccentBar(
                panel,
                PropToolsEditorTheme.FloatingDockAccent,
                8f,
                PropToolsEditorSpacing.AccentVerticalInset
            );

            if (statusType != MessageType.None)
            {
                Color statusColor = StatusColor(statusType);
                Rect statusMarker = new Rect(
                    panel.x + 12f,
                    panel.yMax - 4f,
                    28f,
                    2f
                );
                PropToolsEditorDrawing.RoundedRect(
                    statusMarker,
                    statusColor,
                    statusColor,
                    1f
                );
            }
        }

        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        GUILayout.Label(title, PropToolsEditorStyles.ActionTitle);
        GUILayout.Label(
            status,
            PropToolsEditorStyles.ActionDescription,
            GUILayout.MinHeight(16f)
        );
        EditorGUILayout.EndVertical();

        GUILayout.Space(PropToolsEditorSpacing.Small);
        float buttonWidth = Mathf.Clamp(
            EditorGUIUtility.currentViewWidth * 0.18f,
            132f,
            190f
        );
        EditorGUILayout.BeginVertical(
            GUILayout.Width(buttonWidth),
            GUILayout.ExpandHeight(true)
        );
        GUILayout.FlexibleSpace();
        bool clicked = PropToolsEditorControls.PrimaryButton(buttonText, 28f);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        return clicked;
    }

    public static void SubHeader(string text)
    {
        DrawInlineHeader(text);
    }

    public static void Space(float height)
    {
        Rect rect = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
        DrawBackground(rect);
    }

    private static void BeginModuleShell()
    {
        EditorGUILayout.BeginVertical(ModuleShellStyle);
    }

    private static Color StatusColor(MessageType statusType)
    {
        switch (statusType)
        {
            case MessageType.Error:
                return PropToolsEditorTheme.Error;
            case MessageType.Warning:
                return PropToolsEditorTheme.Warning;
            case MessageType.Info:
                return PropToolsEditorTheme.InfoAccent;
            default:
                return PropToolsEditorTheme.Accent;
        }
    }

    private static void EndModuleShell()
    {
        EditorGUILayout.EndVertical();
    }

    private static void ModuleBody(Action content)
    {
        PanelBody(content, ModuleBodyStyle, drawBorder: true);
    }

    private static void NestedBody(Action content)
    {
        PanelBody(content, NestedBodyStyle, drawBorder: false);
    }

    private static void PanelBody(Action content, GUIStyle style, bool drawBorder)
    {
        Rect rect = EditorGUILayout.BeginVertical(style);

        if (Event.current.type == EventType.Repaint)
        {
            PropToolsEditorDrawing.RoundedRect(
                rect,
                Color.Lerp(
                    PropToolsEditorTheme.PanelTop,
                    PropToolsEditorTheme.PanelBottom,
                    0.42f
                ),
                drawBorder
                    ? PropToolsEditorTheme.Border
                    : PropToolsEditorTheme.BorderSoft,
                drawBorder
                    ? PropToolsEditorSpacing.ModuleCornerRadius
                    : PropToolsEditorSpacing.NestedCornerRadius,
                drawBorder
            );
        }

        content?.Invoke();

        EditorGUILayout.EndVertical();
    }

    private static bool SmallFoldout(string title, bool fallback)
    {
        string key = BuildGroupStateKey(title);
        bool expanded = LoadGroupState(key, fallback);

        Rect rect = GUILayoutUtility.GetRect(0, 27f, GUILayout.ExpandWidth(true));
        bool hover = rect.Contains(Event.current.mousePosition);

        Color left = expanded
            ? new Color(0.245f, 0.125f, 0.060f, 1f)
            : hover
                ? new Color(0.205f, 0.115f, 0.065f, 1f)
                : new Color(0.150f, 0.105f, 0.078f, 1f);

        PropToolsEditorDrawing.RoundedRect(
            rect,
            Color.Lerp(left, PropToolsEditorTheme.BackgroundDark, 0.52f),
            hover
                ? PropToolsEditorTheme.Border
                : PropToolsEditorTheme.BorderSoft,
            PropToolsEditorSpacing.NestedCornerRadius,
            hover
        );
        PropToolsEditorDrawing.RoundedAccentBar(
            rect,
            PropToolsEditorTheme.Accent,
            4f,
            PropToolsEditorSpacing.AccentVerticalInset
        );

        Rect arrowRect = new Rect(rect.x + 9f, rect.y + 5f, 16f, 16f);
        Rect labelRect = new Rect(rect.x + 32f, rect.y + 4f, rect.width - 42f, 18f);

        GUI.Label(arrowRect, expanded ? "▼" : "▶", PropToolsEditorStyles.FoldoutArrow);

        GUI.Label(
            labelRect,
            PropToolsEditorTooltips.Content(title),
            SmallFoldoutLabelStyle
        );

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&
            rect.Contains(Event.current.mousePosition))
        {
            expanded = !expanded;
            SaveGroupState(key, expanded);

            Event.current.Use();
            GUI.changed = true;
        }

        return expanded;
    }

    private static void DrawInlineHeader(string text)
    {
        Rect rect = GUILayoutUtility.GetRect(0, 22f, GUILayout.ExpandWidth(true));

        if (Event.current.type == EventType.Repaint)
        {
            Rect plate = new Rect(rect.x, rect.y + 3f, Mathf.Min(rect.width, 170f), 17f);
            PropToolsEditorDrawing.RoundedRect(
                plate,
                PropToolsEditorTheme.PanelHeader,
                PropToolsEditorTheme.BorderSoft,
                PropToolsEditorSpacing.NestedCornerRadius
            );
            PropToolsEditorDrawing.RoundedAccentBar(
                plate,
                PropToolsEditorTheme.Accent,
                4f,
                3f
            );
            PropToolsEditorDrawing.BottomLine(rect, PropToolsEditorTheme.BorderSoft);
        }

        GUI.Label(
            new Rect(rect.x + 10f, rect.y + 3f, rect.width - 20f, 18f),
            PropToolsEditorTooltips.Content(text),
            PropToolsEditorStyles.SubHeader
        );
    }

    private static void DrawBackground(Rect rect)
    {
        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(rect, PropToolsEditorTheme.Background);
    }

    private static GUIStyle SmallFoldoutLabelStyle =>
        smallFoldoutLabelStyle ?? (smallFoldoutLabelStyle =
            new GUIStyle(PropToolsEditorStyles.SubHeader)
            {
                fontSize = 11
            });

    private static GUIStyle ModuleShellStyle
    {
        get
        {
            if (moduleShellStyle == null)
            {
                moduleShellStyle = new GUIStyle
                {
                    margin = new RectOffset(0, 0, 0, 6),
                    padding = new RectOffset(0, 0, 0, 0),
                    normal =
                    {
                        background = PropToolsEditorDrawing.Texture(PropToolsEditorTheme.Background)
                    }
                };
            }

            return moduleShellStyle;
        }
    }

    private static GUIStyle ModuleBodyStyle
    {
        get
        {
            if (moduleBodyStyle == null)
            {
                moduleBodyStyle = new GUIStyle
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(
                        Mathf.RoundToInt(PropToolsEditorSpacing.CardPaddingX),
                        Mathf.RoundToInt(PropToolsEditorSpacing.CardPaddingX),
                        Mathf.RoundToInt(PropToolsEditorSpacing.CardPaddingY),
                        Mathf.RoundToInt(PropToolsEditorSpacing.CardPaddingY)
                    )
                };
            }

            return moduleBodyStyle;
        }
    }

    private static GUIStyle NestedBodyStyle
    {
        get
        {
            if (nestedBodyStyle == null)
            {
                nestedBodyStyle = new GUIStyle
                {
                    margin = new RectOffset(0, 0, 0, 6),
                    padding = new RectOffset(9, 9, 8, 8)
                };
            }

            return nestedBodyStyle;
        }
    }

    private static GUIStyle ItemShellStyle
    {
        get
        {
            if (itemShellStyle == null)
            {
                itemShellStyle = new GUIStyle
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }

            return itemShellStyle;
        }
    }

    private static GUIStyle ItemBodyStyle
    {
        get
        {
            if (itemBodyStyle == null)
            {
                itemBodyStyle = new GUIStyle
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(10, 10, 9, 9)
                };
            }

            return itemBodyStyle;
        }
    }

    private static GUIStyle ActionPanelStyle
    {
        get
        {
            if (actionPanelStyle == null)
            {
                actionPanelStyle = new GUIStyle
                {
                    margin = new RectOffset(0, 0, 4, 8),
                    padding = new RectOffset(14, 14, 12, 12)
                };
            }

            return actionPanelStyle;
        }
    }

    private static GUIStyle ActionFooterStyle
    {
        get
        {
            if (actionFooterStyle == null)
            {
                actionFooterStyle = new GUIStyle
                {
                    margin = new RectOffset(8, 8, 3, 5),
                    padding = new RectOffset(12, 10, 4, 4)
                };
            }

            return actionFooterStyle;
        }
    }

    private static string BuildGroupStateKey(string title)
    {
        string objectKey = "NoObject";

        if (Selection.activeObject != null)
        {
            try
            {
                objectKey = GlobalObjectId.GetGlobalObjectIdSlow(Selection.activeObject).ToString();
            }
            catch
            {
                objectKey = Selection.activeObject.GetInstanceID().ToString();
            }
        }

        return "PropToolsEditor.GroupFoldout." + CleanKeyPart(objectKey) + "." + CleanKeyPart(title);
    }

    private static string CleanKeyPart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Empty";

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

    private static bool LoadGroupState(string key, bool fallback)
    {
        if (SessionState.GetBool(key + ".known", false))
            return SessionState.GetBool(key, fallback);

        bool value = EditorPrefs.GetBool(key, fallback);

        SessionState.SetBool(key + ".known", true);
        SessionState.SetBool(key, value);

        return value;
    }

    private static void SaveGroupState(string key, bool value)
    {
        SessionState.SetBool(key + ".known", true);
        SessionState.SetBool(key, value);

        EditorPrefs.SetBool(key, value);
    }
}
}
