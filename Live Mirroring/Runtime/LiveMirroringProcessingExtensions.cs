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

            if (failedProcessors.Contains(id))
                continue;

            try
            {
                processor.Process(system);
            }
            catch (Exception exception)
            {
                failedProcessors.Add(id);
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
                catch
                {
                    // One optional extension must not prevent other extensions from loading.
                }
            }
        }

        return discovered
            .GroupBy(processor => processor.ProcessorId ?? processor.GetType().FullName)
            .Select(group => group.First())
            .OrderBy(processor => processor.Stage)
            .ThenBy(processor => processor.Order)
            .ThenBy(processor => processor.ProcessorId, StringComparer.Ordinal)
            .ToArray();
    }
}
}
