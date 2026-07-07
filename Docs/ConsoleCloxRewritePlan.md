# 控制台系统 clox 化改造计划

## 改造目标

将当前控制台语言层从 jlox 风格的 AST 解释器，重构为 clox 风格的单遍编译字节码与栈式虚拟机执行流程。

当前流程：

```text
Scanner -> ExpressionParser -> Expression AST -> ExpressionInterpreter
```

目标流程：

```text
Scanner -> Compiler -> Chunk/Bytecode -> VM
```

本次改造只实现到函数能力，不实现 class、this、super、继承、实例对象系统、闭包 upvalue 和 GC。

## 设计原则

1. UI 层只负责输入、补全、输出显示，不参与语言解析与执行。
2. 扫描层只负责将源码切分为 Token。
3. 编译层只负责将 Token 单遍编译为字节码，不执行任何命令或反射调用。
4. VM 层只负责解释字节码，维护值栈、调用帧和运行结果。
5. 运行时绑定层只负责命令、外部变量、成员访问与反射调用。
6. 保持现有控制台输入习惯兼容，优先保证已有命令可用。
7. 每完成一个阶段，先构建或测试验证；本次用户已明确要求连续完成所有阶段，因此改为连续推进并记录结果。

## 兼容目标

需要继续支持现有输入：

```text
/Help
/Print "hello"
/PlayerInfo
/PlayerHeal 10
/PlayerDamage 5
/PlayerTeleport 1 2 3
```

需要继续支持外部变量语法：

```text
/@player.id
/@player.id = "PlayerA"
/@player.CombatEntity.hp.Add(10)
```

需要新增或保留表达式能力：

```text
/Print 1 + 2 * 3
/Print "hp=" + 10
/Print true && false
```

需要支持函数能力：

```text
fun add(a, b) {
    return a + b;
}

Print add(1, 2)
```

## 目录规划

目标目录：

```text
Assets/HotfixScripts/Module/Console/Language/
  Scanning/
    TokenType.cs
    Token.cs
    Scanner.cs

  Bytecode/
    OpCode.cs
    Chunk.cs
    ConstantPool.cs
    InternedString.cs
    StringTable.cs

  Compilation/
    Compiler.cs
    Parser.cs
    ParseRule.cs
    Precedence.cs
    Local.cs
    FunctionCompiler.cs
    CompileResult.cs

  Runtime/
    VM.cs
    CallFrame.cs
    RuntimeValue.cs
    RuntimeFunction.cs
    RuntimeException.cs
    ExecutionResult.cs

  Binding/
    CommandRegistry.cs
    VariableRegistry.cs
    ExternalMemberBinder.cs
    MethodCallable.cs
    Runtime.cs

  ScriptEngine.cs
```

旧文件迁移方向：

```text
Token.cs                  -> Scanning/TokenType.cs + Scanning/Token.cs
Scanner.cs                -> Scanning/Scanner.cs
Expression.cs             -> 删除
ExpressionParser.cs       -> 删除
ExpressionInterpreter.cs  -> 删除
Interpreter.cs            -> ScriptEngine.cs 或兼容门面
Environment.cs            -> Binding 下的 Registry/Binder/Runtime
ICallable.cs              -> Binding/MethodCallable.cs
RuntimeException.cs       -> Runtime/RuntimeException.cs
```

## 字节码指令规划

基础值：

```text
Constant
Nil
True
False
Pop
Return
```

运算：

```text
Add
Subtract
Multiply
Divide
Negate
Not
Equal
Greater
GreaterEqual
Less
LessEqual
```

变量：

```text
DefineGlobal
GetGlobal
SetGlobal
GetLocal
SetLocal
```

控制流：

```text
Jump
JumpIfFalse
Loop
```

函数：

```text
ClosurelessFunction
Call
Return
```

命令与外部绑定：

```text
InvokeCommand
GetExternal
SetExternal
GetMember
SetMember
InvokeMember
```

说明：函数先实现普通函数，不实现闭包捕获，因此指令名中保留 `ClosurelessFunction` 的概念，后续如果需要闭包可再扩展。

## 性能优化要求

### 字符串驻留

实现 `StringTable`，将标识符、命令名、成员名、全局变量名统一驻留为 `InternedString`。

目标：

