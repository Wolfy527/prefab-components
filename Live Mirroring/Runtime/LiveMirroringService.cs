namespace Wolfy.PropTools.Customer.LiveMirroring
{
using Wolfy.PropTools.Customer.Authoring;

using System.Collections.Generic;
using UnityEngine;

public static class LiveMirroringService
{
    public static void UpdateMirroring(LiveMirroringSystem system)
    {
        LiveMirroringProcessorRegistry.Run(
            system,
            LiveMirroringProcessingStage.BeforeCore
        );
        ApplyScaleReference(system);
        MirrorEnabledPairs(system);
        LiveMirroringProcessorRegistry.Run(
            system,
            LiveMirroringProcessingStage.AfterCore
        );
    }

    public static void ApplyScaleReference(LiveMirroringSystem system)
    {
        if (system == null || !system.applyScaleReference || system.scaleReference == null)
            return;

        List<Transform> targets = new List<Transform>();
        HashSet<Transform> seen = new HashSet<Transform>();
        CollectAllTargets(system, targets, seen);
        ApplyScaleReference(system, targets);
    }

    public static void ApplyScaleReference(
        LiveMirroringSystem system,
        IReadOnlyList<Transform> targets)
    {
        if (system == null ||
            !system.applyScaleReference ||
            system.scaleReference == null ||
            targets == null)
        {
            return;
        }

        Vector3 desiredWorldScale = system.scaleReference.lossyScale;

        for (int i = 0; i < targets.Count; i++)
            ApplyWorldScale(targets[i], desiredWorldScale);
    }

    public static void MirrorEnabledPairs(LiveMirroringSystem system)
    {
        if (system == null)
            return;

        if (system.mirrorOptions == null)
            system.mirrorOptions = new LiveMirroringSystem.MirrorOptions();

        Transform center = ResolveRoot(system);

        foreach (LiveMirroringSystem.MirrorPair pair in CollectEnabledPairs(system))
            MirrorPair(system, pair, center);
    }

    public static List<Transform> CollectAllTargets(LiveMirroringSystem system)
    {
        List<Transform> output = new List<Transform>();
        HashSet<Transform> seen = new HashSet<Transform>();
        CollectAllTargets(system, output, seen);
        return output;
    }

    public static void CollectAllTargets(
        LiveMirroringSystem system,
        List<Transform> output,
        HashSet<Transform> seen)
    {
        if (output == null || seen == null)
            return;

        output.Clear();
        seen.Clear();

        if (system != null && system.pairs != null)
        {
            foreach (LiveMirroringSystem.MirrorPair pair in system.pairs)
            {
                if (pair == null)
                    continue;

                AddTarget(pair.sourceTarget, output, seen);
                AddTarget(pair.mirroredTarget, output, seen);
            }
        }
    }

    public static Transform ResolveRoot(LiveMirroringSystem system)
    {
        if (system == null)
            return null;

        if (system.mirrorCenter != null)
            return system.mirrorCenter;

        if (system.transform.parent != null)
            return system.transform.parent;

        return system.transform.root;
    }

    private static List<LiveMirroringSystem.MirrorPair> CollectEnabledPairs(
        LiveMirroringSystem system)
    {
        List<LiveMirroringSystem.MirrorPair> output =
            new List<LiveMirroringSystem.MirrorPair>();

        if (system == null || system.pairs == null)
            return output;

        HashSet<Transform> controlledMirroredTargets = new HashSet<Transform>();

        foreach (LiveMirroringSystem.MirrorPair pair in system.pairs)
        {
            if (pair == null ||
                !pair.mirrorEnabled ||
                pair.sourceTarget == null ||
                pair.mirroredTarget == null ||
                pair.sourceTarget == pair.mirroredTarget ||
                !controlledMirroredTargets.Add(pair.mirroredTarget))
            {
                continue;
            }

            output.Add(pair);
        }

        return output;
    }

    private static void MirrorPair(
        LiveMirroringSystem system,
        LiveMirroringSystem.MirrorPair pair,
        Transform center)
    {
        if (center == null)
            return;

        Transform source = pair.sourceTarget;
        Transform mirrored = pair.mirroredTarget;

        if (system.mirrorOptions.mirrorPosition)
        {
            mirrored.position = center.TransformPoint(
                MirrorVector(system, center.InverseTransformPoint(source.position))
            );
        }

        if (system.mirrorOptions.mirrorRotation)
            MirrorRotation(system, source, mirrored, pair.mirroredRotationOffset, center);

        if (system.mirrorOptions.mirrorScale)
            ApplyWorldScale(mirrored, source.lossyScale);
    }

    private static void MirrorRotation(
        LiveMirroringSystem system,
        Transform source,
        Transform mirrored,
        Vector3 offset,
        Transform center)
    {
        Vector3 localForward = MirrorVector(system, center.InverseTransformDirection(source.forward));
        Vector3 localUp = MirrorVector(system, center.InverseTransformDirection(source.up));
        Vector3 worldForward = center.TransformDirection(localForward);
        Vector3 worldUp = center.TransformDirection(localUp);

        if (worldForward.sqrMagnitude < 0.0001f || worldUp.sqrMagnitude < 0.0001f)
            return;

        mirrored.rotation = Quaternion.LookRotation(worldForward, worldUp) * Quaternion.Euler(offset);
    }

    private static Vector3 MirrorVector(LiveMirroringSystem system, Vector3 value)
    {
        switch (system.mirrorOptions.mirrorAxis)
        {
            case LiveMirroringSystem.Axis.X:
                value.x *= -1f;
                break;
            case LiveMirroringSystem.Axis.Y:
                value.y *= -1f;
                break;
            case LiveMirroringSystem.Axis.Z:
                value.z *= -1f;
                break;
        }

        return value;
    }

    private static void ApplyWorldScale(Transform target, Vector3 desiredWorldScale)
    {
        if (target == null)
            return;

        Transform parent = target.parent;

        if (parent == null)
        {
            if ((target.localScale - desiredWorldScale).sqrMagnitude > 0.00000001f)
                target.localScale = desiredWorldScale;

            return;
        }

        Vector3 parentScale = parent.lossyScale;
        Vector3 localScale = new Vector3(
            SafeDivide(desiredWorldScale.x, parentScale.x),
            SafeDivide(desiredWorldScale.y, parentScale.y),
            SafeDivide(desiredWorldScale.z, parentScale.z)
        );

        if ((target.localScale - localScale).sqrMagnitude > 0.00000001f)
            target.localScale = localScale;
    }

    private static void AddTarget(
        Transform target,
        List<Transform> output,
        HashSet<Transform> seen)
    {
        if (target == null || !seen.Add(target))
            return;

        output.Add(target);
    }

    private static float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) < 0.00001f ? value : value / divisor;
    }
}
}
