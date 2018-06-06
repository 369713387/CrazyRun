using System;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class StandardEventName : Attribute
    {
        public string sendName;
        public string path;
        public string tooltip;

        public StandardEventName(string sendName, string path, string tooltip)
        {
            this.sendName = sendName;
            this.path = path;
            this.tooltip = tooltip;
        }
    }

    public class AnalyticsEventParameter : Attribute
    {
        public string sendName;
        public string tooltip;
        public string groupId;

        public AnalyticsEventParameter(string sendName, string tooltip, string groupId = null)
        {
            this.sendName = sendName;
            this.tooltip = tooltip;
            this.groupId = groupId;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredParameter : AnalyticsEventParameter
    {
        public RequiredParameter(string sendName, string tooltip, string groupId = null) : base(sendName, tooltip, groupId)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class OptionalParameter : AnalyticsEventParameter
    {
        public OptionalParameter(string sendName, string tooltip) : base(sendName, tooltip)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CustomizableEnum : Attribute
    {
        public bool Customizable;
        public CustomizableEnum(bool customizable)
        {
            this.Customizable = customizable;
        }
    }    
}

