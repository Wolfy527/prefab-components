namespace Wolfy.PropTools.EditorUI
{
using System;
using UnityEditor;
using UnityEngine;

public static class PropToolsEditor
{
    public static void Header(
        string title,
        string subtitle = null,
        float availableWidth = 0f) =>
        PropToolsEditorLayout.Header(title, subtitle, availableWidth);

    public static void Module(ref bool expanded, string title, bool enabled, string subtitle, bool selected, Action content) =>
        PropToolsEditorLayout.Module(ref expanded, title, enabled, subtitle, selected, content);

    public static void Module(ref bool expanded, string title, SerializedProperty enabledProperty, string subtitle, bool selected, Action content) =>
        PropToolsEditorLayout.Module(ref expanded, title, enabledProperty, subtitle, selected, content);

    public static void Group(string title, Action content) =>
        PropToolsEditorLayout.Group(title, content);

    public static void GroupCard(string title, Action content) =>
        PropToolsEditorLayout.GroupCard(title, content);

    public static void Section(string title, string subtitle = null) =>
        PropToolsEditorLayout.Section(title, subtitle);

    public static bool Foldout(ref bool expanded, string title, string subtitle = null, bool enabled = true, bool selected = false, SerializedProperty toggleProperty = null, string compactSummary = null) =>
        PropToolsEditorFoldouts.Foldout(ref expanded, title, subtitle, enabled, selected, toggleProperty, compactSummary);

    public static bool FoldoutHeader(string title, bool expanded, bool enabled = true, string subtitle = null, bool selected = false, SerializedProperty toggleProperty = null, string compactSummary = null) =>
        PropToolsEditorFoldouts.FoldoutHeader(title, expanded, enabled, subtitle, selected, toggleProperty, compactSummary);

    public static void Card(Action content) =>
        PropToolsEditorLayout.Card(content);

    public static bool ItemCard(
        string stateKey,
        string badge,
        string title,
        string summary,
        bool defaultExpanded,
        Action content,
        string titleTooltip = null,
        string badgeTooltip = null,
        UnityEngine.Object highlightTarget = null,
        string highlightItemId = null) =>
        PropToolsEditorLayout.ItemCard(
            stateKey,
            badge,
            title,
            summary,
            defaultExpanded,
            content,
            titleTooltip,
            badgeTooltip,
            highlightTarget,
            highlightItemId
        );

    public static bool FeatureCard(string title, string tooltip, bool added, bool highlighted = false) =>
        PropToolsEditorLayout.FeatureCard(title, tooltip, added, highlighted);

    public static bool ActionPanel(string title, string description, string buttonText) =>
        PropToolsEditorLayout.ActionPanel(title, description, buttonText);

    public static bool ActionFooter(
        string title,
        string status,
        string buttonText,
        MessageType statusType = MessageType.None) =>
        PropToolsEditorLayout.ActionFooter(
            title,
            status,
            buttonText,
            statusType
        );

    public static void SubHeader(string text) =>
        PropToolsEditorLayout.SubHeader(text);

    public static void Property(SerializedProperty property, string label = null) =>
        PropToolsEditorFields.Property(property, label);

    public static bool Toggle(SerializedObject so, string propertyName, string label) =>
        PropToolsEditorControls.Toggle(so, propertyName, label);

    public static bool Toggle(SerializedProperty property, string label) =>
        PropToolsEditorControls.Toggle(property, label);

    public static bool Toggle(bool value, string label) =>
        PropToolsEditorControls.Toggle(value, label);

    public static bool ToggleBox(Rect rect, bool value) =>
        PropToolsEditorControls.ToggleBox(rect, value);

    public static void TextField(SerializedObject so, string propertyName, string label) =>
        PropToolsEditorFields.TextField(so, propertyName, label);

    public static void DelayedTextField(SerializedObject so, string propertyName, string label) =>
        PropToolsEditorFields.DelayedTextField(so, propertyName, label);

    public static void IntField(SerializedObject so, string propertyName, string label) =>
        PropToolsEditorFields.IntField(so, propertyName, label);

    public static void FloatField(SerializedObject so, string propertyName, string label) =>
        PropToolsEditorFields.FloatField(so, propertyName, label);

    public static void Vector2Field(SerializedObject so, string propertyName, string label) =>
        PropToolsEditorFields.Vector2Field(so, propertyName, label);

    public static void Vector3Field(SerializedObject so, string propertyName, string label) =>
        PropToolsEditorFields.Vector3Field(so, propertyName, label);

    public static T ObjectField<T>(SerializedObject so, string propertyName, string label, bool allowSceneObjects = true) where T : UnityEngine.Object =>
        PropToolsEditorFields.ObjectField<T>(so, propertyName, label, allowSceneObjects);

    public static void ObjectField(SerializedProperty property, Type objectType, string label = null, bool allowSceneObjects = true) =>
        PropToolsEditorFields.ObjectField(property, objectType, label, allowSceneObjects);

    public static void EnumField(SerializedProperty property, string label = null) =>
        PropToolsEditorFields.EnumField(property, label);

    public static void Popup(
        SerializedProperty property,
        string label,
        string[] displayOptions,
        string[] values) =>
        PropToolsEditorFields.Popup(property, label, displayOptions, values);

    public static void Stepper(SerializedObject so, string propertyName, string label, int min, int max, Func<int, string> displayFormatter = null) =>
        PropToolsEditorControls.Stepper(so, propertyName, label, min, max, displayFormatter);

    public static void Stepper(SerializedProperty property, string label, int min, int max, Func<int, string> displayFormatter = null) =>
        PropToolsEditorControls.Stepper(property, label, min, max, displayFormatter);

    public static void Info(string title, string message = null) =>
        PropToolsEditorMessages.Info(title, message);

    public static void Warning(string title, string message = null) =>
        PropToolsEditorMessages.Warning(title, message);

    public static void Error(string title, string message = null) =>
        PropToolsEditorMessages.Error(title, message);

    public static void Success(string title, string message = null) =>
        PropToolsEditorMessages.Success(title, message);

    public static void Stats(string text) =>
        PropToolsEditorMessages.Stats(text);

    public static void Hint(
        string message,
        float minimumHeight = 34f) =>
        PropToolsEditorMessages.Hint(message, minimumHeight);

    public static bool Button(string text, float height = 28f) =>
        PropToolsEditorControls.PrimaryButton(text, height);

    public static bool PrimaryButton(string text, float height = 30f) =>
        PropToolsEditorControls.PrimaryButton(text, height);

    public static bool SecondaryButton(string text, float height = 26f) =>
        PropToolsEditorControls.SecondaryButton(text, height);

    public static bool DangerButton(string text, float height = 26f) =>
        PropToolsEditorControls.DangerButton(text, height);

    public static bool MiniButton(Rect rect, string text) =>
        PropToolsEditorControls.MiniButton(rect, text);

    public static bool MiniButton(string text, float width = 72f, float height = 22f) =>
        PropToolsEditorControls.MiniButton(text, width, height);

    public static bool MissingObject(SerializedObject so, string propertyName)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        return property == null || property.objectReferenceValue == null;
    }

    public static bool Bool(SerializedObject so, string propertyName)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        return property != null && property.boolValue;
    }

    public static void SpaceTiny() => GUILayout.Space(PropToolsEditorSpacing.Tiny);
    public static void SpaceSmall() => GUILayout.Space(PropToolsEditorSpacing.Small);
    public static void SpaceMedium() => GUILayout.Space(PropToolsEditorSpacing.Medium);
    public static void SpaceLarge() => GUILayout.Space(PropToolsEditorSpacing.Large);
    public static void SpaceSection() => GUILayout.Space(PropToolsEditorSpacing.Section);
}
}
