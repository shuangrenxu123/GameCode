using System;
using System.Collections.Generic;

namespace Helper
{
    public sealed class ScriptEngine
    {
        readonly StringTable strings = new();
        readonly HostEnvironment host;
        readonly Compiler compiler;
        readonly VM vm;

        public ScriptEngine()
        {
            host = new HostEnvironment(strings);
            compiler = new Compiler(strings);
            vm = new VM(host);
        }

        public ExecutionResult Execute(string source)
        {
            CompileResult compileResult = compiler.Compile(source);
            if (!compileResult.Success)
            {
                return ExecutionResult.Fail(compileResult.Error);
            }

            return vm.Execute(compileResult.Function);
        }

        public void RegisterVariable(string name, object instance, bool readOnly = false)
        {
            host.RegisterVariable(name, instance, readOnly);
        }

        public void RegisterVariable(string name, Func<object> getter, Action<object> setter = null, Type declaredType = null)
        {
            host.RegisterVariable(name, getter, setter, declaredType);
        }

        public List<CommandSuggestion> MatchCommandSuggestions(string keyword)
        {
            return host.MatchCommands(keyword);
        }

        public List<string> MatchCommands(string keyword)
        {
            List<CommandSuggestion> suggestions = MatchCommandSuggestions(keyword);
            List<string> result = new(suggestions.Count);
            foreach (CommandSuggestion suggestion in suggestions)
            {
                result.Add(suggestion.DisplayText);
            }

            return result;
        }
    }
}
