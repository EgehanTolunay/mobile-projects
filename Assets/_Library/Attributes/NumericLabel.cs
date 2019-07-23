using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Khalreon.Attributes
{

    public class NumericLabel : CombinablePropertyAttribute, IFullPropertyDrawer
    {
#if UNITY_EDITOR
        public void DrawProperty(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            // Draw label
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

            // Calculate rects
            var nameRect = new Rect(rect.x, rect.y, 90, rect.height);

            float value = 0;
            if (property.propertyType == SerializedPropertyType.Float)
            {
                value = property.floatValue;
            }
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                value = property.intValue;
            }

            EditorGUI.LabelField(nameRect, value.ToString());

            EditorGUI.EndProperty();
        }
#endif
    }
}