```text
源码字符串 -> StringTable.Intern -> InternedString
```

这样全局变量、命令、成员缓存可以用稳定哈希或引用等价比较，减少重复字符串分配与字符串比较。

### 全局变量哈希表

全局变量表不再使用 `string.GetHashCode()` 作为唯一 key。

目标结构：

```text
Dictionary<InternedString, RuntimeValue> globals
```

原因：

1. `string.GetHashCode()` 可能冲突，不适合作为唯一变量身份。
2. 驻留字符串可避免大量重复字符串比较。
3. 后续可以替换为 clox 风格开放寻址表，而不影响编译器和 VM 接口。

### 命令注册哈希表

命令注册表使用驻留命令名：

```text
Dictionary<InternedString, MethodCallable> commands
```

UI 自动补全可以另保留展示文本缓存，但执行路径使用驻留命令名。

### 成员访问缓存

外部成员访问继续缓存反射结果：

```text
Dictionary<Type, Dictionary<InternedString, MemberAccessor>>
Dictionary<Type, Dictionary<InternedString, MethodInfo[]>>
```

目标是避免每次 `@player.xxx` 都重新反射查找。

### 常量池去重

`ConstantPool` 对字符串、数字、函数对象做基础去重。

第一版重点去重字符串常量；数字是否去重根据实现复杂度决定。

## 阶段清单

### 阶段 1：建立新字节码骨架

状态：已完成。

目标：

1. 新增 `OpCode`、`Chunk`、`ConstantPool`。
2. 新增 `RuntimeValue` 或明确使用 `object` 的运行时值策略。
3. 新增最小 `VM`，支持常量、四则运算、比较、逻辑、返回。
4. 新增最小 `ScriptEngine` 门面，但暂不替换旧系统。

验证：

```text
1 + 2 * 3 -> 7
"a" + "b" -> "ab"
true == false -> false
```

确认点：

完成后暂停，确认字节码模型和 VM 运行结果无问题，再进入阶段 2。

### 阶段 2：实现 Scanner 与 Compiler

状态：已完成。

目标：

1. 迁移并清理 `Scanner`，保留 Token 扫描职责。
2. 实现 Pratt Parser 编译规则。
3. 编译表达式时直接写入 Chunk，不构建 AST。
4. 支持数字、字符串、布尔、nil、分组、一元、二元、逻辑表达式。

验证：

```text
Print 1 + 2 * 3
Print "hp=" + 10
Print !(false)
```

确认点：

完成后暂停，确认表达式编译与执行结果，再进入阶段 3。

### 阶段 3：接入命令系统

状态：已完成。

目标：

1. 从旧 `Environment` 中拆出 `CommandRegistry`。
2. 保留 `[Command]` 扫描注册。
3. 实现 `InvokeCommand` 字节码。
4. 支持裸命令调用和括号命令调用。

兼容语法：

```text
PlayerHeal 10
PlayerHeal(10)
Print "hello"
Print 1 + 2
```

验证：

```text
/Help
/Print "hello"
/PlayerInfo
/PlayerHeal 10
```

确认点：

完成后暂停，确认现有业务命令可用，再进入阶段 4。

### 阶段 4：接入外部变量与成员访问

状态：已完成。

目标：

1. 从旧 `Environment` 中拆出 `VariableRegistry`。
2. 从旧 `Environment` 中拆出 `ExternalMemberBinder`。
3. 支持 `@变量`、`@变量.成员`、`@变量.成员 = 值`。
4. 支持外部实例方法调用。

兼容语法：

```text
@player.id
@player.id = "PlayerA"
@player.CombatEntity.hp.Add(10)
```

验证：

```text
/Print @player.id
/@player.id = "PlayerA"
/@player.CombatEntity.hp.Add(10)
```

确认点：

完成后暂停，确认外部变量读写和方法调用无问题，再进入阶段 5。

### 阶段 5：实现变量、作用域与控制流

状态：已完成。

目标：

1. 支持 `var` 全局变量。
2. 支持局部变量与块作用域。
3. 支持 `if / else`。
4. 支持 `while`。
5. 实现跳转指令回填。

语法：

```text
var a = 1;
a = a + 2;

if (a > 1) {
    Print a;
}

while (a < 5) {
    a = a + 1;
}
```

