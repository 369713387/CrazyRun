using System;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    public class ListContainerDrawer : PropertyDrawer
    {
        protected class State
        {
            internal ReorderableList m_ReorderableList;
            public int lastSelectedIndex;
        }
        protected SerializedProperty m_FieldsArray;
        protected ReorderableList m_ReorderableList;
        protected int m_LastSelectedIndex;

        const int kExtraSpacing = 9;

        Dictionary<string, State> m_States = new Dictionary<string, State>();

        /* should be const, but won't compile if so */
//        GUIContent kNoFieldContent = new GUIContent("No Field");

        private State GetState(SerializedProperty prop)
        {
            State state;
            string key = prop.propertyPath;
            m_States.TryGetValue(key, out state);
            if (state == null)
            {
                state = new State();
                SerializedProperty fieldsArray = prop.FindPropertyRelative(GetListName());
                state.m_ReorderableList = new ReorderableList(prop.serializedObject, fieldsArray, false, true, true, true);
                state.m_ReorderableList.drawHeaderCallback = DrawHeader;
                state.m_ReorderableList.drawElementCallback = DrawElement;
                state.m_ReorderableList.drawFooterCallback = DrawFooter;
                state.m_ReorderableList.onSelectCallback = SelectParam;
                state.m_ReorderableList.onReorderCallback = EndDragChild;
                state.m_ReorderableList.onAddCallback = AddElement;
                state.m_ReorderableList.onRemoveCallback = RemoveButton;
                // Two standard lines with standard spacing between and extra spacing below to better separate items visually.
                state.m_ReorderableList.elementHeight = GetElementHeight();
                // The index should be -1 if the array size is 0.
                state.m_ReorderableList.index = fieldsArray.arraySize - 1;

                m_States[key] = state;
            }
            return state;
        }

        protected virtual float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing + kExtraSpacing;
        }


        private State RestoreState(SerializedProperty prop)
        {
            State state = GetState(prop);
            m_FieldsArray = state.m_ReorderableList.serializedProperty;
            m_ReorderableList = state.m_ReorderableList;
            m_LastSelectedIndex = state.lastSelectedIndex;

            return state;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            State state = RestoreState(property);

            OnGUI(position);

            state.lastSelectedIndex = m_LastSelectedIndex;
        }

        protected virtual void DrawHeader(Rect headerRect)
        {
            headerRect.height = 16;
            GUI.Label(headerRect, "Parameters");
        }

        public void DrawFooter (Rect footerRect)
        {
            bool enableAddButton = m_ReorderableList.displayAdd;
            bool enableRemoveButton = m_ReorderableList.displayRemove;
            float buttonWidth = 25f;
            float rightEdge = footerRect.xMax;
            float leftEdge = rightEdge - (buttonWidth * 2f) - 8f;
            footerRect = new Rect(leftEdge, footerRect.y, rightEdge - leftEdge, footerRect.height);
            Rect addRect = new Rect(leftEdge + 4, footerRect.y - 3, buttonWidth, 13);
            addRect.yMax += 5;
            Rect removeRect = new Rect(rightEdge - 29, footerRect.y - 3, buttonWidth, 13);
            removeRect.yMax += 5;
            removeRect.width -= 5;
            removeRect.xMax += 5;
            if (Event.current.type == EventType.repaint)
            {
                ReorderableList.defaultBehaviours.footerBackground.Draw(footerRect, false, false, false, false);
            }

            GUI.enabled = enableAddButton;

            if (GUI.Button(addRect, m_ReorderableList.onAddDropdownCallback != null ? ReorderableList.defaultBehaviours.iconToolbarPlusMore : ReorderableList.defaultBehaviours.iconToolbarPlus, ReorderableList.defaultBehaviours.preButton))
            {
                if (m_ReorderableList.onAddDropdownCallback != null)
                    m_ReorderableList.onAddDropdownCallback(addRect, m_ReorderableList);
                else if (m_ReorderableList.onAddCallback != null)
                    m_ReorderableList.onAddCallback(m_ReorderableList);
                else
                    ReorderableList.defaultBehaviours.DoAddButton(m_ReorderableList);

                if (m_ReorderableList.onChangedCallback != null)
                    m_ReorderableList.onChangedCallback(m_ReorderableList);
            }

            GUI.enabled = enableRemoveButton &&
                m_ReorderableList.index >= 0 && 
                m_ReorderableList.index < m_ReorderableList.count &&
                (m_ReorderableList.onCanRemoveCallback == null || m_ReorderableList.onCanRemoveCallback(m_ReorderableList));
            
            if (GUI.Button(removeRect, ReorderableList.defaultBehaviours.iconToolbarMinus, ReorderableList.defaultBehaviours.preButton))
            {
                if (m_ReorderableList.onRemoveCallback == null)
                    RemoveButton(m_ReorderableList);
                else
                    m_ReorderableList.onRemoveCallback(m_ReorderableList);

                if (m_ReorderableList.onChangedCallback != null)
                    m_ReorderableList.onChangedCallback(m_ReorderableList);
            }

            GUI.enabled = true;
        }

        Rect[] GetRowRects(Rect rect)
        {
            Rect[] rects = new Rect[4];

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 2;

            Rect nameRect = rect;
            nameRect.width *= 0.3f;

            Rect staticRect = nameRect;
            staticRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Rect propRect = rect;
            propRect.xMin = staticRect.xMax + EditorGUIUtility.standardVerticalSpacing;

            Rect targetRect = propRect;
            targetRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            rects[0] = nameRect;
            rects[1] = staticRect;
            rects[2] = propRect;
            rects[3] = targetRect;
            return rects;
        }


        void OnGUI(Rect position)
        {
            if (m_FieldsArray == null || !m_FieldsArray.isArray)
            {
                return;
            }
            if (m_ReorderableList != null)
            {
                var oldIdentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                m_ReorderableList.DoList(position);
                EditorGUI.indentLevel = oldIdentLevel;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            RestoreState(property);

            float height = 0f;
            if (m_ReorderableList != null)
            {
                height = m_ReorderableList.GetHeight();
            }
            return height;
        }


        virtual protected void DrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            if (m_FieldsArray.arraySize > 0)
            {
                var field = m_FieldsArray.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField (rect, field);
            }
        }

        protected virtual void AddElement(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);
            m_LastSelectedIndex = list.index;
        }

        protected virtual void SelectParam(ReorderableList list)
        {
            m_LastSelectedIndex = list.index;
        }

        void EndDragChild(ReorderableList list)
        {
            m_LastSelectedIndex = list.index;
        }

        protected virtual void RemoveButton(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            m_LastSelectedIndex = list.index;
        }

