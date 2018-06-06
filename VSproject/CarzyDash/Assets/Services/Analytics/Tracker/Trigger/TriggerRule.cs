using System;
using System.Reflection;

namespace UnityEngine.Analytics.Experimental.Tracker
{
    [Serializable]
    public class TriggerRule
    {
        // Target property to test, i.e., the lefthand side of the comparison
        [SerializeField]
        TrackableField m_Target;

        // The comparison to make between the two (or three) operands
        [SerializeField]
        TriggerOperator m_Operator;

        // The primary righthand comparison
        [SerializeField]
        ValueProperty m_Value;

        // A secondary righthand (for use with "between" operator)
        [SerializeField]
        ValueProperty m_Value2;

        public TriggerRule ()
        {
        }

        public bool Test()
        {
            // the rule was not set properly, so return true
            if(m_Value.valueType == null)
            {
                return true;
            }

            // target does not exist
            if (m_Target == null || m_Target.GetValue() == null)
            {
                return false;
            }
            // target does not have specified property
            if (m_Value == null || m_Value.propertyValue == null)
            {
                return false;
            }

            object currentValue = m_Target.GetValue();
            string propertyType = m_Value.valueType;
            //TODO: fix this too
            if(propertyType == typeof(string).ToString())
            {
                return TestByString((string)currentValue);
            }
            if(propertyType == typeof (bool).ToString())
            {
                return TestByBool((bool)currentValue);
            }
            if(propertyType == typeof(float).ToString())
            {
                return TestByDouble((float)currentValue);
            }
            if(propertyType == typeof(double).ToString())
            {
                return TestByDouble((double)currentValue);
            }
            if(propertyType == typeof(decimal).ToString())
            {
                return TestByDouble((double)(decimal)currentValue);
            }
            if(propertyType == typeof(int).ToString())
            {
                return TestByDouble((int)currentValue);
            }
            if(propertyType == typeof(short).ToString())
            {
                return TestByDouble((short)currentValue);
            }
            if(propertyType == typeof(long).ToString())
            {
                return TestByDouble((long)currentValue);
            }
            if(propertyType == "enum")
            {
                return TestByEnum(((Enum)currentValue).ToString().ToLower());
            }

            return TestByObject(currentValue);
        }

        bool TestByObject(object currentValue)
        {
            bool passTest = false;
            switch(m_Operator)
            {
            case TriggerOperator.Equals:
                passTest = currentValue.Equals(m_Value.propertyValue);
                break;
            case TriggerOperator.DoesNotEqual:
                passTest = currentValue.Equals(m_Value.propertyValue) == false;
                break;
            }
            return passTest;
        }

        bool TestByEnum(string currentValue)
        {
            bool passTest = false;
            switch(m_Operator)
            {
            case TriggerOperator.Equals:
                passTest = currentValue.Equals(m_Value.propertyValue.ToString().ToLower());
                break;
            case TriggerOperator.DoesNotEqual:
                passTest = currentValue.Equals(m_Value.propertyValue.ToString().ToLower()) == false;
                break;
            }
            return passTest;
        }

        bool TestByString(string currentValue)
        {
            bool passTest = false;
            switch(m_Operator)
            {
            case TriggerOperator.Equals:
                passTest = currentValue.Equals(m_Value.propertyValue);
                break;
            case TriggerOperator.DoesNotEqual:
                passTest = currentValue.Equals(m_Value.propertyValue) == false;
                break;
            }
            return passTest;
        }

        bool TestByBool(bool currentValue)
        {
            bool passTest = false;
            bool propertyValue = bool.Parse (m_Value.propertyValue);
            switch(m_Operator)
            {
            case TriggerOperator.Equals:
                passTest = currentValue.Equals(propertyValue);
                break;
            case TriggerOperator.DoesNotEqual:
                passTest = currentValue.Equals(propertyValue) == false;
                break;
            }
            return passTest;
        }

        bool TestByDouble(double currentValue)
        {
            bool passTest = false;
            double propertyValue = GetDouble (m_Value.propertyValue);


            switch(m_Operator)
            {
            case TriggerOperator.Equals:
                passTest = SafeEquals (currentValue, propertyValue);
                break;
            case TriggerOperator.DoesNotEqual:
                passTest = !SafeEquals (currentValue, propertyValue);
                break;
            case TriggerOperator.IsGreaterThan:
                passTest = currentValue > propertyValue;
                break;
            case TriggerOperator.IsGreaterThanOrEqualTo:
                passTest = currentValue > propertyValue || SafeEquals (currentValue, propertyValue);
                break;
            case TriggerOperator.IsLessThan:
                passTest = currentValue < propertyValue;
                break;
            case TriggerOperator.IsLessThanOrEqualTo:
                passTest = currentValue < propertyValue || SafeEquals (currentValue, propertyValue);
                break;
            case TriggerOperator.IsBetween:
                double propertyValue2 = GetDouble (m_Value2.propertyValue);
                passTest = currentValue > propertyValue && currentValue < propertyValue2;
                break;
            case TriggerOperator.IsBetweenOrEqualTo:
                double propertyValue2b = GetDouble (m_Value2.propertyValue);
                passTest = SafeEquals (currentValue, propertyValue) ||
                    SafeEquals (currentValue, propertyValue2b) ||
                    (currentValue > propertyValue && currentValue < propertyValue2b);
                break;
            }
            return passTest;
        }

        // Allows for floating point rounding errors on some platforms.
        bool SafeEquals(double double1, double double2)
        {
            // Using a hard coded value instead of Epsilon due to a precision error when getting the target value.
            // For instance, the target value property of 2.3 is actually 2.29999995231628
            return Math.Abs(double1 - double2) < 0.0000001d;
        }

        double GetDouble(object value)
        {
            return double.Parse (value.ToString ());
        }
    }
}

