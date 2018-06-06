using UnityEditor;
using System;
using System.Collections.Generic;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    class CustomEnumPopup
    {
        public static Type GetEnumType(string enumName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }

        public static string Popup(Rect rect, SerializedProperty property, bool canCustomize = false)
        {            
            SerializedProperty m_EnumType = property.FindPropertyRelative("m_EnumType");
            SerializedProperty m_ValueType = property.FindPropertyRelative("m_ValueType");

            if (m_EnumType.stringValue.Length == 0)
                return m_ValueType.stringValue;
      
            SerializedProperty m_EnumTypeIsCustomizable = property.FindPropertyRelative("m_EnumTypeIsCustomizable");
            SerializedProperty m_EditingCustomValue = property.FindPropertyRelative("m_EditingCustomValue");
            SerializedProperty m_PopupIndex = property.FindPropertyRelative("m_PopupIndex");
            SerializedProperty m_CustomValue = property.FindPropertyRelative("m_CustomValue");

            List<string> list = new List<string>();
            
            list.AddRange(Enum.GetNames(GetEnumType(m_EnumType.stringValue)));

            for (int i = 0; i < list.Count; i++)
            {
                var index = list[i].IndexOf("_", StringComparison.Ordinal);
                if (index > -1)
                {
                    list[i] = list[i].Replace('_', '.');
                }
            }

            canCustomize |= m_EnumTypeIsCustomizable.boolValue;
            if (canCustomize)
            {
                list.Add("Other");
            }

            if (list.Count == 0)
                return m_ValueType.stringValue;

            if (m_PopupIndex.intValue >= list.Count)
                m_PopupIndex.intValue = list.Count - 1;

            // if we are editing, return edited string
            if (m_EditingCustomValue.boolValue)
            {
                var btnRect = new Rect(rect.x, rect.y, rect.width / 2, rect.height);
                if (GUI.Button(btnRect, new GUIContent(list[m_PopupIndex.intValue]), EditorStyles.popup))
                {
                    var menu = new GenericMenu();
                    for (int i = 0; i < list.Count; ++i)
                    {
                        menu.AddItem(new GUIContent(list[i]), i == m_PopupIndex.intValue,
                            (object option) =>
                            {
                                m_PopupIndex.intValue = (int)option;
                                if (canCustomize && m_PopupIndex.intValue == list.Count - 1)
                                {
                                    m_CustomValue.stringValue = "";
                                    m_EditingCustomValue.boolValue = true;
                                }
                                else
                                {
                                    m_EditingCustomValue.boolValue = false;
                                }
                                property.serializedObject.ApplyModifiedProperties();
                            }, i);
                    }
                    menu.DropDown(rect);
                }
                var textRect = new Rect(rect.x + rect.width/2, rect.y, rect.width / 2, rect.height);
                m_CustomValue.stringValue = EditorGUI.TextField(textRect, m_CustomValue.stringValue);
                return m_CustomValue.stringValue;
            }

            if (GUI.Button(rect, new GUIContent(list[m_PopupIndex.intValue]), EditorStyles.popup))
            {
                var menu = new GenericMenu();
                for (int i = 0; i < list.Count; ++i)
                {
                    menu.AddItem(new GUIContent(list[i]), i == m_PopupIndex.intValue, 
                        (object option) => {
                            m_PopupIndex.intValue = (int)option;
                            if (canCustomize && m_PopupIndex.intValue == list.Count - 1)
                            {
                                m_CustomValue.stringValue = "";
                                m_EditingCustomValue.boolValue = true;
                            }
                            property.serializedObject.ApplyModifiedProperties();
                        }, i);
                }
                menu.DropDown(rect);
            }

            property.serializedObject.ApplyModifiedProperties();

            // Return standard string value except for custom value.

            var indexSelected = m_PopupIndex.intValue;
            var indexLast = list.Count - (list[list.Count - 1].ToLower() == "other" ? 2 : 1);

            if (indexSelected >= 0 && indexSelected <= indexLast)
            {
                var values = Enum.GetValues(GetEnumType(m_EnumType.stringValue));
                return AnalyticsEvent.EnumToString(values.GetValue(indexSelected));
            }

            return list[m_PopupIndex.intValue];
        }      
    }
}