//        public GenericMenu BuildPopupList(Object target, SerializedProperty field)
//        {
//            GameObject targetToUse;
//            if (target is Component)
//                targetToUse = ((Component)target).gameObject;
//            else
//                targetToUse = (GameObject)target;
//
//
//            var fieldName = field.FindPropertyRelative("m_FieldPath").stringValue;
//
//            var menu = new GenericMenu();               
//
//            menu.AddItem(kNoFieldContent,
//                string.IsNullOrEmpty(fieldName),
//                () => {});
//
//            if (targetToUse == null)
//                return menu;
//
//            menu.AddSeparator("");
//            GeneratePopupForType(menu, targetToUse, targetToUse, field, "", 0);
//
//            Component[] comps = targetToUse.GetComponents<Component> ();
//
//            foreach (Component comp in comps)
//            {
//                if (comp == null)
//                    continue;
//
//                GeneratePopupForType(menu, comp, comp, field, "", 0);
//            }
//            return menu;
//        }

        public static object GetValue(MemberInfo m, object v)
        {
            object ret = null;
            try
            {
                ret = ((m is FieldInfo) ?
                    ((FieldInfo)m).GetValue(v) :
                    ((PropertyInfo)m).GetValue(v, null));
            }
            /* some properties are not supported, and we should just not list them */
            catch (TargetInvocationException) {}
            /* we don't support indexed properties, either, which trigger this exception */
            catch (TargetParameterCountException) {}
            return ret;
        }

        private void GeneratePopupForType(GenericMenu menu,
            Object originalTarget,
            object target,
            SerializedProperty fieldProp,
            String prefix,
            int depth)
        {
            var fields = Array.FindAll(target.GetType().GetMembers(),
                x => (x.GetType().Name == "MonoProperty" ||
                    x.GetType().Name == "MonoField"));


            foreach (var field in fields)
            {
                var path = "";
                if (prefix == "")
                    path = field.Name;
                else 
                    path = String.Concat(prefix, "/", field.Name);

                Type myType = field.GetType();

                myType = (field.GetType().Name == "MonoField" ?
                    ((FieldInfo)field).FieldType :
                    ((PropertyInfo)field).PropertyType);

                if (myType.IsPrimitive || myType == typeof(string))
                {
                    var fieldPath = path.Replace("/", ".");
                    var activated = ((fieldProp.FindPropertyRelative("m_Target").objectReferenceValue
                        == originalTarget) &&
                        (fieldProp.FindPropertyRelative("m_FieldPath").stringValue
                            == fieldPath));

                    menu.AddItem(new GUIContent(originalTarget.GetType().Name + "/" + path),
                        activated,
                        SetProperty,
                        new PropertySetter(fieldProp,
                            originalTarget,
                            fieldPath));
                }
                else if (depth <= 1) 
                {
                    /* it must be a struct, and we can expand it */
                    object temp;
                    if ((field.Name == "mesh" && target.GetType().Name == "MeshFilter") ||
                        ((field.Name == "material" || field.Name == "materials")
                            && target is Renderer))
                        continue;

                    temp = GetValue(field, target);
                    if (temp != null) 
                    {
                        GeneratePopupForType(menu,
                            originalTarget,
                            (object)(temp),
                            fieldProp,
                            path,
                            depth+1);

                    }
                }
                /* ignore structs at depth > 1, because we can't expand forever and 
             * we don't want to send structs as strings 
             */
            }
        }

        static void SetProperty(object source)
        {
            ((PropertySetter) source).Assign();
        }

        static void ClearProperty(object source)
        {
            ((PropertySetter) source).Clear();
        }

        struct PropertySetter
        {
            readonly SerializedProperty m_Prop;
            readonly object m_Target;
            readonly String m_FieldPath;

            public PropertySetter(SerializedProperty p,
                object target,
                String fp)
            {
                m_Prop = p;
                m_Target = target;
                m_FieldPath = fp;
            }

            public void Assign()
            {
                m_Prop.FindPropertyRelative("m_Target").objectReferenceValue = (Object)m_Target;
                m_Prop.FindPropertyRelative("m_FieldPath").stringValue = m_FieldPath;

                m_Prop.serializedObject.ApplyModifiedProperties();
            }

            public void Clear()
            {
                m_Prop.FindPropertyRelative("m_Target").objectReferenceValue = null;
                m_Prop.FindPropertyRelative("m_FieldPath").stringValue = null;

                m_Prop.serializedObject.ApplyModifiedProperties();

            }
        }




        virtual protected string GetListName()
        {
            return "m_Fields";
        }
    }
}

