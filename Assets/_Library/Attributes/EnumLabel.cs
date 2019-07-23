using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Khalreon.Attributes
{

    public class EnumLabel : CombinablePropertyAttribute, IFullPropertyDrawer
    {
#if UNITY_EDITOR
        public void DrawProperty(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            // Draw label
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

            // Calculate rects
            var nameRect = new Rect(rect.x, rect.y, 90, rect.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            string propertyName = property.name;
            int value = property.enumValueIndex;
            string[] enumNames = property.enumDisplayNames;
            
            EditorGUI.LabelField(nameRect, enumNames[value]);

            EditorGUI.EndProperty();
        }
#endif
    }
}

