using System;
using UnityEditor;
using UnityEngine.UI;

namespace UnityEngine.Analytics.Experimental.Tracker
{

    [CustomEditor (typeof(AnalyticsEventTracker))]
    public class AnalyticsEventTrackerEditor : Editor
    {
        // Standard line margin across the component and its drawers
        public readonly static float k_LineMargin = 2f;
        public readonly static float k_LeftListMargin = 40f;
        public readonly static float k_LeftLabelMargin = 100f;
        public readonly static float k_MinRightColumnPercentage = .42f;

        public static float StandardFieldWidth(Rect position)
        {
            return (position.width * (1f - AnalyticsEventTrackerEditor.k_MinRightColumnPercentage)) + position.x;
        }

        public static float StandardFieldX(Rect position)
        {
            return position.width * AnalyticsEventTrackerEditor.k_MinRightColumnPercentage;
        }

        public override void OnInspectorGUI()
        {
            // Assume true for older versions
            bool analyticsEnabled = true;
            #if UNITY_5_5_OR_NEWER
            analyticsEnabled = UnityEditor.Analytics.AnalyticsSettings.enabled;
            #endif

            if (analyticsEnabled) {
                serializedObject.Update ();
                bool cachedWordWrap = EditorStyles.textArea.wordWrap;
                EditorStyles.textArea.wordWrap = true;
                TriggerGUI ();
                PayloadGUI ();
                EditorStyles.textArea.wordWrap = cachedWordWrap;
                serializedObject.ApplyModifiedProperties ();
            } else {
                EditorGUILayout.HelpBox ("This Component is designed to work with Unity Analytics, which is not currently enabled.\nTo enable Analytics, go to Window/Services, select Analytics and click the 'Enable Analytics' button.", MessageType.Warning);
            }
        }

        void TriggerGUI()
        {
            SerializedProperty m_Trigger = serializedObject.FindProperty ("m_Trigger");
            EditorGUILayout.PropertyField(m_Trigger);
        }

        void PayloadGUI()
        {
            SerializedProperty m_EventPayload = serializedObject.FindProperty ("m_EventPayload");
            EditorGUILayout.PropertyField(m_EventPayload);
        }
    }
}
