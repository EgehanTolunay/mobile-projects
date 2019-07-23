using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Khalreon.Attributes
{
    public class BoolLabel : CombinablePropertyAttribute, IFullPropertyDrawer
    {
#if UNITY_EDITOR
        public void DrawProperty(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            // Draw label
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

            // Calculate rects
            var nameRect = new Rect(rect.x, rect.y, 90, rect.height);

            if (property.propertyType == SerializedPropertyType.Boolean)
            {
                bool value = false;
                value = property.boolValue;

                EditorGUI.LabelField(nameRect, value.ToString());
            }
            else
            {
                EditorGUI.LabelField(nameRect, "[Type != Boolean]");
            }

            EditorGUI.EndProperty();
        }
#endif
    }
}