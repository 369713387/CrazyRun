using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;


namespace UnityEngine.Analytics.Experimental.Tracker
{
    [CustomPropertyDrawer (typeof(EventTrigger))]
    class EventTriggerDrawer : PropertyDrawer {

        static GUIContent[] triggerTypeContent = new GUIContent[] {
            new GUIContent("Lifecycle", EditorGUIUtility.ObjectContent(null, typeof(GameObject)).image),
            new GUIContent("UI", EditorGUIUtility.ObjectContent(null, typeof(Button)).image),
            new GUIContent("Timer", EditorGUIUtility.IconContent("SpeedScale").image),
            // new GUIContent("Method", EditorGUIUtility.ObjectContent(null, typeof(MonoScript)).image)
        };

        GUIContent lifecycleLabelContent = new GUIContent ("Lifecycle Event", "An event in the MonoBehaviour standard lifecycle which you'd like to use as a trigger to send this event.");
        GUIContent boolLabelContent = new GUIContent ("Match", "Does the event fire when all rules match, when any rule matches, or when no rules match?");
        GUIContent applyRulesLabelContent = new GUIContent("Apply Rules", "When triggering, test against a set of rules and fire only if the terms are met.");

        GUIContent initTimeLabelContent = new GUIContent("Initial Time", "Time in seconds before the first event fires.");
        GUIContent repeatTimeLabelContent = new GUIContent("Poll Time", "After the initial event, frequency in seconds with which this event repeatedly fires (WARNING: sending events too frequently could overrun your event limits).");
        GUIContent repetitionsLabelContent = new GUIContent("Repetitions", "The maximum number of times this event can fire. The default 0 value allows the event to continue firing indefinitely.");

        const string k_Never = "Never";
        const string k_HookUpUITrigger = "Wire up a button or other UI event to this component's `TriggerEvent` method.";

        float terminalHeight = 0f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            terminalHeight = OnTypeGUI (position, property, label, true);
            terminalHeight += 2.0f * AnalyticsEventTrackerEditor.k_LineMargin;
        }

        float OnTypeGUI(Rect position, SerializedProperty property, GUIContent label, bool draw)
        {
            float height = 0f;
            SerializedProperty m_Type = property.FindPropertyRelative ("m_Type");
            SerializedProperty m_LifecycleEvent = property.FindPropertyRelative ("m_LifecycleEvent");
            SerializedProperty m_IsTriggerExpanded = property.FindPropertyRelative ("m_IsTriggerExpanded");
            SerializedProperty m_RepeatTime = property.FindPropertyRelative ("m_RepeatTime");
            SerializedProperty m_ApplyRules = property.FindPropertyRelative ("m_ApplyRules");
            string triggerFoldoutText = "";
            if (draw) {
                string lazyInfo = GetLazyInfo (m_Type, m_ApplyRules, m_LifecycleEvent, m_RepeatTime);
                triggerFoldoutText = m_IsTriggerExpanded.boolValue ? "When" : string.Concat ("When: ", lazyInfo);
            }

            Rect rect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );
            height += rect.height;

            if (draw) {
                m_IsTriggerExpanded.boolValue = EditorGUI.Foldout (rect, m_IsTriggerExpanded.boolValue, triggerFoldoutText, true);
            }

