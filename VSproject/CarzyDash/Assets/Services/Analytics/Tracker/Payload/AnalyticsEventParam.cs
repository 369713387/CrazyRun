using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [Serializable]
    public class AnalyticsEventParam
    {
        public enum RequirementType
        {
            None,
            Required,
            Optional
        }

        [SerializeField]
        RequirementType m_RequirementType = RequirementType.None;
        public RequirementType requirementType
        {
            get { return m_RequirementType; }
        }

        [SerializeField]
        string m_GroupID;
        public string groupID
        {
            get { return m_GroupID; }
        }

        #if UNITY_EDITOR
        // Editor-specific properties
        #pragma warning disable 0414
        [SerializeField]
        string m_Tooltip = "";
        #pragma warning restore
        #endif

        [SerializeField]
        string m_Name;
        [SerializeField]
        ValueProperty m_Value;
        public ValueProperty valueProperty
        {
            get
            {
                return m_Value;
            }
        }

        public AnalyticsEventParam (string name = null, params Type[] validTypes)
        {
            m_Name = name;

            if (validTypes.Length > 0)
            {
                
            }
        }

        public string name
        {
            get { return m_Name; }
        }

        public object value
        {
            get
            {
                return m_Value.propertyValue;
//                if (m_TrackableField != null)
//                {
//                    return m_TrackableField.GetValue();
//                }
//
//                var paramType = Type.GetType(m_ParamTypeName);
//
//                if (paramType == typeof(char))    return m_ParamStringValue[0];
//                if (paramType == typeof(sbyte))   return sbyte.Parse(m_ParamStringValue);
//                if (paramType == typeof(byte))    return byte.Parse(m_ParamStringValue);
//                if (paramType == typeof(short))   return short.Parse(m_ParamStringValue);
//                if (paramType == typeof(ushort))  return ushort.Parse(m_ParamStringValue);
//                if (paramType == typeof(int))     return int.Parse(m_ParamStringValue);
//                if (paramType == typeof(uint))    return uint.Parse(m_ParamStringValue);
//                if (paramType == typeof(long))    return long.Parse(m_ParamStringValue);
//                if (paramType == typeof(ulong))   return ulong.Parse(m_ParamStringValue);
//                if (paramType == typeof(bool))    return bool.Parse(m_ParamStringValue);
//                if (paramType == typeof(float))   return float.Parse(m_ParamStringValue);
//                if (paramType == typeof(double))  return double.Parse(m_ParamStringValue);
//                if (paramType == typeof(decimal)) return decimal.Parse(m_ParamStringValue);
//
//                return m_ParamStringValue;
            }
        }
    }
}
