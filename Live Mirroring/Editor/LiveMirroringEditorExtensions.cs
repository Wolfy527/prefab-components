namespace Wolfy.PropTools.Customer.LiveMirroring.Editor
{
using Wolfy.PropTools.Customer.LiveMirroring;
using Wolfy.PropTools.EditorUI;

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public interface ILiveMirroringInspectorSection
{
    string SectionId { get; }
    string Title { get; }
    int Order { get; }
    bool DefaultExpanded { get; }
    bool IsVisible(SerializedObject system);
    string GetSummary(SerializedObject system);
    void Draw(SerializedObject system);
}

public enum LiveMirroringValidationSeverity
{
    Info,
    Warning,
    Error
}

public sealed class LiveMirroringValidationMessage
{
    public LiveMirroringValidationSeverity Severity { get; }
    public string Title { get; }
    public string Message { get; }

    public LiveMirroringValidationMessage(
        LiveMirroringValidationSeverity severity,
        string title,
        string message = null)
    {
        Severity = severity;
        Title = title;
        Message = message;
    }
}

public interface ILiveMirroringValidationContributor
{
    string ContributorId { get; }
    int Order { get; }
    void Validate(
        SerializedObject system,
        List<LiveMirroringValidationMessage> messages);
}

public sealed class LiveMirroringPreviewContext
{
    public LiveMirroringSystem System { get; }
    public Transform Target { get; }
    public GameObject PreviewInstance { get; }

    public LiveMirroringPreviewContext(
        LiveMirroringSystem system,
        Transform target,
        GameObject previewInstance)
    {
        System = system;
        Target = target;
        PreviewInstance = previewInstance;
    }
}

public interface ILiveMirroringPreviewContributor
{
    string ContributorId { get; }
    int Order { get; }
    void OnPreviewCreated(LiveMirroringPreviewContext context);
    void UpdatePreview(LiveMirroringPreviewContext context);
}

public static class LiveMirroringEditorExtensionRegistry
{
    private static IReadOnlyList<ILiveMirroringInspectorSection> sections;
    private static IReadOnlyList<ILiveMirroringValidationContributor> validators;
    private static IReadOnlyList<ILiveMirroringPreviewContributor> previewContributors;

    public static IReadOnlyList<ILiveMirroringInspectorSection> GetSections() =>
        sections ?? (sections = Discover<ILiveMirroringInspectorSection>(
            section => section.SectionId,
            section => section.Order));

    public static IReadOnlyList<ILiveMirroringValidationContributor> GetValidators() =>
        validators ?? (validators = Discover<ILiveMirroringValidationContributor>(
            contributor => contributor.ContributorId,
            contributor => contributor.Order));

    public static IReadOnlyList<ILiveMirroringPreviewContributor> GetPreviewContributors() =>
        previewContributors ?? (previewContributors = Discover<ILiveMirroringPreviewContributor>(
            contributor => contributor.ContributorId,
            contributor => contributor.Order));

    private static IReadOnlyList<T> Discover<T>(
        Func<T, string> idSelector,
        Func<T, int> orderSelector)
    {
        return TypeCache.GetTypesDerivedFrom<T>()
            .Where(type => !type.IsAbstract && !type.IsInterface &&
                           type.GetConstructor(Type.EmptyTypes) != null)
            .Select(Create<T>)
            .Where(item => item != null)
            .GroupBy(item => idSelector(item) ?? item.GetType().FullName)
            .Select(group => group.First())
            .OrderBy(orderSelector)
            .ThenBy(idSelector, StringComparer.Ordinal)
            .ToArray();
    }

    private static T Create<T>(Type type)
    {
        try
        {
            return (T)Activator.CreateInstance(type);
        }
        catch
        {
            return default(T);
        }
    }
}
}
