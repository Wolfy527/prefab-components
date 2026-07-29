namespace Wolfy.PropTools.Customer.LiveMirroring
{
using Wolfy.PropTools.Customer.Authoring;

using System;
using UnityEngine;

[ExecuteAlways]
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, null, "Assembly-CSharp", "LiveMirroringSystem")]
public class LiveMirroringSystem : AuthoringOnlyComponent
{
    public override bool RemoveGameObjectWithComponent => true;

    public enum Axis { X, Y, Z }

    [Serializable]
    public class MirrorOptions
    {
        public bool mirrorPosition = true;
        public bool mirrorRotation = true;
        public bool mirrorScale = true;
        public Axis mirrorAxis = Axis.X;
    }

    [Serializable]
    public class MirrorPair
    {
        public bool mirrorEnabled = true;
        public string pairName = "Mirror Pair";
        public Transform sourceTarget;
        public Transform mirroredTarget;
        public Vector3 mirroredRotationOffset;
    }

    [SerializeField]
    private int dataVersion;

    public int DataVersion => dataVersion;

    public bool liveMirror = true;
    public Transform mirrorCenter;

    public bool applyScaleReference = true;
    public Transform scaleReference;

    public MirrorPair[] pairs;

    public bool showScenePreview = true;
    public GameObject previewSource;
    public Material previewMaterial;

    public MirrorOptions mirrorOptions = new MirrorOptions();

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (dataVersion > LiveMirroringMigrationService.CurrentDataVersion)
            return;

        EnsureSerializedDefaults();
        LiveMirroringMigrationService.MigrateIfNeeded(this);
#endif
    }

    private void Update()
    {
        if (!Application.isPlaying && liveMirror)
            MirrorAll();
    }

    public void MirrorAll()
    {
        EnsureSerializedDefaults();
#if UNITY_EDITOR
        LiveMirroringService.UpdateMirroring(this);
#endif
    }

    public void SetDataVersion(int version)
    {
        dataVersion = version;
    }

    private void EnsureSerializedDefaults()
    {
        if (mirrorOptions == null)
            mirrorOptions = new MirrorOptions();
    }
}
}
