using System;
using System.Collections.Generic;
using System.Reflection;
using AIBlackboard;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BT.RuntimeSerialization
{
    public sealed partial class BTTreeRuntimeRunner
    {
        [Serializable]
        public sealed class InspectorBlackboardEntry
        {
            [LabelText("Key")]
            public string key;

            [LabelText("类型")]
            public InspectorBlackboardValueType type = InspectorBlackboardValueType.String;

            [LabelText("值")]
            [SerializeReference, InlineProperty, HideReferenceObjectPicker, HideLabel]
            public InspectorBlackboardValue value;

            public void EnsureValueType()
            {
                if (value != null && value.Type == type)
                    return;

                value = type switch
                {
                    InspectorBlackboardValueType.Int => new InspectorBlackboardIntValue(),
                    InspectorBlackboardValueType.Float => new InspectorBlackboardFloatValue(),
                    InspectorBlackboardValueType.Bool => new InspectorBlackboardBoolValue(),
                    InspectorBlackboardValueType.Object => new InspectorBlackboardObjectValue(),
                    _ => new InspectorBlackboardStringValue(),
                };
            }
        }

        [Serializable]
        public abstract class InspectorBlackboardValue
        {
            public abstract InspectorBlackboardValueType Type { get; }
            public abstract void Apply(Blackboard target, string key);
        }

        [Serializable]
        public sealed class InspectorBlackboardStringValue : InspectorBlackboardValue
        {
            [LabelText("String")]
            public string value;

            public override InspectorBlackboardValueType Type => InspectorBlackboardValueType.String;

            public override void Apply(Blackboard target, string key)
            {
                target.SetValue<string, string>(key, value ?? string.Empty);
            }
        }

        [Serializable]
        public sealed class InspectorBlackboardIntValue : InspectorBlackboardValue
        {
            [LabelText("Int")]
            public int value;

            public override InspectorBlackboardValueType Type => InspectorBlackboardValueType.Int;

            public override void Apply(Blackboard target, string key)
            {
                target.SetValue<string, int>(key, value);
            }
        }

        [Serializable]
        public sealed class InspectorBlackboardFloatValue : InspectorBlackboardValue
        {
            [LabelText("Float")]
            public float value;

            public override InspectorBlackboardValueType Type => InspectorBlackboardValueType.Float;

            public override void Apply(Blackboard target, string key)
            {
                target.SetValue<string, float>(key, value);
            }
        }

        [Serializable]
        public sealed class InspectorBlackboardBoolValue : InspectorBlackboardValue
        {
            [LabelText("Bool")]
            public bool value;

            public override InspectorBlackboardValueType Type => InspectorBlackboardValueType.Bool;

            public override void Apply(Blackboard target, string key)
            {
                target.SetValue<string, bool>(key, value);
            }
        }

        [Serializable]
        public sealed class InspectorBlackboardObjectValue : InspectorBlackboardValue
        {
            [LabelText("Object")]
            public UnityEngine.Object value;

            public override InspectorBlackboardValueType Type => InspectorBlackboardValueType.Object;

            public override void Apply(Blackboard target, string key)
            {
                SetObjectValue(target, key, value);
            }
        }

        public enum InspectorBlackboardValueType
        {
            String = 0,
            Int = 1,
            Float = 2,
            Bool = 3,
            Object = 4,
        }

        [Header("Blackboard Override")]
        [LabelText("Inspector 黑板条目（覆盖运行时 JSON）")]
        [SerializeField] List<InspectorBlackboardEntry> inspectorBlackboard = new();

        static readonly MethodInfo SetValueGenericWithRawKeyMethod = ResolveSetValueGenericWithRawKey();

        void EnsureInspectorBlackboardValueTypes()
        {
            if (inspectorBlackboard == null)
                return;

            for (var i = 0; i < inspectorBlackboard.Count; i++)
            {
                var entry = inspectorBlackboard[i];
                entry?.EnsureValueType();
            }
        }

        void ApplyInspectorBlackboard(Blackboard target)
        {
            if (target == null || inspectorBlackboard == null || inspectorBlackboard.Count == 0)
                return;

            for (var i = 0; i < inspectorBlackboard.Count; i++)
            {
                var entry = inspectorBlackboard[i];
                if (entry == null || string.IsNullOrEmpty(entry.key))
                    continue;

                entry.value?.Apply(target, entry.key);
            }
        }

        static void SetObjectValue(Blackboard target, string key, UnityEngine.Object obj)
        {
            if (SetValueGenericWithRawKeyMethod == null)
                return;

            var valueType = obj != null ? obj.GetType() : typeof(UnityEngine.Object);
            var method = SetValueGenericWithRawKeyMethod.MakeGenericMethod(typeof(string), valueType);
            method.Invoke(target, new object[] { key, obj });
        }

        static MethodInfo ResolveSetValueGenericWithRawKey()
        {
            var methods = typeof(Blackboard).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name != "SetValue")
                    continue;
                if (!method.IsGenericMethodDefinition)
                    continue;
                if (method.GetGenericArguments().Length != 2)
                    continue;
                if (method.GetParameters().Length != 2)
                    continue;
                return method;
            }

            return null;
        }
    }
}
