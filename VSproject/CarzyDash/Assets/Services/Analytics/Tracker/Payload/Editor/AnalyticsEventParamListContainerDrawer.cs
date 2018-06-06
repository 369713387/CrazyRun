using System;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using UnityEditorInternal;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    internal class DefinedParameter
    {
        public AnalyticsEventParam.RequirementType requirement;
        public string groupId;
        public string tooltip;
        public bool customEnum;
        public Type type;
    }



    [CustomPropertyDrawer (typeof(AnalyticsEventParamListContainer))]
    public class AnalyticsEventParamListContainerDrawer : ListContainerDrawer
    {
        const string k_RemoveParameter = "You must disable or remove another parameter to enable this one.";

        Dictionary<string, DefinedParameter> definedParams = new Dictionary<string, DefinedParameter> ();
        AnalyticsEventTracker tracker = null;
        Type payloadType = null;
        bool refreshTracker = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI (position, property, label);
            if (tracker == null) {
                tracker = property.serializedObject.targetObject as AnalyticsEventTracker;
                refreshTracker = true;
            }
            BuildRequirementsList (tracker.payload.standardEventType);

            //ensure that we are never selecting off the end of the array
            if (m_ReorderableList.index >= m_FieldsArray.arraySize)
            {
                m_ReorderableList.index = -1;
            }

            UpdateDisplayAdd (m_ReorderableList);
            UpdateDisplayRemove (m_ReorderableList);

            refreshTracker = false;
        }

        void BuildRequirementsList(Type type)
        {
            if (type != null && type != payloadType && m_FieldsArray != null) {
        
                payloadType = type;
                definedParams.Clear ();
 

                FieldInfo[] fieldInfos = payloadType.GetFields ();

                foreach (FieldInfo fieldInfo in fieldInfos) {

                    string key = "";

                    AnalyticsEventParam.RequirementType requirement = AnalyticsEventParam.RequirementType.None;
                    string groupId = null;
                    string tooltip = null;
                    RequiredParameter requiredAttr = 
                        (RequiredParameter)Attribute.GetCustomAttribute (fieldInfo, typeof(RequiredParameter));
                    if (requiredAttr != null) {
                        key = requiredAttr.sendName;
                        requirement = AnalyticsEventParam.RequirementType.Required;
                        groupId = requiredAttr.groupId;
                        tooltip = requiredAttr.tooltip;
                    }
                    else
                    {
                        OptionalParameter optionalAttr = 
                            (OptionalParameter)Attribute.GetCustomAttribute (fieldInfo, typeof(OptionalParameter));
                        key = optionalAttr.sendName;
                        requirement = AnalyticsEventParam.RequirementType.Optional;
                        tooltip = optionalAttr.tooltip;
                    }
                    CustomizableEnum customEnum = (CustomizableEnum)Attribute.GetCustomAttribute(fieldInfo, typeof(CustomizableEnum));

                    DefinedParameter parm;
                    if (!definedParams.ContainsKey (key)) {
                        var newParm = new DefinedParameter ();
                        newParm.type = null;
                        definedParams.Add(key, newParm);
                    }
                    parm = definedParams[key];
                    parm.requirement = requirement;
                    parm.groupId = groupId;
                    parm.tooltip = tooltip;
                    //Assert(parm.type == null || parm.type == fieldInfo.FieldType);
                    parm.type = fieldInfo.FieldType;
                    parm.customEnum = customEnum != null ? customEnum.Customizable : false;
                }

                // only clear out and reset the fields array if this is the result of a type change instead
                // of the result of refreshing the tracker (i.e. the drawer was destroyed and recreated)
                if (!refreshTracker)
                {
                    m_FieldsArray.ClearArray();
                    foreach (KeyValuePair<string, DefinedParameter> kv in definedParams) {
                        base.AddElement (m_ReorderableList);

                        var listItem = m_FieldsArray.GetArrayElementAtIndex (m_FieldsArray.arraySize - 1);

                        SerializedProperty m_Name = listItem.FindPropertyRelative ("m_Name");
                        m_Name.stringValue = kv.Key;

                        SerializedProperty m_RequirementType = listItem.FindPropertyRelative("m_RequirementType");
                        m_RequirementType.enumValueIndex = (int)kv.Value.requirement;

                        SerializedProperty m_GroupId = listItem.FindPropertyRelative("m_GroupID");
                        m_GroupId.stringValue = kv.Value.groupId;

                        SerializedProperty m_Tooltip = listItem.FindPropertyRelative("m_Tooltip");
                        m_Tooltip.stringValue = kv.Value.tooltip;
                    
                        SerializedProperty m_Value = listItem.FindPropertyRelative ("m_Value");
                        SerializedProperty m_ValueType = m_Value.FindPropertyRelative ("m_ValueType");
                        SerializedProperty m_EnumType = m_Value.FindPropertyRelative("m_EnumType");
                        SerializedProperty m_EnumTypeIsCustomizable = m_Value.FindPropertyRelative("m_EnumTypeIsCustomizable");
                        m_EnumTypeIsCustomizable.boolValue = kv.Value.customEnum;
                        
                        m_ValueType.stringValue = kv.Value.type.ToString();

                        if (kv.Value.type.IsEnum)
                        {
                            m_EnumType.stringValue = kv.Value.type.ToString();
                        }

                        SerializedProperty m_CanDisable = m_Value.FindPropertyRelative("m_CanDisable");
                        m_CanDisable.boolValue = kv.Value.requirement == AnalyticsEventParam.RequirementType.Optional || !string.IsNullOrEmpty(kv.Value.groupId);

                        SerializedProperty m_PropertyType = m_Value.FindPropertyRelative("m_PropertyType");
                        if (m_CanDisable.boolValue)
                        {
                            // disable any values that don't have an invalid default value
                            bool hasInvalidDefault = kv.Value.type == typeof(string) ? true : false;
                            m_PropertyType.enumValueIndex = hasInvalidDefault ? (int)ValueProperty.PropertyType.Static : (int)ValueProperty.PropertyType.Disabled;
                        }
                        else
                        {
                            m_PropertyType.enumValueIndex = (int)ValueProperty.PropertyType.Static;
                        }

                        SerializedProperty m_FixedType = m_Value.FindPropertyRelative("m_FixedType");
                        m_FixedType.boolValue = true;
                    }
                }
            }
        }

        protected override void SelectParam(ReorderableList list)
        {
            m_LastSelectedIndex = list.index;
            UpdateDisplayRemove (list);
        }

        protected override void AddElement(ReorderableList list)
        {
            base.AddElement (list);

            if (m_FieldsArray.arraySize == 0)
                return;

            var field = m_FieldsArray.GetArrayElementAtIndex(list.index);

            var m_Name = field.FindPropertyRelative("m_Name");
            m_Name.stringValue = "";

            var m_RequirementType = field.FindPropertyRelative("m_RequirementType");
            m_RequirementType.enumValueIndex = (int)AnalyticsEventParam.RequirementType.None;

            var m_ValueProp = field.FindPropertyRelative("m_Value");
            var m_FixedType = m_ValueProp.FindPropertyRelative("m_FixedType");
            var m_PropertyType = m_ValueProp.FindPropertyRelative("m_PropertyType");
            var m_ValueType = m_ValueProp.FindPropertyRelative("m_ValueType");
            var m_Value = m_ValueProp.FindPropertyRelative("m_Value");
            var m_CanDisable = m_ValueProp.FindPropertyRelative("m_CanDisable");
            field.FindPropertyRelative("m_Tooltip").stringValue = string.Empty;
            m_FixedType.boolValue = false;
            m_PropertyType.enumValueIndex = (int)ValueProperty.PropertyType.Static;
            m_ValueType.stringValue = typeof(string).ToString();
            m_Value.stringValue = "";
            m_CanDisable.boolValue = false;

            m_ValueProp.serializedObject.ApplyModifiedProperties();
            field.serializedObject.ApplyModifiedProperties ();

            UpdateDisplayAdd(list);
            UpdateDisplayRemove (list);
        }

        protected override void RemoveButton(ReorderableList list)
        {
            base.RemoveButton (list);
            UpdateDisplayAdd(list);
            UpdateDisplayRemove (list);
        }

        protected override void DrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            if (m_FieldsArray.arraySize == 0)
                return;
                
            SerializedProperty field = m_FieldsArray.GetArrayElementAtIndex(index);
            
            if (m_FieldsArray.arraySize - GetDisabledFieldCount() >= AnalyticsEventTrackerSettings.paramCountMax)
            {
                SerializedProperty m_Value = field.FindPropertyRelative("m_Value");
                SerializedProperty m_PropertyType = m_Value.FindPropertyRelative("m_PropertyType");
                ValueProperty.PropertyType propertyType = (ValueProperty.PropertyType)m_PropertyType.enumValueIndex;

                bool enabled = propertyType != ValueProperty.PropertyType.Disabled;
                GUI.enabled = enabled;
                EditorGUI.PropertyField(rect, field);
                GUI.enabled = true;

                if (!enabled)
                {
                    rect.yMax -= 2.0f * AnalyticsEventTrackerEditor.k_LineMargin;
                    rect.yMin = rect.yMax - EditorGUIUtility.singleLineHeight - 2.0f * AnalyticsEventTrackerEditor.k_LineMargin;
                    rect.xMin += AnalyticsEventTrackerEditor.k_LineMargin + AnalyticsEventTrackerEditor.k_LeftListMargin;
                    EditorGUI.HelpBox(rect, k_RemoveParameter, MessageType.Warning);
                }
            }
            else
            {
                EditorGUI.PropertyField(rect, field);
            }
        }

        void UpdateDisplayAdd(ReorderableList list)
        {
            list.displayAdd = (m_FieldsArray.arraySize < AnalyticsEventTrackerSettings.paramCountMax + GetDisabledFieldCount());
        }

        void UpdateDisplayRemove(ReorderableList list)
        {
            list.displayRemove = false;

            if (list.index < 0)
            {
                return;
            }

            if (m_FieldsArray.arraySize == 0)
                return;

            var field = m_FieldsArray.GetArrayElementAtIndex(list.index);

            if (field == null)
            {
                return;
            }

            var requirementType = field.FindPropertyRelative("m_RequirementType");
            list.displayRemove = (
                list.index >= definedParams.Count && 
                requirementType.enumValueIndex == (int)AnalyticsEventParam.RequirementType.None
            );
        }

        protected override string GetListName ()
        {
            return "m_Parameters";
        }

        protected override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight * 4f;
        }

        protected override void DrawHeader(Rect headerRect)
        {
            headerRect.height = 16;
            var disabledCount = GetDisabledFieldCount();
            string headerText = string.Format(
                "Parameters: {0}/{1}{2}", 
                m_FieldsArray.arraySize - disabledCount, 
                AnalyticsEventTrackerSettings.paramCountMax,
                disabledCount > 0 ? string.Format(" (Disabled: {0})", disabledCount) : string.Empty
            );
            GUI.Label(headerRect, headerText);
        }

        int GetDisabledFieldCount ()
        {
            int count = 0;
            for (int i = 0; i < m_FieldsArray.arraySize; i++)
            {
                var paramProp = m_FieldsArray.GetArrayElementAtIndex(i);
                var valueProp = paramProp.FindPropertyRelative("m_Value");
                var propertyTypeProp = valueProp.FindPropertyRelative("m_PropertyType");

                if (propertyTypeProp.enumValueIndex == (int)ValueProperty.PropertyType.Disabled)
                {
                    count++;
                }
            }
            return count;
        }
    }
}

