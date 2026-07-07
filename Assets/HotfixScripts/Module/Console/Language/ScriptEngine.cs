using System;
using System.Collections.Generic;

namespace Helper
{
    public sealed class ScriptEngine
    {
        readonly StringTable strings = new();
        readonly Runtime runtime;
        readonly Compiler compiler;
        readonly VM vm;

        public ScriptEngine()
        {
            runtime = new Runtime(strings);
            compiler = new Compiler(strings);
            vm = new VM(runtime);
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
            runtime.RegisterVariable(name, instance, readOnly);
        }

        public void RegisterVariable(string name, Func<object> getter, Action<object> setter = null, Type declaredType = null)
        {
            runtime.RegisterVariable(name, getter, setter, declaredType);
        }

        public List<CommandSuggestion> MatchCommandSuggestions(string keyword)
        {
            return runtime.MatchCommands(keyword);
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
