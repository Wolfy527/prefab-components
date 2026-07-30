namespace Wolfy.PropTools.EditorUI
{
using System;
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorFields
{
    private const float LabelWidth = 178f;
    private static int currentInteractionScopeId;
    private static bool interactionMouseDown;
    private static bool interactionClickedTextField;
    private static string lastFocusedTextControl;

    public readonly struct InteractionScope : IDisposable
    {
        private readonly UnityEngine.Object owner;
        private readonly int previousScopeId;
        private readonly bool previousMouseDown;
        private readonly bool previousClickedTextField;

        internal InteractionScope(
            UnityEngine.Object owner,
            int previousScopeId,
            bool previousMouseDown,
            bool previousClickedTextField)
        {
            this.owner = owner;
            this.previousScopeId = previousScopeId;
            this.previousMouseDown = previousMouseDown;
            this.previousClickedTextField = previousClickedTextField;
        }

        public void Dispose()
        {
            if (interactionMouseDown &&
                !interactionClickedTextField &&
                !string.IsNullOrEmpty(lastFocusedTextControl))
            {
                GUI.FocusControl(null);
                GUIUtility.keyboardControl = 0;
                EditorGUIUtility.editingTextField = false;
                lastFocusedTextControl = null;

                if (owner is EditorWindow window)
                    window.Repaint();
                else if (owner is Editor editor)
                    editor.Repaint();
            }

            currentInteractionScopeId = previousScopeId;
            interactionMouseDown = previousMouseDown;
            interactionClickedTextField = previousClickedTextField;
        }
    }

    public static InteractionScope PushInteractionScope(
        UnityEngine.Object owner)
    {
        InteractionScope scope = new InteractionScope(
            owner,
            currentInteractionScopeId,
            interactionMouseDown,
            interactionClickedTextField
        );
        currentInteractionScopeId =
            owner != null ? owner.GetInstanceID() : 0;
        interactionMouseDown =
            Event.current.type == EventType.MouseDown &&
            Event.current.button == 0;
        interactionClickedTextField = false;
        return scope;
    }

    public static void Property(SerializedProperty property, string label = null)
    {
        if (property == null)
            return;

        string finalLabel = string.IsNullOrWhiteSpace(label) ? property.displayName : label;

        switch (property.propertyType)
        {
            case SerializedPropertyType.Boolean: PropToolsEditorControls.Toggle(property, finalLabel); break;
            case SerializedPropertyType.String: DrawString(property, finalLabel); break;
            case SerializedPropertyType.Integer: DrawInt(property, finalLabel); break;
            case SerializedPropertyType.Float: DrawFloat(property, finalLabel); break;
            case SerializedPropertyType.ObjectReference: DrawObject(property, typeof(UnityEngine.Object), finalLabel, true); break;
            case SerializedPropertyType.Enum: DrawEnum(property, finalLabel); break;
            case SerializedPropertyType.Vector2: DrawVector2(property, finalLabel); break;
            case SerializedPropertyType.Vector3: DrawVector3(property, finalLabel); break;
            default:
                EditorGUILayout.PropertyField(
                    property,
                    PropToolsEditorTooltips.Content(finalLabel, property),
                    true
                );
                PropToolsEditorTooltips.Track(
                    GUILayoutUtility.GetLastRect(),
                    finalLabel,
                    property
                );
                break;
        }
    }

    public static void TextField(SerializedObject so, string propertyName, string label) { SerializedProperty p = so.FindProperty(propertyName); if (p != null) DrawString(p, label); }
    public static void DelayedTextField(SerializedObject so, string propertyName, string label) { SerializedProperty p = so.FindProperty(propertyName); if (p != null) DrawDelayedString(p, label); }
    public static void IntField(SerializedObject so, string propertyName, string label) { SerializedProperty p = so.FindProperty(propertyName); if (p != null) DrawInt(p, label); }
    public static void FloatField(SerializedObject so, string propertyName, string label) { SerializedProperty p = so.FindProperty(propertyName); if (p != null) DrawFloat(p, label); }
    public static void Vector2Field(SerializedObject so, string propertyName, string label) { SerializedProperty p = so.FindProperty(propertyName); if (p != null) DrawVector2(p, label); }
    public static void Vector3Field(SerializedObject so, string propertyName, string label) { SerializedProperty p = so.FindProperty(propertyName); if (p != null) DrawVector3(p, label); }

    public static T ObjectField<T>(SerializedObject so, string propertyName, string label, bool allowSceneObjects = true) where T : UnityEngine.Object
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p == null) return null;
        DrawObject(p, typeof(T), label, allowSceneObjects);
        return p.objectReferenceValue as T;
    }

    public static void ObjectField(SerializedProperty property, Type objectType, string label = null, bool allowSceneObjects = true)
    {
        if (property != null) DrawObject(property, objectType, label ?? property.displayName, allowSceneObjects);
    }

    public static void EnumField(SerializedProperty property, string label = null)
    {
        if (property != null) DrawEnum(property, label ?? property.displayName);
    }

    public static void Popup(
        SerializedProperty property,
        string label,
        string[] displayOptions,
        string[] values)
    {
        if (property == null || displayOptions == null || values == null ||
            displayOptions.Length == 0 || displayOptions.Length != values.Length)
        {
            return;
        }

        Rect r = BeginRow(label, property);
        int currentIndex = Array.IndexOf(values, property.stringValue);

        if (currentIndex < 0)
            currentIndex = 0;

        DrawFieldBackground(r, r.Contains(Event.current.mousePosition));
        GUI.Label(
            new Rect(r.x + 5f, r.y + 1f, r.width - 24f, r.height - 2f),
            displayOptions[currentIndex],
            PropToolsEditorStyles.CustomPopup
        );
        GUI.Label(
            new Rect(r.xMax - 18f, r.y + 1f, 16f, r.height - 2f),
            "▼",
            PropToolsEditorStyles.MiniButtonLabel
        );

        EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

        if (Event.current.type != EventType.MouseDown ||
            Event.current.button != 0 ||
            !r.Contains(Event.current.mousePosition))
        {
            return;
        }

        GenericMenu menu = new GenericMenu();

        for (int i = 0; i < displayOptions.Length; i++)
        {
            string option = displayOptions[i];
            string value = values[i];
            menu.AddItem(new GUIContent(option), i == currentIndex, () =>
            {
                property.serializedObject.Update();
                property.stringValue = value;
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.DropDown(r);
        Event.current.Use();
    }

    private static void DrawString(SerializedProperty property, string label)
    {
        Rect r = BeginRow(label, property);
        property.stringValue = TextInput(
            r,
            property.stringValue,
            ControlKey(property, "string")
        );
        DrawFocusBorder(r);
    }

    private static void DrawDelayedString(SerializedProperty property, string label)
    {
        Rect r = BeginRow(label, property);
        string controlName = PrepareTextInput(
            r,
            ControlKey(property, "delayed-string")
        );
        property.stringValue = EditorGUI.DelayedTextField(
            r,
            property.stringValue,
            PropToolsEditorStyles.CustomTextField
        );
        SelectAllWhenFocusChanges(controlName);
        DrawFocusBorder(r);
    }

    private static void DrawInt(SerializedProperty property, string label)
    {
        Rect r = BeginRow(label, property);
        string current = property.intValue.ToString();
        string next = TextInput(
            r,
            current,
            ControlKey(property, "integer")
        );
        DrawFocusBorder(r);

        if (next != current && int.TryParse(next, out int parsed))
            property.intValue = parsed;
    }

    private static void DrawFloat(SerializedProperty property, string label)
    {
        Rect r = BeginRow(label, property);
        string current = property.floatValue.ToString("0.###");
        string next = TextInput(
            r,
            current,
            ControlKey(property, "float")
        );
        DrawFocusBorder(r);

        if (next != current && float.TryParse(next, out float parsed))
            property.floatValue = parsed;
    }

    private static void DrawObject(SerializedProperty property, Type type, string label, bool allowSceneObjects)
    {
        Rect r = BeginRow(label, property);
        UnityEngine.Object current = property.objectReferenceValue;
        string display = current != null ? $"{current.name} ({current.GetType().Name})" : "None";

        DrawFieldBackground(r, r.Contains(Event.current.mousePosition));
        GUI.Label(new Rect(r.x + 1f, r.y + 1f, r.width - 22f, r.height - 2f), display, PropToolsEditorStyles.ObjectLabel);

        Rect clearRect = new Rect(r.xMax - 18f, r.y + 2f, 16f, r.height - 4f);
        GUI.Label(clearRect, current != null ? "×" : "◎", PropToolsEditorStyles.MiniButtonLabel);
        HandleObjectFieldEvents(property, type, r, clearRect);
    }

    private static void HandleObjectFieldEvents(SerializedProperty property, Type type, Rect fieldRect, Rect clearRect)
    {
        Event e = Event.current;

        if ((e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && fieldRect.Contains(e.mousePosition))
        {
            UnityEngine.Object dragged = GetFirstValidDraggedObject(type);

            if (dragged != null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    property.objectReferenceValue = dragged;
                }

                e.Use();
            }
        }

        if (e.type == EventType.MouseDown && e.button == 0 && fieldRect.Contains(e.mousePosition))
        {
            if (clearRect.Contains(e.mousePosition))
            {
                property.objectReferenceValue = null;
                e.Use();
                return;
            }

            if (property.objectReferenceValue != null)
            {
                EditorGUIUtility.PingObject(property.objectReferenceValue);
                Selection.activeObject = property.objectReferenceValue;
                e.Use();
            }
        }
    }

    private static UnityEngine.Object GetFirstValidDraggedObject(Type type)
    {
        foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
        {
            if (obj == null) continue;
            if (type.IsInstanceOfType(obj)) return obj;

            GameObject go = obj as GameObject;
            if (go != null && typeof(Component).IsAssignableFrom(type))
            {
                Component component = go.GetComponent(type);
                if (component != null) return component;
            }
        }

        return null;
    }

    private static void DrawEnum(SerializedProperty property, string label)
    {
        Rect r = BeginRow(label, property);
        string current = property.enumDisplayNames[property.enumValueIndex];

        DrawFieldBackground(r, r.Contains(Event.current.mousePosition));
        GUI.Label(new Rect(r.x + 5f, r.y + 1f, r.width - 24f, r.height - 2f), current, PropToolsEditorStyles.CustomPopup);
        GUI.Label(new Rect(r.xMax - 18f, r.y + 1f, 16f, r.height - 2f), "▼", PropToolsEditorStyles.MiniButtonLabel);

        EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < property.enumDisplayNames.Length; i++)
            {
                int index = i;
                string option = property.enumDisplayNames[i];

                menu.AddItem(new GUIContent(option), index == property.enumValueIndex, () =>
                {
                    property.serializedObject.Update();
                    property.enumValueIndex = index;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.DropDown(r);
            Event.current.Use();
        }
    }

    private static void DrawVector2(SerializedProperty property, string label)
    {
        Vector2 v = property.vector2Value;
        Rect r = BeginRow(label, property);
        DrawAxis(r, "X", ref v.x, 0, 2, property);
        DrawAxis(r, "Y", ref v.y, 1, 2, property);
        property.vector2Value = v;
    }

    private static void DrawVector3(SerializedProperty property, string label)
    {
        Vector3 v = property.vector3Value;
        Rect r = BeginRow(label, property);
        DrawAxis(r, "X", ref v.x, 0, 3, property);
        DrawAxis(r, "Y", ref v.y, 1, 3, property);
        DrawAxis(r, "Z", ref v.z, 2, 3, property);
        property.vector3Value = v;
    }

    private static void DrawAxis(
        Rect fullRect,
        string axis,
        ref float value,
        int index,
        int count,
        SerializedProperty property)
    {
        float gap = 5f;
        float axisW = 15f;
        float width = (fullRect.width - gap * (count - 1)) / count;

        Rect rect = new Rect(fullRect.x + (width + gap) * index, fullRect.y, width, fullRect.height);
        Rect labelRect = new Rect(rect.x + 3f, rect.y + 1f, axisW, rect.height - 2f);
        Rect valueRect = new Rect(labelRect.xMax + 2f, rect.y + 2f, rect.width - axisW - 7f, rect.height - 4f);

        DrawFieldBackground(rect, rect.Contains(Event.current.mousePosition));
        GUI.Label(labelRect, axis, PropToolsEditorStyles.ValueLabel);

        string current = value.ToString("0.###");
        string next = TextInput(
            valueRect,
            current,
            ControlKey(property, "axis-" + axis)
        );
        DrawFocusBorder(valueRect);

        if (next != current && float.TryParse(next, out float parsed))
            value = parsed;
    }

    private static Rect BeginRow(string label, SerializedProperty property)
    {
        Rect row = GUILayoutUtility.GetRect(0, 25f, GUILayout.ExpandWidth(true));
        bool hover = row.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint && hover)
            EditorGUI.DrawRect(row, new Color(PropToolsEditorTheme.Accent.r, PropToolsEditorTheme.Accent.g, PropToolsEditorTheme.Accent.b, 0.035f));

        PropToolsEditorHighlight.Draw(property, row);

        Rect labelRect = new Rect(row.x + 4f, row.y + 4f, LabelWidth - 8f, 18f);
        Rect fieldRect = new Rect(row.x + LabelWidth, row.y + 2f, row.width - LabelWidth - 4f, 21f);

        PropToolsEditorDrawing.LabelDivider(
            row,
            fieldRect.x - 7f
        );
        GUI.Label(labelRect, PropToolsEditorTooltips.Content(label, property), PropToolsEditorStyles.PropertyLabel);
        PropToolsEditorTooltips.Track(labelRect, label, property);

        return fieldRect;
    }

    private static void DrawFieldBackground(Rect rect, bool hover)
    {
        PropToolsEditorDrawing.Inset(
            rect,
            hover ? PropToolsEditorTheme.FieldHover : PropToolsEditorTheme.Field,
            hover ? PropToolsEditorTheme.FieldBorderHover : PropToolsEditorTheme.FieldBorder
        );
    }

    private static void DrawFocusBorder(Rect rect)
    {
        if (Event.current.type != EventType.Repaint)
            return;

        if (GUI.GetNameOfFocusedControl() == string.Empty)
            return;
    }

    public static string TextInput(
        Rect rect,
        string value,
        string stableKey)
    {
        string controlName = PrepareTextInput(rect, stableKey);
        string next = GUI.TextField(
            rect,
            value ?? string.Empty,
            PropToolsEditorStyles.CustomTextField
        );
        SelectAllWhenFocusChanges(controlName);
        return next;
    }

    private static string PrepareTextInput(
        Rect rect,
        string stableKey)
    {
        string controlName =
            $"PropTools.Text.{currentInteractionScopeId}.{stableKey}";
        GUI.SetNextControlName(controlName);

        if (interactionMouseDown &&
            rect.Contains(Event.current.mousePosition))
        {
            interactionClickedTextField = true;
        }

        return controlName;
    }

    private static void SelectAllWhenFocusChanges(
        string controlName)
    {
        if (GUI.GetNameOfFocusedControl() != controlName)
            return;

        if (lastFocusedTextControl == controlName)
            return;

        TextEditor editor = GUIUtility.GetStateObject(
            typeof(TextEditor),
            GUIUtility.keyboardControl
        ) as TextEditor;
        editor?.SelectAll();
        lastFocusedTextControl = controlName;
    }

    private static string ControlKey(
        SerializedProperty property,
        string suffix)
    {
        int targetId =
            property?.serializedObject?.targetObject != null
                ? property.serializedObject.targetObject.GetInstanceID()
                : 0;
        return $"{targetId}.{property?.propertyPath}.{suffix}";
    }
}
}
