using System;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Analytics.Experimental;
using System.Reflection;

namespace UnityEngine.Analytics.Experimental.Tracker
{

    [CustomPropertyDrawer (typeof(StandardEventPayload))]
    public class StandardEventPayloadDrawer : PropertyDrawer
    {
        float terminalHeight = 0;
        float oldTerminalHeight = 0;

        const string k_NeedRequiredParam = "You must fill in the required parameter value for {0}.\n";
        const string k_NeedNameValueCustomParam = "You must fill in the name and value for all custom parameters.\n";
        const string k_NeedNameCustomParam = "You must fill in the name for all custom parameters.\n";
        const string k_NeedValueCustomParam = "You must fill in the value for all custom parameters.\n";
        const string k_DuplicateNameParam = "Parameter name {0} cannot be used more than once.\n";
        const string k_NeedCustomEventName = "You must set the custom event name.\n";

        static readonly string k_CustomEvent = "CustomEvent";

        GUIContent sendEventLabelContent = new GUIContent ("Send Event", "Select the type of event you want to send.");
        GUIContent nameLabelContent = new GUIContent("Name", "The name of the event you want to send. By convention, event names are lower_snake_case.");

        static string[] eventTypes;
        string eventType;
        string tooltip;
        Rect nameFieldRect;
        bool eventNameFieldMarkedDirty;

        GenericMenu eventsMenu;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            terminalHeight = OnGUI (position, property, label, true);
            if (oldTerminalHeight != terminalHeight)
            {
                EditorUtility.SetDirty( property.serializedObject.targetObject as AnalyticsEventTracker);
                oldTerminalHeight = terminalHeight;
            }
        }

        float OnGUI(Rect position, SerializedProperty property, GUIContent label, bool draw)
        {
            if (eventTypes == null) {
                BuildEventTypes ();
            }
            if (eventsMenu == null) {
                BuildEventsMenu(property);
            }
            float height = 0f;

            // FOLDOUT
            SerializedProperty m_IsEventExpanded = property.FindPropertyRelative ("m_IsEventExpanded");
            Rect rect = new Rect (position.x, position.y, AnalyticsEventTrackerEditor.k_LeftLabelMargin, EditorGUIUtility.singleLineHeight);
            if (draw) {
                m_IsEventExpanded.boolValue = EditorGUI.Foldout (rect, m_IsEventExpanded.boolValue, sendEventLabelContent, true);
            }
            height += rect.height;
            if (m_IsEventExpanded.boolValue) {
                // EVENT
                height += EventGUI (position, property, draw);
                position.y += height;
                height += NameGUI (position, property, eventType != k_CustomEvent, true, draw);

                // PARAMETERS
                height += ParametersGUI (position, property, draw);
            } else {
                // NAME WHEN CLOSED
                height += NameGUI (position, property, eventType != k_CustomEvent, false, draw);
            }
            return height;
        }

        float EventGUI(Rect position, SerializedProperty property, bool draw)
        {
            float height = 0;
            float standardFieldX = AnalyticsEventTrackerEditor.StandardFieldX (position);
            float standardFieldWidth = AnalyticsEventTrackerEditor.StandardFieldWidth (position);

            var m_StandardEventType = property.FindPropertyRelative ("m_StandardEventType");
            eventType = m_StandardEventType.stringValue;

            int selectedIndex = Array.IndexOf(eventTypes, eventType);

            Rect eventTypeRect = new Rect(
                standardFieldX,
                position.y,
                standardFieldWidth,
                EditorGUIUtility.singleLineHeight
            );

            if (selectedIndex > -1)
            {
                if (draw)
                {
                    if (GUI.Button(eventTypeRect, new GUIContent(selectedIndex < 0 ? k_CustomEvent : eventTypes[selectedIndex]), EditorStyles.popup))
                    {
                        BuildEventsMenu(property, selectedIndex);
                        eventsMenu.DropDown(new Rect(eventTypeRect.x, eventTypeRect.y - EditorGUIUtility.singleLineHeight - 2f, eventTypeRect.width, eventTypeRect.height));
                    }
                    eventType = eventTypes[selectedIndex];
                }
                height += AnalyticsEventTrackerEditor.k_LineMargin;
            }

            return height;
        }

        struct SelectedEventInfo
        {
            public SerializedProperty property;
            public string eventTypeName;

            public SelectedEventInfo (SerializedProperty property, string eventTypeName)
            {
                this.property = property;
                this.eventTypeName = eventTypeName;
            }
        }

