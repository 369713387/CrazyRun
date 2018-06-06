using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    enum BoolPopup {
        True,
        False
    }

    enum ValueTypePopup {
        STRING,
        FLOAT,
        LONG,
        INT32,
        BOOLEAN,
        ENUM
    }

    [CustomPropertyDrawer (typeof(ValueProperty))]
    class ValuePropertyDrawer : PropertyDrawer
    {
        GUIContent valueLabelContent = new GUIContent("Value", "The resulting value. Values can be either static (set by you in the field below) or dynamic (attached to the properties of a GameObject component).");
        GUIContent andLabelContent = new GUIContent("And");

        float terminalHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect valueLabelRect = new Rect(position.x,
                position.y,
                AnalyticsEventTrackerEditor.k_LeftListMargin,
                EditorGUIUtility.singleLineHeight);

            GUIContent labelContent = position.x > position.width * .5f ? andLabelContent : valueLabelContent;
            EditorGUI.LabelField (valueLabelRect, labelContent);

            ValueProperty.PropertyType propertyType = DynamicCheckboxGUI (position, property);
            string valueType = TypeGUI (position, property, propertyType);
            position.y += EditorGUIUtility.singleLineHeight + AnalyticsEventTrackerEditor.k_LineMargin;
            ValueGUI(position, property, propertyType, valueType);

            terminalHeight = position.height * 2f;
        }

        ValueProperty.PropertyType DynamicCheckboxGUI(Rect position, SerializedProperty property)
        {
            float width = (position.width - AnalyticsEventTrackerEditor.k_LeftListMargin) * .5f;
            SerializedProperty m_PropertyType = property.FindPropertyRelative("m_PropertyType");
            Rect rect = new Rect(position.x + AnalyticsEventTrackerEditor.k_LeftListMargin,
                position.y,
                width,
                EditorGUIUtility.singleLineHeight
            );

            ValueProperty.PropertyType oldPropertyType = (ValueProperty.PropertyType)m_PropertyType.enumValueIndex;

            SerializedProperty m_CanDisable = property.FindPropertyRelative("m_CanDisable");
            if (m_CanDisable.boolValue)
            {
                m_PropertyType.enumValueIndex = EditorGUI.Popup (rect, m_PropertyType.enumValueIndex, Enum.GetNames(typeof(ValueProperty.PropertyType)));
            }
            else
            {
                List<string> names = new List<string>(Enum.GetNames(typeof(ValueProperty.PropertyType)));
                names.RemoveAt((int)ValueProperty.PropertyType.Disabled);
                int index = m_PropertyType.enumValueIndex - 1;
                index = EditorGUI.Popup(rect, index, names.ToArray());
                m_PropertyType.enumValueIndex = index + 1;
            }

            // if we are switching away from dynamic then clear out the dynamic values
            if ((ValueProperty.PropertyType)m_PropertyType.enumValueIndex != oldPropertyType &&
                oldPropertyType == ValueProperty.PropertyType.Dynamic)
            {
                SerializedProperty m_Target = property.FindPropertyRelative("m_Target");
                m_Target.FindPropertyRelative("m_Type").stringValue = null;
                m_Target.FindPropertyRelative("m_Path").stringValue = string.Empty;
            }

            return (ValueProperty.PropertyType)m_PropertyType.enumValueIndex;
        }

        string TypeGUI(Rect position, SerializedProperty property, ValueProperty.PropertyType propertyType)
        {
            float width = (position.width - AnalyticsEventTrackerEditor.k_LeftListMargin) * .5f;
            Rect rect = new Rect(position.x + AnalyticsEventTrackerEditor.k_LeftListMargin + width,
                position.y,
                width,
                EditorGUIUtility.singleLineHeight
            );

            SerializedProperty m_ValueType = property.FindPropertyRelative("m_ValueType");
            SerializedProperty m_FixedType = property.FindPropertyRelative("m_FixedType");
            SerializedProperty m_Target = property.FindPropertyRelative("m_Target");

            if (property.FindPropertyRelative("m_FixedType").boolValue)
            {
                var validTypeNames = m_Target.FindPropertyRelative("m_ValidTypeNames");
                validTypeNames.ClearArray();
                validTypeNames.InsertArrayElementAtIndex(0);
                var element = validTypeNames.GetArrayElementAtIndex(0);
                element.stringValue = m_ValueType.stringValue;
                m_Target.FindPropertyRelative("m_EnumType").stringValue = property.FindPropertyRelative("m_EnumType").stringValue;
                if (property.FindPropertyRelative("m_EnumTypeIsCustomizable").boolValue && propertyType == ValueProperty.PropertyType.Dynamic)
                {
                    validTypeNames.InsertArrayElementAtIndex(1);
                    element = validTypeNames.GetArrayElementAtIndex(1);
                    element.stringValue = typeof(string).ToString();
                }
            }
            else
            {
                var validTypeNames = m_Target.FindPropertyRelative("m_ValidTypeNames");
                validTypeNames.ClearArray();
                validTypeNames.InsertArrayElementAtIndex(0);
                var element = validTypeNames.GetArrayElementAtIndex(0);
                element.stringValue = typeof(int).ToString();
                validTypeNames.InsertArrayElementAtIndex(1);
                element = validTypeNames.GetArrayElementAtIndex(1);
                element.stringValue = typeof(bool).ToString();
                validTypeNames.InsertArrayElementAtIndex(2);
                element = validTypeNames.GetArrayElementAtIndex(2);
                element.stringValue = typeof(decimal).ToString();
                validTypeNames.InsertArrayElementAtIndex(3);
                element = validTypeNames.GetArrayElementAtIndex(3);
                element.stringValue = typeof(double).ToString();
                validTypeNames.InsertArrayElementAtIndex(4);
                element = validTypeNames.GetArrayElementAtIndex(4);
                element.stringValue = typeof(float).ToString();
                validTypeNames.InsertArrayElementAtIndex(5);
                element = validTypeNames.GetArrayElementAtIndex(5);
                element.stringValue = typeof(long).ToString();
                validTypeNames.InsertArrayElementAtIndex(6);
                element = validTypeNames.GetArrayElementAtIndex(6);
                element.stringValue = typeof(short).ToString();
                validTypeNames.InsertArrayElementAtIndex(7);
                element = validTypeNames.GetArrayElementAtIndex(7);
                element.stringValue = typeof(string).ToString();
                if(propertyType == ValueProperty.PropertyType.Dynamic)
                {
                    validTypeNames.InsertArrayElementAtIndex(8);
                    element = validTypeNames.GetArrayElementAtIndex(8);
                    element.stringValue = "enum";
                }
            }

            string selectedType = m_Target.FindPropertyRelative("m_Type").stringValue;

            EditorGUI.BeginDisabledGroup (propertyType == ValueProperty.PropertyType.Dynamic || m_FixedType.boolValue);
            if (!string.IsNullOrEmpty(m_ValueType.stringValue) && CustomEnumPopup.GetEnumType(m_ValueType.stringValue) != null)
            {
                property.FindPropertyRelative("m_EnumType").stringValue = m_ValueType.stringValue;
                m_ValueType.stringValue = "enum";
            }
            List<string> typePopupStrings = new List<string>();
            var validTypes = m_Target.FindPropertyRelative("m_ValidTypeNames");
            int selectedVal = 0;
            for (int i = 0; i < validTypes.arraySize; i++)
            {
                var t = validTypes.GetArrayElementAtIndex(i);
                var friendlyString = GetFriendlyStringFromTypeString(t.stringValue);
                if (!string.IsNullOrEmpty(friendlyString) && !typePopupStrings.Contains(friendlyString))
                {
                    typePopupStrings.Add(friendlyString);
                    selectedVal = typePopupStrings.IndexOf(friendlyString);
                }
            }

            if(!string.IsNullOrEmpty(selectedType))
            {
                selectedVal = typePopupStrings.IndexOf(GetFriendlyStringFromTypeString(selectedType));
            }

            var newValIndex = EditorGUI.Popup(rect, selectedVal, typePopupStrings.ToArray());
            if (newValIndex < 0 || newValIndex >= typePopupStrings.Count)
            {
                newValIndex = typePopupStrings.Count - 1;
            }
            if(typePopupStrings.Count > 0)
            {
                var newVal = typePopupStrings[newValIndex];
                switch (newVal)
                {
                    case "String":
                        m_ValueType.stringValue = typeof(string).ToString();
                        break;
                    case "Int":
                        m_ValueType.stringValue = typeof(int).ToString();
                        break;
                    case "Float":
                        m_ValueType.stringValue = typeof(float).ToString();
                        break;
                    case "Bool":
                        m_ValueType.stringValue = typeof(bool).ToString();
                        break;
                    case "Enum":
                        m_ValueType.stringValue = "enum";
                        break;
                }
            }

            m_Target.FindPropertyRelative("m_Type").stringValue = m_ValueType.stringValue;
           
            EditorGUI.EndDisabledGroup ();

            return m_ValueType.stringValue;
        }

        string GetFriendlyStringFromTypeString (string typeStr)
        {
            if (typeStr == typeof(string).ToString())
            {
                return("String");
            }
            else if (typeStr == typeof(int).ToString() || typeStr == typeof(short).ToString() || typeStr == typeof(long).ToString())
            {
                return("Int");
            }
            else if (typeStr == typeof(float).ToString() || typeStr == typeof(double).ToString() || typeStr == typeof(decimal).ToString())
            {
                return("Float");
            }
            else if (typeStr == typeof(bool).ToString())
            {
                return("Bool");
            }
            else if (typeStr == "enum")
            {
                return("Enum");
            }
            else
            {
                return null;
            }
        }

        void ValueGUI(Rect position, SerializedProperty property, ValueProperty.PropertyType propertyType, string valueType)
        {
            SerializedProperty m_Target = property.FindPropertyRelative("m_Target");
            SerializedProperty m_Value = property.FindPropertyRelative("m_Value");

            Rect rect = new Rect (position.x + AnalyticsEventTrackerEditor.k_LeftListMargin,
                position.y,
                position.width - (AnalyticsEventTrackerEditor.k_LeftListMargin),
                EditorGUIUtility.singleLineHeight);

            switch (propertyType)
            {
                case ValueProperty.PropertyType.Dynamic:
                    
                    EditorGUI.PropertyField(rect, m_Target, GUIContent.none);
                    break;

                case ValueProperty.PropertyType.Static:
                    if (valueType == typeof(string).ToString())
                    {
                        m_Value.stringValue = EditorGUI.TextField(rect, m_Value.stringValue);
                    }
                    else if (valueType == typeof(bool).ToString())
                    {
                        if (m_Value.stringValue != "True" && m_Value.stringValue != "False")
                        {
                            m_Value.stringValue = "True";
                        }
                        BoolPopup boolPopup = (BoolPopup)Enum.Parse(typeof(BoolPopup), m_Value.stringValue);
                        boolPopup = (BoolPopup)EditorGUI.EnumPopup(rect, boolPopup);
                        m_Value.stringValue = boolPopup.ToString();
                    }
                    else if (valueType == typeof(int).ToString() || valueType == typeof(short).ToString() || valueType == typeof(long).ToString())
                    {
                        var intValue = 0;
                        int.TryParse(m_Value.stringValue, out intValue);
                        intValue = EditorGUI.IntField(rect, intValue);
                        m_Value.stringValue = intValue.ToString();
                    }
                    else if (valueType == typeof(float).ToString() || valueType == typeof(double).ToString() || valueType == typeof(decimal).ToString())
                    {
                        var floatValue = 0f;
                        float.TryParse(m_Value.stringValue, out floatValue);
                        floatValue = EditorGUI.FloatField(rect, floatValue);
                        m_Value.stringValue = floatValue.ToString();
                    }
                    else if (valueType == "enum")
                    {
                        m_Value.stringValue = CustomEnumPopup.Popup(rect, property);
                    }

                    break;
                case ValueProperty.PropertyType.Disabled:
                default:
                    break;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return terminalHeight;
        }
    }
}

