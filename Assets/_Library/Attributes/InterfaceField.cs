using UnityEngine;

namespace Khalreon.Attributes
{

    public class InterfaceFieldAttribute : CombinablePropertyAttribute, IFullPropertyDrawer
    {
        public readonly string name;

        public readonly System.Type type;

        public InterfaceFieldAttribute(string name, System.Type type)
        {
            this.name = name;
            this.type = type;
        }

#if UNITY_EDITOR
        public void DrawProperty(Rect rect, UnityEditor.SerializedProperty property,
                                 GUIContent label)
        {
            label.text = name;

            property.objectReferenceValue = UnityEditor.EditorGUI.ObjectField(rect, label , property.objectReferenceValue, type, true);

            if(property.type != type.ToString())
            {
                property.objectReferenceValue = null;
            }
        }
#endif
    }
}
