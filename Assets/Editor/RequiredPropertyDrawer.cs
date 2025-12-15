#if UNITY_EDITOR

using Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        private bool IsScriptableObjectProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference &&
                   property.objectReferenceValue is ScriptableObject;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var requiredAttribute = (RequiredAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            var currentY = position.y;

            // Handle ScriptableObject expansion if applicable
            if (IsScriptableObjectProperty(property))
            {
                // Draw foldout and property field
                property.isExpanded = EditorGUI.Foldout(
                    new Rect(position.x, currentY, 15, EditorGUIUtility.singleLineHeight),
                    property.isExpanded, GUIContent.none);

                var objectRect = new Rect(position.x + 15, currentY, position.width - 15,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(objectRect, property, label, false);

                currentY += EditorGUIUtility.singleLineHeight;

                // Draw expanded ScriptableObject properties
                if (property.isExpanded && property.objectReferenceValue != null)
                {
                    var data = property.objectReferenceValue as ScriptableObject;
                    if (data)
                    {
                        EditorGUI.indentLevel++;
                        var serializedObject = new SerializedObject(data);
                        serializedObject.Update();

                        var prop = serializedObject.GetIterator();
                        if (prop.NextVisible(true))
                            do
                            {
                                if (prop.name == "m_Script") continue;

                                var height = EditorGUI.GetPropertyHeight(prop, null, true);
                                var propRect = new Rect(position.x, currentY, position.width, height);

                                EditorGUI.BeginChangeCheck();
                                EditorGUI.PropertyField(propRect, prop, true);
                                currentY += height + EditorGUIUtility.standardVerticalSpacing;
                            } while (prop.NextVisible(false));

                        if (serializedObject.hasModifiedProperties)
                        {
                            serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty(data);
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }
            else
            {
                // Standard property field for non-ScriptableObject properties
                var propertyRect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(propertyRect, property, label);
                currentY += EditorGUIUtility.singleLineHeight;
            }

            EditorGUI.EndProperty();

            // Show error below if property is null
            if (property.objectReferenceValue == null)
            {
                var helpBoxRect = new Rect(
                    position.x,
                    currentY + 2,
                    position.width,
                    EditorGUIUtility.singleLineHeight);

                EditorGUI.HelpBox(helpBoxRect, requiredAttribute.ErrorMessage, MessageType.Error);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight; // Property field height

            // Add height for ScriptableObject expansion
            if (IsScriptableObjectProperty(property) && property.isExpanded && property.objectReferenceValue != null)
            {
                var data = property.objectReferenceValue as ScriptableObject;
                if (data != null)
                {
                    var serializedObject = new SerializedObject(data);
                    var prop = serializedObject.GetIterator();

                    if (prop.NextVisible(true))
                        do
                        {
                            if (prop.name == "m_Script") continue;
                            var propHeight = EditorGUI.GetPropertyHeight(prop, null, true);
                            height += propHeight + EditorGUIUtility.standardVerticalSpacing;
                        } while (prop.NextVisible(false));
                }
            }

            // Add height for error message if property is null
            if (property.objectReferenceValue == null)
                height += EditorGUIUtility.singleLineHeight + 2; // Error message height

            return height;
        }
    }
}

#endif