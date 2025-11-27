using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using ConsoleLog;
using UnityEngine;

namespace Helper
{
    public class Environment
    {

        /// <summary>
        /// 外部环境
        /// </summary>
        Environment enclosing;

        /// <summary>
        /// 创建根级执行环境并自动加载全部命令与变量。
        /// </summary>
        public Environment()
        {
            enclosing = null;
            LoadMethod();
        }
        /// <summary>
        /// 使用父环境创建一个新的子环境。
        /// </summary>
        /// <param name="environment">上层作用域。</param>
        public Environment(Environment environment)
        {
            enclosing = environment;
        }
        private static readonly Dictionary<Type, string> TypeAliases = new()
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

        // 变量
        private readonly Dictionary<int, object> variables = new();
        //定义的方法
        private readonly Dictionary<string, MethodCallable> callable = new(StringComparer.Ordinal);

        // 缓存的命令展示文本
        private readonly Dictionary<string, string> commandDisplayCache = new(StringComparer.Ordinal);

        // 注册C#变量
        private readonly Dictionary<int, ConsoleVariableBinding> externalVariables = new();
        // 
        private readonly Dictionary<Type, Dictionary<string, MemberAccessor>> memberAccessorCache = new();
        private readonly Dictionary<Type, Dictionary<string, MethodInfo[]>> methodCache = new();
        /// <summary>
        /// 在当前环境链中查找变量值。
        /// </summary>
        /// <param name="token">变量标识符。</param>
        /// <returns>匹配到的变量值。</returns>
        public object GetVariables(Token token)
        {
            var hashCode = token.sourceString.GetHashCode();
            //现在自己的环境中寻找
            if (variables.TryGetValue(hashCode, out object value))
            {
                return value;
            }
            // 如果有外层环境,那就尝试去外层环境找找
            if (enclosing != null)
                return enclosing.GetVariables(token);
            throw new RuntimeException(token, $"变量未定义{token.sourceString}");
        }
        /// <summary>
        /// 根据名称获取已注册的命令方法。
        /// </summary>
        /// <param name="methodName">命令名。</param>
        /// <returns>对应的可调用包装。</returns>
        public MethodCallable GetMethod(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new Exception("方法名不能为空");
            }

            if (callable.TryGetValue(methodName, out var value))
            {
                return value;
            }
            throw new Exception($"未找到该方法{methodName}");
        }

        /// <summary>
        /// 根据关键字匹配已注册的命令，并返回带签名与描述的候选文本。
        /// </summary>
        /// <param name="keyword">用于过滤命令名的关键字，允许为空。</param>
        /// <returns>匹配到的命令展示文本列表。</returns>
        public List<string> MatchCommands(string keyword)
        {
            keyword ??= string.Empty;
            string loweredKeyword = keyword.ToLowerInvariant();
            List<string> matches = new(commandDisplayCache.Count);

            foreach (var pair in callable)
            {
                if (string.IsNullOrEmpty(loweredKeyword) ||
                    pair.Key.IndexOf(loweredKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (commandDisplayCache.TryGetValue(pair.Key, out var displayText))
                    {
                        matches.Add(displayText);
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// 在当前环境中直接定义变量。
        /// </summary>
        /// <param name="varName">变量名。</param>
        /// <param name="variable">变量值。</param>
        public void Define(string varName, object variable)
        {
            int hashCode = varName.GetHashCode();
            variables[hashCode] = variable;
        }

        /// <summary>
        /// 为环境链中的变量重新赋值。
        /// </summary>
        /// <param name="name">变量名标记。</param>
        /// <param name="variable">新值。</param>
        public void Assign(Token name, object variable)
        {
            int hashCode = name.sourceString.GetHashCode();
            //如果是本环境的值,那就更新一下
            if (variables.ContainsKey(hashCode))
            {
                variables[hashCode] = variable;
                return;
            }
            //尝试更新外层环境
            if (enclosing != null)
            {
                enclosing.Assign(name, variable);
                return;
            }
            throw new RuntimeException(name, $"变量未定义{name.sourceString}");
        }

        /// <summary>
        /// 扫描执行程序集并加载命令与外部变量。
        /// </summary>
        void LoadMethod()
        {
            Type[] totalType = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in totalType)
            {
                LoadCommands(type);
                LoadVariables(type);
            }
            //return commands;
        }
        /// <summary>
        /// 为指定类型注册所有带 CommandAttribute 的静态方法。
        /// </summary>
        /// <param name="type">要扫描的类型。</param>
        void LoadCommands(Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methods.Length == 0)
            {
                return;
            }
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute == null)
                    continue;
                var command = LoadMethod(method, attribute);
                if (command != null)
                {
                    RegisterCommand(command);
                }
            }
        }

        /// <summary>
        /// 为指定类型注册所有标记 ConsoleVariableAttribute 的静态字段或属性。
        /// </summary>
        /// <param name="type">要扫描的类型。</param>
        void LoadVariables(Type type)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var field in type.GetFields(flags))
            {
                var attribute = field.GetCustomAttribute<ConsoleVariableAttribute>();
                if (attribute == null)
                    continue;

                if (!field.IsStatic)
                    continue;

                Func<object> getter = () => field.GetValue(null);
                Action<object> setter = null;
                if (!(field.IsInitOnly || field.IsLiteral))
                {
                    setter = value => field.SetValue(null, value);
                }

                RegisterExternalVariableInternal(attribute.Name ?? field.Name, field.FieldType, getter, setter);
            }

