using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Helper
{
    public readonly struct CommandSuggestion
    {
        public readonly string InsertText;
        public readonly string DisplayText;

        public CommandSuggestion(string insertText, string displayText)
        {
            InsertText = insertText;
            DisplayText = displayText;
        }
    }

    static class ArgumentBufferPool
    {
        const int MaxCachedArraysPerLength = 32;
        static readonly Dictionary<int, Stack<object[]>> Pools = new();

        public static object[] Rent(int length)
        {
            if (length <= 0)
            {
                return Array.Empty<object>();
            }

            lock (Pools)
            {
                if (Pools.TryGetValue(length, out Stack<object[]> pool) && pool.Count > 0)
                {
                    return pool.Pop();
                }
            }

            return new object[length];
        }

        public static void Return(object[] buffer, int length)
        {
            if (buffer == null || length <= 0)
            {
                return;
            }

            Array.Clear(buffer, 0, length);
            lock (Pools)
            {
                if (!Pools.TryGetValue(length, out Stack<object[]> pool))
                {
                    pool = new Stack<object[]>(MaxCachedArraysPerLength);
                    Pools[length] = pool;
                }

                if (pool.Count < MaxCachedArraysPerLength)
                {
                    pool.Push(buffer);
                }
            }
        }
    }

    public sealed class HostEnvironment
    {
        readonly StringTable strings;
        readonly CommandRegistry commandRegistry;
        readonly VariableRegistry variableRegistry;
        readonly ExternalMemberBinder memberBinder;

        public HostEnvironment(StringTable strings)
        {
            this.strings = strings ?? throw new ArgumentNullException(nameof(strings));
            commandRegistry = new CommandRegistry(strings);
            variableRegistry = new VariableRegistry(strings);
            memberBinder = new ExternalMemberBinder(strings);
            LoadAssemblyBindings();
        }

        public bool HasCommand(InternedString name)
        {
            return commandRegistry.Contains(name);
        }

        public object InvokeCommand(InternedString name, List<object> args)
        {
            return commandRegistry.Invoke(name, args);
        }

        public List<CommandSuggestion> MatchCommands(string keyword)
        {
            return commandRegistry.Match(keyword);
        }

        public void RegisterCommand(string name, string displayText, Func<List<object>, object> handler)
        {
            commandRegistry.Register(name, displayText, handler);
        }

        public void RegisterVariable(string name, object instance, bool readOnly = false)
        {
            variableRegistry.Register(name, instance, readOnly);
        }

        public void RegisterVariable(string name, Func<object> getter, Action<object> setter = null, Type declaredType = null)
        {
            variableRegistry.Register(name, getter, setter, declaredType);
        }

        public object GetExternal(InternedString name)
        {
            return variableRegistry.Get(name);
        }

        public void SetExternal(InternedString name, object value)
        {
            variableRegistry.Set(name, value);
        }

        public object GetMember(object target, InternedString memberName)
        {
            return memberBinder.GetMember(target, memberName);
        }

        public void SetMember(object target, InternedString memberName, object value)
        {
            memberBinder.SetMember(target, memberName, value);
        }

        public object InvokeMember(object target, InternedString methodName, List<object> args)
        {
            return memberBinder.InvokeMember(target, methodName, args);
        }

        void LoadAssemblyBindings()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.IsDynamic)
                    continue;

                Type[] types = GetAssemblyTypes(assembly);
                foreach (Type type in types)
                {
                    if (type == null)
                        continue;

                    commandRegistry.LoadCommands(type);
                    variableRegistry.LoadVariables(type);
                }
            }
        }

        static Type[] GetAssemblyTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types ?? Array.Empty<Type>();
            }
            catch (Exception)
            {
                return Array.Empty<Type>();
            }
        }
    }

    public sealed class CommandRegistry
    {
        readonly StringTable strings;
        readonly Dictionary<InternedString, ICommandCallable> commands = new();
        readonly Dictionary<InternedString, string> displayCache = new();

        public CommandRegistry(StringTable strings)
        {
            this.strings = strings;
        }

        public bool Contains(InternedString name)
        {
            return commands.ContainsKey(name);
        }

        public void LoadCommands(Type type)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo[] methods = type.GetMethods(flags);
            foreach (MethodInfo method in methods)
            {
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute == null)
                    continue;

                InternedString name = strings.Intern(attribute.Name ?? method.Name);
                MethodCallable callable = new(name, method, attribute.Description);
                commands[name] = callable;
                displayCache[name] = callable.DisplayText;
            }
        }

        public void Register(string name, string displayText, Func<List<object>, object> handler)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("命令名称不能为空", nameof(name));
            }

            InternedString commandName = strings.Intern(name);
            DelegateCommandCallable callable = new(displayText, handler);
            commands[commandName] = callable;
            displayCache[commandName] = callable.DisplayText;
        }

        public object Invoke(InternedString name, List<object> args)
        {
            if (!commands.TryGetValue(name, out ICommandCallable callable))
            {
                throw new RuntimeException($"未找到命令 {name.Value}");
            }

            return callable.Execute(args);
        }

        public List<CommandSuggestion> Match(string keyword)
        {
            keyword ??= string.Empty;
            List<CommandSuggestion> matches = new(commands.Count);
            foreach (KeyValuePair<InternedString, ICommandCallable> pair in commands)
            {
                if (keyword.Length == 0 ||
                    pair.Key.Value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string display = displayCache.TryGetValue(pair.Key, out string cached)
                        ? cached
                        : pair.Value.DisplayText;
                    matches.Add(new CommandSuggestion(pair.Key.Value, display));
                }
            }

            return matches;
        }
    }

    public interface ICommandCallable
    {
        string DisplayText { get; }
        object Execute(List<object> args);
    }

    public sealed class DelegateCommandCallable : ICommandCallable
    {
        readonly Func<List<object>, object> handler;

        public DelegateCommandCallable(string displayText, Func<List<object>, object> handler)
        {
            DisplayText = displayText ?? string.Empty;
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public string DisplayText { get; }

        public object Execute(List<object> args)
        {
            return handler(args ?? new List<object>());
        }
    }

    public sealed class MethodCallable : ICommandCallable
    {
        static readonly List<object> EmptyArguments = new(0);
        static readonly Dictionary<Type, string> TypeAliases = new()
        {
            { typeof(void), "void" },
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "string" },
            { typeof(object), "object" }
        };

        readonly MethodInfo method;
        readonly ParameterInfo[] parameters;
        readonly string description;
        readonly int requiredArgumentCount;

        public InternedString Name { get; }
        public string DisplayText { get; }

        public MethodCallable(InternedString name, MethodInfo method, string description, bool buildDisplay = true)
        {
            Name = name;
            this.method = method;
            this.description = description;
            parameters = method.GetParameters();
            requiredArgumentCount = CountRequiredArguments();
            DisplayText = buildDisplay ? BuildDisplayText() : string.Empty;
        }

        public object Execute(List<object> args)
        {
            args ??= EmptyArguments;
            if (args.Count > parameters.Length)
            {
                throw new RuntimeException($"{Name.Value} 参数过多");
            }

            object[] converted = ArgumentBufferPool.Rent(parameters.Length);
            try
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i >= args.Count)
                    {
                        if (parameters[i].HasDefaultValue)
                        {
                            converted[i] = parameters[i].DefaultValue;
                            continue;
                        }

                        throw new RuntimeException($"{Name.Value} 缺少参数 {parameters[i].Name}");
                    }

                    converted[i] = ConvertValue(args[i], parameters[i].ParameterType, parameters[i].Name);
                }

                return method.Invoke(null, converted);
            }
            finally
            {
                ArgumentBufferPool.Return(converted, parameters.Length);
            }
        }

        public bool TryInvoke(object target, List<object> args, out object result)
        {
            args ??= EmptyArguments;
            result = null;
            if (args.Count < requiredArgumentCount || args.Count > parameters.Length)
            {
                return false;
            }

            object[] converted = ArgumentBufferPool.Rent(parameters.Length);
            try
            {
                if (!TryPrepareArguments(args, converted))
                {
                    return false;
                }

                result = method.Invoke(target, converted);
                return true;
            }
            finally
            {
                ArgumentBufferPool.Return(converted, parameters.Length);
            }
        }

        int CountRequiredArguments()
        {
            int count = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].HasDefaultValue)
                {
                    count++;
                }
            }

            return count;
        }

        bool TryPrepareArguments(List<object> args, object[] converted)
        {
            int index = 0;
            for (; index < args.Count; index++)
            {
                try
                {
                    converted[index] = ConvertValue(args[index], parameters[index].ParameterType, parameters[index].Name);
                }
                catch (RuntimeException)
                {
                    return false;
                }
            }

            for (; index < parameters.Length; index++)
            {
                if (!parameters[index].HasDefaultValue)
                {
                    return false;
                }

                converted[index] = parameters[index].DefaultValue;
            }

            return true;
        }

        string BuildDisplayText()
        {
            StringBuilder builder = new();
            builder.Append(Name.Value);
            builder.Append('(');
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                builder.Append(GetDisplayTypeName(parameter.ParameterType));
                builder.Append(' ');
                builder.Append(parameter.Name);
                if (parameter.HasDefaultValue)
                {
                    builder.Append(" = ");
                    builder.Append(FormatDefaultValue(parameter.DefaultValue));
                }

                if (i < parameters.Length - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(')');
            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.Append(" - ");
                builder.Append(description);
            }

            return builder.ToString();
        }

        static string GetDisplayTypeName(Type type)
        {
            if (TypeAliases.TryGetValue(type, out string alias))
            {
                return alias;
            }

            if (type.IsArray)
            {
                return GetDisplayTypeName(type.GetElementType()) + "[]";
            }

            if (type.IsGenericType)
            {
                string name = type.Name;
                int tickIndex = name.IndexOf('`');
                if (tickIndex >= 0)
                {
                    name = name[..tickIndex];
                }

                Type[] args = type.GetGenericArguments();
                StringBuilder builder = new();
                builder.Append(name);
                builder.Append('<');
                for (int i = 0; i < args.Length; i++)
                {
                    builder.Append(GetDisplayTypeName(args[i]));
                    if (i < args.Length - 1)
                    {
                        builder.Append(", ");
                    }
                }

                builder.Append('>');
                return builder.ToString();
            }

            return type.Name;
        }

        static string FormatDefaultValue(object value)
        {
            if (value == null)
                return "null";
            if (value is string str)
                return $"\"{str}\"";
            if (value is bool boolValue)
                return boolValue ? "true" : "false";
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static object ConvertValue(object value, Type targetType, string displayName)
        {
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (targetType == typeof(object))
            {
                return value;
            }

            if (value == null)
            {
                if (targetType.IsValueType)
                {
                    throw new RuntimeException($"{displayName} 不允许为 nil");
                }

                return null;
            }

            if (targetType.IsInstanceOfType(value))
            {
                return value;
            }

            if (targetType == typeof(string))
            {
                return value.ToString();
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value.ToString(), true);
            }

            try
            {
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new RuntimeException($"{displayName} 期望 {targetType.Name} 类型");
            }
        }
    }

    public sealed class VariableRegistry
    {
        readonly StringTable strings;
        readonly Dictionary<InternedString, VariableBinding> variables = new();

        public VariableRegistry(StringTable strings)
        {
            this.strings = strings;
        }

        public void LoadVariables(Type type)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (FieldInfo field in type.GetFields(flags))
            {
                VariableAttribute attribute = field.GetCustomAttribute<VariableAttribute>();
                if (attribute == null || !field.IsStatic)
                    continue;

                Func<object> getter = () => field.GetValue(null);
                Action<object> setter = null;
                if (!field.IsInitOnly && !field.IsLiteral)
                {
                    setter = value => field.SetValue(null, MethodCallable.ConvertValue(value, field.FieldType, field.Name));
                }

                RegisterInternal(attribute.Name ?? field.Name, field.FieldType, getter, setter);
            }

            foreach (PropertyInfo property in type.GetProperties(flags))
            {
                VariableAttribute attribute = property.GetCustomAttribute<VariableAttribute>();
                if (attribute == null)
                    continue;

                MethodInfo getterInfo = property.GetGetMethod(true);
                if (getterInfo == null || !getterInfo.IsStatic)
                    continue;

                Func<object> getter = () => getterInfo.Invoke(null, null);
                Action<object> setter = null;
                MethodInfo setterInfo = property.GetSetMethod(true);
                if (setterInfo != null && setterInfo.IsStatic)
                {
                    setter = value =>
                    {
                        object[] converted = ArgumentBufferPool.Rent(1);
                        try
                        {
                            converted[0] = MethodCallable.ConvertValue(value, property.PropertyType, property.Name);
                            setterInfo.Invoke(null, converted);
                        }
                        finally
                        {
                            ArgumentBufferPool.Return(converted, 1);
                        }
                    };
                }

                RegisterInternal(attribute.Name ?? property.Name, property.PropertyType, getter, setter);
            }
        }

        public void Register(string name, object instance, bool readOnly)
        {
            object captured = instance;
            Func<object> getter = () => captured;
            Action<object> setter = null;
            if (!readOnly)
            {
                setter = value => captured = value;
            }

            RegisterInternal(name, captured?.GetType() ?? typeof(object), getter, setter);
        }

        public void Register(string name, Func<object> getter, Action<object> setter, Type declaredType)
        {
            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter));
            }

            Type type = declaredType;
            if (type == null)
            {
                object sample = getter();
                type = sample?.GetType() ?? typeof(object);
            }

            RegisterInternal(name, type, getter, setter);
        }

        public object Get(InternedString name)
        {
            if (!variables.TryGetValue(name, out VariableBinding binding))
            {
                throw new RuntimeException($"未找到外部变量 {name.Value}");
            }

            return binding.Getter();
        }

        public void Set(InternedString name, object value)
        {
            if (!variables.TryGetValue(name, out VariableBinding binding))
            {
                throw new RuntimeException($"未找到外部变量 {name.Value}");
            }

            if (binding.Setter == null)
            {
                throw new RuntimeException($"外部变量 {name.Value} 为只读");
            }

            binding.Setter(MethodCallable.ConvertValue(value, binding.ValueType, binding.Name.Value));
        }

        void RegisterInternal(string name, Type valueType, Func<object> getter, Action<object> setter)
        {
            if (string.IsNullOrEmpty(name) || getter == null)
                return;

            InternedString interned = strings.Intern(name);
            variables[interned] = new VariableBinding(interned, valueType, getter, setter);
        }

        sealed class VariableBinding
        {
            public InternedString Name { get; }
            public Type ValueType { get; }
            public Func<object> Getter { get; }
            public Action<object> Setter { get; }

            public VariableBinding(InternedString name, Type valueType, Func<object> getter, Action<object> setter)
            {
                Name = name;
                ValueType = valueType ?? typeof(object);
                Getter = getter;
                Setter = setter;
            }
        }
    }

    public sealed class ExternalMemberBinder
    {
        static readonly List<object> EmptyArguments = new(0);
        readonly Dictionary<Type, Dictionary<InternedString, MemberAccessor>> memberAccessorCache = new();
        readonly Dictionary<Type, Dictionary<InternedString, MethodCallable[]>> methodCache = new();
        readonly Dictionary<MemberMethodCacheKey, MethodCallable> matchedMethodCache = new();

        public ExternalMemberBinder(StringTable strings)
        {
        }

        public object GetMember(object target, InternedString memberName)
        {
            if (target == null)
            {
                throw new RuntimeException($"访问 {memberName.Value} 时对象为空");
            }

            MemberAccessor accessor = GetMemberAccessor(target.GetType(), memberName);
            if (accessor == null || !accessor.CanRead)
            {
                throw new RuntimeException($"属性或字段 {memberName.Value} 不存在或不可读");
            }

            return accessor.GetValue(target);
        }

        public void SetMember(object target, InternedString memberName, object value)
        {
            if (target == null)
            {
                throw new RuntimeException($"设置 {memberName.Value} 时对象为空");
            }

            MemberAccessor accessor = GetMemberAccessor(target.GetType(), memberName);
            if (accessor == null || !accessor.CanWrite)
            {
                throw new RuntimeException($"属性或字段 {memberName.Value} 不存在或不可写");
            }

            object converted = MethodCallable.ConvertValue(value, accessor.MemberType, memberName.Value);
            accessor.SetValue(target, converted);
        }

        public object InvokeMember(object target, InternedString methodName, List<object> args)
        {
            if (target == null)
            {
                throw new RuntimeException($"调用 {methodName.Value} 时对象为空");
            }

            Type targetType = target.GetType();
            args ??= EmptyArguments;
            MemberMethodCacheKey key = new(targetType, methodName, args);
            if (matchedMethodCache.TryGetValue(key, out MethodCallable cached))
            {
                if (cached.TryInvoke(target, args, out object cachedResult))
                {
                    return cachedResult;
                }

                matchedMethodCache.Remove(key);
            }

            MethodCallable[] methods = GetMethodCandidates(targetType, methodName);
            foreach (MethodCallable method in methods)
            {
                if (method.TryInvoke(target, args, out object result))
                {
                    matchedMethodCache[key] = method;
                    return result;
                }
            }

            throw new RuntimeException($"未找到匹配的方法 {methodName.Value}");
        }

        MemberAccessor GetMemberAccessor(Type type, InternedString memberName)
        {
            if (!memberAccessorCache.TryGetValue(type, out Dictionary<InternedString, MemberAccessor> cache))
            {
                cache = new Dictionary<InternedString, MemberAccessor>();
                memberAccessorCache[type] = cache;
            }

            if (cache.TryGetValue(memberName, out MemberAccessor accessor))
            {
                return accessor;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo property = type.GetProperty(memberName.Value, flags);
            if (property != null)
            {
                accessor = new MemberAccessor(property, null);
                cache[memberName] = accessor;
                return accessor;
            }

            FieldInfo field = type.GetField(memberName.Value, flags);
            if (field != null)
            {
                accessor = new MemberAccessor(null, field);
                cache[memberName] = accessor;
                return accessor;
            }

            cache[memberName] = null;
            return null;
        }

        MethodCallable[] GetMethodCandidates(Type type, InternedString methodName)
        {
            if (!methodCache.TryGetValue(type, out Dictionary<InternedString, MethodCallable[]> cache))
            {
                cache = new Dictionary<InternedString, MethodCallable[]>();
                methodCache[type] = cache;
            }

            if (cache.TryGetValue(methodName, out MethodCallable[] methods))
            {
                return methods;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo[] candidates = Array.FindAll(type.GetMethods(flags), method => string.Equals(method.Name, methodName.Value, StringComparison.Ordinal));
            methods = new MethodCallable[candidates.Length];
            for (int i = 0; i < candidates.Length; i++)
            {
                methods[i] = new MethodCallable(methodName, candidates[i], null, false);
            }

            cache[methodName] = methods;
            return methods;
        }

        readonly struct MemberMethodCacheKey : IEquatable<MemberMethodCacheKey>
        {
            readonly Type targetType;
            readonly InternedString methodName;
            readonly ArgumentSignature signature;
            readonly int hash;

            public MemberMethodCacheKey(Type targetType, InternedString methodName, List<object> args)
            {
                this.targetType = targetType;
                this.methodName = methodName;
                signature = new ArgumentSignature(args);

                HashCode hashCode = new();
                hashCode.Add(targetType);
                hashCode.Add(methodName);
                hashCode.Add(signature);
                hash = hashCode.ToHashCode();
            }

            public bool Equals(MemberMethodCacheKey other)
            {
                return targetType == other.targetType
                    && Equals(methodName, other.methodName)
                    && signature.Equals(other.signature);
            }

            public override bool Equals(object obj)
            {
                return obj is MemberMethodCacheKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return hash;
            }
        }

        readonly struct ArgumentSignature : IEquatable<ArgumentSignature>
        {
            readonly int count;
            readonly Type first;
            readonly Type second;
            readonly Type third;
            readonly Type fourth;
            readonly Type[] rest;
            readonly int hash;

            public ArgumentSignature(List<object> args)
            {
                count = args?.Count ?? 0;
                first = GetArgumentType(args, 0);
                second = GetArgumentType(args, 1);
                third = GetArgumentType(args, 2);
                fourth = GetArgumentType(args, 3);
                rest = null;

                HashCode hashCode = new();
                hashCode.Add(count);
                hashCode.Add(first);
                hashCode.Add(second);
                hashCode.Add(third);
                hashCode.Add(fourth);

                if (count > 4)
                {
                    rest = new Type[count - 4];
                    for (int i = 4; i < count; i++)
                    {
                        Type type = GetArgumentType(args, i);
                        rest[i - 4] = type;
                        hashCode.Add(type);
                    }
                }

                hash = hashCode.ToHashCode();
            }

            public bool Equals(ArgumentSignature other)
            {
                if (count != other.count
                    || first != other.first
                    || second != other.second
                    || third != other.third
                    || fourth != other.fourth)
                {
                    return false;
                }

                if (count <= 4)
                {
                    return true;
                }

                for (int i = 0; i < rest.Length; i++)
                {
                    if (rest[i] != other.rest[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public override bool Equals(object obj)
            {
                return obj is ArgumentSignature other && Equals(other);
            }

            public override int GetHashCode()
            {
                return hash;
            }

            static Type GetArgumentType(List<object> args, int index)
            {
                if (args == null || index >= args.Count)
                {
                    return null;
                }

                return args[index]?.GetType();
            }
        }

        sealed class MemberAccessor
        {
            readonly PropertyInfo property;
            readonly FieldInfo field;
            readonly MethodInfo getter;
            readonly MethodInfo setter;

            public Type MemberType { get; }
            public bool CanRead { get; }
            public bool CanWrite { get; }

            public MemberAccessor(PropertyInfo property, FieldInfo field)
            {
                this.property = property;
                this.field = field;

                if (property != null)
                {
                    getter = property.GetGetMethod(true);
                    setter = property.GetSetMethod(true);
                    MemberType = property.PropertyType;
                    CanRead = getter != null;
                    CanWrite = setter != null;
                }
                else
                {
                    MemberType = field.FieldType;
                    CanRead = true;
                    CanWrite = !field.IsInitOnly;
                }
            }

            public object GetValue(object target)
            {
                if (property != null)
                {
                    return getter.Invoke(target, null);
                }

                return field.GetValue(target);
            }

            public void SetValue(object target, object value)
            {
                if (property != null)
                {
                    object[] args = ArgumentBufferPool.Rent(1);
                    try
                    {
                        args[0] = value;
                        setter.Invoke(target, args);
                    }
                    finally
                    {
                        ArgumentBufferPool.Return(args, 1);
                    }
                    return;
                }

                field.SetValue(target, value);
            }
        }
    }
}
