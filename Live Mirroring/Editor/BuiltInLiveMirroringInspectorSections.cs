namespace Wolfy.PropTools.Customer.LiveMirroring.Editor
{
using Wolfy.PropTools.Customer.LiveMirroring;
using Wolfy.PropTools.EditorUI;

using UnityEditor;
using UnityEngine;

public abstract class LiveMirroringInspectorSectionBase : ILiveMirroringInspectorSection
{
    public abstract string SectionId { get; }
    public abstract string Title { get; }
    public abstract int Order { get; }
    public virtual bool DefaultExpanded => true;
    public virtual bool IsVisible(SerializedObject system) => true;
    public virtual string GetSummary(SerializedObject system) => null;
    public abstract void Draw(SerializedObject system);

    protected static SerializedProperty Property(SerializedObject system, string name) =>
        system?.FindProperty(name);
}

public sealed class LiveMirroringCoreInspectorSection : LiveMirroringInspectorSectionBase
{
    public override string SectionId => "wolfy.live-mirroring.core";
    public override string Title => "Live Mirroring";
    public override int Order => 10;
    public override string GetSummary(SerializedObject system) =>
        (Property(system, "liveMirror")?.boolValue ?? false) ? "Enabled" : "Paused";
    public override void Draw(SerializedObject system) =>
        LiveMirroringSections.DrawCore(system);
}

public sealed class LiveMirroringScaleInspectorSection : LiveMirroringInspectorSectionBase
{
    public override string SectionId => "wolfy.live-mirroring.scale";
    public override string Title => "Scale Reference";
    public override int Order => 20;

    public override string GetSummary(SerializedObject system)
    {
        SerializedProperty enabled = Property(system, "applyScaleReference");
        SerializedProperty reference = Property(system, "scaleReference");

        if (enabled == null || !enabled.boolValue)
            return "Off";

        return reference == null || reference.objectReferenceValue == null
            ? "Needs Reference"
            : "Enabled";
    }

    public override void Draw(SerializedObject system) =>
        LiveMirroringSections.DrawScaleReference(system);
}

public sealed class LiveMirroringPairsInspectorSection : LiveMirroringInspectorSectionBase
{
    public override string SectionId => "wolfy.live-mirroring.pairs";
    public override string Title => "Mirror Pairs";
    public override int Order => 30;

    public override string GetSummary(SerializedObject system)
    {
        int count = Property(system, "pairs")?.arraySize ?? 0;
        return $"{count} Pair{(count == 1 ? "" : "s")}";
    }

    public override void Draw(SerializedObject system) =>
        LiveMirroringSections.DrawPairs(system);
}

public sealed class LiveMirroringPreviewInspectorSection : LiveMirroringInspectorSectionBase
{
    public override string SectionId => "wolfy.live-mirroring.preview";
    public override string Title => "Scene Preview Ghosts";
    public override int Order => 40;

    public override string GetSummary(SerializedObject system)
    {
        SerializedProperty enabled = Property(system, "showScenePreview");
        SerializedProperty source = Property(system, "previewSource");

        if (enabled == null || !enabled.boolValue)
            return "Off";

        return source == null || source.objectReferenceValue == null
            ? "Needs Source"
            : "Enabled";
    }

    public override void Draw(SerializedObject system) =>
        LiveMirroringSections.DrawPreview(system);
}

public sealed class LiveMirroringAdvancedInspectorSection : LiveMirroringInspectorSectionBase
{
    public override string SectionId => "wolfy.live-mirroring.advanced";
    public override string Title => "Advanced Mirror Options";
    public override int Order => 50;
    public override bool DefaultExpanded => false;
    public override void Draw(SerializedObject system) =>
        LiveMirroringSections.DrawAdvanced(system);
}
}
