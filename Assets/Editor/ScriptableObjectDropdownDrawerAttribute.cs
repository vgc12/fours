#if UNITY_EDITOR


using Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(ScriptableObjectDropdownAttribute), true)]
    public class ScriptableObjectDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var totalHeight = EditorGUIUtility.singleLineHeight;

            if (property.objectReferenceValue == null || !property.isExpanded)
                return totalHeight;

            var data = property.objectReferenceValue as ScriptableObject;
            if (data == null) return totalHeight;

            var serializedObject = new SerializedObject(data);
            var prop = serializedObject.GetIterator();

            if (prop.NextVisible(true))
                do
                {
                    if (prop.name == "m_Script") continue;
                    var height = EditorGUI.GetPropertyHeight(prop, null, true);
                    totalHeight += height + EditorGUIUtility.standardVerticalSpacing;
                } while (prop.NextVisible(false));

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);


            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, 15, EditorGUIUtility.singleLineHeight),
                property.isExpanded, GUIContent.none);

            var objectRect = new Rect(position.x + 15, position.y, position.width - 15,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(objectRect, property, label, true);

            if (property.isExpanded && property.objectReferenceValue != null)
            {
                var data = property.objectReferenceValue as ScriptableObject;
                if (data)
                {
                    // Create indented area for ScriptableObject properties
                    EditorGUI.indentLevel++;

                    var serializedObject = new SerializedObject(data);
                    serializedObject.Update();

                    var yPos = position.y + EditorGUIUtility.singleLineHeight +
                               EditorGUIUtility.standardVerticalSpacing;

                    var prop = serializedObject.GetIterator();
                    if (prop.NextVisible(true))
                        do
                        {
                            // Skip the script reference
                            if (prop.name == "m_Script") continue;

                            var height = EditorGUI.GetPropertyHeight(prop, null, true);
                            var propRect = new Rect(position.x, yPos, position.width, height);

                            EditorGUI.BeginChangeCheck();
                            EditorGUI.PropertyField(propRect, prop, true);

                            yPos += height + EditorGUIUtility.standardVerticalSpacing;
                        } while (prop.NextVisible(false));

                    // Apply changes to the ScriptableObject
                    if (serializedObject.hasModifiedProperties)
                    {
                        serializedObject.ApplyModifiedProperties();
                        // Mark the asset as dirty so changes are saved
                        EditorUtility.SetDirty(data);
                    }

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}


#endif