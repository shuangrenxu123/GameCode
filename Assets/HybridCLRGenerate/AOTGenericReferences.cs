using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

    // {{ AOT assemblies
    public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
    {
        "Google.Protobuf.dll",
        "System.Core.dll",
        "Unity.Animation.Rigging.dll",
        "Unity.InputSystem.dll",
        "UnityEngine.AssetBundleModule.dll",
        "UnityEngine.CoreModule.dll",
        "mscorlib.dll",
    };
    // }}

    // {{ constraint implement type
    // }} 

    // {{ AOT generic types
    // Google.Protobuf.IDeepCloneable<object>
    // Google.Protobuf.IMessage<object>
    // Google.Protobuf.MessageParser.<>c__DisplayClass2_0<object>
    // Google.Protobuf.MessageParser<object>
    // System.Action<!!0>
    // System.Action<!0>
    // System.Action<BT.BTResult>
    // System.Action<BuffTag>
    // System.Action<LitJson.PropertyMetadata>
    // System.Action<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Action<System.Net.Sockets.SocketError>
    // System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>
    // System.Action<UnityEngine.Vector2>
    // System.Action<byte>
    // System.Action<float>
    // System.Action<int>
    // System.Action<object,object>
    // System.Action<object>
    // System.ArraySegment.Enumerator<byte>
    // System.ArraySegment<byte>
    // System.Buffers.MemoryManager<byte>
    // System.ByReference<byte>
    // System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<object>
    // System.Collections.Concurrent.ConcurrentQueue.Segment<object>
    // System.Collections.Concurrent.ConcurrentQueue<object>
    // System.Collections.Generic.ArraySortHelper<BT.BTResult>
    // System.Collections.Generic.ArraySortHelper<BuffTag>
    // System.Collections.Generic.ArraySortHelper<LitJson.PropertyMetadata>
    // System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.ArraySortHelper<UnityEngine.Vector2>
    // System.Collections.Generic.ArraySortHelper<byte>
    // System.Collections.Generic.ArraySortHelper<int>
    // System.Collections.Generic.ArraySortHelper<object>
    // System.Collections.Generic.Comparer<BT.BTResult>
    // System.Collections.Generic.Comparer<BuffTag>
    // System.Collections.Generic.Comparer<LitJson.PropertyMetadata>
    // System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.Comparer<UnityEngine.Vector2>
    // System.Collections.Generic.Comparer<byte>
    // System.Collections.Generic.Comparer<float>
    // System.Collections.Generic.Comparer<int>
    // System.Collections.Generic.Comparer<object>
    // System.Collections.Generic.Dictionary.Enumerator<AudioLayer,object>
    // System.Collections.Generic.Dictionary.Enumerator<Fight.ActionPointType,object>
    // System.Collections.Generic.Dictionary.Enumerator<HTN.WSProperties,byte>
    // System.Collections.Generic.Dictionary.Enumerator<HTN.WSProperties,float>
    // System.Collections.Generic.Dictionary.Enumerator<HTN.WSProperties,int>
    // System.Collections.Generic.Dictionary.Enumerator<HTN.WSProperties,object>
    // System.Collections.Generic.Dictionary.Enumerator<int,int>
    // System.Collections.Generic.Dictionary.Enumerator<int,object>
    // System.Collections.Generic.Dictionary.Enumerator<object,LitJson.ArrayMetadata>
    // System.Collections.Generic.Dictionary.Enumerator<object,LitJson.ObjectMetadata>
    // System.Collections.Generic.Dictionary.Enumerator<object,LitJson.PropertyMetadata>
    // System.Collections.Generic.Dictionary.Enumerator<object,object>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<AudioLayer,object>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<Fight.ActionPointType,object>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HTN.WSProperties,byte>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HTN.WSProperties,float>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HTN.WSProperties,int>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HTN.WSProperties,object>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,LitJson.ArrayMetadata>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,LitJson.ObjectMetadata>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,LitJson.PropertyMetadata>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
    // System.Collections.Generic.Dictionary.KeyCollection<AudioLayer,object>
    // System.Collections.Generic.Dictionary.KeyCollection<Fight.ActionPointType,object>
    // System.Collections.Generic.Dictionary.KeyCollection<HTN.WSProperties,byte>
    // System.Collections.Generic.Dictionary.KeyCollection<HTN.WSProperties,float>
    // System.Collections.Generic.Dictionary.KeyCollection<HTN.WSProperties,int>
    // System.Collections.Generic.Dictionary.KeyCollection<HTN.WSProperties,object>
    // System.Collections.Generic.Dictionary.KeyCollection<int,int>
    // System.Collections.Generic.Dictionary.KeyCollection<int,object>
    // System.Collections.Generic.Dictionary.KeyCollection<object,LitJson.ArrayMetadata>
    // System.Collections.Generic.Dictionary.KeyCollection<object,LitJson.ObjectMetadata>
    // System.Collections.Generic.Dictionary.KeyCollection<object,LitJson.PropertyMetadata>
    // System.Collections.Generic.Dictionary.KeyCollection<object,object>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<AudioLayer,object>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<Fight.ActionPointType,object>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HTN.WSProperties,byte>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HTN.WSProperties,float>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HTN.WSProperties,int>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HTN.WSProperties,object>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,LitJson.ArrayMetadata>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,LitJson.ObjectMetadata>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,LitJson.PropertyMetadata>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
    // System.Collections.Generic.Dictionary.ValueCollection<AudioLayer,object>
    // System.Collections.Generic.Dictionary.ValueCollection<Fight.ActionPointType,object>
    // System.Collections.Generic.Dictionary.ValueCollection<HTN.WSProperties,byte>
    // System.Collections.Generic.Dictionary.ValueCollection<HTN.WSProperties,float>
    // System.Collections.Generic.Dictionary.ValueCollection<HTN.WSProperties,int>
    // System.Collections.Generic.Dictionary.ValueCollection<HTN.WSProperties,object>
    // System.Collections.Generic.Dictionary.ValueCollection<int,int>
    // System.Collections.Generic.Dictionary.ValueCollection<int,object>
    // System.Collections.Generic.Dictionary.ValueCollection<object,LitJson.ArrayMetadata>
    // System.Collections.Generic.Dictionary.ValueCollection<object,LitJson.ObjectMetadata>
    // System.Collections.Generic.Dictionary.ValueCollection<object,LitJson.PropertyMetadata>
    // System.Collections.Generic.Dictionary.ValueCollection<object,object>
    // System.Collections.Generic.Dictionary<AudioLayer,object>
    // System.Collections.Generic.Dictionary<Fight.ActionPointType,object>
    // System.Collections.Generic.Dictionary<HTN.WSProperties,byte>
    // System.Collections.Generic.Dictionary<HTN.WSProperties,float>
    // System.Collections.Generic.Dictionary<HTN.WSProperties,int>
    // System.Collections.Generic.Dictionary<HTN.WSProperties,object>
    // System.Collections.Generic.Dictionary<int,int>
    // System.Collections.Generic.Dictionary<int,object>
    // System.Collections.Generic.Dictionary<object,LitJson.ArrayMetadata>
    // System.Collections.Generic.Dictionary<object,LitJson.ObjectMetadata>
    // System.Collections.Generic.Dictionary<object,LitJson.PropertyMetadata>
    // System.Collections.Generic.Dictionary<object,object>
    // System.Collections.Generic.EqualityComparer<AudioLayer>
    // System.Collections.Generic.EqualityComparer<BuffTag>
    // System.Collections.Generic.EqualityComparer<Fight.ActionPointType>
    // System.Collections.Generic.EqualityComparer<HTN.WSProperties>
    // System.Collections.Generic.EqualityComparer<LitJson.ArrayMetadata>
    // System.Collections.Generic.EqualityComparer<LitJson.ObjectMetadata>
    // System.Collections.Generic.EqualityComparer<LitJson.PropertyMetadata>
    // System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.EqualityComparer<byte>
    // System.Collections.Generic.EqualityComparer<float>
    // System.Collections.Generic.EqualityComparer<int>
    // System.Collections.Generic.EqualityComparer<object>
    // System.Collections.Generic.HashSet.Enumerator<BuffTag>
    // System.Collections.Generic.HashSet.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.HashSet.Enumerator<int>
    // System.Collections.Generic.HashSet.Enumerator<object>
    // System.Collections.Generic.HashSet<BuffTag>
    // System.Collections.Generic.HashSet<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.HashSet<int>
    // System.Collections.Generic.HashSet<object>
    // System.Collections.Generic.HashSetEqualityComparer<BuffTag>
    // System.Collections.Generic.HashSetEqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.HashSetEqualityComparer<int>
    // System.Collections.Generic.HashSetEqualityComparer<object>
    // System.Collections.Generic.ICollection<BT.BTResult>
    // System.Collections.Generic.ICollection<BuffTag>
    // System.Collections.Generic.ICollection<LitJson.PropertyMetadata>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<AudioLayer,object>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<Fight.ActionPointType,object>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HTN.WSProperties,byte>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HTN.WSProperties,float>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HTN.WSProperties,int>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HTN.WSProperties,object>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,LitJson.ArrayMetadata>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,LitJson.ObjectMetadata>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,LitJson.PropertyMetadata>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.ICollection<UnityEngine.Vector2>
    // System.Collections.Generic.ICollection<byte>
    // System.Collections.Generic.ICollection<int>
    // System.Collections.Generic.ICollection<object>
    // System.Collections.Generic.IComparer<BT.BTResult>
    // System.Collections.Generic.IComparer<BuffTag>
    // System.Collections.Generic.IComparer<LitJson.PropertyMetadata>
    // System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.IComparer<UnityEngine.Vector2>
    // System.Collections.Generic.IComparer<byte>
    // System.Collections.Generic.IComparer<float>
    // System.Collections.Generic.IComparer<int>
    // System.Collections.Generic.IComparer<object>
    // System.Collections.Generic.IDictionary<int,object>
    // System.Collections.Generic.IDictionary<object,LitJson.ArrayMetadata>
    // System.Collections.Generic.IDictionary<object,LitJson.ObjectMetadata>
    // System.Collections.Generic.IDictionary<object,LitJson.PropertyMetadata>
    // System.Collections.Generic.IDictionary<object,object>
    // System.Collections.Generic.IEnumerable<BT.BTResult>
    // System.Collections.Generic.IEnumerable<BuffTag>
    // System.Collections.Generic.IEnumerable<LitJson.PropertyMetadata>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<AudioLayer,object>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Fight.ActionPointType,object>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HTN.WSProperties,byte>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HTN.WSProperties,float>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HTN.WSProperties,int>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HTN.WSProperties,object>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,LitJson.ArrayMetadata>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,LitJson.ObjectMetadata>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,LitJson.PropertyMetadata>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.IEnumerable<UnityEngine.Vector2>
    // System.Collections.Generic.IEnumerable<byte>
    // System.Collections.Generic.IEnumerable<int>
    // System.Collections.Generic.IEnumerable<object>
    // System.Collections.Generic.IEnumerator<BT.BTResult>
    // System.Collections.Generic.IEnumerator<BuffTag>
    // System.Collections.Generic.IEnumerator<LitJson.PropertyMetadata>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<AudioLayer,object>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<Fight.ActionPointType,object>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HTN.WSProperties,byte>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HTN.WSProperties,float>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HTN.WSProperties,int>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HTN.WSProperties,object>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,LitJson.ArrayMetadata>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,LitJson.ObjectMetadata>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,LitJson.PropertyMetadata>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.IEnumerator<UnityEngine.Vector2>
    // System.Collections.Generic.IEnumerator<byte>
    // System.Collections.Generic.IEnumerator<int>
    // System.Collections.Generic.IEnumerator<object>
    // System.Collections.Generic.IEqualityComparer<AudioLayer>
    // System.Collections.Generic.IEqualityComparer<BuffTag>
    // System.Collections.Generic.IEqualityComparer<Fight.ActionPointType>
    // System.Collections.Generic.IEqualityComparer<HTN.WSProperties>
    // System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.IEqualityComparer<int>
    // System.Collections.Generic.IEqualityComparer<object>
    // System.Collections.Generic.IList<BT.BTResult>
    // System.Collections.Generic.IList<BuffTag>
    // System.Collections.Generic.IList<LitJson.PropertyMetadata>
    // System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.IList<UnityEngine.Vector2>
    // System.Collections.Generic.IList<byte>
    // System.Collections.Generic.IList<int>
    // System.Collections.Generic.IList<object>
    // System.Collections.Generic.KeyValuePair<AudioLayer,object>
    // System.Collections.Generic.KeyValuePair<Fight.ActionPointType,object>
    // System.Collections.Generic.KeyValuePair<HTN.WSProperties,byte>
    // System.Collections.Generic.KeyValuePair<HTN.WSProperties,float>
    // System.Collections.Generic.KeyValuePair<HTN.WSProperties,int>
    // System.Collections.Generic.KeyValuePair<HTN.WSProperties,object>
    // System.Collections.Generic.KeyValuePair<int,int>
    // System.Collections.Generic.KeyValuePair<int,object>
    // System.Collections.Generic.KeyValuePair<object,LitJson.ArrayMetadata>
    // System.Collections.Generic.KeyValuePair<object,LitJson.ObjectMetadata>
    // System.Collections.Generic.KeyValuePair<object,LitJson.PropertyMetadata>
    // System.Collections.Generic.KeyValuePair<object,object>
    // System.Collections.Generic.List.Enumerator<BT.BTResult>
    // System.Collections.Generic.List.Enumerator<BuffTag>
    // System.Collections.Generic.List.Enumerator<LitJson.PropertyMetadata>
    // System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.List.Enumerator<UnityEngine.Vector2>
    // System.Collections.Generic.List.Enumerator<byte>
    // System.Collections.Generic.List.Enumerator<int>
    // System.Collections.Generic.List.Enumerator<object>
    // System.Collections.Generic.List<BT.BTResult>
    // System.Collections.Generic.List<BuffTag>
    // System.Collections.Generic.List<LitJson.PropertyMetadata>
    // System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.List<UnityEngine.Vector2>
    // System.Collections.Generic.List<byte>
    // System.Collections.Generic.List<int>
    // System.Collections.Generic.List<object>
    // System.Collections.Generic.ObjectComparer<BT.BTResult>
    // System.Collections.Generic.ObjectComparer<BuffTag>
    // System.Collections.Generic.ObjectComparer<LitJson.PropertyMetadata>
    // System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.ObjectComparer<UnityEngine.Vector2>
    // System.Collections.Generic.ObjectComparer<byte>
    // System.Collections.Generic.ObjectComparer<float>
    // System.Collections.Generic.ObjectComparer<int>
    // System.Collections.Generic.ObjectComparer<object>
    // System.Collections.Generic.ObjectEqualityComparer<AudioLayer>
    // System.Collections.Generic.ObjectEqualityComparer<BuffTag>
    // System.Collections.Generic.ObjectEqualityComparer<Fight.ActionPointType>
    // System.Collections.Generic.ObjectEqualityComparer<HTN.WSProperties>
    // System.Collections.Generic.ObjectEqualityComparer<LitJson.ArrayMetadata>
    // System.Collections.Generic.ObjectEqualityComparer<LitJson.ObjectMetadata>
    // System.Collections.Generic.ObjectEqualityComparer<LitJson.PropertyMetadata>
    // System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.ObjectEqualityComparer<byte>
    // System.Collections.Generic.ObjectEqualityComparer<float>
    // System.Collections.Generic.ObjectEqualityComparer<int>
    // System.Collections.Generic.ObjectEqualityComparer<object>
    // System.Collections.Generic.Queue.Enumerator<object>
    // System.Collections.Generic.Queue<object>
    // System.Collections.Generic.Stack.Enumerator<int>
    // System.Collections.Generic.Stack.Enumerator<object>
    // System.Collections.Generic.Stack<int>
    // System.Collections.Generic.Stack<object>
    // System.Collections.ObjectModel.ReadOnlyCollection<BT.BTResult>
    // System.Collections.ObjectModel.ReadOnlyCollection<BuffTag>
    // System.Collections.ObjectModel.ReadOnlyCollection<LitJson.PropertyMetadata>
    // System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector2>
    // System.Collections.ObjectModel.ReadOnlyCollection<byte>
    // System.Collections.ObjectModel.ReadOnlyCollection<int>
    // System.Collections.ObjectModel.ReadOnlyCollection<object>
    // System.Comparison<BT.BTResult>
    // System.Comparison<BuffTag>
    // System.Comparison<LitJson.PropertyMetadata>
    // System.Comparison<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Comparison<UnityEngine.Vector2>
    // System.Comparison<byte>
    // System.Comparison<int>
    // System.Comparison<object>
    // System.EventHandler<object>
    // System.Func<System.Threading.Tasks.VoidTaskResult>
    // System.Func<int>
    // System.Func<object,System.Threading.Tasks.VoidTaskResult>
    // System.Func<object,byte>
    // System.Func<object,float>
    // System.Func<object,int>
    // System.Func<object,object,object>
    // System.Func<object,object>
    // System.Func<object>
    // System.IEquatable<object>
    // System.Linq.Buffer<object>
    // System.Linq.Enumerable.<IntersectIterator>d__74<BuffTag>
    // System.Linq.Enumerable.Iterator<object>
    // System.Linq.Enumerable.WhereArrayIterator<object>
    // System.Linq.Enumerable.WhereEnumerableIterator<object>
    // System.Linq.Enumerable.WhereListIterator<object>
    // System.Linq.EnumerableSorter<object,float>
    // System.Linq.EnumerableSorter<object>
    // System.Linq.OrderedEnumerable.<GetEnumerator>d__1<object>
    // System.Linq.OrderedEnumerable<object,float>
    // System.Linq.OrderedEnumerable<object>
    // System.Linq.Set<BuffTag>
    // System.Memory<byte>
    // System.Nullable<long>
    // System.Predicate<BT.BTResult>
    // System.Predicate<BuffTag>
    // System.Predicate<LitJson.PropertyMetadata>
    // System.Predicate<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Predicate<UnityEngine.Vector2>
    // System.Predicate<byte>
    // System.Predicate<int>
    // System.Predicate<object>
    // System.ReadOnlyMemory<byte>
    // System.ReadOnlySpan.Enumerator<byte>
    // System.ReadOnlySpan<byte>
    // System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>
    // System.Runtime.CompilerServices.AsyncTaskMethodBuilder<int>
    // System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
    // System.Runtime.CompilerServices.ConditionalWeakTable.CreateValueCallback<object,object>
    // System.Runtime.CompilerServices.ConditionalWeakTable.Enumerator<object,object>
    // System.Runtime.CompilerServices.ConditionalWeakTable<object,object>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.Threading.Tasks.VoidTaskResult>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<int>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.Threading.Tasks.VoidTaskResult>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<int>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<int>
    // System.Runtime.CompilerServices.TaskAwaiter<System.Threading.Tasks.VoidTaskResult>
    // System.Runtime.CompilerServices.TaskAwaiter<int>
    // System.Runtime.CompilerServices.TaskAwaiter<object>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<int>
    // System.Span<byte>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.Threading.Tasks.VoidTaskResult>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<int>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<object>
    // System.Threading.Tasks.Sources.IValueTaskSource<int>
    // System.Threading.Tasks.Task<System.Threading.Tasks.VoidTaskResult>
    // System.Threading.Tasks.Task<int>
    // System.Threading.Tasks.Task<object>
    // System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.Threading.Tasks.VoidTaskResult>
    // System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<int>
    // System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
    // System.Threading.Tasks.TaskFactory<System.Threading.Tasks.VoidTaskResult>
    // System.Threading.Tasks.TaskFactory<int>
    // System.Threading.Tasks.TaskFactory<object>
    // System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<int>
    // System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<int>
    // System.Threading.Tasks.ValueTask<int>
    // System.Tuple<object,object>
    // UnityEngine.Animations.Rigging.AnimationJobBinder<UnityEngine.Animations.Rigging.TwoBoneIKConstraintJob,UnityEngine.Animations.Rigging.TwoBoneIKConstraintData>
    // UnityEngine.Animations.Rigging.RigConstraint<UnityEngine.Animations.Rigging.TwoBoneIKConstraintJob,UnityEngine.Animations.Rigging.TwoBoneIKConstraintData,object>
    // UnityEngine.Events.InvokableCall<object>
    // UnityEngine.Events.UnityAction<object>
    // UnityEngine.Events.UnityEvent<object>
    // UnityEngine.InputSystem.InputBindingComposite<UnityEngine.Vector2>
    // UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>
    // UnityEngine.InputSystem.InputProcessor<UnityEngine.Vector2>
    // UnityEngine.InputSystem.Utilities.InlinedArray<object>
    // UnityEngine.Playables.ScriptPlayable<object>
    // }}

    public void RefMethods()
    {
        // object Google.Protobuf.ProtoPreconditions.CheckNotNull<object>(object,string)
        // !!0 System.Activator.CreateInstance<!!0>()
        // object System.Activator.CreateInstance<object>()
        // int System.Linq.Enumerable.Count<BuffTag>(System.Collections.Generic.IEnumerable<BuffTag>)
        // object System.Linq.Enumerable.FirstOrDefault<object>(System.Collections.Generic.IEnumerable<object>)
        // System.Collections.Generic.IEnumerable<BuffTag> System.Linq.Enumerable.Intersect<BuffTag>(System.Collections.Generic.IEnumerable<BuffTag>,System.Collections.Generic.IEnumerable<BuffTag>)
        // System.Collections.Generic.IEnumerable<BuffTag> System.Linq.Enumerable.IntersectIterator<BuffTag>(System.Collections.Generic.IEnumerable<BuffTag>,System.Collections.Generic.IEnumerable<BuffTag>,System.Collections.Generic.IEqualityComparer<BuffTag>)
        // System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,float>(System.Collections.Generic.IEnumerable<object>,System.Func<object,float>)
        // object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
        // System.Collections.Generic.List<BuffTag> System.Linq.Enumerable.ToList<BuffTag>(System.Collections.Generic.IEnumerable<BuffTag>)
        // System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
        // System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
        // System.Memory<byte> System.MemoryExtensions.AsMemory<byte>(byte[],int,int)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter,FileDownloader.<StreamCopy>d__27>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter&,FileDownloader.<StreamCopy>d__27&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>,FileDownloader.<StreamCopy>d__27>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>&,FileDownloader.<StreamCopy>d__27&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdater.<Init>d__10>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdater.<Init>d__10&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter,FileDownloader.<StreamCopy>d__27>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter&,FileDownloader.<StreamCopy>d__27&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>,FileDownloader.<StreamCopy>d__27>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>&,FileDownloader.<StreamCopy>d__27&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdater.<Init>d__10>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdater.<Init>d__10&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter,DataDownloader.<StreamCopy>d__26>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter&,DataDownloader.<StreamCopy>d__26&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>,DataDownloader.<StreamCopy>d__26>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>&,DataDownloader.<StreamCopy>d__26&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdater.<ReqUpdateInfo>d__20>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdater.<ReqUpdateInfo>d__20&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<FileDownloader.<StreamCopy>d__27>(FileDownloader.<StreamCopy>d__27&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdater.<Init>d__10>(HotUpdater.<Init>d__10&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<DataDownloader.<StreamCopy>d__26>(DataDownloader.<StreamCopy>d__26&)
        // System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<HotUpdater.<ReqUpdateInfo>d__20>(HotUpdater.<ReqUpdateInfo>d__20&)
        // System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>,DataDownloader.<StartDownload>d__24>(System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>&,DataDownloader.<StartDownload>d__24&)
        // System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>,FileDownloader.<StartDownload>d__26>(System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>&,FileDownloader.<StartDownload>d__26&)
        // System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,FileDownloader.<StartDownload>d__26>(System.Runtime.CompilerServices.TaskAwaiter&,FileDownloader.<StartDownload>d__26&)
        // System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdater.<Download>d__11>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdater.<Download>d__11&)
        // System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,FileDownloader.<StartDownload>d__26>(System.Runtime.CompilerServices.TaskAwaiter<object>&,FileDownloader.<StartDownload>d__26&)
        // System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<DataDownloader.<StartDownload>d__24>(DataDownloader.<StartDownload>d__24&)
        // System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<FileDownloader.<StartDownload>d__26>(FileDownloader.<StartDownload>d__26&)
        // System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotUpdater.<Download>d__11>(HotUpdater.<Download>d__11&)
        // object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
        // System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
        // object System.Threading.Interlocked.CompareExchange<object>(object&,object,object)
        // System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<UnityEngine.Vector2>(UnityEngine.Vector2&)
        // int Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<UnityEngine.Vector2>()
        // !!0 UnityEngine.AssetBundle.LoadAsset<!!0>(string)
        // UnityEngine.AssetBundleRequest UnityEngine.AssetBundle.LoadAssetAsync<!0>(string)
        // object UnityEngine.Component.GetComponent<object>()
        // object UnityEngine.Component.GetComponentInChildren<object>()
        // object UnityEngine.Component.GetComponentInParent<object>()
        // object[] UnityEngine.Component.GetComponentsInChildren<object>()
        // object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
        // object UnityEngine.GameObject.AddComponent<object>()
        // object UnityEngine.GameObject.GetComponent<object>()
        // object UnityEngine.GameObject.GetComponentInChildren<object>()
        // object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
        // object UnityEngine.GameObject.GetComponentInParent<object>()
        // object UnityEngine.GameObject.GetComponentInParent<object>(bool)
        // object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
        // UnityEngine.Vector2 UnityEngine.InputSystem.InputAction.CallbackContext.ReadValue<UnityEngine.Vector2>()
        // UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ApplyProcessors<UnityEngine.Vector2>(int,UnityEngine.Vector2,UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>)
        // UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ReadValue<UnityEngine.Vector2>(int,int,bool)
        // !!0 UnityEngine.Object.FindObjectOfType<!!0>()
        // object UnityEngine.Object.FindObjectOfType<object>()
        // object[] UnityEngine.Object.FindObjectsOfType<object>()
        // object UnityEngine.Object.Instantiate<object>(object)
        // object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
        // object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
        // object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion)
        // object[] UnityEngine.Resources.ConvertObjects<object>(UnityEngine.Object[])
        // object UnityEngine.Resources.Load<object>(string)
    }
}