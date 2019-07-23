using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Khalreon.Attributes
{

    public class Label : CombinablePropertyAttribute, IFullPropertyDrawer
    {
#if UNITY_EDITOR
        public void DrawProperty(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            // Calculate rects
            var nameRect = new Rect(rect.x, rect.y, rect.width - 10, rect.height);

            string propertyText = property.stringValue;

            EditorGUI.LabelField(nameRect, propertyText);

            EditorGUI.EndProperty();
        }

        public override IEnumerable<SerializedPropertyType> SupportedTypes
        {
            get
            {
                yield return SerializedPropertyType.String;
            }
        }
#endif
    }
}