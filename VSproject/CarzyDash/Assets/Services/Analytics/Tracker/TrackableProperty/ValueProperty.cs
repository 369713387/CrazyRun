using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [Serializable]
    public class ValueProperty
    {
        #if UNITY_EDITOR
        // Editor-specific properties
        #pragma warning disable 0414

        [SerializeField]
        bool m_EditingCustomValue = false;

        [SerializeField]
        int m_PopupIndex = 0;
        
        [SerializeField]
        string m_CustomValue;        
        
        [SerializeField]
        bool m_FixedType = false;

        [SerializeField]
        string m_EnumType;

        [SerializeField]
        bool m_EnumTypeIsCustomizable = true;

        [SerializeField]
        bool m_CanDisable = false;

        #pragma warning restore
        #endif

        public enum PropertyType
        {
            Disabled,
            Static,
            Dynamic
        }

        [SerializeField]
        PropertyType m_PropertyType = PropertyType.Static;

        // Derived type of this value
        [SerializeField]
        string m_ValueType;
        public string valueType
        {
            get {
                return m_ValueType;
            }
            set {
                m_ValueType = value;
            }
        }

        // A user entered value
        [SerializeField]
        string m_Value = "";
        public string propertyValue
        {
            get {
                if (m_PropertyType == PropertyType.Dynamic && m_Target != null)
                {
                    return m_Target.GetValue().ToString();
                }
                return m_Value;
            }
        }

        // A dynamically-derived value
        [SerializeField]
        TrackableField m_Target;
        public TrackableField target
        {
            get {
                return m_Target;
            }
        }

        public bool IsValid()
        {
            switch (m_PropertyType)
            {
                case PropertyType.Static:
                    return !string.IsNullOrEmpty(m_Value) || Type.GetType(m_ValueType) != typeof(string);
                case PropertyType.Dynamic:
                    return m_Target != null && m_Target.GetValue() != null;
                case PropertyType.Disabled:
                default:
                    return false;
            }
        }
    }
}

