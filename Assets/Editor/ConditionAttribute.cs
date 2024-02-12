using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
[AttributeUsage(AttributeTargets.Field)]
public class ConditionAttribute : PropertyAttribute
{
    public enum ConditionType
    {
        isTrue,
        isFalse,
        isNull,
        isNotNull,
        isEqualTo,
        isLessThan,
        isGreaterThan,
    }

    public enum VisibilityType
    {
        NoEditor,
        Hide,
    }

    public string[] conditionPropertyNames;
    public ConditionType[] conditionTypes;
    public float[] values;
    public VisibilityType visibility;

    public ConditionAttribute(string conditionProtyNames,ConditionType conditionType,VisibilityType visibilityType = VisibilityType.Hide, float conditionValue = 0)
    {
        this.values = new float[] { conditionValue };
        this.conditionPropertyNames = new string[] { conditionProtyNames };
        this.visibility = visibilityType;
        this.conditionTypes = new ConditionType[] { conditionType};
    }
    public ConditionAttribute(string[] conditionProtyNames, ConditionType[] conditionType, float[] conditionValue ,VisibilityType visibilityType= VisibilityType.Hide)
    {
        this.values = conditionValue;
        this.conditionPropertyNames = conditionProtyNames;
        this.visibility = visibilityType;
        this.conditionTypes = conditionType;
    }
}


//#if UNINT_EDITOR

[CustomPropertyDrawer(typeof(ConditionAttribute))]
public class ConditionAttributeEditor : PropertyDrawer
{
    ConditionAttribute target;
    bool result = false;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        result = CheckCondition(property);
        if(target.visibility == ConditionAttribute.VisibilityType.Hide)
        {
            if (result)
                EditorGUI.PropertyField(position, property, label, true);
        }
        else if(target.visibility == ConditionAttribute.VisibilityType.NoEditor)
        {

            GUI.enabled = result;
            EditorGUI.PropertyField (position, property, label, true);
            GUI.enabled = false;
        }
    }
    bool CheckCondition(SerializedProperty property)
    {
        bool output = true;

        for(int i =0; i < target.conditionPropertyNames.Length; i++)
        {
            output &= EvaluateCondition(property, target.conditionPropertyNames[i], target.conditionTypes[i], target.values[i]);
        }
        return output;
    }
    bool EvaluateCondition(SerializedProperty property,string conditionProperName, ConditionAttribute.ConditionType conditionType,float value)
    {
        var output = true;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditionProperName);

        if(conditionProperty == null)
        {

        }
        //result = true;
        SerializedPropertyType conditionPropertyType = property.propertyType;
        if(conditionPropertyType == SerializedPropertyType.Boolean)
        {
            if (conditionType == ConditionAttribute.ConditionType.isTrue)
                output = conditionProperty.boolValue;
            else if(conditionType == ConditionAttribute.ConditionType.isFalse)
                output = !conditionProperty.boolValue;
        }
        else if( conditionPropertyType == SerializedPropertyType.Float)
        {
            var v = conditionProperty.floatValue;
            if(conditionType == ConditionAttribute.ConditionType.isLessThan)
                output = v < value;
            else if(conditionType == ConditionAttribute.ConditionType.isGreaterThan)
                output = v > value;
            else if(conditionType == ConditionAttribute.ConditionType.isEqualTo)
                output = v == value;
        }
        return output;
    }

}

