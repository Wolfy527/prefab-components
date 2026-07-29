namespace Wolfy.PropTools.Customer.LiveMirroring
{
using Wolfy.PropTools.Customer.Authoring;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum LiveMirroringProcessingStage
{
    BeforeCore,
    AfterCore
}

public interface ILiveMirroringProcessor
{
    string ProcessorId { get; }
    int Order { get; }
    LiveMirroringProcessingStage Stage { get; }
    void Process(LiveMirroringSystem system);
}

#if UNITY_EDITOR
public static class LiveMirroringProcessorRegistry
{
    private static IReadOnlyList<ILiveMirroringProcessor> processors;
    private static readonly HashSet<string> failedProcessors = new HashSet<string>();

    public static IReadOnlyList<ILiveMirroringProcessor> GetProcessors()
    {
        if (processors == null)
            processors = DiscoverProcessors();

        return processors;
    }

    public static void Run(
        LiveMirroringSystem system,
        LiveMirroringProcessingStage stage)
    {
        if (system == null)
            return;

        foreach (ILiveMirroringProcessor processor in GetProcessors())
        {
            if (processor == null || processor.Stage != stage)
                continue;

            string id = string.IsNullOrWhiteSpace(processor.ProcessorId)
                ? processor.GetType().FullName
                : processor.ProcessorId;
            string failureKey = id + ":" + system.GetInstanceID();

            if (failedProcessors.Contains(failureKey))
                continue;

            try
            {
                processor.Process(system);
            }
            catch (Exception exception)
            {
                failedProcessors.Add(failureKey);
                Debug.LogException(exception, system);
            }
        }
    }

    public static void Refresh()
    {
        processors = null;
        failedProcessors.Clear();
    }

    private static IReadOnlyList<ILiveMirroringProcessor> DiscoverProcessors()
    {
        List<ILiveMirroringProcessor> discovered = new List<ILiveMirroringProcessor>();
        Type contract = typeof(ILiveMirroringProcessor);

        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(type => type != null).ToArray();
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Live Mirroring could not inspect optional processor " +
                    $"assembly '{assembly.FullName}': {exception.Message}");
                continue;
            }

            foreach (Type type in types)
            {
                if (type == null || type.IsAbstract || type.IsInterface ||
                    !contract.IsAssignableFrom(type) ||
                    type.GetConstructor(Type.EmptyTypes) == null)
                {
                    continue;
                }

                try
                {
                    discovered.Add((ILiveMirroringProcessor)Activator.CreateInstance(type));
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        $"Live Mirroring could not create optional processor " +
                        $"'{type.FullName}': {exception.Message}");
                }
            }
        }

        return discovered
            .GroupBy(processor =>
                string.IsNullOrWhiteSpace(processor.ProcessorId)
                    ? processor.GetType().FullName
                    : processor.ProcessorId.Trim())
            .Select(ResolveCollision)
            .Where(processor => processor != null)
            .OrderBy(processor => processor.Stage)
            .ThenBy(processor => processor.Order)
            .ThenBy(processor => processor.ProcessorId, StringComparer.Ordinal)
            .ToArray();
    }

    private static ILiveMirroringProcessor ResolveCollision(
        IGrouping<string, ILiveMirroringProcessor> group)
    {
        ILiveMirroringProcessor[] candidates = group.ToArray();
        if (candidates.Length == 1)
            return candidates[0];

        ILiveMirroringProcessor[] builtIn = candidates
            .Where(candidate =>
                candidate.GetType().Assembly ==
                typeof(LiveMirroringProcessorRegistry).Assembly)
            .ToArray();
        ILiveMirroringProcessor selected =
            builtIn.Length == 1 ? builtIn[0] : null;

        Debug.LogError(
            $"Live Mirroring found {candidates.Length} processors using ID " +
            $"'{group.Key}'. " +
            (selected != null
                ? $"The built-in '{selected.GetType().FullName}' was retained."
                : "All conflicting processors were disabled.") +
            " Conflicting IDs must be renamed."
        );
        return selected;
    }
}
#endif
}
