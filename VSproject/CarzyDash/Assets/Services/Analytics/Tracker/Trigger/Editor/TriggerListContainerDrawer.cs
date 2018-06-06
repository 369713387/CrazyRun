using System;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [CustomPropertyDrawer (typeof(TriggerListContainer))]
    public class TriggerListContainerDrawer : ListContainerDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI (position, property, label);
        }

        protected override string GetListName ()
        {
            return "m_Rules";
        }

        protected override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight * 5;
        }

        protected override void DrawHeader(Rect headerRect)
        {
            headerRect.height = 16;
            string headerText = string.Concat ("Rules: ", m_FieldsArray.arraySize);
            GUI.Label(headerRect, headerText);
        }

        protected override void AddElement(ReorderableList list)
        {
            base.AddElement(list);

            if (m_FieldsArray.arraySize == 0)
                return;

            var field = m_FieldsArray.GetArrayElementAtIndex(list.index);

            var m_Operator = field.FindPropertyRelative("m_Operator");
            m_Operator.enumValueIndex = 0;

            var m_ValueProp = field.FindPropertyRelative("m_Value");
            var m_FixedType = m_ValueProp.FindPropertyRelative("m_FixedType");
            var m_PropertyType = m_ValueProp.FindPropertyRelative("m_PropertyType");
            var m_ValueType = m_ValueProp.FindPropertyRelative("m_ValueType");
            var m_Value = m_ValueProp.FindPropertyRelative("m_Value");
            m_FixedType.boolValue = false;
            m_PropertyType.enumValueIndex = (int)ValueProperty.PropertyType.Static;
            m_ValueType.stringValue = typeof(string).ToString();
            m_Value.stringValue = "";

            var m_ValueProp2 = field.FindPropertyRelative("m_Value");
            var m_FixedType2 = m_ValueProp2.FindPropertyRelative("m_FixedType");
            var m_PropertyType2 = m_ValueProp2.FindPropertyRelative("m_PropertyType");
            var m_ValueType2 = m_ValueProp2.FindPropertyRelative("m_ValueType");
            var m_ValuePropValue2 = m_ValueProp2.FindPropertyRelative("m_Value");
            m_FixedType2.boolValue = false;
            m_PropertyType2.enumValueIndex = (int)ValueProperty.PropertyType.Static;
            m_ValueType2.stringValue = typeof(string).ToString();
            m_ValuePropValue2.stringValue = "";

            UpdateDisplayAdd(list);
        }

        protected override void RemoveButton(ReorderableList list)
        {
            base.RemoveButton(list);
            UpdateDisplayAdd(list);
        }

        void UpdateDisplayAdd(ReorderableList list)
        {
            list.displayAdd = (m_FieldsArray.arraySize < AnalyticsEventTrackerSettings.triggerRuleCountMax);
        }
    }
}

