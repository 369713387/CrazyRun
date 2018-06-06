using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    public enum TriggerLifecycleEvent
    {
        None = 0,
        Awake,
        Start,
        OnEnable,
        OnDisable,
        OnApplicationPause,
        OnApplicationUnpause,
        OnDestroy,
    }
}

