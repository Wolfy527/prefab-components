namespace Wolfy.PropTools.Customer.LiveMirroring.Editor
{
using Wolfy.PropTools.Customer.LiveMirroring;
using Wolfy.PropTools.EditorUI;

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class LiveMirroringPreviewDrawer
{
    private const double SystemDiscoveryInterval = 5.0;
    private const int MaximumPreviewInstances = 128;

    private class PreviewState
    {
        public LiveMirroringSystem system;
        public GameObject source;
        public Material material;
        public readonly List<Transform> targets = new List<Transform>();
        public readonly List<Transform> collectedTargets = new List<Transform>();
        public readonly HashSet<Transform> collectedTargetSet = new HashSet<Transform>();
        public readonly List<GameObject> instances = new List<GameObject>();
    }

    private static readonly Dictionary<int, PreviewState> states = new Dictionary<int, PreviewState>();
    private static readonly HashSet<string> failedContributors = new HashSet<string>();
    private static readonly List<LiveMirroringSystem> cachedSystems =
        new List<LiveMirroringSystem>();
    private static readonly HashSet<int> activeIds = new HashSet<int>();
    private static readonly List<int> inactiveIds = new List<int>();
    private static bool systemsDirty = true;
    private static double nextDiscoveryTime;

    static LiveMirroringPreviewDrawer()
    {
        EditorApplication.update -= UpdatePreviews;
        EditorApplication.update += UpdatePreviews;

        AssemblyReloadEvents.beforeAssemblyReload -= DestroyAllPreviews;
        AssemblyReloadEvents.beforeAssemblyReload += DestroyAllPreviews;

        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        EditorApplication.hierarchyChanged -= MarkSystemsDirty;
        EditorApplication.hierarchyChanged += MarkSystemsDirty;
    }

    private static void UpdatePreviews()
    {
        if (Application.isPlaying)
        {
            DestroyAllPreviews();
            return;
        }

        double now = EditorApplication.timeSinceStartup;

        if (systemsDirty || now >= nextDiscoveryTime)
            RefreshSystems(now);

        activeIds.Clear();

        for (int systemIndex = 0; systemIndex < cachedSystems.Count; systemIndex++)
        {
            LiveMirroringSystem system = cachedSystems[systemIndex];

            if (system == null ||
                !system.isActiveAndEnabled ||
                !system.showScenePreview ||
                system.previewSource == null)
            {
                continue;
            }

            int id = system.GetInstanceID();
            activeIds.Add(id);

            if (!states.TryGetValue(id, out PreviewState state))
            {
                state = new PreviewState { system = system };
                states[id] = state;
            }

            CollectPreviewTargets(
                system,
                state.collectedTargets,
                state.collectedTargetSet
            );

            // Previewing must not mutate authored targets while live mirroring
            // itself is disabled.
            if (system.liveMirror)
            {
                LiveMirroringService.ApplyScaleReference(
                    system,
                    state.collectedTargets);
            }

            bool needsRebuild =
                state.source != system.previewSource ||
                state.material != system.previewMaterial ||
                !TargetsMatch(state.targets, state.collectedTargets);

            if (needsRebuild)
                RebuildState(state, system, state.collectedTargets);

            UpdateInstanceTransforms(state);
        }

        CleanupInactiveStates(activeIds);
    }

    private static void RefreshSystems(double now)
    {
        cachedSystems.Clear();
        cachedSystems.AddRange(Object.FindObjectsOfType<LiveMirroringSystem>(true));
        systemsDirty = false;
        nextDiscoveryTime = now + SystemDiscoveryInterval;
    }

    private static void MarkSystemsDirty()
    {
        systemsDirty = true;
    }

    private static bool TargetsMatch(List<Transform> a, List<Transform> b)
    {
        if (a.Count != b.Count)
            return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    private static void CollectPreviewTargets(
        LiveMirroringSystem system,
        List<Transform> output,
        HashSet<Transform> seen)
    {
        output.Clear();
        seen.Clear();
        if (system?.pairs == null)
            return;

        foreach (LiveMirroringSystem.MirrorPair pair in system.pairs)
        {
            if (pair == null)
                continue;

            AddPreviewTarget(pair.sourceTarget, output, seen);
            AddPreviewTarget(pair.mirroredTarget, output, seen);
            if (output.Count >= MaximumPreviewInstances)
                return;
        }
    }

    private static void AddPreviewTarget(
        Transform target,
        ICollection<Transform> output,
        ISet<Transform> seen)
    {
        if (target != null &&
            output.Count < MaximumPreviewInstances &&
            seen.Add(target))
        {
            output.Add(target);
        }
    }

    private static void RebuildState(
        PreviewState state,
        LiveMirroringSystem system,
        List<Transform> currentTargets)
    {
        DestroyStateInstances(state);

        state.system = system;
        state.source = system.previewSource;
        state.material = system.previewMaterial;

        state.targets.Clear();
        state.targets.AddRange(currentTargets);

        for (int i = 0; i < state.targets.Count; i++)
        {
            GameObject container = new GameObject($"[Scene Preview] {system.name} {i + 1}");
            container.hideFlags = HideFlags.HideAndDontSave;

            GameObject clone = Object.Instantiate(system.previewSource);
            clone.name = system.previewSource.name;
            clone.transform.SetParent(container.transform, false);

            SetHideFlagsRecursive(container);
            DisableRuntimeInteraction(container);
            ApplyPreviewMaterial(container, system.previewMaterial);
            NotifyPreviewCreated(system, state.targets[i], container);

            state.instances.Add(container);
        }

        UpdateInstanceTransforms(state);
    }

    private static void UpdateInstanceTransforms(PreviewState state)
    {
        for (int i = 0; i < state.instances.Count; i++)
        {
            if (i >= state.targets.Count || state.instances[i] == null || state.targets[i] == null)
                continue;

            Transform instance = state.instances[i].transform;
            Transform target = state.targets[i];
            Vector3 position = target.position;
            Quaternion rotation = target.rotation;
            Vector3 scale = target.lossyScale;

            if ((instance.position - position).sqrMagnitude > 0.00000001f)
                instance.position = position;

            if (Quaternion.Angle(instance.rotation, rotation) > 0.001f)
                instance.rotation = rotation;

            if ((instance.localScale - scale).sqrMagnitude > 0.00000001f)
                instance.localScale = scale;

            NotifyPreviewUpdated(state.system, target, state.instances[i]);
        }
    }

    private static void NotifyPreviewCreated(
        LiveMirroringSystem system,
        Transform target,
        GameObject instance)
    {
        foreach (ILiveMirroringPreviewContributor contributor in
                 LiveMirroringEditorExtensionRegistry.GetPreviewContributors())
        {
            InvokePreviewContributor(
                contributor,
                system,
                target,
                instance,
                true
            );
        }
    }

    private static void NotifyPreviewUpdated(
        LiveMirroringSystem system,
        Transform target,
        GameObject instance)
    {
        foreach (ILiveMirroringPreviewContributor contributor in
                 LiveMirroringEditorExtensionRegistry.GetPreviewContributors())
        {
            InvokePreviewContributor(
                contributor,
                system,
                target,
                instance,
                false
            );
        }
    }

    private static void InvokePreviewContributor(
        ILiveMirroringPreviewContributor contributor,
        LiveMirroringSystem system,
        Transform target,
        GameObject instance,
        bool created)
    {
        if (contributor == null)
            return;

        string contributorId = string.IsNullOrWhiteSpace(
            contributor.ContributorId)
            ? contributor.GetType().FullName
            : contributor.ContributorId;
        string failureKey =
            contributorId + ":" +
            (system != null ? system.GetInstanceID().ToString() : "Missing");
        if (failedContributors.Contains(failureKey))
            return;

        try
        {
            LiveMirroringPreviewContext context =
                new LiveMirroringPreviewContext(system, target, instance);

            if (created)
                contributor.OnPreviewCreated(context);
            else
                contributor.UpdatePreview(context);
        }
        catch (System.Exception exception)
        {
            failedContributors.Add(failureKey);
            Debug.LogException(exception, system);
        }
    }

    private static void ApplyPreviewMaterial(GameObject root, Material material)
    {
        if (material == null)
            return;

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
                materials[i] = material;

            renderer.sharedMaterials = materials;
        }
    }

    private static void DisableRuntimeInteraction(GameObject root)
    {
        foreach (Behaviour behaviour in root.GetComponentsInChildren<Behaviour>(true))
            behaviour.enabled = false;

        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            collider.enabled = false;

        foreach (AudioSource audioSource in root.GetComponentsInChildren<AudioSource>(true))
            audioSource.enabled = false;

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private static void SetHideFlagsRecursive(GameObject root)
    {
        root.hideFlags = HideFlags.HideAndDontSave;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            child.gameObject.hideFlags = HideFlags.HideAndDontSave;
    }

    private static void CleanupInactiveStates(HashSet<int> activeIds)
    {
        inactiveIds.Clear();

        foreach (int id in states.Keys)
        {
            if (!activeIds.Contains(id))
                inactiveIds.Add(id);
        }

        foreach (int id in inactiveIds)
        {
            DestroyStateInstances(states[id]);
            states.Remove(id);
        }
    }

    private static void DestroyStateInstances(PreviewState state)
    {
        foreach (GameObject instance in state.instances)
        {
            if (instance != null)
                Object.DestroyImmediate(instance);
        }

        state.instances.Clear();
    }

    private static void DestroyAllPreviews()
    {
        foreach (PreviewState state in states.Values)
            DestroyStateInstances(state);

        states.Clear();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            DestroyAllPreviews();

        systemsDirty = true;
    }
}
}
