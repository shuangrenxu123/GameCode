using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using BT.EditorIntegration;
using UnityEditor;
using UnityEngine;

namespace BT.Editor.Generator
{
    static class BTCodegen
    {
        const string GeneratedEditorNodesDir = "Assets/Editor/BTWindowEditor/Generated/Nodes";
        const string GeneratedRuntimeFactoryPath = "Assets/HotfixScripts/Script/Module/AI/BTNode/Generated/BTGeneratedNodeFactory.cs";
        const string HashAssetPath = "Assets/Editor/BTWindowEditor/Generated/.bt_codegen_hash.txt";

        [MenuItem("Tools/BT/Regenerate Generated BT Nodes")]
        static void Regenerate()
        {
            var types = GetAnnotatedRuntimeTypes();
            var hash = ComputeHash(types);

            Directory.CreateDirectory(GeneratedEditorNodesDir);
            Directory.CreateDirectory(Path.GetDirectoryName(GeneratedRuntimeFactoryPath) ?? "Assets");

            AssetDatabase.StartAssetEditing();
            try
            {
                GenerateRuntimeFactory(types);
                GenerateEditorNodes(types);
                File.WriteAllText(HashAssetPath, hash);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }

        [InitializeOnLoadMethod]
        static void AutoRegenerateIfNeeded()
        {
            var types = GetAnnotatedRuntimeTypes();
            var hash = ComputeHash(types);

            var previousHash = File.Exists(HashAssetPath) ? File.ReadAllText(HashAssetPath) : null;
            if (string.Equals(previousHash, hash, StringComparison.Ordinal) && GeneratedFilesExist(types))
                return;

            Regenerate();
        }

        static bool GeneratedFilesExist(List<(Type, BTEditorNodeAttribute, ConstructorInfo)> types)
        {
            try
            {
                for (var i = 0; i < types.Count; i++)
                {
                    var type = types[i].Item1;
                    var path = Path.Combine(GeneratedEditorNodesDir, $"{type.Name}.cs").Replace('\\', '/');
                    if (!File.Exists(Path.GetFullPath(path)))
                        return false;
                }

                if (!File.Exists(Path.GetFullPath(GeneratedRuntimeFactoryPath)))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        static List<(Type, BTEditorNodeAttribute, ConstructorInfo)> GetAnnotatedRuntimeTypes()
        {
            var result = new List<(Type, BTEditorNodeAttribute, ConstructorInfo)>();

            var nodeBaseType = typeof(BTNode);
            foreach (var t in TypeCache.GetTypesDerivedFrom<BTNode>())
            {
                if (t == nodeBaseType || t.IsAbstract)
                    continue;

                var attr = t.GetCustomAttribute<BTEditorNodeAttribute>(inherit: false);
                if (attr == null)
                    continue;

                var ctor = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(c => c.GetCustomAttribute<BTEditorConstructorAttribute>(inherit: false) != null);

                result.Add((t, attr, ctor));
            }

            result.Sort((a, b) => string.CompareOrdinal(a.Item1.FullName, b.Item1.FullName));
            return result;
        }

        static string ComputeHash(List<(Type, BTEditorNodeAttribute, ConstructorInfo)> types)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var (t, attr, ctor) in types)
            {
                sb.Append(t.AssemblyQualifiedName).Append('|')
                    .Append(attr.Path).Append('|')
                    .Append(attr.Kind).Append('|');

                if (ctor != null)
                {
                    sb.Append(ctor.ToString());
                }

                var exposed = GetExposedMembers(t);
                for (var i = 0; i < exposed.Count; i++)
                {
                    var e = exposed[i];
                    sb.Append('|')
                        .Append(e.jsonName).Append(':')
                        .Append(e.type.AssemblyQualifiedName).Append(':')
                        .Append(e.runtimeMemberName).Append(':')
                        .Append((int)e.memberKind);
                }
                sb.AppendLine();
            }

            using var sha = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var hash = sha.ComputeHash(bytes);
            return BytesToHex(hash);
        }

        static string BytesToHex(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var sb = new System.Text.StringBuilder(bytes.Length * 2);
            for (var i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        static string TypeToCodeName(Type t)
        {
            if (t == null)
                return "object";

            if (t == typeof(string))
                return "string";
            if (t == typeof(int))
                return "int";
            if (t == typeof(float))
                return "float";
            if (t == typeof(bool))
                return "bool";

            // 仅处理本项目需要的类型（基础类型 + enum）。嵌套类型 FullName 会包含 '+'，C# 语法需要用 '.'。
            var name = t.FullName ?? t.Name;
            name = name.Replace('+', '.');
            return $"global::{name}";
        }

        enum ExposedMemberKind
        {
            Field = 0,
            Property = 1,
        }

        struct ExposedMember
        {
            public string safeName;
            public string jsonName;
            public Type type;
            public string runtimeMemberName;
            public ExposedMemberKind memberKind;
        }

        static List<ExposedMember> GetExposedMembers(Type runtimeType)
        {
            var result = new List<ExposedMember>();

            foreach (var m in runtimeType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = m.GetCustomAttribute<BTEditorExposeAttribute>(inherit: false);
                if (attr == null)
                    continue;

                if (m is FieldInfo f)
                {
                    if (f.IsStatic || f.IsLiteral || f.IsInitOnly)
                    {
                        Debug.LogWarning($"[BTCodegen] 跳过 {runtimeType.FullName}.{f.Name}：字段不可写。");
                        continue;
                    }

                    if (!f.IsPublic && !f.IsAssembly)
                    {
                        Debug.LogWarning($"[BTCodegen] 跳过 {runtimeType.FullName}.{f.Name}：字段必须是 public 或 internal 才能被生成的工厂赋值。");
                        continue;
                    }

                    if (!IsSupportedArgType(f.FieldType))
                    {
                        Debug.LogWarning($"[BTCodegen] 跳过 {runtimeType.FullName}.{f.Name}：不支持的参数类型 {f.FieldType.FullName}。");
                        continue;
                    }

                    var jsonName = string.IsNullOrEmpty(attr.Name) ? f.Name : attr.Name;
                    result.Add(new ExposedMember
                    {
                        safeName = SanitizeIdentifier(f.Name),
                        jsonName = jsonName,
                        type = f.FieldType,
                        runtimeMemberName = f.Name,
                        memberKind = ExposedMemberKind.Field,
                    });
                }
                else if (m is PropertyInfo p)
                {
                    if (p.GetIndexParameters().Length > 0)
                    {
                        Debug.LogWarning($"[BTCodegen] 跳过 {runtimeType.FullName}.{p.Name}：不支持索引器属性。");
                        continue;
                    }

                    var setMethod = p.GetSetMethod(nonPublic: true);
                    if (setMethod == null)
                    {
                        Debug.LogWarning($"[BTCodegen] 跳过 {runtimeType.FullName}.{p.Name}：属性没有 setter。");
                        continue;
                    }

                    if (!setMethod.IsPublic && !setMethod.IsAssembly)
                    {
                        Debug.LogWarning($"[BTCodegen] 跳过 {runtimeType.FullName}.{p.Name}：属性 setter 必须是 public 或 internal 才能被生成的工厂赋值。");
                        continue;
                    }

                    if (!IsSupportedArgType(p.PropertyType))
                    {
                        Debug.LogWarning($"[BTCodegen] 跳过 {runtimeType.FullName}.{p.Name}：不支持的参数类型 {p.PropertyType.FullName}。");
                        continue;
                    }

                    var jsonName = string.IsNullOrEmpty(attr.Name) ? p.Name : attr.Name;
                    result.Add(new ExposedMember
                    {
                        safeName = SanitizeIdentifier(p.Name),
                        jsonName = jsonName,
                        type = p.PropertyType,
                        runtimeMemberName = p.Name,
                        memberKind = ExposedMemberKind.Property,
                    });
                }
            }

            result.Sort((a, b) => string.CompareOrdinal(a.jsonName, b.jsonName));
            EnsureUniqueSafeNames(result);
            return result;
        }

        static bool IsSupportedArgType(Type t)
        {
            return t == typeof(string) || t == typeof(int) || t == typeof(float) || t == typeof(bool) ||
                   t == typeof(Vector2) || t == typeof(Vector3) ||
                   t.IsEnum;
        }

        static void EnsureUniqueSafeNames(List<ExposedMember> members)
        {
            if (members == null || members.Count == 0)
                return;

            var used = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < members.Count; i++)
            {
                var safe = members[i].safeName;
                if (string.IsNullOrEmpty(safe))
                    safe = "arg";

                if (!used.Add(safe))
                {
                    var suffix = 2;
                    var candidate = safe + "_" + suffix;
                    while (!used.Add(candidate))
                    {
                        suffix++;
                        candidate = safe + "_" + suffix;
                    }
                    safe = candidate;
                }

                var m = members[i];
                m.safeName = safe;
                members[i] = m;
            }
        }

        static string BuildArgReadExpression(Type t, string jsonName)
        {
            if (t == typeof(string))
                return $"GetString(args, \"{jsonName}\", \"\")";
            if (t == typeof(int))
                return $"GetInt(args, \"{jsonName}\", 0)";
            if (t == typeof(float))
                return $"GetFloat(args, \"{jsonName}\", 0f)";
            if (t == typeof(bool))
                return $"GetBool(args, \"{jsonName}\", false)";
            if (t == typeof(Vector2))
                return $"GetVector2(args, \"{jsonName}\", Vector2.zero)";
            if (t == typeof(Vector3))
                return $"GetVector3(args, \"{jsonName}\", Vector3.zero)";
            if (t.IsEnum)
                return $"({TypeToCodeName(t)})GetInt(args, \"{jsonName}\", 0)";
            return $"default({TypeToCodeName(t)})";
        }

        static List<string> BuildApplyExposedStatements(Type runtimeType)
        {
            var exposed = GetExposedMembers(runtimeType);
            if (exposed.Count == 0)
                return new List<string>();

            var result = new List<string>(exposed.Count);
            for (var i = 0; i < exposed.Count; i++)
            {
                var e = exposed[i];
                result.Add($"node.{e.runtimeMemberName} = {BuildArgReadExpression(e.type, e.jsonName)};");
            }
            return result;
        }

        static void GenerateRuntimeFactory(List<(Type, BTEditorNodeAttribute, ConstructorInfo)> types)
        {
            var compositeTypes = types.Where(t => t.Item2.Kind == BTEditorNodeKind.Composite).ToList();
            var decoratorTypes = types.Where(t => t.Item2.Kind == BTEditorNodeKind.Decorator).ToList();
            var leafTypes = types.Where(t => t.Item2.Kind != BTEditorNodeKind.Composite && t.Item2.Kind != BTEditorNodeKind.Decorator).ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Globalization;");
            sb.AppendLine("using BT.RuntimeSerialization;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace BT");
            sb.AppendLine("{");
            sb.AppendLine("    // Auto-generated by BTCodegen.cs. Do not edit by hand.");
            sb.AppendLine("    public static class BTGeneratedNodeFactory");
            sb.AppendLine("    {");
            sb.AppendLine("        public enum NodeKind { Leaf = 0, Decorator = 1, Composite = 2 }");
            sb.AppendLine();
            sb.AppendLine("        public static NodeKind GetKind(string typeId)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (typeId)");
            sb.AppendLine("            {");
            foreach (var t in compositeTypes)
                sb.AppendLine($"                case \"{t.Item1.FullName}\":");
            if (compositeTypes.Count > 0)
                sb.AppendLine("                    return NodeKind.Composite;");
            foreach (var t in decoratorTypes)
                sb.AppendLine($"                case \"{t.Item1.FullName}\":");
            if (decoratorTypes.Count > 0)
                sb.AppendLine("                    return NodeKind.Decorator;");
            sb.AppendLine("                default: return NodeKind.Leaf;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        public static BTNode CreateComposite(string typeId, List<BTArgJson> args)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (typeId)");
            sb.AppendLine("            {");
            foreach (var t in compositeTypes)
            {
                var ctor = t.Item3 ?? t.Item1.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                    continue;

                var apply = BuildApplyExposedStatements(t.Item1);
                if (ctor is ConstructorInfo ci && ci.GetParameters().Length > 0)
                {
                    var ctorArgs = BuildCtorArgsExpression(ci);
                    if (apply.Count == 0)
                    {
                        sb.AppendLine($"                case \"{t.Item1.FullName}\": return new {TypeToCodeName(t.Item1)}({ctorArgs});");
                    }
                    else
                    {
                        sb.AppendLine($"                case \"{t.Item1.FullName}\":");
                        sb.AppendLine("                {");
                        sb.AppendLine($"                    var node = new {TypeToCodeName(t.Item1)}({ctorArgs});");
                        for (var i = 0; i < apply.Count; i++)
                            sb.AppendLine($"                    {apply[i]}");
                        sb.AppendLine("                    return node;");
                        sb.AppendLine("                }");
                    }
                }
                else
                {
                    if (apply.Count == 0)
                    {
                        sb.AppendLine($"                case \"{t.Item1.FullName}\": return new {TypeToCodeName(t.Item1)}();");
                    }
                    else
                    {
                        sb.AppendLine($"                case \"{t.Item1.FullName}\":");
                        sb.AppendLine("                {");
                        sb.AppendLine($"                    var node = new {TypeToCodeName(t.Item1)}();");
                        for (var i = 0; i < apply.Count; i++)
                            sb.AppendLine($"                    {apply[i]}");
                        sb.AppendLine("                    return node;");
                        sb.AppendLine("                }");
                    }
                }
            }
            sb.AppendLine("                default: return null;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        public static BTNode CreateDecorator(string typeId, List<BTArgJson> args, BTNode child)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (typeId)");
            sb.AppendLine("            {");
            foreach (var t in decoratorTypes)
            {
                var ctor = t.Item3 ?? FindDecoratorCtor(t.Item1);
                if (ctor == null)
                    continue;

                var ctorArgs = BuildCtorArgsExpression(ctor, includeChild: true);
                var apply = BuildApplyExposedStatements(t.Item1);
                if (apply.Count == 0)
                {
                    sb.AppendLine($"                case \"{t.Item1.FullName}\": return new {TypeToCodeName(t.Item1)}({ctorArgs});");
                }
                else
                {
                    sb.AppendLine($"                case \"{t.Item1.FullName}\":");
                    sb.AppendLine("                {");
                    sb.AppendLine($"                    var node = new {TypeToCodeName(t.Item1)}({ctorArgs});");
                    for (var i = 0; i < apply.Count; i++)
                        sb.AppendLine($"                    {apply[i]}");
                    sb.AppendLine("                    return node;");
                    sb.AppendLine("                }");
                }
            }
            sb.AppendLine("                default: return null;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        public static BTNode CreateLeaf(string typeId, List<BTArgJson> args)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (typeId)");
            sb.AppendLine("            {");
            foreach (var t in leafTypes)
            {
                var ctor = t.Item3 ?? t.Item1.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                    continue;

                if (ctor.GetParameters().Any(p => p.ParameterType == typeof(BTNode)))
                    continue;

                var apply = BuildApplyExposedStatements(t.Item1);
                if (ctor.GetParameters().Length > 0)
                {
                    var ctorArgs = BuildCtorArgsExpression(ctor);
                    if (apply.Count == 0)
                    {
                        sb.AppendLine($"                case \"{t.Item1.FullName}\": return new {TypeToCodeName(t.Item1)}({ctorArgs});");
                    }
                    else
                    {
                        sb.AppendLine($"                case \"{t.Item1.FullName}\":");
                        sb.AppendLine("                {");
                        sb.AppendLine($"                    var node = new {TypeToCodeName(t.Item1)}({ctorArgs});");
                        for (var i = 0; i < apply.Count; i++)
                            sb.AppendLine($"                    {apply[i]}");
                        sb.AppendLine("                    return node;");
                        sb.AppendLine("                }");
                    }
                }
                else
                {
                    if (apply.Count == 0)
                    {
                        sb.AppendLine($"                case \"{t.Item1.FullName}\": return new {TypeToCodeName(t.Item1)}();");
                    }
                    else
                    {
                        sb.AppendLine($"                case \"{t.Item1.FullName}\":");
                        sb.AppendLine("                {");
                        sb.AppendLine($"                    var node = new {TypeToCodeName(t.Item1)}();");
                        for (var i = 0; i < apply.Count; i++)
                            sb.AppendLine($"                    {apply[i]}");
                        sb.AppendLine("                    return node;");
                        sb.AppendLine("                }");
                    }
                }
            }
            sb.AppendLine("                default: return null;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        static string GetString(List<BTArgJson> args, string name, string defaultValue)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (args == null) return defaultValue;");
            sb.AppendLine("            for (var i = 0; i < args.Count; i++)");
            sb.AppendLine("            {");
            sb.AppendLine("                var a = args[i];");
            sb.AppendLine("                if (a != null && a.name == name) return a.value ?? defaultValue;");
            sb.AppendLine("            }");
            sb.AppendLine("            return defaultValue;");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        static int GetInt(List<BTArgJson> args, string name, int defaultValue)");
            sb.AppendLine("        {");
            sb.AppendLine("            var s = GetString(args, name, null);");
            sb.AppendLine("            if (string.IsNullOrEmpty(s)) return defaultValue;");
            sb.AppendLine("            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : defaultValue;");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        static float GetFloat(List<BTArgJson> args, string name, float defaultValue)");
            sb.AppendLine("        {");
            sb.AppendLine("            var s = GetString(args, name, null);");
            sb.AppendLine("            if (string.IsNullOrEmpty(s)) return defaultValue;");
            sb.AppendLine("            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : defaultValue;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        static bool GetBool(List<BTArgJson> args, string name, bool defaultValue)");
            sb.AppendLine("        {");
            sb.AppendLine("            var s = GetString(args, name, null);");
            sb.AppendLine("            if (string.IsNullOrEmpty(s)) return defaultValue;");
            sb.AppendLine("            if (bool.TryParse(s, out var v)) return v;");
            sb.AppendLine("            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i != 0 : defaultValue;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        static Vector2 GetVector2(List<BTArgJson> args, string name, Vector2 defaultValue)");
            sb.AppendLine("        {");
            sb.AppendLine("            var s = GetString(args, name, null);");
            sb.AppendLine("            if (string.IsNullOrEmpty(s)) return defaultValue;");
            sb.AppendLine("            var parts = s.Split(',');");
            sb.AppendLine("            if (parts.Length < 2) return defaultValue;");
            sb.AppendLine("            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) return defaultValue;");
            sb.AppendLine("            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return defaultValue;");
            sb.AppendLine("            return new Vector2(x, y);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        static Vector3 GetVector3(List<BTArgJson> args, string name, Vector3 defaultValue)");
            sb.AppendLine("        {");
            sb.AppendLine("            var s = GetString(args, name, null);");
            sb.AppendLine("            if (string.IsNullOrEmpty(s)) return defaultValue;");
            sb.AppendLine("            var parts = s.Split(',');");
            sb.AppendLine("            if (parts.Length < 3) return defaultValue;");
            sb.AppendLine("            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) return defaultValue;");
            sb.AppendLine("            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return defaultValue;");
            sb.AppendLine("            if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z)) return defaultValue;");
            sb.AppendLine("            return new Vector3(x, y, z);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            WriteIfChanged(GeneratedRuntimeFactoryPath, sb.ToString());
        }

        static ConstructorInfo FindDecoratorCtor(Type t)
        {
            return t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Any(p => p.ParameterType == typeof(BTNode)));
        }

        static string BuildCtorArgsExpression(ConstructorInfo ctor, bool includeChild = false)
        {
            var ps = ctor.GetParameters();
            var expr = new List<string>(ps.Length);
            foreach (var p in ps)
            {
                if (p.ParameterType == typeof(BTNode))
                {
                    expr.Add("child");
                    continue;
                }

                if (p.ParameterType == typeof(string))
                    expr.Add($"GetString(args, \"{p.Name}\", \"\")");
                else if (p.ParameterType == typeof(int))
                    expr.Add($"GetInt(args, \"{p.Name}\", 0)");
                else if (p.ParameterType == typeof(float))
                    expr.Add($"GetFloat(args, \"{p.Name}\", 0f)");
                else if (p.ParameterType == typeof(bool))
                    expr.Add($"GetBool(args, \"{p.Name}\", false)");
                else if (p.ParameterType == typeof(Vector2))
                    expr.Add($"GetVector2(args, \"{p.Name}\", Vector2.zero)");
                else if (p.ParameterType == typeof(Vector3))
                    expr.Add($"GetVector3(args, \"{p.Name}\", Vector3.zero)");
                else if (p.ParameterType.IsEnum)
                    expr.Add($"({TypeToCodeName(p.ParameterType)})GetInt(args, \"{p.Name}\", 0)");
                else
                    expr.Add($"default({TypeToCodeName(p.ParameterType)})");
            }

            return string.Join(", ", expr);
        }

        static void GenerateEditorNodes(List<(Type, BTEditorNodeAttribute, ConstructorInfo)> types)
        {
            foreach (var (t, attr, ctor) in types)
            {
                var fileName = $"{t.Name}.cs";
                var outPath = Path.Combine(GeneratedEditorNodesDir, fileName).Replace('\\', '/');
                var content = GenerateEditorNodeSource(t, attr, ctor);
                WriteIfChanged(outPath, content);
            }
        }

        static string GenerateEditorNodeSource(Type runtimeType, BTEditorNodeAttribute attr, ConstructorInfo ctor)
        {
            var className = runtimeType.Name;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Globalization;");
            sb.AppendLine("using BT.Editor.Internal;");
            sb.AppendLine("using BT.Editor.RuntimeJson;");
            sb.AppendLine("using BT.EditorIntegration;");
            sb.AppendLine("using BT.RuntimeSerialization;");
            sb.AppendLine("using Unity.GraphToolkit.Editor;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace BT.Editor.Generated.Nodes");
            sb.AppendLine("{");
            sb.AppendLine("    // Auto-generated by BTCodegen.cs. Do not edit by hand.");
            sb.AppendLine("    [Serializable]");
            sb.AppendLine("    [UseWithGraph(typeof(BTTreeGraph))]");
            sb.AppendLine($"    class {className} : Node, IBTRuntimeJsonNode");
            sb.AppendLine("    {");
            sb.AppendLine("        const string ParentPortId = \"Parent\";");
            sb.AppendLine("        const string ChildPortId = \"Child\";");
            sb.AppendLine();
            sb.AppendLine("        [SerializeField] string nodeId;");

            var ctorArgs = GetCtorArgs(ctor);
            var exposed = GetExposedMembers(runtimeType);

            var allArgs = new List<(string safeName, string jsonName, Type type)>(ctorArgs.Count + exposed.Count);
            var usedJsonNames = new HashSet<string>(StringComparer.Ordinal);
            var usedSafeNames = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < ctorArgs.Count; i++)
            {
                var a = ctorArgs[i];
                allArgs.Add((a.name, a.originalName, a.type));
                usedJsonNames.Add(a.originalName);
                usedSafeNames.Add(a.name);
            }

            for (var i = 0; i < exposed.Count; i++)
            {
                var e = exposed[i];
                if (!usedJsonNames.Add(e.jsonName))
                    continue;

                var safe = string.IsNullOrEmpty(e.safeName) ? "arg" : e.safeName;
                if (!usedSafeNames.Add(safe))
                {
                    var suffix = 2;
                    var candidate = safe + "_" + suffix;
                    while (!usedSafeNames.Add(candidate))
                    {
                        suffix++;
                        candidate = safe + "_" + suffix;
                    }
                    safe = candidate;
                }

                allArgs.Add((safe, e.jsonName, e.type));
            }

            foreach (var a in allArgs)
            {
                var fieldDecl = a.type == typeof(string) ? $"        [SerializeField] string {a.safeName};"
                    : a.type == typeof(int) ? $"        [SerializeField] int {a.safeName};"
                    : a.type == typeof(float) ? $"        [SerializeField] float {a.safeName};"
                    : a.type == typeof(bool) ? $"        [SerializeField] bool {a.safeName};"
                    : a.type == typeof(Vector2) ? $"        [SerializeField] Vector2 {a.safeName};"
                    : a.type == typeof(Vector3) ? $"        [SerializeField] Vector3 {a.safeName};"
                    : a.type.IsEnum ? $"        [SerializeField] {TypeToCodeName(a.type)} {a.safeName};"
                    : null;
                if (fieldDecl != null)
                    sb.AppendLine(fieldDecl);
            }

            sb.AppendLine();
            sb.AppendLine("        public string NodeId => nodeId;");
            sb.AppendLine($"        public string RuntimeTypeId => \"{runtimeType.FullName}\";");
            sb.AppendLine($"        public BTEditorNodeKind Kind => BTEditorNodeKind.{attr.Kind};");
            sb.AppendLine($"        public BTChildCapacity ChildCapacity => BTChildCapacity.{attr.ChildCapacity};");
            sb.AppendLine();
            sb.AppendLine("        public override void OnEnable()");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrEmpty(nodeId)) nodeId = Guid.NewGuid().ToString(\"N\");");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        protected override void OnDefineOptions(IOptionDefinitionContext context)");
            sb.AppendLine("        {");
            if (allArgs.Count == 0)
            {
                sb.AppendLine("        }");
            }
            else
            {
                foreach (var a in allArgs)
                {
                    var optionTypeName = a.type == typeof(string) ? "string"
                        : a.type == typeof(int) ? "int"
                        : a.type == typeof(float) ? "float"
                        : a.type == typeof(bool) ? "bool"
                        : a.type == typeof(Vector2) ? "Vector2"
                        : a.type == typeof(Vector3) ? "Vector3"
                        : a.type.IsEnum ? TypeToCodeName(a.type)
                        : null;

                    if (optionTypeName == null)
                        continue;

                    var defaultValueExpr = a.type == typeof(string) ? $"{a.safeName} ?? string.Empty"
                        : a.safeName;

                    sb.AppendLine($"            context.AddOption<{optionTypeName}>(\"{a.safeName}\")");
                    sb.AppendLine($"                .WithDisplayName(\"{a.jsonName}\")");
                    sb.AppendLine($"                .WithDefaultValue({defaultValueExpr});");
                }
                sb.AppendLine("        }");
            }
            sb.AppendLine();

            sb.AppendLine("        protected override void OnDefinePorts(IPortDefinitionContext context)");
            sb.AppendLine("        {");
            sb.AppendLine("            context.AddInputPort(ParentPortId).WithDisplayName(\"Parent\").Build();");
            sb.AppendLine();
            sb.AppendLine("            if (ChildCapacity == BTChildCapacity.Single)");
            sb.AppendLine("            {");
            sb.AppendLine("                var output = context.AddOutputPort(ChildPortId).WithDisplayName(\"Child\").Build();");
            sb.AppendLine("                GraphToolkitPortCapacityUtil.TryForceSingleCapacity(output);");
            sb.AppendLine("                return;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            if (ChildCapacity == BTChildCapacity.Multi)");
            sb.AppendLine("            {");
            sb.AppendLine("                context.AddOutputPort(\"Children\").WithDisplayName(\"Children\").Build();");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        public List<BTArgJson> CollectArgs()");
            sb.AppendLine("        {");
            if (allArgs.Count == 0)
            {
                sb.AppendLine("            return new List<BTArgJson>();");
            }
            else
            {
                foreach (var a in allArgs)
                {
                    var optionTypeName = a.type == typeof(string) ? "string"
                        : a.type == typeof(int) ? "int"
                        : a.type == typeof(float) ? "float"
                        : a.type == typeof(bool) ? "bool"
                        : a.type == typeof(Vector2) ? "Vector2"
                        : a.type == typeof(Vector3) ? "Vector3"
                        : a.type.IsEnum ? TypeToCodeName(a.type)
                        : null;
                    if (optionTypeName == null)
                        continue;

                    sb.AppendLine($"            var opt_{a.safeName} = GetNodeOptionByName(\"{a.safeName}\");");
                    sb.AppendLine($"            if (opt_{a.safeName} != null && opt_{a.safeName}.TryGetValue<{optionTypeName}>(out var v_{a.safeName})) {a.safeName} = v_{a.safeName};");
                }
                sb.AppendLine();
                sb.AppendLine("            return new List<BTArgJson>");
                sb.AppendLine("            {");
                foreach (var a in allArgs)
                {
                    if (a.type == typeof(string))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.jsonName}\", type = BTArgType.String, value = {a.safeName} ?? string.Empty }},");
                    else if (a.type == typeof(int))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.jsonName}\", type = BTArgType.Int, value = {a.safeName}.ToString(CultureInfo.InvariantCulture) }},");
                    else if (a.type == typeof(float))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.jsonName}\", type = BTArgType.Float, value = {a.safeName}.ToString(CultureInfo.InvariantCulture) }},");
                    else if (a.type == typeof(bool))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.jsonName}\", type = BTArgType.Bool, value = {a.safeName} ? \"true\" : \"false\" }},");
                    else if (a.type == typeof(Vector2))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.jsonName}\", type = BTArgType.Vector2, value = $\"{{{a.safeName}.x.ToString(CultureInfo.InvariantCulture)}},{{{a.safeName}.y.ToString(CultureInfo.InvariantCulture)}}\" }},");
                    else if (a.type == typeof(Vector3))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.jsonName}\", type = BTArgType.Vector3, value = $\"{{{a.safeName}.x.ToString(CultureInfo.InvariantCulture)}},{{{a.safeName}.y.ToString(CultureInfo.InvariantCulture)}},{{{a.safeName}.z.ToString(CultureInfo.InvariantCulture)}}\" }},");
                    else if (a.type.IsEnum)
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.jsonName}\", type = BTArgType.Int, value = ((int){a.safeName}).ToString(CultureInfo.InvariantCulture) }},");
                }
                sb.AppendLine("            };");
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        static List<(string name, string originalName, Type type)> GetCtorArgs(ConstructorInfo ctor)
        {
            var result = new List<(string, string, Type)>();
            if (ctor == null)
                return result;

            foreach (var p in ctor.GetParameters())
            {
                if (p.ParameterType == typeof(BTNode))
                    continue;

                if (p.ParameterType != typeof(string) && p.ParameterType != typeof(int) && p.ParameterType != typeof(float) && p.ParameterType != typeof(bool) &&
                    p.ParameterType != typeof(Vector2) && p.ParameterType != typeof(Vector3) && !p.ParameterType.IsEnum)
                    continue;

                var safeName = SanitizeIdentifier(p.Name);
                result.Add((safeName, p.Name, p.ParameterType));
            }

            return result;
        }

        static string SanitizeIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "arg";
            var s = new string(name.Where(ch => char.IsLetterOrDigit(ch) || ch == '_').ToArray());
            if (string.IsNullOrEmpty(s))
                s = "arg";
            if (char.IsDigit(s[0]))
                s = "_" + s;
            return s;
        }

        static void WriteIfChanged(string assetPath, string content)
        {
            var fullPath = Path.GetFullPath(assetPath);
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(fullPath))
            {
                var existing = File.ReadAllText(fullPath);
                if (string.Equals(existing, content, StringComparison.Ordinal))
                    return;
            }

            File.WriteAllText(fullPath, content);
        }
    }
}
