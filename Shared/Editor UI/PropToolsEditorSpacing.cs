namespace Wolfy.PropTools.EditorUI
{
using UnityEngine;

public static class PropToolsEditorSpacing
{
    public const float Tiny = 2f;
    public const float Small = 5f;
    public const float Medium = 8f;
    public const float Large = 12f;
    public const float Section = 16f;

    public const float HeaderHeight = 64f;
    public const float FoldoutHeight = 34f;
    public const float FoldoutHeightWithSubtitle = 44f;

    public const float PanelHeaderHeight = 24f;
    public const float CardPaddingX = 10f;
    public const float CardPaddingY = 8f;
    public const float ModuleCornerRadius = 5f;
    public const float NestedCornerRadius = 4f;
    public const float AccentVerticalInset = 4f;

    public const float RowHeight = 24f;
    public const float ButtonHeight = 30f;
    public const float MiniButtonHeight = 22f;

    public static void TinySpace() => GUILayout.Space(Tiny);
    public static void SmallSpace() => GUILayout.Space(Small);
    public static void MediumSpace() => GUILayout.Space(Medium);
    public static void LargeSpace() => GUILayout.Space(Large);
    public static void SectionSpace() => GUILayout.Space(Section);
}
}
