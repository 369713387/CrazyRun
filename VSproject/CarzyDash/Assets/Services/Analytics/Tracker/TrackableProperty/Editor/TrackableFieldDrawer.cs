using System;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics.Experimental.Tracker;
using Object = UnityEngine.Object;

namespace UnityEditor.Analytics.Experimental.Tracker
{
    [CustomPropertyDrawer(typeof(TrackableField), true)]
    public class TrackableFieldDrawer : PropertyDrawer
    {
        static readonly string k_EmptyPath = "No Field";
        static readonly int k_maxDepth = 1;

        SerializedProperty m_Prop;
        SerializedProperty m_TargetProperty;
        SerializedProperty m_PathProperty;
        SerializedProperty m_ValidTypeNamesProperty;

        GUIContent m_TargetContent = new GUIContent("", "The target driving the value.");
        GUIContent m_PathContent = new GUIContent("", "The path of the field driving the value.");

        Rect m_TargetPosition;
        Rect m_PathPosition;

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            m_Prop = property;
            m_TargetProperty = property.FindPropertyRelative("m_Target");
            m_PathProperty = property.FindPropertyRelative("m_Path");
            m_ValidTypeNamesProperty = property.FindPropertyRelative("m_ValidTypeNames");
            string[] validTypes = new string[m_ValidTypeNamesProperty.arraySize];
            for (int i = 0; i < m_ValidTypeNamesProperty.arraySize; i++)
            {
                validTypes[i] = m_ValidTypeNamesProperty.GetArrayElementAtIndex(i).stringValue;
            }

            m_PathContent.text = k_EmptyPath;

            m_TargetPosition = new Rect(position.x,
                position.y,
                (position.width * .5f),
                EditorGUIUtility.singleLineHeight);

            m_PathPosition = new Rect(m_TargetPosition.x + m_TargetPosition.width,
                position.y,
                m_TargetPosition.width,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.BeginChangeCheck();
            {
                GUI.Box(m_TargetPosition, m_TargetContent);
                EditorGUI.PropertyField(m_TargetPosition, m_TargetProperty, GUIContent.none);

                if (EditorGUI.EndChangeCheck())
                {
                    m_PathProperty.stringValue = null;
                }
            }

            var target = m_TargetProperty.objectReferenceValue;

            GUI.enabled = (target != null);

            if (target != null && !string.IsNullOrEmpty(m_PathProperty.stringValue))
            {
                m_PathContent.text = string.Concat(target.GetType().Name, ".", m_PathProperty.stringValue);
            }

            if (GUI.Button(m_PathPosition, m_PathContent, EditorStyles.popup))
            {
                FieldSelectMenu(target, validTypes).DropDown(m_PathPosition);
            }

            GUI.enabled = true;

            EditorGUI.EndProperty();
        }

        public GenericMenu FieldSelectMenu (Object target, string[] validTypes)
        {
            var menu = new GenericMenu();
            GameObject gameObject = (target is Component) ? ((Component)target).gameObject : (GameObject)target;
            Component[] components = gameObject.GetComponents<Component>();

            menu.AddItem(
                new GUIContent(k_EmptyPath),
                string.IsNullOrEmpty(m_PathProperty.stringValue),
                ClearPath,
                new PropertySetter(m_Prop,
                       null,
                       null,
                       null)
            );

            if (gameObject == null)
            {
                return menu;
            }

            menu.AddSeparator("");

            AddMenuItems(menu, gameObject, gameObject, validTypes);

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null)
                {
                    AddMenuItems(menu, components[i], components[i], validTypes);
                }
            }

