namespace Wolfy.PropTools.EditorUI
{
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorTooltips
{
    private const float TooltipMargin = 6f;
    private const float TooltipOffsetX = 14f;
    private const float TooltipOffsetY = 18f;
    private const float MinimumTooltipWidth = 150f;
    private const float PreferredMaximumTooltipWidth = 360f;

    private static UnityEngine.Object themedOwner;
    private static string themedTooltip;
    private static GUIStyle themedTooltipStyle;
    private static GUIStyle themedTooltipMeasureStyle;

    public readonly struct ThemedScope : IDisposable
    {
        private readonly UnityEngine.Object owner;
        private readonly UnityEngine.Object previousOwner;
        private readonly string previousTooltip;

        internal ThemedScope(
            UnityEngine.Object owner,
            UnityEngine.Object previousOwner,
            string previousTooltip)
        {
            this.owner = owner;
            this.previousOwner = previousOwner;
            this.previousTooltip = previousTooltip;
        }

        public void Dispose()
        {
            if (themedOwner == owner)
                DrawThemedOverlay(owner);

            themedOwner = previousOwner;
            themedTooltip = previousTooltip;
        }
    }

    private static readonly Dictionary<string, string> descriptions =
        new Dictionary<string, string>
        {
            { "Root / Prefab Name", "Names the root object and the prefab created from this setup." },
            { "Create Standard Prefab Hierarchy", "Creates SCALE ME, Prefab Container, and any required setup folders. Turn this off to use an existing hierarchy instead." },
            { "Scale Object Name", "Names the object users scale to resize the complete prop." },
            { "Prop Container Name", "Names the object that holds the visible and functional parts of the prop." },
            { "Target Setup Name", "Names the object that contains generated constraint and attachment targets." },
            { "Add Parent Constraint To Prop Container", "Adds a parent constraint whose sources are filled from the generated targets." },

            { "World Drop Type", "Chooses a standard World Drop or a synced version that supports late joiners." },
            { "Placement", "Chooses the prefab variant intended for this prop's placement or handedness." },
            { "Use Scaled Prefab", "Uses the World Drop prefab variant designed to work with a scalable prop hierarchy." },
            { "Prefab Override", "Optionally use a specific World Drop prefab instead of the automatically selected one. Leave this empty for automatic selection." },

            { "Parent Folder", "Existing project folder that will contain the new asset folder." },
            { "Asset Folder Name", "Names the new top-level folder created for this asset." },
            { "Animations Folder Name", "Names the required root folder for animation files and related folders." },
            { "Models / FBXs Folder Name", "Names the required root folder for models and FBX source files." },
            { "Prefabs Folder Name", "Names the required root folder for finished and working prefabs." },
            { "Menu Prefabs Folder Name", "Names the baseline Prefabs child used for prefabs referenced by VRChat expression menus." },
            { "Textures & Materials Folder Name", "Names the required root folder for textures, materials, and shader outputs." },
            { "Project Folder Name", "Names this movable project folder." },
            { "Folder Hierarchy", "Editable project-folder tree. Drop on a folder center to nest it, or on a custom row edge to reorder siblings." },
            { "Starter Assets Folder", "Chooses which folder receives the optional controller and VRC expression assets." },
            { "Animator Controller Name", "Names the empty starter Animator Controller. Use {Asset Name} to insert the asset folder name." },
            { "Expression Parameters Name", "Names the empty VRC expression-parameters asset. Use {Asset Name} to insert the asset folder name." },
            { "Expressions Menu Name", "Names the empty VRC expressions-menu asset. Use {Asset Name} to insert the asset folder name." },
            { "Asset Folder Setup", "Creates a reusable project folder layout and optional starter controller and VRC expression assets." },
            { "+ Add Folder", "Adds a custom folder beneath the asset root. Drag its grip to nest or reorder it." },
            { "Generate Asset Folders", "Creates missing configured folders and starter assets. Existing project files are not moved, replaced, or deleted." },
            { "Build Asset Folders", "Creates missing configured folders and starter assets. Existing project files are not moved, replaced, or deleted." },

            { "Folder Name", "Names the hierarchy folder that contains the generated raycast setup." },
            { "Raycaster Name", "Names the constrained object that holds the raycast component." },
            { "Look At Anchor Name", "Names the anchor used to keep supported World Drop props facing the user." },
            { "Raycaster Template", "Optional prefab copied into the generated raycaster. Leave empty to create a basic holder." },
            { "Remove Unused Raycast Targets", "Deletes previously generated raycast targets that no longer have a card here." },
            { "Configure World Drop Facing", "Connects the raycast anchor to a supported World Drop setup so the placed prop can face the user." },
            { "Facing Yaw", "Sets the resting horizontal facing angle for the World Drop look-at setup." },

            { "Generated Target Prefix", "Text placed before every generated target name, usually the prop or product name." },
            { "Source Side Label", "Short label added to targets on the source side, such as R." },
            { "Mirrored Side Label", "Short label added to targets on the opposite side, such as L." },
            { "Source Folder Name", "Names the folder containing source-side generated targets." },
            { "Mirrored Folder Name", "Names the folder containing automatically mirrored targets." },
            { "Remove Unused Generated Targets", "Deletes previously generated targets that no longer have a target card here." },
            { "Target Name", "The descriptive part of this generated object's name." },
            { "Source Bone", "Avatar bone the source-side target follows through its armature link." },
            { "Opposite Bone", "Avatar bone the mirrored target follows through its armature link." },
            { "Attach To Bone", "Avatar bone this generated raycast target follows." },
            { "Rotation Offset", "Adds a local rotation adjustment after mirroring or linking the target." },
            { "Target Options", "Opens less frequently changed naming and mirroring settings for this target." },
            { "Use Global Side Labels", "Uses the shared R/L-style labels above instead of labels unique to this target." },
            { "Source Label", "Custom side label used only by this target." },
            { "Opposite Label", "Custom mirrored-side label used only by this target." },

            { "Default Position", "Local position assigned when a target is first generated." },
            { "Default Rotation", "Local rotation assigned when a target is first generated." },
            { "Default Scale", "Local scale assigned when a target is first generated." },
            { "Local Position", "Local position used for generated targets." },
            { "Local Euler Rotation", "Local rotation used for generated targets, shown as X, Y, and Z angles." },
            { "Local Scale", "Local scale used for generated targets." },
            { "Apply Defaults To Existing Targets", "Reapplies these transform defaults when regenerating targets that already exist." },
            { "Apply Default Transform To Existing Targets", "Reapplies these transform defaults when regenerating targets that already exist." },
            { "Create Editor-Only Preview Helper", "Creates a helper object for editor previews that removes itself from the uploaded avatar." },
            { "Create Destroyed On Upload Child", "Creates a preview helper that is excluded from the uploaded avatar." },
            { "Preview Helper Name", "Names the editor-only preview helper object." },
            { "Destroyed On Upload Name", "Names the helper object that is excluded from upload." },
            { "Copy Preview Gizmos", "Copies useful scene gizmos from the preview source onto the helper." },
            { "Copy Preview Source Gizmos", "Copies useful scene gizmos from the preview source onto the helper." },

            { "Generate Mirrored Target Pairs", "Generates both source and opposite-side targets and connects them to Live Mirroring." },
            { "Live Mirroring Object Name", "Names the editor-only object that manages mirrored target placement." },
            { "Generate Scale Reference Object", "Creates a shared object that lets users scale the prop and its target spacing together." },
            { "Scale Reference Object Name", "Names the generated scale-reference object." },
            { "Scale Reference", "Object whose scale is used when previewing or applying grouped target spacing." },

            { "Auto Assign Preview Settings", "Automatically fills preview references from the generated prefab setup." },
            { "Show Scene Preview", "Shows editor-only ghost previews of mirrored objects in the Scene view." },
            { "Preview Source", "The object used as the visible source for scene-preview ghosts." },
            { "Preview Material", "Material used to draw scene-preview ghosts without changing the prop's real materials." },

            { "Add Full Controller To Root", "Adds a VRCFury Full Controller component to the generated prop root." },
            { "Add VRCFury Armature Links", "Adds VRCFury Armature Link components so generated targets follow their selected avatar bones." },

            { "Enable Live Mirroring", "Updates mirrored targets live in the editor while this system is active." },
            { "Mirror Root", "Transform whose local space defines the center and direction of the mirror." },
            { "Apply Scale Reference", "Includes the scale-reference object's scale when calculating mirrored placement." },
            { "Mirror Position", "Mirrors the target's position across the selected axis." },
            { "Mirror Rotation", "Mirrors the target's rotation across the selected axis." },
            { "Mirror Scale", "Mirrors the target's scale across the selected axis." },
            { "Mirror Axis", "Axis used as the left-to-right mirror plane." },
            { "Mirror Enabled", "Allows this pair to update through Live Mirroring." },
            { "Pair Name", "Friendly name used to identify this mirroring pair in the inspector." },
            { "Source Target", "Object whose transform drives this mirroring pair." },
            { "Mirrored Target", "Object updated as the mirrored result of the source target." },

            { "Target Naming & Organization", "Controls generated target names, hierarchy folders, and individual target definitions." },
            { "Generated Targets", "Targets that will be created when the prefab setup is generated or updated." },
            { "Raycast", "Creates reusable avatar-relative locations and a constrained raycaster object." },
            { "World Drop", "Builds the selected World Drop prefab structure into the generated prop." },
            { "Highlights", "Highlights the Builder settings controlled by the hierarchy entry under the mouse. Turn this off to keep only the hierarchy row hover." },
            { "Hierarchy Preview", "Preview of the hierarchy and components the current settings will generate." },
            { "Target Defaults & Preview Helpers", "Default target transforms and editor-only preview helper settings." },
            { "Live Mirroring", "Settings for generating and previewing opposite-side target pairs." },
            { "Scene Preview", "Controls editor-only ghost previews for placement and mirroring." },
            { "VRCFury Integration", "Optional VRCFury components added during prop generation." },
            { "Mirror Pairs", "Source and mirrored objects managed together by the Live Mirroring system." },
            { "+ Add Target", "Adds another generated target definition." },
            { "+ Add Pair", "Adds another source and mirrored target pair." },
            { "+ Add", "Adds this optional feature to the generator." },
            { "Remove", "Removes this item or optional feature from the current configuration." },
            { "Generate Prop Setup", "Builds or updates the prop hierarchy using the current settings." },
            { "Generate Targets", "Creates or updates targets using the current definitions." },
            { "Build Prop Setup", "Builds or updates the prop hierarchy using the current settings." },
            { "Build Targets", "Creates or updates targets using the current definitions." }
        };

    public static string Get(string label, SerializedProperty property = null)
    {
        if (!string.IsNullOrWhiteSpace(label) && descriptions.TryGetValue(label, out string description))
            return description;

        return property != null && !string.IsNullOrWhiteSpace(property.tooltip)
            ? property.tooltip
            : string.Empty;
    }

    public static ThemedScope PushThemedScope(UnityEngine.Object owner)
    {
        ThemedScope scope = new ThemedScope(
            owner,
            themedOwner,
            themedTooltip
        );
        themedOwner = owner;
        themedTooltip = null;
        return scope;
    }

    public static void Track(
        Rect rect,
        string label,
        SerializedProperty property = null)
    {
        Track(rect, Get(label, property));
    }

    public static void Track(Rect rect, string tooltip)
    {
        if (themedOwner == null ||
            string.IsNullOrWhiteSpace(tooltip) ||
            !rect.Contains(Event.current.mousePosition))
        {
            return;
        }

        themedTooltip = tooltip.Trim();

        if (Event.current.type == EventType.MouseMove)
        {
            if (themedOwner is EditorWindow window)
                window.Repaint();
            else if (themedOwner is Editor editor)
                editor.Repaint();
        }
    }

    public static GUIContent Content(string label, SerializedProperty property = null) =>
        new GUIContent(
            label,
            themedOwner == null ? Get(label, property) : string.Empty
        );

    public static GUIContent Content(string label, string tooltip) =>
        new GUIContent(
            label,
            themedOwner == null ? tooltip ?? string.Empty : string.Empty
        );

    public static GUIContent Content(GUIContent source)
    {
        if (source == null)
            return GUIContent.none;

        return new GUIContent(
            source.text,
            source.image,
            themedOwner == null ? source.tooltip : string.Empty
        );
    }

    public static void Track(Rect rect, GUIContent content)
    {
        if (content != null)
            Track(rect, content.tooltip);
    }

    private static void DrawThemedOverlay(UnityEngine.Object owner)
    {
        if (Event.current.type != EventType.Repaint ||
            string.IsNullOrWhiteSpace(themedTooltip))
        {
            return;
        }

        float windowWidth;
        float windowHeight;

        if (owner is EditorWindow window)
        {
            windowWidth = window.position.width;
            windowHeight = window.position.height;
        }
        else if (owner is Editor)
        {
            windowWidth = EditorGUIUtility.currentViewWidth;
            EditorWindow hostWindow = EditorWindow.mouseOverWindow;
            windowHeight = hostWindow != null
                ? hostWindow.position.height
                : Screen.currentResolution.height;
        }
        else
        {
            return;
        }

        windowWidth = Mathf.Max(1f, windowWidth);
        windowHeight = Mathf.Max(1f, windowHeight);
        float usableWidth = Mathf.Max(
            1f,
            windowWidth - (TooltipMargin * 2f)
        );
        float usableHeight = Mathf.Max(
            1f,
            windowHeight - (TooltipMargin * 2f)
        );

        GUIContent content = new GUIContent(themedTooltip);
        Vector2 measured = ThemedTooltipMeasureStyle.CalcSize(content);
        float minimumWidth = Mathf.Min(
            MinimumTooltipWidth,
            usableWidth
        );
        float preferredMaximumWidth = Mathf.Min(
            PreferredMaximumTooltipWidth,
            usableWidth
        );
        float width = Mathf.Clamp(
            Mathf.Ceil(
                measured.x +
                ThemedTooltipStyle.padding.horizontal
            ),
            minimumWidth,
            preferredMaximumWidth
        );
        float height = CalculateTooltipHeight(content, width);

        if (height > usableHeight && width < usableWidth)
        {
            width = usableWidth;
            height = CalculateTooltipHeight(content, width);
        }

        Vector2 mouse = Event.current.mousePosition;
        float x = mouse.x + TooltipOffsetX;
        float y = mouse.y + TooltipOffsetY;

        if (x + width > windowWidth - TooltipMargin)
            x = mouse.x - width - TooltipOffsetX;

        if (y + height > windowHeight - TooltipMargin)
            y = mouse.y - height - TooltipOffsetY;

        Rect card = new Rect(
            Mathf.Clamp(
                x,
                TooltipMargin,
                Mathf.Max(
                    TooltipMargin,
                    windowWidth - width - TooltipMargin
                )
            ),
            Mathf.Clamp(
                y,
                TooltipMargin,
                Mathf.Max(
                    TooltipMargin,
                    windowHeight - height - TooltipMargin
                )
            ),
            width,
            height
        );

        PropToolsEditorDrawing.RoundedRect(
            new Rect(card.x + 2f, card.y + 2f, card.width, card.height),
            PropToolsEditorTheme.BackgroundDark,
            PropToolsEditorTheme.BackgroundDark,
            4f
        );
        PropToolsEditorDrawing.RoundedRect(
            card,
            PropToolsEditorTheme.Panel,
            PropToolsEditorTheme.BorderStrong,
            4f
        );
        GUI.Label(card, content, ThemedTooltipStyle);
    }

    private static float CalculateTooltipHeight(
        GUIContent content,
        float width) =>
        Mathf.Max(
            EditorGUIUtility.singleLineHeight +
            ThemedTooltipStyle.padding.vertical,
            Mathf.Ceil(
                ThemedTooltipStyle.CalcHeight(
                    content,
                    Mathf.Max(1f, width)
                )
            )
        );

    private static GUIStyle ThemedTooltipStyle =>
        themedTooltipStyle ?? (themedTooltipStyle =
            new GUIStyle(PropToolsEditorStyles.Label)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                padding = new RectOffset(8, 8, 7, 7)
            });

    private static GUIStyle ThemedTooltipMeasureStyle =>
        themedTooltipMeasureStyle ?? (themedTooltipMeasureStyle =
            new GUIStyle(PropToolsEditorStyles.Label)
            {
                wordWrap = false,
                padding = new RectOffset(0, 0, 0, 0)
            });

}
}
