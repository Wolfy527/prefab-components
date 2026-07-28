namespace Wolfy.PropTools.Customer.Authoring.Editor
{
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabId))]
public sealed class PrefabIdEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        PrefabId prefabId = target as PrefabId;

        EditorGUILayout.HelpBox(
            "Stores the information needed to reopen this prefab in Prefab Builder. " +
            "This component is removed automatically in play mode and during avatar upload.",
            MessageType.Info
        );

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField(
                "Prefab ID",
                prefabId?.Id ?? string.Empty
            );
            EditorGUILayout.IntField(
                "Prefab Schema",
                prefabId?.PrefabSchema ?? 0
            );
            EditorGUILayout.IntField(
                "Builder Data",
                prefabId?.BuilderDataVersion ?? 0
            );
        }

        if (prefabId != null &&
            !string.IsNullOrWhiteSpace(prefabId.BuilderPackageVersion))
        {
            EditorGUILayout.LabelField(
                "Created With",
                $"Prefab Builder {prefabId.BuilderPackageVersion}"
            );
        }
    }
}
}