            return menu;
        }

        void AddMenuItems (GenericMenu menu, Object originalTarget, object target, string[] validTypes, string baseMenuPath = null, int depth = 0)
        {
            var menuItemPath = new StringBuilder();

            var members = Array.FindAll(
                target.GetType().GetMembers(),
                x => (x.GetType().Name == "MonoProperty" || x.GetType().Name == "MonoField")
            );

            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                var memberType = (member.GetType().Name == "MonoField")
                    ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType;
                menuItemPath.Remove(0, menuItemPath.Length);

                if (!string.IsNullOrEmpty(baseMenuPath))
                {
                    menuItemPath.Append(baseMenuPath);
                    menuItemPath.Append("/");
                }

                menuItemPath.Append(member.Name);
                if (IsValidType(memberType))
                {
                    string typeStr = memberType.ToString();
                    if (memberType.IsEnum)
                    {
                        typeStr = "enum";
                    }
                    if (isValidFromTypeArray(typeStr, validTypes))
                    {
                        var memberPath = menuItemPath.ToString().Replace("/", ".");
                        if (typeStr.Equals("enum"))
                        {
                            if (!string.IsNullOrEmpty(m_Prop.FindPropertyRelative("m_EnumType").stringValue))
                            {
                                if (memberType.Equals(CustomEnumPopup.GetEnumType(m_Prop.FindPropertyRelative("m_EnumType").stringValue)))
                                {
                                    menu.AddItem(
                                    new GUIContent(string.Concat(originalTarget.GetType().Name, "/", menuItemPath)),
                                    m_TargetProperty.objectReferenceValue == originalTarget && m_PathProperty.stringValue == memberPath,
                                    SetProperty,
                                    new PropertySetter(m_Prop,
                                           originalTarget,
                                           memberPath,
                                           typeStr)
                                    );
                                }
                            }
                            else
                            {
                                menu.AddItem(
                                    new GUIContent(string.Concat(originalTarget.GetType().Name, "/", menuItemPath)),
                                    m_TargetProperty.objectReferenceValue == originalTarget && m_PathProperty.stringValue == memberPath,
                                    SetPropertyWithEnum,
                                    new PropertySetterWithEnum(m_Prop,
                                           originalTarget,
                                           memberPath,
                                           typeStr,
                                           memberType.ToString())
                                    );
                            }
                        }
                        else
                        {
                            menu.AddItem(
                                new GUIContent(string.Concat(originalTarget.GetType().Name, "/", menuItemPath)),
                                m_TargetProperty.objectReferenceValue == originalTarget && m_PathProperty.stringValue == memberPath,
                                SetProperty,
                                new PropertySetter(m_Prop,
                                       originalTarget,
                                       memberPath,
                                       typeStr)
                            );
                        }


                    }
                }
                else if (depth <= k_maxDepth && IsValidSubTarget(target, member))
                {
                    var memberValue = GetMemberValue(target, member);

                    if (memberValue != null)
                    {
                        AddMenuItems(menu, originalTarget, memberValue, validTypes, menuItemPath.ToString(), depth + 1);
                    }
                }
            }
        }

        bool IsValidSubTarget (object target, MemberInfo member)
        {
            if (GetMemberValue(target, member) is bool) return false;
            if (target is GameObject && member.Name == "scene") return false;
            if (target is MeshFilter && member.Name == "mesh") return false;
            if (target is Renderer)
            {
                if (member.Name == "material" || member.Name == "materials") return false;
            }
            if (target is Vector2)
            {
                if (member.Name == "magnitude" || member.Name == "normalized") return true;
                if (member.Name == "x" || member.Name == "y") return true;
                return false;
            }
            if (target is Vector3)
            {
                if (member.Name == "x" || member.Name == "y" || member.Name == "z") return true;
                return false;
            }
            if (target is Color)
            {
                if (member.Name == "grayscale") return true;
                if (member.Name != "r" && member.Name != "g" && member.Name != "b" && member.Name != "a") return false;
            }
            if (target is Transform)
            {
                if (member.Name == "root") return false;
                if (member.Name == "right" || member.Name == "up" || member.Name == "forward" || member.Name == "magnitude") return false;
                if (member.Name == "worldToLocalMatrix" || member.Name == "localToWorldMatrix") return false;
            }
            if (target is Quaternion && member.Name == "identity") return false;
            if (target is Rect && member.Name == "zero") return false;
            if (member.Name == "runInEditMode" ||
                member.Name == "hasChanged" || 
                member.Name == "gameObject" ||
                member.Name == "transform" ||
                member.Name == "rectTransform" ||
                member.Name == "canvas" ||
                member.Name == "canvasRenderer" ||
                member.Name == "rootCanvas" ||
                member.Name == "parent" ||
                member.Name == "hideFlags" ||
                member.Name == "name" ||
                member.Name == "tag")
            {
                return false;
            }

            return true;
        }

        bool isValidFromTypeArray (string testType, params string[] validTypes)
        {
            if (validTypes != null && validTypes.Length > 0)
            {
                return Array.IndexOf(validTypes, testType) >= 0;
            }
            else
            {
                return true;
            }
        }

        bool IsValidType (Type testType)
        {
            return (
                testType.IsEnum ||
                testType == typeof(int) ||
                testType == typeof(float) ||
                testType == typeof(string) ||
                testType == typeof(bool)
            );
        }

        object GetMemberValue (object target, MemberInfo member)
        {
            object value = null;

            try
            {
                if (member is FieldInfo)
                {
                    value = ((FieldInfo)member).GetValue(target);
                }
                else
                {
                    value = ((PropertyInfo)member).GetValue(target, null);
                }
            }
            // Some properties are not supported, and we should just not list them.
            catch (TargetInvocationException) { }
            // We don't support indexed properties, either, which trigger this exception.
            catch (TargetParameterCountException) { }

            return value;
        }

        static void SetProperty (object source)
        {
            ((PropertySetter)source).Assign();
        }

        static void ClearPath(object source)
        {
            ((PropertySetter)source).ClearPath();
        }

        static void ClearProperty (object source)
        {
            ((PropertySetter)source).Clear();
        }

        struct PropertySetter
        {
            readonly SerializedProperty m_Prop;
            readonly object m_Target;
            readonly String m_FieldPath;
            readonly String m_Type;

            public PropertySetter (SerializedProperty p,
                object target,
                String fp,
                String t)
            {
                m_Prop = p;
                m_Target = target;
                m_FieldPath = fp;
                m_Type = t;
            }

            public void Assign ()
            {
                m_Prop.FindPropertyRelative("m_Target").objectReferenceValue = (Object)m_Target;
                m_Prop.FindPropertyRelative("m_Path").stringValue = m_FieldPath;
                m_Prop.FindPropertyRelative("m_Type").stringValue = m_Type;

                m_Prop.serializedObject.ApplyModifiedProperties();
            }

            public void Clear ()
            {
                m_Prop.FindPropertyRelative("m_Target").objectReferenceValue = null;
                m_Prop.FindPropertyRelative("m_Path").stringValue = null;
                m_Prop.FindPropertyRelative("m_Type").stringValue = null;

                m_Prop.serializedObject.ApplyModifiedProperties();
            }

            public void ClearPath()
            {
                m_Prop.FindPropertyRelative("m_Path").stringValue = null;
                m_Prop.FindPropertyRelative("m_Type").stringValue = null;

                m_Prop.serializedObject.ApplyModifiedProperties();
            }
        }

        static void SetPropertyWithEnum (object source)
        {
            ((PropertySetterWithEnum)source).Assign();
        }

        static void ClearPropertyWithEnum (object source)
        {
            ((PropertySetterWithEnum)source).Clear();
        }

        struct PropertySetterWithEnum
        {
            readonly SerializedProperty m_Prop;
            readonly object m_Target;
            readonly String m_FieldPath;
            readonly String m_Type;
            readonly String m_EnumType;

            public PropertySetterWithEnum (SerializedProperty p,
                object target,
                String fp,
                String t,
                String eT)
            {
                m_Prop = p;
                m_Target = target;
                m_FieldPath = fp;
                m_Type = t;
                m_EnumType = eT;
            }

            public void Assign ()
            {
                m_Prop.FindPropertyRelative("m_Target").objectReferenceValue = (Object)m_Target;
                m_Prop.FindPropertyRelative("m_Path").stringValue = m_FieldPath;
                m_Prop.FindPropertyRelative("m_Type").stringValue = m_Type;
                m_Prop.FindPropertyRelative("m_EnumType").stringValue = m_EnumType;

                m_Prop.serializedObject.ApplyModifiedProperties();
            }

            public void Clear ()
            {
                m_Prop.FindPropertyRelative("m_Target").objectReferenceValue = null;
                m_Prop.FindPropertyRelative("m_Path").stringValue = null;
                m_Prop.FindPropertyRelative("m_Type").stringValue = null;
                m_Prop.FindPropertyRelative("m_EnumType").stringValue = null;

                m_Prop.serializedObject.ApplyModifiedProperties();

            }
        }
    }
}
