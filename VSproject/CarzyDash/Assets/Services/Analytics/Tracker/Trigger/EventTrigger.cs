using System;
using System.Collections.Generic;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [Serializable]
    public class TriggerListContainer
    {
        [SerializeField]
        List<TriggerRule> m_Rules = new List<TriggerRule>();
        internal List<TriggerRule> rules{
            get
            {
                return m_Rules;
            }
            set
            {
                m_Rules = value;
            }
        }
    }



    [Serializable]
    public class EventTrigger
    {
        #if UNITY_EDITOR
        // Editor-specific properties
        #pragma warning disable 0414
        [SerializeField]
        bool m_IsTriggerExpanded = true;
        #pragma warning restore
        #endif

        [SerializeField]
        TriggerType m_Type;
        public TriggerType triggerType
        {
            get {
                return m_Type;
            }
        }

        // Properties for Lifecycle
        [SerializeField]
        TriggerLifecycleEvent m_LifecycleEvent;
        public TriggerLifecycleEvent lifecycleEvent
        {
            get {
                return m_LifecycleEvent;
            }
        }

        // Properties for Property watcher
        [SerializeField]
        bool m_ApplyRules = false;

        [SerializeField]
        TriggerListContainer m_Rules;

        [SerializeField]
        TriggerBool m_TriggerBool = TriggerBool.All;

        [SerializeField]
        float m_InitTime = 5f;
        public float initTime
        {
            get {
                return m_InitTime;
            }
            set {
                m_InitTime = value;
            }
        }

        [SerializeField]
        float m_RepeatTime = 5f;
        public float repeatTime
        {
            get {
                return m_RepeatTime;
            }
            set {
                m_RepeatTime = value;
            }
        }

        [SerializeField]
        int m_Repetitions = 0;
        public int repetitions
        {
            get {
                return m_Repetitions;
            }
            set {
                m_Repetitions = value;
            }
        }

        public int repetitionCount = 0;


        // Delegate for triggering when using properties
        internal delegate void OnTrigger();
        OnTrigger m_TriggerFunction;

        // Properties for Method Watch
        [SerializeField]
        TriggerMethod m_Method;

        public EventTrigger ()
        {
            m_Rules = new TriggerListContainer();
            m_Rules.rules.Add(new TriggerRule());
        }

        public void AddRule()
        {
            var newRule = new TriggerRule();
            m_Rules.rules.Add(newRule);
        }

        public void RemoveRule(int index)
        {
            m_Rules.rules.RemoveAt (index);
        }

        public bool Test()
        {
            // If no rules, pass test.
            if (!m_ApplyRules) {
                return true;
            }

            // If all repetitions used, fail test.
            if (repetitions > 0 && repetitionCount >= repetitions) {
                return false;
            }

            bool passTest = false;
            int passCount = 0;
            int count = 0;

            // Do we meet the terms of the rules?
            foreach(TriggerRule rule in m_Rules.rules)
            {
                count++;
                if (rule.Test ()) {
                    passCount ++;
                }
                switch (m_TriggerBool) {
                case TriggerBool.All:
                    if (passCount < count) {
                        passTest = false;
                        break;
                    }
                    break;
                case TriggerBool.None:
                    if (passCount > 0) {
                        passTest = false;
                        break;
                    }
                    break;
                case TriggerBool.Any:
                    if (passCount > 0) {
                        passTest = true;
                        break;
                    }
                    break;
                }
            }

            if ((m_TriggerBool == TriggerBool.All && passCount == count) ||
                (m_TriggerBool == TriggerBool.None && passCount == 0)) {
                passTest = true;
            }

            // Increment repetitions, if using.
            if (repetitions > 0 && passTest) {
                repetitionCount++;
            }
            return passTest;
        }
    }
}

