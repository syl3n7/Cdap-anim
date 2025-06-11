using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute
{
    public string ConditionalSourceField { get; private set; }
    public bool HideInInspector { get; private set; }
    public object[] CompareValues { get; private set; }

    public ConditionalHideAttribute(string conditionalSourceField, params object[] compareValues)
    {
        ConditionalSourceField = conditionalSourceField;
        CompareValues = compareValues;
        HideInInspector = false;
    }

    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector, params object[] compareValues)
    {
        ConditionalSourceField = conditionalSourceField;
        HideInInspector = hideInInspector;
        CompareValues = compareValues;
    }
}
