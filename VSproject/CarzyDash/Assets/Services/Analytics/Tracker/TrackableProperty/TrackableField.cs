using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [Serializable]
    public class TrackableField : TrackableProperty
    {
        [SerializeField]
        string[] m_ValidTypeNames;

        public TrackableField (params Type[] validTypes)
        {
            if (validTypes == null || validTypes.Length == 0)
            {
                return;
            }

            m_ValidTypeNames = new string[validTypes.Length];

            for (int i = 0; i < validTypes.Length; i++)
            {
                m_ValidTypeNames[i] = validTypes[i].ToString();
            }
        }

        public object GetValue ()
        {
            if (m_Target == null || string.IsNullOrEmpty(m_Path))
            {
                return null;
            }

            object value = m_Target;

            foreach (var s in m_Path.Split('.'))
            {
                try
                {
                    var tmp = value.GetType().GetProperty(s);
                    value = tmp.GetValue(value, null);
                }
                catch
                {
                    var tmp = value.GetType().GetField(s);
                    value = tmp.GetValue(value);
                }
            }

            return value;
        }

        [SerializeField]
        string m_Type;

        [SerializeField]
        string m_EnumType;
    }
}
