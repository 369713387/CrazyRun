﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [AddComponentMenu("Analytics/Experimental/Analytics Event Tracker")]
    public class AnalyticsEventTracker : MonoBehaviour
    {

        [SerializeField]
        public EventTrigger m_Trigger = new EventTrigger();

        [SerializeField]
        StandardEventPayload m_EventPayload = new StandardEventPayload();
        public StandardEventPayload payload
        {
            get
            {
                return m_EventPayload;
            }
        }

        public void TriggerEvent ()
        {
            SendEvent();
        }

        AnalyticsResult SendEvent ()
        {
            if (m_Trigger.Test())
            {
                return payload.Send();
            }
            return AnalyticsResult.Ok;
        }

        void Awake ()
        {
            if (m_Trigger.triggerType == TriggerType.Lifecycle &&
                m_Trigger.lifecycleEvent == TriggerLifecycleEvent.Awake)
            {
                SendEvent();
            }
        }

        void Start ()
        {
            if (m_Trigger.triggerType == TriggerType.Lifecycle &&
                m_Trigger.lifecycleEvent == TriggerLifecycleEvent.Start)
            {
                SendEvent();
            }
            else if (m_Trigger.triggerType == TriggerType.Timer)
            {
                StartCoroutine(TimedTrigger());
            }
        }

        void OnEnable ()
        {
            if (m_Trigger.triggerType == TriggerType.Lifecycle &&
                m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnEnable)
            {
                SendEvent();
            }
        }

        void OnDisable ()
        {
            if (m_Trigger.triggerType == TriggerType.Lifecycle &&
                m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnDisable)
            {
                SendEvent();
            }
        }

        void OnApplicationPause (bool paused)
        {
            if (m_Trigger.triggerType == TriggerType.Lifecycle)
            {
                if (paused && m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnApplicationPause)
                {
                    SendEvent();
                }
                else if (!paused && m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnApplicationUnpause)
                {
                    SendEvent();
                }
            }
        }

        void OnDestroy ()
        {
            if (m_Trigger.triggerType == TriggerType.Lifecycle &&
                m_Trigger.lifecycleEvent == TriggerLifecycleEvent.OnDestroy)
            {
                SendEvent();
            }
        }

        IEnumerator TimedTrigger ()
        {
            if (m_Trigger.initTime > 0)
            {
                yield return new WaitForSeconds(m_Trigger.initTime);
            }

            SendEvent();

            while (m_Trigger.repetitions == 0 || m_Trigger.repetitionCount <= m_Trigger.repetitions)
            {
                if (m_Trigger.repeatTime > 0)
                {
                    yield return new WaitForSeconds(m_Trigger.repeatTime);
                }
                else
                {
                    yield return null;
                }

                SendEvent();
            }
        }
    }
}