        void SetSelectedStandardEvent (object eventInfo)
        {
            var info = (SelectedEventInfo)eventInfo;

            info.property.FindPropertyRelative("m_StandardEventType").stringValue = info.eventTypeName;

            ConformEventType(info.property, info.eventTypeName);
            eventNameFieldMarkedDirty = true;
            info.property.serializedObject.ApplyModifiedProperties();
        }

        void ConformEventType(SerializedProperty property, string eventType)
        {
            int index = Array.IndexOf (eventTypes, eventType);
            Type t = EventPayloads.s_EventTypes [index];
            if (t != null)
            {
                AnalyticsEventTracker tracker = property.serializedObject.targetObject as AnalyticsEventTracker;
                tracker.payload.standardEventType = t;

                StandardEventName attr = 
                    (StandardEventName)Attribute.GetCustomAttribute(t, typeof(StandardEventName));
                if (attr != null) {
                    UpdateEventName(property, (attr as StandardEventName).sendName);
                }
            }
        }

        float NameGUI(Rect position, SerializedProperty property, bool disable, bool foldout, bool draw)
        {
            if (draw) {

                SerializedProperty m_Name = property.FindPropertyRelative ("m_Name");

                if (foldout) {
                    Rect nameLabelRect = new Rect (position.x, position.y, 
                                             AnalyticsEventTrackerEditor.k_LeftLabelMargin, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField (nameLabelRect, nameLabelContent);
                }

                float standardFieldX = AnalyticsEventTrackerEditor.StandardFieldX (position);
                float standardFieldWidth = AnalyticsEventTrackerEditor.StandardFieldWidth (position);

                nameFieldRect = new Rect (
                    standardFieldX,
                    position.y,
                    standardFieldWidth,
                    EditorGUIUtility.singleLineHeight
                );

                GUI.SetNextControlName ("event_name_textfield");
                string eventName = m_Name.stringValue;

                EditorGUI.BeginDisabledGroup (disable);
                eventName = EditorGUI.TextField (nameFieldRect, eventName);
                ShowEventNameTooltip(property, nameFieldRect);
                EditorGUI.EndDisabledGroup ();

                if (eventName != m_Name.stringValue) {
                    UpdateEventName (property, eventName);
                }
                if (eventNameFieldMarkedDirty) {
                    RemoveFocus ();
                }
            }
            return EditorGUIUtility.singleLineHeight;
        }

        void RemoveFocus()
        {
            GUI.SetNextControlName ("null_control");
            GUI.FocusControl ("null_control");
            eventNameFieldMarkedDirty = false;
        }

        void ShowEventNameTooltip(SerializedProperty property, Rect position)
        {
            if (eventType == k_CustomEvent)
                return;

            int index = Array.IndexOf (eventTypes, eventType);
            Type t = EventPayloads.s_EventTypes [index];
            if (t != null)
            {
                AnalyticsEventTracker tracker = property.serializedObject.targetObject as AnalyticsEventTracker;
                tracker.payload.standardEventType = t;

                StandardEventName attr =
                    (StandardEventName)Attribute.GetCustomAttribute(t, typeof(StandardEventName));
                if (attr != null) {
                    string tooltip = (attr as StandardEventName).tooltip;
                    GUI.Box(position, new GUIContent("", tooltip), GUIStyle.none);
                }
            }
        }

        void UpdateEventName(SerializedProperty property, string name)
        {
            SerializedProperty m_Name = property.FindPropertyRelative ("m_Name");
            m_Name.stringValue = name;
        }

        float ParametersGUI(Rect position, SerializedProperty property, bool draw)
        {
            float height = 0;

            Rect rect = position;
            height = rect.height = EditorGUIUtility.singleLineHeight;

            SerializedProperty parameters = property.FindPropertyRelative("m_Parameters");

            rect.y += EditorGUIUtility.singleLineHeight;
            if (draw) {
                EditorGUI.PropertyField (rect, parameters, true);
            }
            height += EditorGUI.GetPropertyHeight (parameters);

            AnalyticsEventTracker tracker = property.serializedObject.targetObject as AnalyticsEventTracker;
            if (tracker != null)
            {
                string warningMsg = string.Empty;
                int lineCount = 0;

                // find warnings for required types and create dictionary for either or parameter types (required with group id)
                Dictionary<string, List<AnalyticsEventParam>> selectRequired = new Dictionary<string, List<AnalyticsEventParam>>();
                Dictionary<string, int> nameCounts = new Dictionary<string, int>();
                foreach (AnalyticsEventParam param in tracker.payload.parameters.parameters)
                {
                    if (param.requirementType == AnalyticsEventParam.RequirementType.Required)
                    {
                        if (string.IsNullOrEmpty(param.groupID))
                        {
                            if (!param.valueProperty.IsValid())
                            {
                                warningMsg += string.Format(k_NeedRequiredParam, param.name);
                                ++lineCount;
                            }
                        }
                        else
                        {
                            if (!selectRequired.ContainsKey(param.groupID))
                            {
                                selectRequired[param.groupID] = new List<AnalyticsEventParam>();
                            }

                            selectRequired[param.groupID].Add(param);
                        }
                    }

                    if (!string.IsNullOrEmpty(param.name))
                    {
                        if (!nameCounts.ContainsKey(param.name))
                        {
                            nameCounts[param.name] = 1;
                        }
                        else
                        {
                            nameCounts[param.name] = nameCounts[param.name] + 1;
                        }
                    }
                }

                // look at all the either or parameters and see if there are any errors
                foreach (string groupId in selectRequired.Keys)
                {
                    bool anyValid = false;
                    foreach (AnalyticsEventParam param in selectRequired[groupId])
                    {
                        if (param.valueProperty.IsValid())
                        {
                            anyValid = true;
                            break;
                        }
                    }

                    if (!anyValid)
                    {
                        warningMsg += string.Format(k_NeedRequiredParam, string.Join(" or ", selectRequired[groupId].Select(param => param.name).ToArray()));
                        ++lineCount;
                    }
                }

                // add any custom parameter warnings
                bool nameMissing = false;
                bool valueMissing = false;
                foreach (AnalyticsEventParam param in tracker.payload.parameters.parameters)
                {
                    if (param.requirementType == AnalyticsEventParam.RequirementType.None)
                    {
                        if (string.IsNullOrEmpty(param.name))
                        {
                            nameMissing = true;
                        }
                        if (!param.valueProperty.IsValid())
                        {
                            valueMissing = true;
                        }
                    }
                }

                if (nameMissing && valueMissing)
                {
                    warningMsg += k_NeedNameValueCustomParam;
                    ++lineCount;
                }
                else if (nameMissing)
                {
                    warningMsg += k_NeedNameCustomParam;
                    ++lineCount;
                }
                else if (valueMissing)
                {
                    warningMsg += k_NeedValueCustomParam;
                    ++lineCount;
                }

                if (eventType == k_CustomEvent && string.IsNullOrEmpty(property.FindPropertyRelative("m_Name").stringValue))
                {
                    warningMsg += k_NeedCustomEventName;
                    ++lineCount;
                }
                
                // check for duplicated names
                foreach (string name in nameCounts.Keys)
                {
                    if (nameCounts[name] > 1)
                    {
                        warningMsg += string.Format(k_DuplicateNameParam, name);
                        ++lineCount;
                    }
                }
                
                if (lineCount > 0)
                {
                    warningMsg = warningMsg.Trim('\n');
                    int oldFontSize = EditorStyles.helpBox.fontSize;
                    EditorStyles.helpBox.fontSize = 11;

                    rect.y += height - EditorGUIUtility.singleLineHeight / 2.0f;
                    rect.height += (Math.Max(lineCount, 2)- 0.5f) * EditorStyles.helpBox.lineHeight;
                    height += rect.height;

                    EditorGUI.HelpBox(rect, warningMsg, MessageType.Warning);
                    EditorStyles.helpBox.fontSize = oldFontSize;
                }
            }

            return height;
        }

        void BuildEventTypes()
        {
            List<string> nameList = new List<string> ();
            for (int a = 0; a < EventPayloads.s_EventTypes.Length; a++) {
                Type t = EventPayloads.s_EventTypes [a];

                string eventName = t.Name;
                StandardEventName eventNameAttribute =
                    (StandardEventName)Attribute.GetCustomAttribute (t, typeof(StandardEventName));

                if (eventName != k_CustomEvent) {
                    eventName = String.Concat (eventNameAttribute.path, "/", t.Name);
                }

                nameList.Add (eventName);
            }
            eventTypes = nameList.ToArray ();
        }

        void BuildEventsMenu(SerializedProperty property, int selectedIndex = -1)
        {
            eventsMenu = new GenericMenu ();
            for (int i = 0; i < eventTypes.Length; i++) {
                eventsMenu.AddItem (
                    new GUIContent (eventTypes [i]),
                    i == selectedIndex,
                    SetSelectedStandardEvent,
                    new SelectedEventInfo (property, eventTypes [i])
                );
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (terminalHeight == 0f)
                terminalHeight = OnGUI (new Rect(), property, label, false);
            return terminalHeight;
        }
    }
}

