using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics.Experimental.Tracker;
using Object = UnityEngine.Object;

namespace UnityEditor.Analytics.Experimental.Tracker
{
    [CustomPropertyDrawer(typeof(TrackableProperty), true)]
    public abstract class TrackablePropertyDrawer : PropertyDrawer
    {
        protected string m_EmptyPath;

        protected SerializedProperty m_TargetProperty;
        protected SerializedProperty m_PathProperty;

        protected GUIContent m_TargetContent = new GUIContent("", "The target driving the value.");
        protected GUIContent m_PathContent = new GUIContent("", "The path of the field driving the value.");

        protected Rect m_TargetPosition;
        protected Rect m_PathPosition;

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            m_TargetProperty = property.FindPropertyRelative("m_Target");
            m_PathProperty = property.FindPropertyRelative("m_Path");

            m_PathContent.text = m_EmptyPath;

            m_TargetPosition = position;
            m_TargetPosition.xMax = EditorGUIUtility.currentViewWidth * 0.28f - m_TargetPosition.x;
            m_PathPosition = position;
            m_PathPosition.xMin = m_TargetPosition.xMax + EditorGUIUtility.standardVerticalSpacing + 2f;

            EditorGUI.BeginChangeCheck();
            {
                GUI.Box(m_TargetPosition, m_TargetContent);
                EditorGUI.PropertyField(m_TargetPosition, m_TargetProperty, GUIContent.none);

                if (EditorGUI.EndChangeCheck())
                {
                    m_PathProperty.stringValue = null;
                }
            }

            GUI.enabled = (m_TargetProperty.objectReferenceValue != null);

            if (m_TargetProperty.objectReferenceValue != null && !string.IsNullOrEmpty(m_PathProperty.stringValue))
            {
                m_PathContent.text = string.Concat(
                    m_TargetProperty.objectReferenceValue.GetType().Name,
                    ".",
                    m_PathProperty.stringValue
                );
            }

            if (GUI.Button(m_PathPosition, m_PathContent, EditorStyles.popup))
            {
                //DrawPopupMenu().DropDown(m_PathPosition);
            }

            GUI.enabled = true;

            EditorGUI.EndProperty();
        }

        protected GenericMenu DrawPopupMenu ()
        {
            var menu = new GenericMenu();
            var target = m_TargetProperty.objectReferenceValue;
            var gameObject = (target is Component) ? (target as Component).gameObject : target as GameObject;
            var components = gameObject.GetComponents<Component>();

            menu.AddItem(
                new GUIContent(m_EmptyPath),
                string.IsNullOrEmpty(m_PathProperty.stringValue),
                null
            );

            menu.AddSeparator("");

            AddMenuItems(menu, target);

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null)
                {
                    AddMenuItems(menu, target, components[i]);
                }
            }

            return menu;
        }

        protected abstract void AddMenuItems (GenericMenu menu, Object rootTarget, object subTarget = null, string subMenuPath = null, int depth = 0);
    }
}
