using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [Serializable]
    public class TrackableTrigger
    {
        [SerializeField]
        GameObject m_Target;
        [SerializeField]
        string m_MethodPath;
    }
}
