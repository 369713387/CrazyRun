using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics.Experimental.Tracker;

namespace UnityEditor.Analytics.Experimental.Tracker
{
    [CustomPropertyDrawer (typeof(AnalyticsEventParam))]
    public class AnalyticsEventParamDrawer : PropertyDrawer 
    {
        GUIContent nameLabelContent = new GUIContent("Name", "The unique key for this parameter. By convention, keys are lower_snake_case.");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty m_Value = property.FindPropertyRelative("m_Value");
            SerializedProperty m_Name = property.FindPropertyRelative("m_Name");
            SerializedProperty m_RequirementType = property.FindPropertyRelative("m_RequirementType");

            position.y += 2f * AnalyticsEventTrackerEditor.k_LineMargin;

            Rect nameLabelRect = position;
            nameLabelRect.width = AnalyticsEventTrackerEditor.k_LeftListMargin;
            nameLabelRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField (nameLabelRect, nameLabelContent);

            Rect nameRect = position;
            nameRect.width -= nameLabelRect.width;
            nameRect.x += nameLabelRect.width;
            nameRect.height = nameLabelRect.height;
            AnalyticsEventParam.RequirementType requirement = (AnalyticsEventParam.RequirementType)m_RequirementType.enumValueIndex;
            EditorGUI.BeginDisabledGroup (requirement != AnalyticsEventParam.RequirementType.None);
            switch (requirement)
            {
                case AnalyticsEventParam.RequirementType.Required:
                    EditorGUI.TextField(nameRect, m_Name.stringValue + " (required)");
                    break;
                case AnalyticsEventParam.RequirementType.Optional:
                    EditorGUI.TextField(nameRect, m_Name.stringValue);
                    break;
                case AnalyticsEventParam.RequirementType.None:
                default:
                    m_Name.stringValue = EditorGUI.TextField(nameRect, m_Name.stringValue);
                    break;
            }
            EditorGUI.EndDisabledGroup ();

            position.y += EditorGUIUtility.singleLineHeight + AnalyticsEventTrackerEditor.k_LineMargin;
            EditorGUI.PropertyField (position, m_Value);

            if (nameRect.Contains (Event.current.mousePosition)) {
                ShowParamNameTooltip (nameRect, property);
            }
        }

        void ShowParamNameTooltip(Rect position, SerializedProperty property)
        {
            string tooltip = property.FindPropertyRelative ("m_Tooltip").stringValue;
            GUI.Box(position, new GUIContent("", tooltip), GUIStyle.none);
        }
    }
}
