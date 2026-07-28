namespace Wolfy.PropTools.Customer.LiveMirroring.Editor
{
using Wolfy.PropTools.Customer.LiveMirroring;
using Wolfy.PropTools.EditorUI;

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LiveMirroringSystem))]
public class LiveMirroringSystemEditor : Editor
{
    private readonly Dictionary<string, bool> expandedSections =
        new Dictionary<string, bool>();
    private readonly HashSet<string> failedValidators = new HashSet<string>();

    public override bool RequiresConstantRepaint() => false;

    public override void OnInspectorGUI()
    {
        using (PropToolsEditorFields.PushInteractionScope(this))
        {
            if (Event.current.type == EventType.MouseMove)
                Repaint();

            serializedObject.Update();

            PropToolsEditor.Header(
                "Live Mirroring System",
                "Editor tool for live mirrored target positioning and scene preview ghosts."
            );

            LiveMirroringSystem mirroringSystem =
                target as LiveMirroringSystem;

            if (mirroringSystem != null &&
                mirroringSystem.DataVersion < 0)
            {
                PropToolsEditor.Error(
                    "Invalid Mirroring Data",
                    $"This Live Mirroring System has an invalid data version ({mirroringSystem.DataVersion}). " +
                    "Restore the prefab from a valid copy before editing it."
                );
                return;
            }

            if (mirroringSystem != null &&
                mirroringSystem.DataVersion >
                LiveMirroringMigrationService.CurrentDataVersion)
            {
                PropToolsEditor.Error(
                    "Newer Mirroring Data",
                    $"This Live Mirroring System uses data version {mirroringSystem.DataVersion}, but the installed scripts support up to version {LiveMirroringMigrationService.CurrentDataVersion}. " +
                    "Import the newer scripts before editing it."
                );
                return;
            }

            DrawExtensionValidation();

            foreach (ILiveMirroringInspectorSection section in
                     LiveMirroringEditorExtensionRegistry.GetSections())
            {
                if (section == null || !section.IsVisible(serializedObject))
                    continue;

                bool expanded = GetExpanded(section);
                DrawSection(
                    ref expanded,
                    section.Title,
                    section.GetSummary(serializedObject),
                    () => section.Draw(serializedObject)
                );
                expandedSections[section.SectionId] = expanded;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    private bool GetExpanded(ILiveMirroringInspectorSection section)
    {
        if (expandedSections.TryGetValue(section.SectionId, out bool expanded))
            return expanded;

        return section.DefaultExpanded;
    }

    private void DrawExtensionValidation()
    {
        List<LiveMirroringValidationMessage> messages =
            new List<LiveMirroringValidationMessage>();

        foreach (ILiveMirroringValidationContributor contributor in
                 LiveMirroringEditorExtensionRegistry.GetValidators())
        {
            if (contributor == null || failedValidators.Contains(contributor.ContributorId))
                continue;

            try
            {
                contributor.Validate(serializedObject, messages);
            }
            catch (Exception exception)
            {
                failedValidators.Add(contributor.ContributorId);
                Debug.LogException(exception, target);
            }
        }

        foreach (LiveMirroringValidationMessage message in messages)
        {
            if (message == null)
                continue;

            switch (message.Severity)
            {
                case LiveMirroringValidationSeverity.Error:
                    PropToolsEditor.Error(message.Title, message.Message);
                    break;
                case LiveMirroringValidationSeverity.Warning:
                    PropToolsEditor.Warning(message.Title, message.Message);
                    break;
                default:
                    PropToolsEditor.Info(message.Title, message.Message);
                    break;
            }
        }
    }

    private static void DrawSection(
        ref bool expanded,
        string title,
        string summary,
        Action drawContent)
    {
        PropToolsEditor.Foldout(
            ref expanded,
            title,
            compactSummary: summary
        );

        if (!expanded)
            return;

        PropToolsEditor.Card(drawContent);
    }
}
}
