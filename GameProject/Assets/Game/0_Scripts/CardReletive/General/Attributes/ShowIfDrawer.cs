#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        bool shouldShow = GetConditionResult(showIf, property);

        if (shouldShow)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        bool shouldShow = GetConditionResult(showIf, property);

        if (shouldShow)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            return 0f;
        }
    }

    private bool GetConditionResult(ShowIfAttribute showIf, SerializedProperty property)
    {
        object target = property.serializedObject.targetObject;

        foreach (var conditionFieldName in showIf.conditionFieldNames)
        {
            FieldInfo field = target.GetType().GetField(conditionFieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field == null)
                return false;

            if (field.FieldType != typeof(bool))
                return false;

            bool value = (bool)field.GetValue(target);
            if (!value)
                return false;
        }

        return true;
    }
}
#endif