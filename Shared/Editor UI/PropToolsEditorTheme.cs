namespace Wolfy.PropTools.EditorUI
{
using UnityEngine;

public static class PropToolsEditorTheme
{
    public const string HeaderLogoGuid = "c6672c7f5b5d72b4c9ebf65ad64cfc37";
    public const string HeaderLogoPath =
        "Packages/com.wolfy527.prefab-components/Paw Heart Logo.png";

    public static readonly Color Accent = new Color(1.00f, 0.50f, 0.13f, 1f);
    public static readonly Color AccentBright = new Color(1.00f, 0.66f, 0.24f, 1f);
    public static readonly Color AccentDark = new Color(0.58f, 0.24f, 0.06f, 1f);
    public static readonly Color AccentDim = new Color(1.00f, 0.42f, 0.10f, 0.24f);
    public static readonly Color AccentSoft = new Color(1.00f, 0.50f, 0.13f, 0.10f);

    public static readonly Color Background = new Color(0.120f, 0.116f, 0.110f, 1f);
    public static readonly Color BackgroundDark = new Color(0.070f, 0.068f, 0.065f, 1f);

    public static readonly Color HeaderLeft = new Color(0.40f, 0.165f, 0.050f, 1f);
    public static readonly Color HeaderMid = new Color(0.22f, 0.120f, 0.075f, 1f);
    public static readonly Color HeaderRight = new Color(0.090f, 0.083f, 0.077f, 1f);
    public static readonly Color HeaderDescription = new Color(0.055f, 0.050f, 0.046f, 0.72f);

    public static readonly Color ModuleLeft = new Color(0.205f, 0.145f, 0.105f, 1f);
    public static readonly Color ModuleMid = new Color(0.135f, 0.120f, 0.108f, 1f);
    public static readonly Color ModuleRight = new Color(0.078f, 0.075f, 0.071f, 1f);

    public static readonly Color Panel = new Color(0.168f, 0.160f, 0.150f, 1f);
    public static readonly Color PanelTop = new Color(0.178f, 0.169f, 0.158f, 1f);
    public static readonly Color PanelBottom = new Color(0.142f, 0.136f, 0.128f, 1f);
    public static readonly Color PanelHeader = new Color(0.205f, 0.172f, 0.140f, 0.34f);
    public static readonly Color PanelInset = new Color(0.094f, 0.091f, 0.086f, 1f);

    public static readonly Color Card = Panel;
    public static readonly Color CardSoft = PanelTop;
    public static readonly Color CardHover = new Color(0.205f, 0.184f, 0.163f, 1f);
    public static readonly Color CardHeader = PanelHeader;
    public static readonly Color CardInset = PanelInset;
    public static readonly Color ItemHeader = new Color(0.155f, 0.148f, 0.139f, 1f);
    public static readonly Color ItemHeaderHover = new Color(0.190f, 0.171f, 0.151f, 1f);
    public static readonly Color ItemBody = new Color(0.108f, 0.104f, 0.099f, 1f);
    public static readonly Color ItemBadge = new Color(0.355f, 0.165f, 0.060f, 1f);
    public static readonly Color ItemBadgeBorder = new Color(1f, 0.58f, 0.20f, 0.42f);
    public static readonly Color HierarchyRowAlternate =
        new Color(0.145f, 0.130f, 0.114f, 1f);

    public static readonly Color TreeDropTarget =
        new Color(0.25f, 0.46f, 0.60f, 0.65f);
    public static readonly Color TreeRowHover =
        new Color(0.20f, 0.22f, 0.25f, 0.88f);
    public static readonly Color TreeRowEven =
        new Color(0.145f, 0.155f, 0.17f, 0.82f);
    public static readonly Color TreeRowOdd =
        new Color(0.12f, 0.13f, 0.145f, 0.82f);
    public static readonly Color TreeRequiredAccent =
        new Color(0.91f, 0.48f, 0.20f, 0.9f);
    public static readonly Color TreeCustomAccent =
        new Color(0.36f, 0.62f, 0.75f, 0.75f);
    public static readonly Color TreeGrip =
        new Color(0.56f, 0.60f, 0.65f, 0.9f);
    public static readonly Color TreeGripHover =
        new Color(0.82f, 0.86f, 0.9f, 0.95f);
    public static readonly Color TreeArrow =
        new Color(0.72f, 0.75f, 0.79f, 0.95f);
    public static readonly Color TreeInsertion =
        new Color(0.35f, 0.78f, 1f, 0.98f);

    public static readonly Color Field = new Color(0.080f, 0.078f, 0.074f, 1f);
    public static readonly Color FieldHover = new Color(0.118f, 0.102f, 0.088f, 1f);
    public static readonly Color FieldFocus = new Color(0.145f, 0.105f, 0.075f, 1f);
    public static readonly Color FieldBorder = new Color(1.00f, 0.50f, 0.14f, 0.28f);
    public static readonly Color FieldBorderHover = new Color(1.00f, 0.56f, 0.16f, 0.48f);
    public static readonly Color FieldBorderFocus = new Color(1.00f, 0.62f, 0.22f, 0.78f);

    public static readonly Color ToggleOff = new Color(0.105f, 0.098f, 0.090f, 1f);
    public static readonly Color ToggleOffHover = new Color(0.145f, 0.128f, 0.110f, 1f);
    public static readonly Color ToggleOn = new Color(0.54f, 0.225f, 0.060f, 1f);
    public static readonly Color ToggleOnHover = new Color(0.66f, 0.285f, 0.075f, 1f);
    public static readonly Color ToggleThumbOff = new Color(0.58f, 0.545f, 0.505f, 1f);
    public static readonly Color ToggleThumbOn = new Color(1.00f, 0.88f, 0.68f, 1f);

    public static readonly Color ButtonPrimaryLeft = new Color(0.62f, 0.250f, 0.060f, 1f);
    public static readonly Color ButtonPrimaryRight = new Color(0.18f, 0.090f, 0.050f, 1f);
    public static readonly Color ButtonSecondaryLeft = new Color(0.260f, 0.210f, 0.165f, 1f);
    public static readonly Color ButtonSecondaryRight = new Color(0.115f, 0.100f, 0.090f, 1f);
    public static readonly Color ButtonDangerLeft = new Color(0.540f, 0.100f, 0.055f, 1f);
    public static readonly Color ButtonDangerRight = new Color(0.170f, 0.050f, 0.045f, 1f);

    public static readonly Color Border = new Color(1f, 0.52f, 0.18f, 0.22f);
    public static readonly Color BorderSoft = new Color(1f, 0.55f, 0.22f, 0.09f);
    public static readonly Color BorderStrong = new Color(1f, 0.58f, 0.18f, 0.68f);
    public static readonly Color BorderDark = new Color(0.035f, 0.030f, 0.026f, 0.88f);

    public static readonly Color FloatingDock =
        new Color(0.145f, 0.118f, 0.096f, 1f);
    public static readonly Color FloatingDockBorder =
        new Color(1f, 0.56f, 0.18f, 0.58f);
    public static readonly Color FloatingDockAccent = Accent;

    public static readonly Color Text = new Color(0.985f, 0.948f, 0.890f, 1f);
    public static readonly Color TextMuted = new Color(0.835f, 0.780f, 0.710f, 1f);
    public static readonly Color TextDim = new Color(0.620f, 0.575f, 0.520f, 1f);
    public static readonly Color Value = new Color(1.00f, 0.86f, 0.60f, 1f);

    public static readonly Color InfoBar = new Color(0.120f, 0.116f, 0.110f, 1f);
    public static readonly Color InfoAccent = new Color(0.60f, 0.72f, 0.88f, 1f);
    public static readonly Color Warning = new Color(1.00f, 0.78f, 0.24f, 1f);
    public static readonly Color Error = new Color(1.00f, 0.28f, 0.18f, 1f);
    public static readonly Color Success = new Color(0.42f, 0.90f, 0.46f, 1f);
}
}
