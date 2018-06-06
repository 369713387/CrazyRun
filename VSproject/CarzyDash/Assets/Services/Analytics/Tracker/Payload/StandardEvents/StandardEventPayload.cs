using System.Collections;
using System.Collections.Generic;
using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [Serializable]
    public class AnalyticsEventParamListContainer
    {
        [SerializeField]
        List<AnalyticsEventParam> m_Parameters = new List<AnalyticsEventParam>();
        public List<AnalyticsEventParam> parameters{
            get
            {
                return m_Parameters;
            }
            set
            {
                m_Parameters = value;
            }
        }
    }

    [Serializable]
    public class StandardEventPayload
    {
        #if UNITY_EDITOR
        // Editor-specific properties
        #pragma warning disable 0414
        [SerializeField]
        bool m_IsEventExpanded = true;

        [SerializeField]
        string m_StandardEventType = "CustomEvent";
        public Type standardEventType;
        #pragma warning restore
        #endif


        [SerializeField]
        AnalyticsEventParamListContainer m_Parameters;
        public AnalyticsEventParamListContainer parameters
        {
            get {
                return m_Parameters;
            }
        }

        static Dictionary<string, object> m_EventData = new Dictionary<string, object>();

        [SerializeField]
        string m_Name = "";
        public string name
        {
            get {
                return m_Name;
            }
            set {
                m_Name = value;
            }
        }

        public StandardEventPayload()
        {
            m_Parameters = new AnalyticsEventParamListContainer ();
        }

        public virtual AnalyticsResult Send ()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception(
                    "Analtyics Event Tracker failed to send the CustomEvent. The event Name field cannot be empty."
                );
            }

            if (!IsCustomDataValid())
            {
                throw new Exception(
                    "Analytics event tracker failed to send. The event data is not valid. Parameter names cannot be null or empty."
                );
            }

            if (!IsRequiredDataValid())
            {
                throw new Exception(
                    "Analytics event tracker failed to send. The event data is not valid. Please check the values of required parameters."
                );
            }

            return AnalyticsEvent.Custom(name, GetParameters());
        }

        IDictionary<string, object> GetParameters()
        {
            m_EventData.Clear();
            var parms = parameters.parameters;

            for (int a = 0; a < parms.Count; a++)
            {
                if (parms[a] != null && parms[a].valueProperty.IsValid())
                {
                    m_EventData.Add(parms[a].name, parms[a].value);
                }
            }

            return m_EventData;
        }

        bool IsCustomDataValid ()
        {
            var p = parameters.parameters;

            for (int i = 0; i < p.Count; i++)
            {
                if (p[i] == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(p[i].name) && p[i].valueProperty.IsValid())
                {
                    return false;
                }
            }

            return true;
        }

        bool IsRequiredDataValid ()
        {
            var p = parameters.parameters;
            var validationGroups = new Dictionary<string, List<bool>>();

            for (int i = 0; i < p.Count; i++)
            {
                if (p[i] != null && p[i].requirementType == AnalyticsEventParam.RequirementType.Required)
                {
                    if (string.IsNullOrEmpty(p[i].groupID))
                    {
                        if (!validationGroups.ContainsKey("none"))
                        {
                            validationGroups.Add("none", new List<bool>());
                        }

                        validationGroups["none"].Add(p[i].valueProperty.IsValid());
                    }
                    else
                    {
                        if (!validationGroups.ContainsKey(p[i].groupID))
                        {
                            validationGroups.Add(p[i].groupID, new List<bool>());
                        }

                        validationGroups[p[i].groupID].Add(p[i].valueProperty.IsValid());
                    }
                }
            }

            foreach (var groupId in validationGroups.Keys)
            {
                switch (groupId)
                {
                case "none":
                    if (validationGroups[groupId].Contains(false))
                    {
                        return false;
                    }
                    break;
                default:
                    if (!validationGroups[groupId].Contains(true))
                    {
                        return false;
                    }
                    break;
                }
            }

            return true;
        }
    }
}