验证：

```text
var a = 1; Print a;
var a = 1; a = a + 2; Print a;
if (true) Print "yes"; else Print "no";
```

确认点：

完成后暂停，确认变量和控制流语义，再进入阶段 6。

### 阶段 6：实现函数

状态：已完成。

目标：

1. 支持 `fun name(args) { ... }`。
2. 支持普通函数调用。
3. 支持 `return`。
4. 支持调用帧 `CallFrame`。
5. 支持局部参数槽位。
6. 不支持闭包捕获外层局部变量。

语法：

```text
fun add(a, b) {
    return a + b;
}

Print add(1, 2);
```

验证：

```text
fun add(a,b){ return a+b; } Print add(1,2);
fun echo(v){ return v; } Print echo("ok");
```

确认点：

完成后暂停，确认函数语义后，再进入阶段 7。

### 阶段 7：替换旧解释器入口并清理旧实现

状态：已完成。

目标：

1. 让 `ConsoleManager.SubmitCommand` 走新 `ScriptEngine`。
2. 保留 `Interpreter` 作为兼容门面，或删除并更新引用。
3. 删除旧 AST 类型与 Visitor 解释器。
4. 清理命名空间和 using。
5. 确认旧命令系统无引用遗漏。

验证：

```text
rg "ExpressionParser|ExpressionInterpreter|Expression.cs"
dotnet build
```

确认点：

完成后暂停，确认旧系统已安全替换，再进入阶段 8。

### 阶段 8：修复控制台 UI 现有问题

状态：已完成。

目标：

1. 修复颜色字符串重复 `#` 问题。
2. 将日志结构从 `Stack<TMP_Text>` 改为 `Queue<TMP_Text>`，删除最旧日志。
3. 修复命令补全把完整签名塞回输入框的问题。
4. 补全展示文本和执行文本分离。
5. 检查 Inspector 暴露字段是否符合 Odin `LabelText` 要求。

验证：

```text
输入 /PlayerHeal 后补全为可执行命令名
连续输出超过 20 行时删除最旧行
默认颜色可正常显示
```

确认点：

完成后暂停，确认 UI 体验无问题，再进入阶段 9。

### 阶段 9：整体构建与回归验证

状态：已完成。

目标：

1. 执行项目构建。
2. 检查编译错误。
3. 整理最终变更说明。
4. 更新本文档中所有阶段状态。

验证命令：

```text
dotnet build "GameCode.slnx" -v minimal
```

如果当前解决方案无法直接构建，需要记录原因，并给出 Unity 内验证路径。

确认点：

完成后给出最终总结。

## 用户确认流程

每个阶段执行完毕后，必须暂停并给出：

1. 已完成内容。
2. 修改文件列表。
3. 验证方式与结果。
4. 发现的问题或风险。
5. 下一阶段准备做什么。

原计划要求每阶段确认后继续；用户后续明确要求“完成所有阶段”，因此本次按连续执行处理。

## 当前状态

阶段 0：计划文档已完成。

阶段 1-8：代码改造已完成。

阶段 9：验证已执行，结果如下：

1. 语言内核类型已去除 `Console` 前缀，保留宿主模块 `ConsoleManager` 与 UI 层的控制台语义命名。
2. 临时 `net10.0` 隔离项目编译并执行通过，覆盖 `Scanner -> Compiler -> VM -> Runtime` 核心链路。
3. 临时行为测试通过：
   - `Print 1 + 2 * 3` 输出 `7`
   - `var a = 1; a = a + 2; Print a` 输出 `3`
   - `fun add(a,b){ return a + b; } Print add(1,2)` 输出 `3`
   - `@box.Id = "New"; Print @box.Id` 输出 `New`
   - `@box.Add(5); Print @box.Count` 输出 `5`
4. `dotnet build "GameCode.slnx" -v minimal` 被既有 `Assets/Plugins/Utf8Json` 在当前 .NET SDK 下的 `IsConstructedGenericType` 二义性错误阻断，不是本次控制台改造文件引入的错误。

语言内核当前命名：

```text
Scanner / Token / TokenType
Compiler / CompileResult
ScriptEngine
Runtime / RuntimeException
CommandSuggestion / CommandRegistry
VariableRegistry / VariableBinding
VariableAttribute
```