            if (m_IsTriggerExpanded.boolValue)
            {
                rect.y += EditorGUIUtility.singleLineHeight;
                height += EditorGUIUtility.singleLineHeight;
                // Radio buttons between areas
                if (draw) {
                    EditorGUI.BeginChangeCheck ();
                    {
                        int triggerTypeValue = GUI.SelectionGrid (rect,
                                                   m_Type.intValue,
                                                   triggerTypeContent,
                                                   triggerTypeContent.Length);
                        if (EditorGUI.EndChangeCheck ()) {
                            m_Type.intValue = triggerTypeValue;
                        }
                    }
                }

                rect.y += EditorGUIUtility.singleLineHeight + 2.0f * AnalyticsEventTrackerEditor.k_LineMargin;
                height += 2.0f * AnalyticsEventTrackerEditor.k_LineMargin;

                // Active work areas
                if (MatchEnum(m_Type, TriggerType.Lifecycle))
                {
                    height += LifecycleGUI (rect, m_LifecycleEvent, draw);
                }
                else if (MatchEnum(m_Type, TriggerType.External))
                {
                    height += ExternalUIGUI (rect, draw);
                }
                else if (MatchEnum(m_Type, TriggerType.Timer))
                {
                    height += TimerGUI(rect, property, label, draw);
                }
                else if (MatchEnum(m_Type, TriggerType.ExposedMethod))
                {
                    rect.height = 35f;
                    EditorGUI.TextArea(rect,
                        "TODO: Methods annotated with [InstrumentedForAnalytics] can be connected here.");
                    height += rect.height - EditorGUIUtility.singleLineHeight ;
                }
                Rect propertyRect = new Rect (position.x,
                    position.y + height,
                    position.width,
                    EditorGUIUtility.singleLineHeight);
                height += PropertyListGUI (propertyRect, property, draw);
            }
            return height;
        }

        float TimerGUI(Rect position, SerializedProperty property, GUIContent label, bool draw)
        {
            float height = 0;

            SerializedProperty m_InitTime = property.FindPropertyRelative ("m_InitTime");
            SerializedProperty m_RepeatTime = property.FindPropertyRelative ("m_RepeatTime");

            EditorGUI.BeginChangeCheck ();
            {
                float standardFieldWidth = AnalyticsEventTrackerEditor.StandardFieldWidth (position);
                float standardFieldX = AnalyticsEventTrackerEditor.StandardFieldX (position);

                Rect initTimeRect = new Rect (position.x, position.y + height + AnalyticsEventTrackerEditor.k_LineMargin, AnalyticsEventTrackerEditor.k_LeftLabelMargin, EditorGUIUtility.singleLineHeight);
                if (draw) {
                    EditorGUI.LabelField (initTimeRect, initTimeLabelContent);
                }

                Rect initTimeFieldRect = new Rect (standardFieldX, initTimeRect.y, standardFieldWidth, initTimeRect.height);
                float initTime = m_InitTime.floatValue;

                if (draw) {
                    initTime = EditorGUI.FloatField (initTimeFieldRect, m_InitTime.floatValue);
                }
                height += initTimeRect.height + AnalyticsEventTrackerEditor.k_LineMargin;


                Rect repeatTimeLabelRect = new Rect (
                    position.x,
                    initTimeRect.y + initTimeRect.height + AnalyticsEventTrackerEditor.k_LineMargin,
                    AnalyticsEventTrackerEditor.k_LeftLabelMargin,
                    EditorGUIUtility.singleLineHeight
                );
                if (draw) {
                    EditorGUI.LabelField (repeatTimeLabelRect, repeatTimeLabelContent);
                }

                Rect repeatTimeFieldRect = new Rect (
                    standardFieldX,
                    repeatTimeLabelRect.y,
                    standardFieldWidth,
                    repeatTimeLabelRect.height
                );
                float repeatTime = m_RepeatTime.floatValue;
                if (draw) {
                    repeatTime = EditorGUI.FloatField (repeatTimeFieldRect, m_RepeatTime.floatValue);
                }
                height += repeatTimeLabelRect.height + AnalyticsEventTrackerEditor.k_LineMargin;

                if (EditorGUI.EndChangeCheck ()) {
                    m_InitTime.floatValue = initTime;
                    m_RepeatTime.floatValue = repeatTime;
                }
            }

            return height;
        }

        float LifecycleGUI(Rect position, SerializedProperty lifecycleProperty, bool draw)
        {
            if (draw) {
                Rect labelRect = new Rect (
                    position.x,
                    position.y,
                    AnalyticsEventTrackerEditor.k_LeftLabelMargin,
                    EditorGUIUtility.singleLineHeight
                );
                EditorGUI.LabelField (labelRect, lifecycleLabelContent);

                Rect propRect = new Rect (
                    AnalyticsEventTrackerEditor.StandardFieldX(position),
                    labelRect.y,
                    AnalyticsEventTrackerEditor.StandardFieldWidth (position),
                    EditorGUIUtility.singleLineHeight
                );

                EditorGUI.PropertyField (propRect, lifecycleProperty, GUIContent.none);
            }
            return EditorGUI.GetPropertyHeight (lifecycleProperty) + AnalyticsEventTrackerEditor.k_LineMargin;
        }

        float ExternalUIGUI(Rect position, bool draw)
        {
            var rect = position;
            rect.height = EditorGUIUtility.singleLineHeight * 2f;
            if (draw) {
                int oldFontSize = EditorStyles.helpBox.fontSize;
                EditorStyles.helpBox.fontSize = 11;
                EditorGUI.HelpBox(rect, k_HookUpUITrigger, MessageType.Info);
                EditorStyles.helpBox.fontSize = oldFontSize;
            }
            return rect.height + AnalyticsEventTrackerEditor.k_LineMargin;
        }

        float PropertyListGUI(Rect position, SerializedProperty property, bool draw)
        {
            SerializedProperty m_ApplyRules = property.FindPropertyRelative ("m_ApplyRules");
            float height = 0f;
            float standardFieldWidth = AnalyticsEventTrackerEditor.StandardFieldWidth (position);
            float standardFieldX = AnalyticsEventTrackerEditor.StandardFieldX (position);
            Rect applyRulesRect = new Rect (position.x,
                position.y,
                AnalyticsEventTrackerEditor.k_LeftLabelMargin,
                EditorGUIUtility.singleLineHeight
            );
            Rect repetitionsLabelRect = new Rect (
                position.x,
                applyRulesRect.y + applyRulesRect.height + AnalyticsEventTrackerEditor.k_LineMargin,
                AnalyticsEventTrackerEditor.k_LeftLabelMargin,
                EditorGUIUtility.singleLineHeight
            );
            Rect repetitionsFieldRect = new Rect (
                standardFieldX,
                repetitionsLabelRect.y,
                standardFieldWidth,
                repetitionsLabelRect.height
            );
            Rect boolLabelRect = new Rect(
                position.x,
                repetitionsLabelRect.y + EditorGUIUtility.singleLineHeight,
                AnalyticsEventTrackerEditor.k_LeftLabelMargin,
                EditorGUIUtility.singleLineHeight
            );
            Rect boolRect = new Rect(
                standardFieldX,
                boolLabelRect.y,
                standardFieldWidth,
                EditorGUIUtility.singleLineHeight
            );
            Rect rulesRect = new Rect(
                position.x,
                boolRect.y + boolRect.height + AnalyticsEventTrackerEditor.k_LineMargin,
                position.width,
                position.height
            );


            EditorGUI.BeginChangeCheck ();
            {
                height += applyRulesRect.height;
                bool applyRules = m_ApplyRules.boolValue;
                if (draw) {
                    applyRules = EditorGUI.ToggleLeft (applyRulesRect, applyRulesLabelContent, m_ApplyRules.boolValue);
                }
                if (EditorGUI.EndChangeCheck ()) {
                    m_ApplyRules.boolValue = applyRules;
                }
            }

            if (m_ApplyRules.boolValue) {
                EditorGUI.BeginChangeCheck ();
                {
                    SerializedProperty m_Repetitions = property.FindPropertyRelative ("m_Repetitions");
                    if (draw) {
                        EditorGUI.LabelField (repetitionsLabelRect, repetitionsLabelContent);
                    }
                    int repetitions = m_Repetitions.intValue;
                    if (draw) {
                        repetitions = EditorGUI.IntField (repetitionsFieldRect, m_Repetitions.intValue);
                    }
                    height += repetitionsLabelRect.height + AnalyticsEventTrackerEditor.k_LineMargin;
                    if (EditorGUI.EndChangeCheck ()) {
                        m_Repetitions.intValue = repetitions;
                    }
                }
                height += boolLabelRect.height;
                if (draw) {
                    EditorGUI.LabelField (boolLabelRect, boolLabelContent);
                }
                height += boolRect.height;

                if (draw) {
                    EditorGUI.PropertyField (boolRect, property.FindPropertyRelative ("m_TriggerBool"), GUIContent.none);
                    EditorGUI.PropertyField (rulesRect, property.FindPropertyRelative ("m_Rules"), true);
                }
                height += EditorGUI.GetPropertyHeight (property.FindPropertyRelative ("m_Rules"));
            }
            return 
                height + AnalyticsEventTrackerEditor.k_LineMargin;
        }

        string GetLazyInfo(SerializedProperty triggerTypeProperty, SerializedProperty applyRules, SerializedProperty lifeCycleProperty, SerializedProperty repeatTime)
        {
            string value = "";
            if (MatchEnum(triggerTypeProperty, TriggerType.Lifecycle))
            {
                SerializedProperty lifecycleEventProperty = lifeCycleProperty;
                string eventName = lifecycleEventProperty.enumNames[lifecycleEventProperty.enumValueIndex].ToString();
                if (lifecycleEventProperty.enumValueIndex == (int)TriggerLifecycleEvent.None) {
                    eventName = k_Never;
                }
                value = eventName;
            }
            else if (MatchEnum(triggerTypeProperty, TriggerType.External))
            {
                value = "On UI Event";
            }
            else if (MatchEnum(triggerTypeProperty, TriggerType.Timer))
            {
                value = string.Concat("Every ", repeatTime.floatValue, " seconds");
            }
            else if (MatchEnum(triggerTypeProperty, TriggerType.ExposedMethod))
            {
                value = "A method is called";
            }
            if (applyRules.boolValue && value != "None") {
                value = string.Concat (value, " and a set of rules is met");
            }

            return value;
        }

        bool MatchEnum(SerializedProperty enumProp, Enum someEnum)
        {
            int index = enumProp.enumValueIndex;
            return enumProp.enumNames [index] == someEnum.ToString ();
        }

        float CalcHeight()
        {
            float height = 0f;
            return height;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (terminalHeight == 0f)
                terminalHeight = OnTypeGUI (new Rect (), property, label, false);
            return terminalHeight;
        }
    }
}