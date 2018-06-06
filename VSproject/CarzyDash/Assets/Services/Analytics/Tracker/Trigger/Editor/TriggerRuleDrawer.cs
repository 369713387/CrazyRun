using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


namespace UnityEngine.Analytics.Experimental.Tracker
{
    [CustomPropertyDrawer (typeof(TriggerRule))]
    class TriggerRuleDrawer : PropertyDrawer {

        GUIContent targetLabelContent = new GUIContent ("Target", "The object and property to watch for signs of a match.");
        float terminalHeight = EditorGUIUtility.singleLineHeight;

        SerializedProperty m_Target;
        SerializedProperty m_Operator;
        SerializedProperty m_Value;
        SerializedProperty m_Value2;
        SerializedProperty m_TargetPropertyType;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            m_Target = property.FindPropertyRelative("m_Target");
            m_Operator = property.FindPropertyRelative("m_Operator");
            m_Value = property.FindPropertyRelative("m_Value");
            m_Value2 =property.FindPropertyRelative("m_Value2");
            m_TargetPropertyType = m_Target.FindPropertyRelative("m_Type");

            SerializedProperty m_EnumTypeIsCustomizable = m_Value.FindPropertyRelative("m_EnumTypeIsCustomizable");
            m_EnumTypeIsCustomizable.boolValue = false;

            Rect targetLabelRect = new Rect(position.x, position.y, AnalyticsEventTrackerEditor.k_LeftListMargin, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField (targetLabelRect, targetLabelContent);

            Rect targetPropRect = new Rect (position.x + AnalyticsEventTrackerEditor.k_LeftListMargin,
                targetLabelRect.y,
                position.width - targetLabelRect.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(targetPropRect, m_Target);
            terminalHeight = EditorGUI.GetPropertyHeight (m_Target);

            position.y += SingleLine();
            terminalHeight += SingleLine();
            terminalHeight += OperatorGUI (position, property);
            position.y += SingleLine();

            if (BetweenLayout ()) {
                position.width = (position.width * .5f) - 5f;
                position.x += position.width + 10f;
                SecondValueGUI (position, property);
                position.x -= position.width + 10f;
            }
            terminalHeight += FirstValueGUI (position, property);
        }

        float FirstValueGUI(Rect position, SerializedProperty property)
        {
            SetTypeForValueProperty(m_Value);
            EditorGUI.PropertyField (position, m_Value);
            return SingleLine() * 2f;
        }

        void SecondValueGUI(Rect position, SerializedProperty property)
        {
            SetTypeForValueProperty(m_Value2);
            EditorGUI.PropertyField (position, m_Value2);
        }

        float OperatorGUI(Rect position, SerializedProperty property)
        {
            Rect rect = new Rect(position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            {
                if (m_TargetPropertyType.stringValue == typeof(int).ToString()
                    || m_TargetPropertyType.stringValue == typeof(short).ToString()
                    || m_TargetPropertyType.stringValue == typeof(long).ToString()
                    || m_TargetPropertyType.stringValue == typeof(float).ToString()
                    || m_TargetPropertyType.stringValue == typeof(double).ToString()
                    || m_TargetPropertyType.stringValue == typeof(decimal).ToString()
                    || string.IsNullOrEmpty(m_TargetPropertyType.stringValue))
                {
                    List<string> operatorList = new List<string>(m_Operator.enumNames);
                    string operatorStr = operatorList[m_Operator.enumValueIndex];
                    TriggerOperator operatorEnum = (TriggerOperator)Enum.Parse(typeof(TriggerOperator), operatorStr);
                    operatorEnum = (TriggerOperator)EditorGUI.EnumPopup(rect, operatorEnum);
                    if (EditorGUI.EndChangeCheck())
                        m_Operator.enumValueIndex = operatorList.IndexOf(operatorEnum.ToString());
                }
                else
                {
                    List<string> operatorList = new List<string>(m_Operator.enumNames);
                    string operatorStr = operatorList[m_Operator.enumValueIndex];
                    TriggerOperator operatorEnum = (TriggerOperator)Enum.Parse(typeof(TriggerOperator), operatorStr);
                    if (m_Operator.enumValueIndex > 1)
                    {
                        m_Operator.enumValueIndex = 1;
                    }
                    operatorEnum = (TriggerOperator)EditorGUI.Popup(rect, m_Operator.enumValueIndex, new string[] { "Equals", "Does Not Equal" });
                    if (EditorGUI.EndChangeCheck())
                        m_Operator.enumValueIndex = operatorList.IndexOf(operatorEnum.ToString());
                }

                return EditorGUI.GetPropertyHeight (property);
            }
        }

        void SetTypeForValueProperty (SerializedProperty valProp)
        {
            if (!string.IsNullOrEmpty(m_TargetPropertyType.stringValue))
            {
                SerializedProperty trackableField = valProp.FindPropertyRelative("m_Target");
                valProp.FindPropertyRelative("m_FixedType").boolValue = true;
                ValueProperty.PropertyType propertyType = (ValueProperty.PropertyType)valProp.FindPropertyRelative("m_PropertyType").enumValueIndex;

                if (trackableField.FindPropertyRelative("m_Type").stringValue != m_TargetPropertyType.stringValue)
                {
                    trackableField.FindPropertyRelative("m_Type").stringValue = m_TargetPropertyType.stringValue;
                    // switching type of the target will null out the value side
                    switch (propertyType)
                    {
                        case ValueProperty.PropertyType.Dynamic:
                            trackableField.FindPropertyRelative("m_Path").stringValue = string.Empty;
                            break;
                        case ValueProperty.PropertyType.Static:
                            valProp.FindPropertyRelative("m_Value").stringValue = string.Empty;
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(m_Target.FindPropertyRelative("m_EnumType").stringValue))
                {
                    valProp.FindPropertyRelative("m_EnumType").stringValue = m_Target.FindPropertyRelative("m_EnumType").stringValue;
                    if (propertyType == ValueProperty.PropertyType.Dynamic &&
                        valProp.FindPropertyRelative("m_Target").FindPropertyRelative("m_EnumType").stringValue != valProp.FindPropertyRelative("m_EnumType").stringValue)
                    {
                        valProp.FindPropertyRelative("m_Target").FindPropertyRelative("m_Path").stringValue = null;
                    }
                }
                var validTypeNames = trackableField.FindPropertyRelative("m_ValidTypeNames");
                validTypeNames.ClearArray();
                validTypeNames.InsertArrayElementAtIndex(0);
                var element = validTypeNames.GetArrayElementAtIndex(0);
                element.stringValue = m_TargetPropertyType.stringValue;
                valProp.FindPropertyRelative("m_ValueType").stringValue = m_TargetPropertyType.stringValue;
            }
            else
            {
                valProp.FindPropertyRelative("m_FixedType").boolValue = false;
            }
        }

        bool BetweenLayout()
        {
            return m_Operator.enumValueIndex == TriggerOperator.IsBetween.GetHashCode () ||
                m_Operator.enumValueIndex == TriggerOperator.IsBetweenOrEqualTo.GetHashCode ();
        }

        float SingleLine()
        {
            return EditorGUIUtility.singleLineHeight + AnalyticsEventTrackerEditor.k_LineMargin;
        }

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            return terminalHeight;
        }
    }
}
