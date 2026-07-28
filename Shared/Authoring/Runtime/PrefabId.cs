namespace Wolfy.PropTools.Customer.Authoring
{
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class PrefabIdObjectReference
{
    [SerializeField]
    private string propertyPath;

    [SerializeField]
    private UnityEngine.Object value;

    public string PropertyPath => propertyPath;
    public UnityEngine.Object Value => value;

    public PrefabIdObjectReference(
        string propertyPath,
        UnityEngine.Object value)
    {
        this.propertyPath = propertyPath ?? string.Empty;
        this.value = value;
    }
}

[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Wolfy/Prefab ID")]
public sealed class PrefabId : AuthoringOnlyComponent
{
    public const int CurrentSchemaVersion = 1;

    [SerializeField, HideInInspector]
    private string prefabId;

    [SerializeField, HideInInspector]
    private int prefabSchema = CurrentSchemaVersion;

    [SerializeField, HideInInspector]
    private int builderDataVersion;

    [SerializeField, HideInInspector]
    private string builderPackageVersion;

    [SerializeField, HideInInspector, TextArea]
    private string builderState;

    [SerializeField, HideInInspector]
    private List<PrefabIdObjectReference> objectReferences =
        new List<PrefabIdObjectReference>();

    [SerializeField, HideInInspector]
    private List<string> builderOwnedPaths = new List<string>();

    public string Id => prefabId;
    public int PrefabSchema => prefabSchema;
    public int BuilderDataVersion => builderDataVersion;
    public string BuilderPackageVersion => builderPackageVersion;
    public string BuilderState => builderState;
    public IReadOnlyList<PrefabIdObjectReference> ObjectReferences =>
        objectReferences;
    public IReadOnlyList<string> BuilderOwnedPaths => builderOwnedPaths;
    public bool HasSnapshot =>
        !string.IsNullOrWhiteSpace(prefabId) &&
        !string.IsNullOrWhiteSpace(builderState);

    public void SetSnapshot(
        string id,
        int dataVersion,
        string packageVersion,
        string state,
        IEnumerable<PrefabIdObjectReference> references,
        IEnumerable<string> ownedPaths)
    {
        prefabId = string.IsNullOrWhiteSpace(id)
            ? Guid.NewGuid().ToString("N")
            : id.Trim();
        prefabSchema = CurrentSchemaVersion;
        builderDataVersion = Mathf.Max(0, dataVersion);
        builderPackageVersion = packageVersion?.Trim() ?? string.Empty;
        builderState = state ?? string.Empty;
        objectReferences = references != null
            ? new List<PrefabIdObjectReference>(references)
            : new List<PrefabIdObjectReference>();
        builderOwnedPaths = ownedPaths != null
            ? new List<string>(ownedPaths)
            : new List<string>();
    }
}
}