            foreach (var property in type.GetProperties(flags))
            {
                var attribute = property.GetCustomAttribute<ConsoleVariableAttribute>();
                if (attribute == null)
                    continue;

                var getterInfo = property.GetGetMethod(true);
                if (getterInfo == null || !getterInfo.IsStatic)
                    continue;

                Func<object> getter = () => getterInfo.Invoke(null, null);
                Action<object> setter = null;
                var setterInfo = property.GetSetMethod(true);
                if (setterInfo != null && setterInfo.IsStatic)
                {
                    setter = value => setterInfo.Invoke(null, new object[] { value });
                }

                RegisterExternalVariableInternal(attribute.Name ?? property.Name, property.PropertyType, getter, setter);
            }
        }

        /// <summary>
        /// 将任意对象实例注册为控制台变量，可选择是否只读。
        /// </summary>
        /// <param name="name">变量名。</param>
        /// <param name="instance">变量引用的对象实例。</param>
        /// <param name="readOnly">是否只读，默认可写。</param>
        public void RegisterVariable(string name, object instance, bool readOnly = false)
        {
            object captured = instance;
            Func<object> getter = () => captured;
            Action<object> setter = null;
            if (!readOnly)
            {
                setter = value => captured = value;
            }

            var declaredType = captured?.GetType() ?? typeof(object);
            RegisterExternalVariableInternal(name, declaredType, getter, setter);
        }

        /// <summary>
        /// 通过自定义访问器注册控制台变量，支持显式声明类型。
        /// </summary>
        /// <param name="name">变量名。</param>
        /// <param name="getter">变量读取委托。</param>
        /// <param name="setter">变量写入委托，可为空表示只读。</param>
        /// <param name="declaredType">声明的变量类型，可为空自动推断。</param>
        public void RegisterVariable(string name, Func<object> getter, Action<object> setter = null, Type declaredType = null)
        {
            if (getter == null)
                throw new System.ArgumentNullException(nameof(getter));
            var type = declaredType;
            if (type == null)
            {
                var sample = getter();
                type = sample?.GetType() ?? typeof(object);
            }
            RegisterExternalVariableInternal(name, type, getter, setter);
        }

        /// <summary>
        /// 将方法包装注册到命令表并缓存展示文本。
        /// </summary>
        /// <param name="command">命令封装。</param>
        void RegisterCommand(MethodCallable command)
        {
            callable[command.name] = command;
            string signature = BuildSignature(command);
            commandDisplayCache[command.name] = BuildDisplayText(signature, command.description);
        }

        /// <summary>
        /// 内部注册外部变量绑定。
        /// </summary>
        /// <param name="name">变量名。</param>
        /// <param name="type">变量类型。</param>
        /// <param name="getter">读取委托。</param>
        /// <param name="setter">写入委托。</param>
        void RegisterExternalVariableInternal(string name, Type type, Func<object> getter, Action<object> setter)
        {
            if (string.IsNullOrEmpty(name) || getter == null)
                return;

            int hashCode = name.GetHashCode();
            externalVariables[hashCode] = new ConsoleVariableBinding(name, type, getter, setter);
        }

        /// <summary>
        /// 读取外部变量或其成员链的值。
        /// </summary>
        /// <param name="expression">外部变量表达式。</param>
        /// <returns>解析得到的值。</returns>
        public object GetExternalVariable(ExternalVariableExpression expression)
        {
            //拿到最上面注册进来的根节点变量
            var binding = ResolveExternalVariable(expression.root);

            object value = binding.Getter();
            //如果他没有成员链,那就直接返回这个值
            if (expression.accessChain == null || expression.accessChain.Count == 0)
            {
                return value;
            }
            //否则就遍历成员链返回最终值
            return TraverseMemberRead(value, expression.accessChain);
        }

        /// <summary>
        /// 为外部变量或其成员写入数值。
        /// </summary>
        /// <param name="expression">外部变量表达式。</param>
        /// <param name="value">要写入的值。</param>
        public void AssignExternal(ExternalVariableExpression expression, object value)
        {
            var binding = ResolveExternalVariable(expression.root);
            if (expression.accessChain == null || expression.accessChain.Count == 0)
            {
                if (binding.Setter == null)
                {
                    throw new RuntimeException(expression.root, $"变量{binding.Name}为只读");
                }
                object convertedValue = ConvertExternalValue(binding.ValueType, binding.Name, value, expression.root);
                binding.Setter(convertedValue);
                return;
            }

            object owner = binding.Getter();
            if (owner == null)
            {
                throw new RuntimeException(expression.root, $"变量{binding.Name}为null，无法访问{expression.accessChain[0].sourceString}");
            }

            //拿到倒数第二个值，并给他赋值
            object parent = TraverseToParent(owner, expression.accessChain);
            Token targetToken = expression.accessChain[^1];
            SetMemberValue(parent, targetToken, value);
        }

        /// <summary>
        /// 调用外部变量成员方法。
        /// </summary>
        /// <param name="expression">外部变量表达式。</param>
        /// <param name="args">调用参数。</param>
        /// <returns>方法返回值。</returns>
        public object InvokeExternalMethod(ExternalVariableExpression expression, List<object> args)
        {
            if (expression.accessChain == null || expression.accessChain.Count == 0)
            {
                throw new RuntimeException(expression.root, "调用外部变量方法需要指定方法名");
            }

            var binding = ResolveExternalVariable(expression.root);
            object owner = binding.Getter();
            if (owner == null)
            {
                throw new RuntimeException(expression.root, $"变量{binding.Name}为null，无法访问{expression.accessChain[0].sourceString}");
            }

            object target = owner;
            if (expression.accessChain.Count > 1)
            {
                target = TraverseMemberRead(owner, expression.accessChain, expression.accessChain.Count - 1);
            }

            Token methodToken = expression.accessChain[^1];
            return InvokeInstanceMethod(target, methodToken, args);
        }

        /// <summary>
        /// 解析外部变量绑定，若当前环境未命中则向外层查找。
        /// </summary>
        /// <param name="token">变量名标记。</param>
        /// <returns>匹配到的绑定信息。</returns>
        ConsoleVariableBinding ResolveExternalVariable(Token token)
        {
            int hashCode = token.sourceString.GetHashCode();
            if (externalVariables.TryGetValue(hashCode, out var binding))
            {
                return binding;
            }
            if (enclosing != null)
            {
                return enclosing.ResolveExternalVariable(token);
            }

            throw new RuntimeException(token, $"未找到变量{token.sourceString}");
        }

        /// <summary>
        /// 将传入的对象转换为外部变量期望的类型。
        /// </summary>
        /// <param name="targetType">目标类型。</param>
        /// <param name="displayName">变量显示名称。</param>
        /// <param name="value">原始值。</param>
        /// <param name="token">来源标记。</param>
        /// <returns>转换后的值。</returns>
        object ConvertExternalValue(Type targetType, string displayName, object value, Token token)
        {
            Type finalType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (value == null)
            {
                if (finalType.IsValueType)
                {
                    throw new RuntimeException(token, $"变量{displayName}不允许为null");
                }
                return null;
            }

            if (finalType.IsInstanceOfType(value))
            {
                return value;
            }

            try
            {
                return System.Convert.ChangeType(value, finalType, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new RuntimeException(token, $"变量{displayName}期望{finalType.Name}类型");
            }
        }

        /// <summary>
        /// 顺序读取成员链的值。
        /// </summary>
        /// <param name="instance">起始对象。</param>
        /// <param name="chain">成员访问链。</param>
        /// <returns>最终成员值。</returns>
        object TraverseMemberRead(object instance, List<Token> chain)
        {
            return TraverseMemberRead(instance, chain, chain.Count);
        }

        /// <summary>
        /// 读取指定长度的成员访问链。
        /// </summary>
        /// <param name="instance">起始对象。</param>
        /// <param name="chain">成员链。</param>
        /// <param name="length">访问深度。</param>
        /// <returns>访问结果。</returns>
        object TraverseMemberRead(object instance, List<Token> chain, int length)
        {
            object current = instance;
            for (int i = 0; i < length; i++)
            {
                current = GetMemberValue(current, chain[i]);
            }
            return current;
        }

        /// <summary>
        /// 将成员链遍历到倒数第二级，得到父对象。
        /// </summary>
        /// <param name="instance">起始对象。</param>
        /// <param name="chain">成员链。</param>
        /// <returns>父级对象。</returns>
        object TraverseToParent(object instance, List<Token> chain)
        {
            if (chain.Count == 1)
            {
                return instance;
            }

            object current = instance;
            for (int i = 0; i < chain.Count - 1; i++)
            {
                current = GetMemberValue(current, chain[i]);
            }
            return current;
        }

        /// <summary>
        /// 读取对象指定成员的值。
        /// </summary>
        /// <param name="instance">目标对象。</param>
        /// <param name="memberToken">成员标记。</param>
        /// <returns>成员值。</returns>
        object GetMemberValue(object instance, Token memberToken)
        {
            if (instance == null)
            {
                throw new RuntimeException(memberToken, $"访问{memberToken.sourceString}时对象为空");
            }
            Type type = instance.GetType();
            MemberAccessor accessor = GetMemberAccessor(type, memberToken.sourceString);
            if (accessor == null || !accessor.CanRead)
            {
                throw new RuntimeException(memberToken, $"属性{memberToken.sourceString}不可读或未找到");
            }

            if (accessor.Property != null)
            {
                return accessor.Getter.Invoke(instance, null);
            }

            return accessor.Field.GetValue(instance);
        }

        /// <summary>
        /// 为对象成员赋值（字段或属性）。
        /// </summary>
        /// <param name="instance">目标对象。</param>
        /// <param name="memberToken">成员标记。</param>
        /// <param name="value">目标值。</param>
        void SetMemberValue(object instance, Token memberToken, object value)
        {
            if (instance == null)
            {
                throw new RuntimeException(memberToken, $"对象为空，无法设置{memberToken.sourceString}");
            }
            Type type = instance.GetType();
            MemberAccessor accessor = GetMemberAccessor(type, memberToken.sourceString);
            if (accessor == null || !accessor.CanWrite)
            {
                throw new RuntimeException(memberToken, $"属性{memberToken.sourceString}为只读或未找到");
            }

            //如果是属性，那么尝试赋值
            if (accessor.Property != null)
            {
                object converted = ConvertExternalValue(accessor.MemberType, memberToken.sourceString, value, memberToken);
                accessor.Setter.Invoke(instance, new object[] { converted });
                return;
            }
            //否则就是字段
            object fieldValue = ConvertExternalValue(accessor.MemberType, memberToken.sourceString, value, memberToken);
            accessor.Field.SetValue(instance, fieldValue);
        }

        /// <summary>
        /// 调用实例方法并根据参数执行重载解析。
        /// </summary>
        /// <param name="instance">目标对象。</param>
        /// <param name="methodToken">方法标记。</param>
        /// <param name="args">调用参数。</param>
        /// <returns>方法返回值。</returns>
        object InvokeInstanceMethod(object instance, Token methodToken, List<object> args)
        {
            if (instance == null)
            {
                throw new RuntimeException(methodToken, $"对象为空，无法调用{methodToken.sourceString}");
            }

            Type type = instance.GetType();
            var methods = GetMethodCandidates(type, methodToken.sourceString);
            foreach (var method in methods)
            {
                if (TryPrepareArguments(method.GetParameters(), args, methodToken, out var converted))
                {
                    return method.Invoke(instance, converted);
                }
            }

            throw new RuntimeException(methodToken, $"未找到匹配的方法{methodToken.sourceString}");
        }

        /// <summary>
        /// 尝试将脚本参数转换为方法所需的参数列表。
        /// </summary>
        /// <param name="parameters">目标方法形参。</param>
        /// <param name="args">脚本传入的参数。</param>
        /// <param name="methodToken">方法标记。</param>
        /// <param name="converted">输出转换结果。</param>
        /// <returns>是否成功匹配。</returns>
        bool TryPrepareArguments(ParameterInfo[] parameters, List<object> args, Token methodToken, out object[] converted)
        {
            converted = Array.Empty<object>();
            if (args.Count > parameters.Length)
            {
                return false;
            }

            converted = new object[parameters.Length];
            int index = 0;
            for (; index < args.Count; index++)
            {
                if (!TryConvertParameter(args[index], parameters[index], methodToken, out converted[index]))
                {
                    return false;
                }
            }

            for (; index < parameters.Length; index++)
            {
                if (parameters[index].HasDefaultValue)
                {
                    converted[index] = parameters[index].DefaultValue;
                    continue;
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// 将单个参数转换为目标类型。
        /// </summary>
        /// <param name="value">原始值。</param>
        /// <param name="parameterInfo">目标形参信息。</param>
        /// <param name="methodToken">方法标记。</param>
        /// <param name="converted">转换后的值。</param>
        /// <returns>是否转换成功。</returns>
        bool TryConvertParameter(object value, ParameterInfo parameterInfo, Token methodToken, out object converted)
        {
            try
            {
                converted = ConvertExternalValue(parameterInfo.ParameterType, parameterInfo.Name, value, methodToken);
                return true;
            }
            catch (RuntimeException)
            {
                converted = null;
                return false;
            }
        }
        /// <summary>
        /// 将反射方法封装为控制台命令并验证参数解析器。
        /// </summary>
        /// <param name="methodInfo">源方法。</param>
        /// <param name="attr">命令特性。</param>
        /// <returns>可调用封装，若校验失败则为 null。</returns>
        private MethodCallable LoadMethod(MethodInfo methodInfo, CommandAttribute attr)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length > 0)
            {
                foreach (var param in parameters)
                {
                    foreach (var par in parameters)
                    {
                        if (!CommandParameterHandle.ContainsParser(par.ParameterType))
                        {
                            Debug.LogError($"未找到解析器{par.ParameterType.Name}");
                            return null;
                        }
                    }
                }
            }
            MethodCallable command = new(attr.Name, methodInfo, attr.Description);
            return command;
        }

        class ConsoleVariableBinding
        {
            public string Name { get; }
            public Type ValueType { get; }
            public Func<object> Getter { get; }
            public Action<object> Setter { get; }

            /// <summary>
            /// 创建控制台变量绑定。
            /// </summary>
            /// <param name="name">变量名。</param>
            /// <param name="valueType">变量类型。</param>
            /// <param name="getter">读取委托。</param>
            /// <param name="setter">写入委托。</param>
            public ConsoleVariableBinding(string name, Type valueType, Func<object> getter, Action<object> setter)
            {
                Name = name;
                ValueType = valueType;
                Getter = getter;
                Setter = setter;
            }
        }

        class MemberAccessor
        {
            public PropertyInfo Property { get; }
            public FieldInfo Field { get; }
            public MethodInfo Getter { get; }
            public MethodInfo Setter { get; }
            public Type MemberType { get; }
            public bool CanRead { get; }
            public bool CanWrite { get; }

            /// <summary>
            /// 根据属性或字段构建成员访问器。
            /// </summary>
            /// <param name="property">属性信息。</param>
            /// <param name="field">字段信息。</param>
            public MemberAccessor(PropertyInfo property, FieldInfo field)
            {
                Property = property;
                Field = field;

                if (property != null)
                {
                    Getter = property.GetGetMethod(true);
                    Setter = property.GetSetMethod(true);
                    MemberType = property.PropertyType;
                    CanRead = Getter != null;
                    CanWrite = Setter != null;
                }
                else if (field != null)
                {
                    MemberType = field.FieldType;
                    Getter = null;
                    Setter = null;
                    CanRead = true;
                    CanWrite = !field.IsInitOnly;
                }
            }
        }

        /// <summary>
        /// 获取或缓存指定类型的成员访问器。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="memberName">成员名。</param>
        /// <returns>成员访问器，若不存在则为 null。</returns>
        MemberAccessor GetMemberAccessor(Type type, string memberName)
        {
            if (!memberAccessorCache.TryGetValue(type, out var cache))
            {
                cache = new Dictionary<string, MemberAccessor>(StringComparer.Ordinal);
                memberAccessorCache[type] = cache;
            }
            if (cache.TryGetValue(memberName, out var accessor))
            {
                return accessor;
            }

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var property = type.GetProperty(memberName, flags);
            if (property != null)
            {
                accessor = new MemberAccessor(property, null);
                cache[memberName] = accessor;
                return accessor;
            }
            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                accessor = new MemberAccessor(null, field);
                cache[memberName] = accessor;
                return accessor;
            }

            cache[memberName] = null;
            return null;
        }

        /// <summary>
        /// 获取类型上匹配名称的实例方法集合并进行缓存。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="methodName">方法名。</param>
        /// <returns>匹配的方法数组。</returns>
        MethodInfo[] GetMethodCandidates(Type type, string methodName)
        {
            if (!methodCache.TryGetValue(type, out var cache))
            {
                cache = new Dictionary<string, MethodInfo[]>(StringComparer.Ordinal);
                methodCache[type] = cache;
            }
            if (cache.TryGetValue(methodName, out var methods))
            {
                return methods;
            }

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            methods = type.GetMethods(flags);
            methods = Array.FindAll(methods, m => string.Equals(m.Name, methodName, StringComparison.Ordinal));
            cache[methodName] = methods;
            return methods;
        }

        /// <summary>
        /// 构建命令的可读签名文本。
        /// </summary>
        /// <param name="callable">命令封装。</param>
        /// <returns>签名字符串。</returns>
        string BuildSignature(MethodCallable callable)
        {
            StringBuilder builder = new();
            builder.Append(callable.name);
            builder.Append('(');
            for (int i = 0; i < callable.parameters.Length; i++)
            {
                var parameter = callable.parameters[i];
                builder.Append(GetDisplayTypeName(parameter.ParameterType));
                builder.Append(' ');
                builder.Append(parameter.Name);
                if (parameter.HasDefaultValue)
                {
                    builder.Append(" = ");
                    builder.Append(FormatDefaultValue(parameter.DefaultValue));
                }
                if (i < callable.parameters.Length - 1)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(')');
            return builder.ToString();
        }

        /// <summary>
        /// 获取类型在签名中展示的友好名称。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <returns>友好名称。</returns>
        string GetDisplayTypeName(Type type)
        {
            if (TypeAliases.TryGetValue(type, out var alias))
            {
                return alias;
            }

            if (type.IsArray)
            {
                return GetDisplayTypeName(type.GetElementType()) + "[]";
            }

            if (type.IsGenericType)
            {
                StringBuilder builder = new();
                string name = type.Name;
                int tickIndex = name.IndexOf('`');
                if (tickIndex >= 0)
                {
                    name = name[..tickIndex];
                }
                builder.Append(name);
                builder.Append('<');
                Type[] arguments = type.GetGenericArguments();
                for (int i = 0; i < arguments.Length; i++)
                {
                    builder.Append(GetDisplayTypeName(arguments[i]));
                    if (i < arguments.Length - 1)
                    {
                        builder.Append(", ");
                    }
                }
                builder.Append('>');
                return builder.ToString();
            }

            return type.Name;
        }

        /// <summary>
        /// 将默认值格式化为签名可读文本。
        /// </summary>
        /// <param name="value">默认值。</param>
        /// <returns>格式化结果。</returns>
        string FormatDefaultValue(object value)
        {
            if (value == null)
                return "null";
            if (value is string str)
                return $"\"{str}\"";
            if (value is bool b)
                return b ? "true" : "false";
            return value.ToString();
        }

        /// <summary>
        /// 组合命令签名与描述用于展示。
        /// </summary>
        /// <param name="signature">命令签名。</param>
        /// <param name="description">命令描述。</param>
        /// <returns>显示文本。</returns>
        string BuildDisplayText(string signature, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return signature;
            return $"{signature} - {description}";
        }
    }
}