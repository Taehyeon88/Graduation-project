#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (GetConditionResult((ShowIfAttribute)attribute, property))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (GetConditionResult((ShowIfAttribute)attribute, property))
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        return -EditorGUIUtility.standardVerticalSpacing;
    }

    private bool GetConditionResult(ShowIfAttribute showIf, SerializedProperty property)
    {
        foreach (var conditionFieldName in showIf.conditionFieldNames)
        {
            SerializedProperty conditionProperty = FindSiblingProperty(property, conditionFieldName);

            if (conditionProperty == null)
                return false;

            if (conditionProperty.propertyType != SerializedPropertyType.Boolean)
                return false;

            if (!conditionProperty.boolValue)
                return false;
        }

        return true;
    }

    private SerializedProperty FindSiblingProperty(SerializedProperty property, string siblingName)
    {
        string path = property.propertyPath;
        int lastDotIndex = path.LastIndexOf('.');

        if (lastDotIndex < 0)
        {
            return property.serializedObject.FindProperty(siblingName);
        }

        string parentPath = path.Substring(0, lastDotIndex);
        string siblingPath = parentPath + "." + siblingName;

        return property.serializedObject.FindProperty(siblingPath);
    }
}
#endif