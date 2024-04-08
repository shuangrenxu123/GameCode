using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"Animancer.dll",
		"Assembly-CSharp-firstpass.dll",
		"Google.Protobuf.dll",
		"System.Core.dll",
		"Unity.InputSystem.dll",
		"UnityEngine.AssetBundleModule.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Animancer.AnimancerTransition<object>
	// Animancer.ICopyable<object>
	// Animancer.MixerState<UnityEngine.Vector2>
	// Animancer.MixerState<float>
	// Google.Protobuf.IDeepCloneable<object>
	// Google.Protobuf.IMessage<object>
	// Google.Protobuf.MessageParser.<>c__DisplayClass2_0<object>
	// Google.Protobuf.MessageParser<object>
	// System.Action<!!0>
	// System.Action<!0>
	// System.Action<BT.BTResult>
	// System.Action<BuffTag>
	// System.Action<Contact>
	// System.Action<HitInfo>
	// System.Action<System.Net.Sockets.SocketError>
	// System.Action<Trigger>
	// System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>
	// System.Action<UnityEngine.RaycastHit>
	// System.Action<UnityEngine.Vector2>
	// System.Action<UnityEngine.Vector3,UnityEngine.Quaternion>
	// System.Action<UnityEngine.Vector3>
	// System.Action<byte>
	// System.Action<float>
	// System.Action<int,int>
	// System.Action<int>
	// System.Action<object,int>
	// System.Action<object,object>
	// System.Action<object>
	// System.ArraySegment.Enumerator<byte>
	// System.ArraySegment<byte>
	// System.Buffers.MemoryManager<byte>
	// System.ByReference<byte>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<object>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<object>
	// System.Collections.Concurrent.ConcurrentQueue<object>
	// System.Collections.Generic.ArraySortHelper<!!0>
	// System.Collections.Generic.ArraySortHelper<BT.BTResult>
	// System.Collections.Generic.ArraySortHelper<BuffTag>
	// System.Collections.Generic.ArraySortHelper<Contact>
	// System.Collections.Generic.ArraySortHelper<HitInfo>
	// System.Collections.Generic.ArraySortHelper<Trigger>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.RaycastHit>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector2>
	// System.Collections.Generic.ArraySortHelper<byte>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<!!0>
	// System.Collections.Generic.Comparer<BT.BTResult>
	// System.Collections.Generic.Comparer<BuffTag>
	// System.Collections.Generic.Comparer<Contact>
	// System.Collections.Generic.Comparer<HitInfo>
	// System.Collections.Generic.Comparer<Trigger>
	// System.Collections.Generic.Comparer<UnityEngine.RaycastHit>
	// System.Collections.Generic.Comparer<UnityEngine.Vector2>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<float>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<!!0,!!1>
	// System.Collections.Generic.Dictionary.Enumerator<Audio.AudioLayer,object>
	// System.Collections.Generic.Dictionary.Enumerator<Fight.ActionPointType,object>
	// System.Collections.Generic.Dictionary.Enumerator<HTN.WSProperties,byte>
	// System.Collections.Generic.Dictionary.Enumerator<HTN.WSProperties,float>
	// System.Collections.Generic.Dictionary.Enumerator<HTN.WSProperties,int>
	// System.Collections.Generic.Dictionary.Enumerator<HTN.WSProperties,object>
	// System.Collections.Generic.Dictionary.Enumerator<PropertySourceType,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,System.ValueTuple<object,int>>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<!!0,!!1>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<Audio.AudioLayer,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<Fight.ActionPointType,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HTN.WSProperties,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HTN.WSProperties,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HTN.WSProperties,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<HTN.WSProperties,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<PropertySourceType,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,System.ValueTuple<object,int>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<!!0,!!1>
	// System.Collections.Generic.Dictionary.KeyCollection<Audio.AudioLayer,object>
	// System.Collections.Generic.Dictionary.KeyCollection<Fight.ActionPointType,object>
	// System.Collections.Generic.Dictionary.KeyCollection<HTN.WSProperties,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<HTN.WSProperties,float>
	// System.Collections.Generic.Dictionary.KeyCollection<HTN.WSProperties,int>
	// System.Collections.Generic.Dictionary.KeyCollection<HTN.WSProperties,object>
	// System.Collections.Generic.Dictionary.KeyCollection<PropertySourceType,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,System.ValueTuple<object,int>>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<!!0,!!1>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<Audio.AudioLayer,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<Fight.ActionPointType,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HTN.WSProperties,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HTN.WSProperties,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HTN.WSProperties,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<HTN.WSProperties,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<PropertySourceType,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,System.ValueTuple<object,int>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<!!0,!!1>
	// System.Collections.Generic.Dictionary.ValueCollection<Audio.AudioLayer,object>
	// System.Collections.Generic.Dictionary.ValueCollection<Fight.ActionPointType,object>
	// System.Collections.Generic.Dictionary.ValueCollection<HTN.WSProperties,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<HTN.WSProperties,float>
	// System.Collections.Generic.Dictionary.ValueCollection<HTN.WSProperties,int>
	// System.Collections.Generic.Dictionary.ValueCollection<HTN.WSProperties,object>
	// System.Collections.Generic.Dictionary.ValueCollection<PropertySourceType,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,System.ValueTuple<object,int>>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<!!0,!!1>
	// System.Collections.Generic.Dictionary<Audio.AudioLayer,object>
	// System.Collections.Generic.Dictionary<Fight.ActionPointType,object>
	// System.Collections.Generic.Dictionary<HTN.WSProperties,byte>
	// System.Collections.Generic.Dictionary<HTN.WSProperties,float>
	// System.Collections.Generic.Dictionary<HTN.WSProperties,int>
	// System.Collections.Generic.Dictionary<HTN.WSProperties,object>
	// System.Collections.Generic.Dictionary<PropertySourceType,object>
	// System.Collections.Generic.Dictionary<int,System.ValueTuple<object,int>>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<!!0>
	// System.Collections.Generic.EqualityComparer<!!1>
	// System.Collections.Generic.EqualityComparer<Audio.AudioLayer>
	// System.Collections.Generic.EqualityComparer<BuffTag>
	// System.Collections.Generic.EqualityComparer<Fight.ActionPointType>
	// System.Collections.Generic.EqualityComparer<HTN.WSProperties>
	// System.Collections.Generic.EqualityComparer<PropertySourceType>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<object,int>>
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
	// System.Collections.Generic.ICollection<!!0>
	// System.Collections.Generic.ICollection<BT.BTResult>
	// System.Collections.Generic.ICollection<BuffTag>
	// System.Collections.Generic.ICollection<Contact>
	// System.Collections.Generic.ICollection<HitInfo>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<!!0,!!1>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<Audio.AudioLayer,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<Fight.ActionPointType,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HTN.WSProperties,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HTN.WSProperties,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HTN.WSProperties,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<HTN.WSProperties,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<PropertySourceType,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,System.ValueTuple<object,int>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<Trigger>
	// System.Collections.Generic.ICollection<UnityEngine.RaycastHit>
	// System.Collections.Generic.ICollection<UnityEngine.Vector2>
	// System.Collections.Generic.ICollection<byte>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<!!0>
	// System.Collections.Generic.IComparer<BT.BTResult>
	// System.Collections.Generic.IComparer<BuffTag>
	// System.Collections.Generic.IComparer<Contact>
	// System.Collections.Generic.IComparer<HitInfo>
	// System.Collections.Generic.IComparer<Trigger>
	// System.Collections.Generic.IComparer<UnityEngine.RaycastHit>
	// System.Collections.Generic.IComparer<UnityEngine.Vector2>
	// System.Collections.Generic.IComparer<byte>
	// System.Collections.Generic.IComparer<float>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IDictionary<object,LitJson.ArrayMetadata>
	// System.Collections.Generic.IDictionary<object,LitJson.ObjectMetadata>
	// System.Collections.Generic.IDictionary<object,LitJson.PropertyMetadata>
	// System.Collections.Generic.IDictionary<object,object>
	// System.Collections.Generic.IEnumerable<!!0>
	// System.Collections.Generic.IEnumerable<BT.BTResult>
	// System.Collections.Generic.IEnumerable<BuffTag>
	// System.Collections.Generic.IEnumerable<Contact>
	// System.Collections.Generic.IEnumerable<HitInfo>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<!!0,!!1>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Audio.AudioLayer,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Fight.ActionPointType,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HTN.WSProperties,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HTN.WSProperties,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HTN.WSProperties,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<HTN.WSProperties,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<PropertySourceType,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,System.ValueTuple<object,int>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<Trigger>
	// System.Collections.Generic.IEnumerable<UnityEngine.InputSystem.InputControlScheme>
	// System.Collections.Generic.IEnumerable<UnityEngine.RaycastHit>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerable<byte>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<!!0>
	// System.Collections.Generic.IEnumerator<BT.BTResult>
	// System.Collections.Generic.IEnumerator<BuffTag>
	// System.Collections.Generic.IEnumerator<Contact>
	// System.Collections.Generic.IEnumerator<HitInfo>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<!!0,!!1>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<Audio.AudioLayer,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<Fight.ActionPointType,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HTN.WSProperties,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HTN.WSProperties,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HTN.WSProperties,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<HTN.WSProperties,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<PropertySourceType,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,System.ValueTuple<object,int>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<Trigger>
	// System.Collections.Generic.IEnumerator<UnityEngine.InputSystem.InputControlScheme>
	// System.Collections.Generic.IEnumerator<UnityEngine.RaycastHit>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerator<byte>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<!!0>
	// System.Collections.Generic.IEqualityComparer<Audio.AudioLayer>
	// System.Collections.Generic.IEqualityComparer<BuffTag>
	// System.Collections.Generic.IEqualityComparer<Fight.ActionPointType>
	// System.Collections.Generic.IEqualityComparer<HTN.WSProperties>
	// System.Collections.Generic.IEqualityComparer<PropertySourceType>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<!!0>
	// System.Collections.Generic.IList<BT.BTResult>
	// System.Collections.Generic.IList<BuffTag>
	// System.Collections.Generic.IList<Contact>
	// System.Collections.Generic.IList<HitInfo>
	// System.Collections.Generic.IList<Trigger>
	// System.Collections.Generic.IList<UnityEngine.RaycastHit>
	// System.Collections.Generic.IList<UnityEngine.Vector2>
	// System.Collections.Generic.IList<byte>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<!!0,!!1>
	// System.Collections.Generic.KeyValuePair<Audio.AudioLayer,object>
	// System.Collections.Generic.KeyValuePair<Fight.ActionPointType,object>
	// System.Collections.Generic.KeyValuePair<HTN.WSProperties,byte>
	// System.Collections.Generic.KeyValuePair<HTN.WSProperties,float>
	// System.Collections.Generic.KeyValuePair<HTN.WSProperties,int>
	// System.Collections.Generic.KeyValuePair<HTN.WSProperties,object>
	// System.Collections.Generic.KeyValuePair<PropertySourceType,object>
	// System.Collections.Generic.KeyValuePair<int,System.ValueTuple<object,int>>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<!!0>
	// System.Collections.Generic.List.Enumerator<BT.BTResult>
	// System.Collections.Generic.List.Enumerator<BuffTag>
	// System.Collections.Generic.List.Enumerator<Contact>
	// System.Collections.Generic.List.Enumerator<HitInfo>
	// System.Collections.Generic.List.Enumerator<Trigger>
	// System.Collections.Generic.List.Enumerator<UnityEngine.RaycastHit>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector2>
	// System.Collections.Generic.List.Enumerator<byte>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<!!0>
	// System.Collections.Generic.List<BT.BTResult>
	// System.Collections.Generic.List<BuffTag>
	// System.Collections.Generic.List<Contact>
	// System.Collections.Generic.List<HitInfo>
	// System.Collections.Generic.List<Trigger>
	// System.Collections.Generic.List<UnityEngine.RaycastHit>
	// System.Collections.Generic.List<UnityEngine.Vector2>
	// System.Collections.Generic.List<byte>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<!!0>
	// System.Collections.Generic.ObjectComparer<BT.BTResult>
	// System.Collections.Generic.ObjectComparer<BuffTag>
	// System.Collections.Generic.ObjectComparer<Contact>
	// System.Collections.Generic.ObjectComparer<HitInfo>
	// System.Collections.Generic.ObjectComparer<Trigger>
	// System.Collections.Generic.ObjectComparer<UnityEngine.RaycastHit>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<float>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<!!0>
	// System.Collections.Generic.ObjectEqualityComparer<!!1>
	// System.Collections.Generic.ObjectEqualityComparer<Audio.AudioLayer>
	// System.Collections.Generic.ObjectEqualityComparer<BuffTag>
	// System.Collections.Generic.ObjectEqualityComparer<Fight.ActionPointType>
	// System.Collections.Generic.ObjectEqualityComparer<HTN.WSProperties>
	// System.Collections.Generic.ObjectEqualityComparer<PropertySourceType>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<object,int>>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<float>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<object>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<!!0>
	// System.Collections.ObjectModel.ReadOnlyCollection<BT.BTResult>
	// System.Collections.ObjectModel.ReadOnlyCollection<BuffTag>
	// System.Collections.ObjectModel.ReadOnlyCollection<Contact>
	// System.Collections.ObjectModel.ReadOnlyCollection<HitInfo>
	// System.Collections.ObjectModel.ReadOnlyCollection<Trigger>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.RaycastHit>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector2>
	// System.Collections.ObjectModel.ReadOnlyCollection<byte>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<!!0>
	// System.Comparison<BT.BTResult>
	// System.Comparison<BuffTag>
	// System.Comparison<Contact>
	// System.Comparison<HitInfo>
	// System.Comparison<Trigger>
	// System.Comparison<UnityEngine.RaycastHit>
	// System.Comparison<UnityEngine.Vector2>
	// System.Comparison<byte>
	// System.Comparison<int>
	// System.Comparison<object>
	// System.EventHandler<object>
	// System.Func<System.Threading.Tasks.VoidTaskResult>
	// System.Func<UnityEngine.InputSystem.InputControlScheme,byte>
	// System.Func<int,byte>
	// System.Func<int>
	// System.Func<object,System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,UnityEngine.Vector2>
	// System.Func<object,byte>
	// System.Func<object,float>
	// System.Func<object,int>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.IComparable<!!0>
	// System.IEquatable<Trigger>
	// System.IEquatable<object>
	// System.Linq.Buffer<object>
	// System.Linq.Enumerable.<IntersectIterator>d__74<BuffTag>
	// System.Linq.Enumerable.Iterator<int>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<int>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,int>
	// System.Linq.Enumerable.WhereSelectListIterator<object,int>
	// System.Linq.EnumerableSorter<object,float>
	// System.Linq.EnumerableSorter<object>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<object>
	// System.Linq.OrderedEnumerable<object,float>
	// System.Linq.OrderedEnumerable<object>
	// System.Linq.Set<BuffTag>
	// System.Memory<byte>
	// System.Nullable<UnityEngine.InputSystem.InputBinding>
	// System.Nullable<long>
	// System.Predicate<!!0>
	// System.Predicate<BT.BTResult>
	// System.Predicate<BuffTag>
	// System.Predicate<Contact>
	// System.Predicate<HitInfo>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Predicate<Trigger>
	// System.Predicate<UnityEngine.InputSystem.InputControlScheme>
	// System.Predicate<UnityEngine.RaycastHit>
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
	// System.ValueTuple<object,int>
	// UnityEngine.Events.InvokableCall<object>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityEvent<object>
	// UnityEngine.InputSystem.InputBindingComposite<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputBindingComposite<float>
	// UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputControl<float>
	// UnityEngine.InputSystem.InputProcessor<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputProcessor<float>
	// UnityEngine.InputSystem.Utilities.InlinedArray<object>
	// UnityEngine.InputSystem.Utilities.ReadOnlyArray.Enumerator<UnityEngine.InputSystem.InputControlScheme>
	// UnityEngine.InputSystem.Utilities.ReadOnlyArray.Enumerator<object>
	// UnityEngine.InputSystem.Utilities.ReadOnlyArray<UnityEngine.InputSystem.InputControlScheme>
	// UnityEngine.InputSystem.Utilities.ReadOnlyArray<object>
	// UnityEngine.Playables.ScriptPlayable<object>
	// }}

	public void RefMethods()
	{
		// object Google.Protobuf.ProtoPreconditions.CheckNotNull<object>(object,string)
		// !!0 LitJson.JsonMapper.ToObject<!!0>(string)
		// object LitJson.JsonMapper.ToObject<object>(string)
		// !!0 System.Activator.CreateInstance<!!0>()
		// object System.Activator.CreateInstance<object>()
		// int System.Linq.Enumerable.Count<BuffTag>(System.Collections.Generic.IEnumerable<BuffTag>)
		// UnityEngine.InputSystem.InputControlScheme System.Linq.Enumerable.First<UnityEngine.InputSystem.InputControlScheme>(System.Collections.Generic.IEnumerable<UnityEngine.InputSystem.InputControlScheme>,System.Func<UnityEngine.InputSystem.InputControlScheme,bool>)
		// object System.Linq.Enumerable.First<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// object System.Linq.Enumerable.FirstOrDefault<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<BuffTag> System.Linq.Enumerable.Intersect<BuffTag>(System.Collections.Generic.IEnumerable<BuffTag>,System.Collections.Generic.IEnumerable<BuffTag>)
		// System.Collections.Generic.IEnumerable<BuffTag> System.Linq.Enumerable.IntersectIterator<BuffTag>(System.Collections.Generic.IEnumerable<BuffTag>,System.Collections.Generic.IEnumerable<BuffTag>,System.Collections.Generic.IEqualityComparer<BuffTag>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,float>(System.Collections.Generic.IEnumerable<object>,System.Func<object,float>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// int System.Linq.Enumerable.Sum<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.List<BuffTag> System.Linq.Enumerable.ToList<BuffTag>(System.Collections.Generic.IEnumerable<BuffTag>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<object>.Select<int>(System.Func<object,int>)
		// System.Memory<byte> System.MemoryExtensions.AsMemory<byte>(byte[],int,int)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter,Downloader.FileDownloader.<StartDownload>d__27>(System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter&,Downloader.FileDownloader.<StartDownload>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>,Downloader.FileDownloader.<StartDownload>d__27>(System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>&,Downloader.FileDownloader.<StartDownload>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter,Downloader.FileDownloader.<StreamCopy>d__28>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter&,Downloader.FileDownloader.<StreamCopy>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>,Downloader.FileDownloader.<StreamCopy>d__28>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>&,Downloader.FileDownloader.<StreamCopy>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Downloader.FileDownloader.<StartDownload>d__27>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Downloader.FileDownloader.<StartDownload>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Downloader.HotUpdater.<Init>d__10>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Downloader.HotUpdater.<Init>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter,Downloader.FileDownloader.<StartDownload>d__27>(System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter&,Downloader.FileDownloader.<StartDownload>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>,Downloader.FileDownloader.<StartDownload>d__27>(System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>&,Downloader.FileDownloader.<StartDownload>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter,Downloader.FileDownloader.<StreamCopy>d__28>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter&,Downloader.FileDownloader.<StreamCopy>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>,Downloader.FileDownloader.<StreamCopy>d__28>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>&,Downloader.FileDownloader.<StreamCopy>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Downloader.FileDownloader.<StartDownload>d__27>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Downloader.FileDownloader.<StartDownload>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Downloader.HotUpdater.<Init>d__10>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Downloader.HotUpdater.<Init>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter,Downloader.DataDownloader.<StreamCopy>d__26>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter&,Downloader.DataDownloader.<StreamCopy>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>,Downloader.DataDownloader.<StreamCopy>d__26>(System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<int>&,Downloader.DataDownloader.<StreamCopy>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,Downloader.HotUpdater.<ReqUpdateInfo>d__20>(System.Runtime.CompilerServices.TaskAwaiter&,Downloader.HotUpdater.<ReqUpdateInfo>d__20&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Downloader.FileDownloader.<StartDownload>d__27>(Downloader.FileDownloader.<StartDownload>d__27&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Downloader.FileDownloader.<StreamCopy>d__28>(Downloader.FileDownloader.<StreamCopy>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Downloader.HotUpdater.<Init>d__10>(Downloader.HotUpdater.<Init>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<Downloader.DataDownloader.<StreamCopy>d__26>(Downloader.DataDownloader.<StreamCopy>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<Downloader.HotUpdater.<ReqUpdateInfo>d__20>(Downloader.HotUpdater.<ReqUpdateInfo>d__20&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>,Downloader.DataDownloader.<StartDownload>d__24>(System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>&,Downloader.DataDownloader.<StartDownload>d__24&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,Downloader.FileDownloader.<StartDownload>d__26>(System.Runtime.CompilerServices.TaskAwaiter&,Downloader.FileDownloader.<StartDownload>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,Downloader.HotUpdater.<Download>d__11>(System.Runtime.CompilerServices.TaskAwaiter&,Downloader.HotUpdater.<Download>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Downloader.DataDownloader.<StartDownload>d__24>(Downloader.DataDownloader.<StartDownload>d__24&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Downloader.FileDownloader.<StartDownload>d__26>(Downloader.FileDownloader.<StartDownload>d__26&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Downloader.HotUpdater.<Download>d__11>(Downloader.HotUpdater.<Download>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Downloader.HotUpdater.<DownloadedCallBack>d__13>(Downloader.HotUpdater.<DownloadedCallBack>d__13&)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// object System.Threading.Interlocked.CompareExchange<object>(object&,object,object)
		// System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<UnityEngine.Vector2>(UnityEngine.Vector2&)
		// System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<float>(float&)
		// int Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<UnityEngine.Vector2>()
		// int Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<float>()
		// !!0 UnityEngine.AssetBundle.LoadAsset<!!0>(string)
		// object UnityEngine.AssetBundle.LoadAsset<object>(string)
		// UnityEngine.AssetBundleRequest UnityEngine.AssetBundle.LoadAssetAsync<!0>(string)
		// !!1 UnityEngine.Component.GetComponent<!!1>()
		// object UnityEngine.Component.GetComponent<object>()
		// !!0 UnityEngine.Component.GetComponentInChildren<!!0>(bool)
		// !!1 UnityEngine.Component.GetComponentInChildren<!!1>(bool)
		// object UnityEngine.Component.GetComponentInChildren<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>(bool)
		// object UnityEngine.Component.GetComponentInParent<object>()
		// !!0[] UnityEngine.Component.GetComponentsInChildren<!!0>(bool)
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.Component.TryGetComponent<!!0>(!!0&)
		// bool UnityEngine.Component.TryGetComponent<!!1>(!!1&)
		// bool UnityEngine.Component.TryGetComponent<object>(object&)
		// !!0 UnityEngine.GameObject.AddComponent<!!0>()
		// !!1 UnityEngine.GameObject.AddComponent<!!1>()
		// object UnityEngine.GameObject.AddComponent<object>()
		// !!0 UnityEngine.GameObject.GetComponent<!!0>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// !!0 UnityEngine.GameObject.GetComponentInChildren<!!0>()
		// !!0 UnityEngine.GameObject.GetComponentInChildren<!!0>(bool)
		// object UnityEngine.GameObject.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object UnityEngine.GameObject.GetComponentInParent<object>()
		// object UnityEngine.GameObject.GetComponentInParent<object>(bool)
		// !!0[] UnityEngine.GameObject.GetComponentsInChildren<!!0>(bool)
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>()
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.GameObject.TryGetComponent<!!0>(!!0&)
		// bool UnityEngine.GameObject.TryGetComponent<!!1>(!!1&)
		// bool UnityEngine.GameObject.TryGetComponent<!!2>(!!2&)
		// bool UnityEngine.GameObject.TryGetComponent<object>(object&)
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputAction.ReadValue<UnityEngine.Vector2>()
		// float UnityEngine.InputSystem.InputAction.ReadValue<float>()
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ApplyProcessors<UnityEngine.Vector2>(int,UnityEngine.Vector2,UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>)
		// float UnityEngine.InputSystem.InputActionState.ApplyProcessors<float>(int,float,UnityEngine.InputSystem.InputControl<float>)
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ReadValue<UnityEngine.Vector2>(int,int,bool)
		// float UnityEngine.InputSystem.InputActionState.ReadValue<float>(int,int,bool)
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion)
		// object UnityEngine.Resources.Load<object>(string)
	}
}