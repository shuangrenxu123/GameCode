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
        const string GeneratedRuntimeFactoryPath = "Assets/HotfixScripts/Module/AI/decision/Behavior Tree/Generated/BTGeneratedNodeFactory.cs";
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

        static void GenerateRuntimeFactory(List<(Type, BTEditorNodeAttribute, ConstructorInfo)> types)
        {
            var compositeTypes = types.Where(t => t.Item2.Kind == BTEditorNodeKind.Composite).ToList();
            var decoratorTypes = types.Where(t => t.Item2.Kind == BTEditorNodeKind.Decorator).ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Globalization;");
            sb.AppendLine("using BT.RuntimeSerialization;");
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

                if (ctor is ConstructorInfo ci && ci.GetParameters().Length > 0)
                {
                    var ctorArgs = BuildCtorArgsExpression(ci);
                    sb.AppendLine($"                case \"{t.Item1.FullName}\": return new {t.Item1.FullName}({ctorArgs});");
                }
                else
                {
                    sb.AppendLine($"                case \"{t.Item1.FullName}\": return new {t.Item1.FullName}();");
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
                sb.AppendLine($"                case \"{t.Item1.FullName}\": return new {t.Item1.FullName}({ctorArgs});");
            }
            sb.AppendLine("                default: return null;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("        public static BTNode CreateLeaf(string typeId, List<BTArgJson> args) => null;");
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
                else if (p.ParameterType.IsEnum)
                    expr.Add($"({p.ParameterType.FullName})GetInt(args, \"{p.Name}\", 0)");
                else
                    expr.Add($"default({p.ParameterType.FullName})");
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

            var extraArgs = GetCtorArgs(ctor);
            foreach (var a in extraArgs)
            {
                var fieldDecl = a.type == typeof(string) ? $"        [SerializeField] string {a.name};"
                    : a.type == typeof(int) ? $"        [SerializeField] int {a.name};"
                    : a.type == typeof(float) ? $"        [SerializeField] float {a.name};"
                    : a.type == typeof(bool) ? $"        [SerializeField] bool {a.name};"
                    : a.type.IsEnum ? $"        [SerializeField] {a.type.FullName} {a.name};"
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
            if (extraArgs.Count == 0)
            {
                sb.AppendLine("            return new List<BTArgJson>();");
            }
            else
            {
                sb.AppendLine("            return new List<BTArgJson>");
                sb.AppendLine("            {");
                foreach (var a in extraArgs)
                {
                    if (a.type == typeof(string))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.originalName}\", type = BTArgType.String, value = {a.name} ?? string.Empty }},");
                    else if (a.type == typeof(int))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.originalName}\", type = BTArgType.Int, value = {a.name}.ToString(CultureInfo.InvariantCulture) }},");
                    else if (a.type == typeof(float))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.originalName}\", type = BTArgType.Float, value = {a.name}.ToString(CultureInfo.InvariantCulture) }},");
                    else if (a.type == typeof(bool))
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.originalName}\", type = BTArgType.Bool, value = {a.name} ? \"true\" : \"false\" }},");
                    else if (a.type.IsEnum)
                        sb.AppendLine($"                new BTArgJson {{ name = \"{a.originalName}\", type = BTArgType.Int, value = ((int){a.name}).ToString(CultureInfo.InvariantCulture) }},");
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

                if (p.ParameterType != typeof(string) && p.ParameterType != typeof(int) && p.ParameterType != typeof(float) && p.ParameterType != typeof(bool) && !p.ParameterType.IsEnum)
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
