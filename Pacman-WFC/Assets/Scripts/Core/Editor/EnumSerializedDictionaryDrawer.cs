using UnityEditor;
using UnityEngine;

namespace JFlex.Core.Editor
{
    [CustomPropertyDrawer(typeof(EnumSerializedDictionary<,>), true)]
    public class EnumSerializedDictionaryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            var keysProp = property.FindPropertyRelative("keys");
            var valuesProp = property.FindPropertyRelative("values");

            for (int i = 0; i < keysProp.arraySize; i++)
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var key = keysProp.GetArrayElementAtIndex(i);
                var value = valuesProp.GetArrayElementAtIndex(i);

                string enumName = key.enumDisplayNames[key.enumValueIndex];

                // Prevents InspectorSpriteDrawer code from drawing sprites used in the enum dictionary
                if (value.propertyType == SerializedPropertyType.ObjectReference &&
                    value.objectReferenceValue is Sprite)
                {
                    value.objectReferenceValue = EditorGUI.ObjectField(
                        position,
                        enumName,
                        value.objectReferenceValue,
                        typeof(Sprite),
                        false
                    );
                }
                else
                {
                    EditorGUI.PropertyField(position, value, new GUIContent(enumName));
                }
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            var valuesProp = property.FindPropertyRelative("values");

            return EditorGUIUtility.singleLineHeight +
                   (valuesProp.arraySize + 1) *
                   (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }
    }
}