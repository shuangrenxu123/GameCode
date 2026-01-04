using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utilities
{

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ConditionAttribute : PropertyAttribute
    {
        public enum ConditionType
        {
            IsTrue,
            IsFalse,
            IsGreaterThan,
            IsEqualTo,
            IsLessThan,
            IsNotNull,
            IsNull,
        }

        public enum VisibilityType
        {
            Hidden,
            NotEditable
        }

        public string[] conditionPropertyNames;
        public ConditionType[] conditionTypes;
        public float[] values;
        public VisibilityType visibilityType;


        /// <summary>
        /// 此属性将根据其他属性条件确定目标属性的可见性。如果目标属性取决于类中的其他一些属性
        /// 
        /// </summary>
        /// <param name="conditionPropertyName">Name of the property used by the condition.</param>
        /// <param name="conditionType">The condition type.</param>
        /// <param name="visibilityType">The visibility action to perform if the condition is not met.</param>
        /// <param name="conditionValue">The condition argument value.</param>
        public ConditionAttribute(string conditionPropertyName, ConditionType conditionType, VisibilityType visibilityType = VisibilityType.Hidden, float conditionValue = 0f)
        {
            this.conditionPropertyNames = new string[] { conditionPropertyName };
            this.conditionTypes = new ConditionType[] { conditionType };
            this.visibilityType = visibilityType;
            this.values = new float[] { conditionValue };

        }

        public ConditionAttribute(string[] conditionPropertyNames, ConditionType[] conditionTypes, float[] conditionValues, VisibilityType visibilityType = VisibilityType.Hidden)
        {
            this.conditionPropertyNames = conditionPropertyNames;
            this.conditionTypes = conditionTypes;
            this.visibilityType = visibilityType;
            this.values = conditionValues;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConditionAttribute))]
    public class ConditionAttributeEditor : PropertyDrawer
    {
        ConditionAttribute target;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            target ??= attribute as ConditionAttribute;

            bool result = CheckCondition(property);
            if (target.visibilityType == ConditionAttribute.VisibilityType.NotEditable)
            {
                GUI.enabled = result;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true;
            }
            else
            {
                if (result)
                    EditorGUI.PropertyField(position, property, label, true);
            }


        }

        bool result = false;

        bool CheckCondition(SerializedProperty property)
        {
            bool output = true;
            for (int i = 0; i < target.conditionPropertyNames.Length; i++)
            {
                output &= EvaluateCondition(property, target.conditionPropertyNames[i], target.conditionTypes[i], target.values[i]);
            }

            return output;
        }

        bool EvaluateCondition(SerializedProperty property, string conditionPropertyName, ConditionAttribute.ConditionType conditionType, float value)
        {
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditionPropertyName);

            // if the "conditionProperty" is null, then the property is probably part of a plain C# serialized class. If so, then find the property root path
            // and look for the target condition property again.
            if (conditionProperty == null)
            {
                string propertyPath = property.propertyPath;
                int lastIndex = propertyPath.LastIndexOf('.');

                if (lastIndex == -1)
                    return true;


                string propertyParentPath = propertyPath.Substring(0, lastIndex);

                conditionProperty = property.serializedObject.FindProperty(propertyParentPath).FindPropertyRelative(conditionPropertyName);

                if (conditionProperty == null)
                    return true;

            }


            result = false;

            SerializedPropertyType conditionPropertyType = conditionProperty.propertyType;

            if (conditionPropertyType == SerializedPropertyType.Boolean)
            {
                if (conditionType == ConditionAttribute.ConditionType.IsTrue)
                    result = conditionProperty.boolValue;
                else if (conditionType == ConditionAttribute.ConditionType.IsFalse)
                    result = !conditionProperty.boolValue;

            }
            else if (conditionPropertyType == SerializedPropertyType.Float)
            {

                float conditionPropertyFloatValue = conditionProperty.floatValue;
                float argumentFloatValue = value;

                switch (conditionType)
                {
                    case ConditionAttribute.ConditionType.IsTrue:
                        result = conditionPropertyFloatValue != 0f;
                        break;
                    case ConditionAttribute.ConditionType.IsFalse:
                        result = conditionPropertyFloatValue == 0f;
                        break;
                    case ConditionAttribute.ConditionType.IsGreaterThan:
                        result = conditionPropertyFloatValue > argumentFloatValue;
                        break;
                    case ConditionAttribute.ConditionType.IsEqualTo:
                        result = conditionPropertyFloatValue == argumentFloatValue;
                        break;
                    case ConditionAttribute.ConditionType.IsLessThan:
                        result = conditionPropertyFloatValue < argumentFloatValue;
                        break;
                }

            }
            else if (conditionPropertyType == SerializedPropertyType.Integer || conditionPropertyType == SerializedPropertyType.Enum)
            {
                int conditionPropertyIntValue = conditionProperty.intValue;
                int argumentIntValue = (int)value;

                switch (conditionType)
                {
                    case ConditionAttribute.ConditionType.IsTrue:
                        result = conditionPropertyIntValue != 0;
                        break;
                    case ConditionAttribute.ConditionType.IsFalse:
                        result = conditionPropertyIntValue == 0;
                        break;
                    case ConditionAttribute.ConditionType.IsGreaterThan:
                        result = conditionPropertyIntValue > argumentIntValue;
                        break;
                    case ConditionAttribute.ConditionType.IsEqualTo:
                        result = conditionPropertyIntValue == argumentIntValue;
                        break;
                    case ConditionAttribute.ConditionType.IsLessThan:
                        result = conditionPropertyIntValue < argumentIntValue;
                        break;
                }

            }
            else if (conditionPropertyType == SerializedPropertyType.ObjectReference)
            {
                UnityEngine.Object conditionPropertyObjectValue = conditionProperty.objectReferenceValue;

                switch (conditionType)
                {
                    case ConditionAttribute.ConditionType.IsNull:
                        result = conditionPropertyObjectValue == null;
                        break;
                    case ConditionAttribute.ConditionType.IsNotNull:
                        result = conditionPropertyObjectValue != null;
                        break;
                }


            }

            return result;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            target ??= attribute as ConditionAttribute;

            return !result && target.visibilityType == ConditionAttribute.VisibilityType.Hidden ? 0f : EditorGUI.GetPropertyHeight(property);
        }



    }

#endif

}

