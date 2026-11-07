using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using Genkit;
using XRL.World;

namespace XRL.Collections;

[Serializable]
public class EventRegistry : IComposite, IDisposable
{
	public class List : PooledContainer<List.Entry>
	{
		public struct Entry(IEventHandler Handler, int Order = 0, bool Serialize = false)
		{
			public IEventHandler Handler = Handler;

			public int Order = Order;

			public bool Serialize = Serialize;
		}

		protected static readonly ConcurrentBag<List> Bag = new ConcurrentBag<List>();

		protected EventRegistry Registry;

		public int EventID;

		protected int Serialized;

		public bool Serialize => Serialized != 0;

		public Type EventType => MinEvent.ResolveEvent(EventID);

		public static List Get(EventRegistry Registry, int EventID = 0)
		{
			if (!Bag.TryTake(out var result))
			{
				result = new List();
			}
			result.Registry = Registry;
			result.EventID = EventID;
			result.Size = 0;
			return result;
		}

		public static void Return(List List)
		{
			if (List.Registry != null)
			{
				List.Dispose();
			}
			else if (List.Size != -1)
			{
				List.Size = -1;
				Bag.Add(List);
			}
		}

		public List()
		{
		}

		public List(EventRegistry Registry, int Capacity = 0, int EventID = 0)
		{
			this.Registry = Registry;
			this.EventID = EventID;
			EnsureCapacity(Capacity);
		}

		public void Require(IEventHandler Handler, int Order = 0, bool Serialize = false)
		{
			int num = IndexOf(Handler);
			if (num == -1)
			{
				Add(new Entry(Handler, Order, Serialize));
				return;
			}
			if (Items[num].Serialize != Serialize)
			{
				Items[num].Serialize = Serialize;
				if (Serialize)
				{
					if (Serialized++ == 0)
					{
						Registry.Serialized++;
					}
				}
				else if (Serialized-- == 1)
				{
					Registry.Serialized--;
				}
			}
			if (Items[num].Order == Order)
			{
				return;
			}
			int num2 = num;
			if (num2 != 0 && Items[num2 - 1].Order > Order)
			{
				while (--num2 > 0 && Items[num2 - 1].Order > Order)
				{
				}
				Shift(num, num2);
			}
			else if (num2 != Length - 1 && Items[num2 + 1].Order <= Order)
			{
				while (++num2 < Length - 1 && Items[num2 + 1].Order <= Order)
				{
				}
				Shift(num, num2);
			}
			Items[num2].Order = Order;
		}

		public void Shift(int From, int To)
		{
			Entry entry = Items[From];
			if (From > To)
			{
				Array.Copy(Items, To, Items, To + 1, From - To);
			}
			else
			{
				Array.Copy(Items, From + 1, Items, From, To - From);
			}
			Items[To] = entry;
			Version++;
		}

		public void Add(IEventHandler Handler, int Order = 0, bool Serialize = false)
		{
			Add(new Entry(Handler, Order, Serialize));
		}

		public void Add(Entry Item)
		{
			if (Size < 0)
			{
				LogDiagnostics(Item);
				Resize(DefaultCapacity);
			}
			else if (Length >= Size)
			{
				Resize(Length * 2);
			}
			int num = InsertOrder(Item.Order);
			try
			{
				if (num < Length)
				{
					Array.Copy(Items, num, Items, num + 1, Length - num);
				}
				if (Item.Serialize && Serialized++ == 0)
				{
					Registry.Serialized++;
				}
				Items[num] = Item;
				Length++;
				Version++;
			}
			catch (Exception exception)
			{
				LogDiagnostics(Item, num, exception);
			}
		}

		private void LogDiagnostics(Entry Item, int Index = -1, Exception Exception = null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Registry.Add");
			stringBuilder.Append("\nIndex: ").Append(Index);
			stringBuilder.Append("\nOrder: ").Append(Item.Order);
			StringBuilder stringBuilder2 = stringBuilder.Append("\nHandler: ");
			IEventHandler handler = Item.Handler;
			stringBuilder2.Append(((handler != null) ? handler.GetType().GetName() : null) ?? "null");
			stringBuilder.Append("\nBasis: ").Append((Item.Handler as IComponent<GameObject>)?.GetBasisGameObject()?.DebugName ?? "none");
			stringBuilder.Append("\nLength: ").Append(Length);
			stringBuilder.Append("\nEventType: ").Append(EventType);
			stringBuilder.Append("\nSize: ").Append(Size);
			stringBuilder.Append("\nArrLen: ").Append(Items.Length);
			stringBuilder.Append("\nArray:");
			for (int i = 0; i < Items.Length; i++)
			{
				Entry entry = Items[i];
				stringBuilder.Append("\n  [").Append(i).Append("] ");
				IEventHandler handler2 = entry.Handler;
				stringBuilder.Append(((handler2 != null) ? handler2.GetType().GetName() : null) ?? "null");
				stringBuilder.Append(", ").Append(entry.Order);
				stringBuilder.Append(", ").Append(entry.Serialize);
			}
			stringBuilder.Append(":\n");
			if (Exception != null)
			{
				MetricsManager.LogException(stringBuilder.ToString(), Exception);
			}
			else
			{
				MetricsManager.LogError(stringBuilder.ToString());
			}
		}

		public bool Remove(IEventHandler Handler)
		{
			int num = IndexOf(Handler);
			if (num != -1)
			{
				RemoveAt(num);
				return true;
			}
			return false;
		}

		public void RemoveAt(int Index)
		{
			if (Index >= Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (Items[Index].Serialize && Serialized-- == 1)
			{
				Registry.Serialized--;
			}
			Length--;
			if (Index < Length)
			{
				Array.Copy(Items, Index + 1, Items, Index, Length - Index);
			}
			Items[Length] = default;
			Version++;
		}

		public int IndexOf(IEventHandler Handler)
		{
			for (int i = 0; i < Length; i++)
			{
				if (Items[i].Handler == Handler)
				{
					return i;
				}
			}
			return -1;
		}

		protected int InsertOrder(int Order)
		{
			if (Length == 0)
			{
				return 0;
			}
			int num;
			if (Order < 0)
			{
				num = 0;
				while (Items[num].Order < Order && ++num < Length)
				{
				}
			}
			else
			{
				num = Length;
				while (Items[num - 1].Order > Order && --num > 0)
				{
				}
			}
			return num;
		}

		public bool Dispatch(MinEvent E)
		{
			int length = Length;
			for (int i = 0; i < length; i++)
			{
				IEventHandler handler = Items[i].Handler;
				if (!E.Dispatch(handler))
				{
					return false;
				}
				if (length != Length)
				{
					length = Length;
					if (i < length && Items[i].Handler != handler)
					{
						i--;
					}
				}
			}
			return true;
		}

		public bool DispatchRange(MinEvent E, int Min = int.MinValue, int Max = int.MaxValue)
		{
			int length = Length;
			for (int i = 0; i < length; i++)
			{
				if (Items[i].Order < Min || Items[i].Order > Max)
				{
					continue;
				}
				IEventHandler handler = Items[i].Handler;
				if (!E.Dispatch(handler))
				{
					return false;
				}
				if (length != Length)
				{
					length = Length;
					if (i < length && Items[i].Handler != handler)
					{
						i--;
					}
				}
			}
			return true;
		}

		public override void Write(SerializationWriter Writer)
		{
			Writer.WriteOptimized(Serialized);
			Writer.WriteOptimized(EventID);
			for (int i = 0; i < Length; i++)
			{
				if (Items[i].Serialize)
				{
					IEventBinder binder = Items[i].Handler.Binder;
					Writer.WriteTokenized(binder);
					binder.WriteBind(Writer, Items[i].Handler, EventID);
					Writer.WriteOptimized(Items[i].Order);
				}
			}
		}

		public override void Read(SerializationReader Reader)
		{
			if (Registry == null)
			{
				throw new InvalidOperationException("Event handler list does not have a parent registry to track serialized registrations.");
			}
			int num = Reader.ReadOptimizedInt32();
			EventID = Reader.ReadOptimizedInt32();
			Items = Pool.Rent(num);
			Size = Items.Length;
			for (int i = 0; i < num; i++)
			{
				IEventHandler eventHandler = ((IEventBinder)Reader.ReadTokenized()).ReadBind(Reader, EventID);
				int order = Reader.ReadOptimizedInt32();
				if (eventHandler == null)
				{
					num--;
					i--;
				}
				else
				{
					Items[i].Handler = eventHandler;
					Items[i].Order = order;
					Items[i].Serialize = true;
				}
			}
			Length = (Serialized = num);
			if (num > 0)
			{
				Registry.Serialized++;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entry[] GetArray()
		{
			return Items;
		}

		/// <summary>Clean up invalid handlers whose game objects have been pooled or otherwise gone out of scope.</summary>
		/// <remarks>A deeper cleansing happens as part of a serialization cycle.</remarks>
		public int Clean()
		{
			int num = 0;
			for (int num2 = Length - 1; num2 >= 0; num2--)
			{
				if (!Items[num2].Handler.IsValid)
				{
					RemoveAt(num2);
					num++;
				}
			}
			return num;
		}

		public override void Dispose()
		{
			base.Dispose();
			Registry = null;
			EventID = 0;
			Serialized = 0;
			Return(this);
		}
	}

	protected struct Slot
	{
		public int ID;

		public int Next;

		public List Value;
	}

	protected static readonly ConcurrentBag<EventRegistry> RegistryPool = new ConcurrentBag<EventRegistry>();

	protected static readonly ArrayPool<int> BucketPool = ArrayPool<int>.Shared;

	protected static readonly ArrayPool<Slot> SlotPool = ArrayPool<Slot>.Shared;

	protected int[] Buckets = Array.Empty<int>();

	protected Slot[] Slots = Array.Empty<Slot>();

	protected int Size;

	protected int Length;

	protected int Amount;

	protected int Next = -1;

	protected int Serialized;

	public int Capacity => Size;

	public int Count => Amount;

	public bool Serialize => Serialized != 0;

	public bool WantFieldReflection => false;

	public List this[MinEvent E]
	{
		get
		{
			int num = IndexOf(E.ID);
			if (num < 0)
			{
				return null;
			}
			return Slots[num].Value;
		}
	}

	public List this[int ID]
	{
		get
		{
			int num = IndexOf(ID);
			if (num < 0)
			{
				return null;
			}
			return Slots[num].Value;
		}
	}

	public static EventRegistry Get()
	{
		if (!RegistryPool.TryTake(out var result))
		{
			result = new EventRegistry();
		}
		result.Size = 0;
		return result;
	}

	public static void Return(EventRegistry Registry)
	{
		if (Registry.Size == 0)
		{
			Registry.Size = -1;
			RegistryPool.Add(Registry);
		}
		else if (Registry.Size > 0)
		{
			Registry.Dispose();
		}
	}

	public EventRegistry()
	{
	}

	public EventRegistry(int Capacity)
	{
		if (Capacity > 0)
		{
			Capacity = Hash.GetSharedCapacity(Capacity);
			Buckets = BucketPool.Rent(Capacity);
			Slots = SlotPool.Rent(Capacity);
			Size = Capacity;
		}
	}

	/// <summary>Register a handler to receive a <see cref="T:XRL.World.MinEvent" />.</summary>
	/// <param name="Handler">An event handler such as <see cref="T:XRL.World.IPart" /> or <see cref="T:XRL.World.Effect" />.</param>
	/// <param name="EventID">The ID of a <see cref="T:XRL.World.MinEvent" />.</param>
	/// <param name="Order">
	/// The handling order of the event. A negative order will execute before standard event cascading,
	/// while a positive order will execute after.
	/// </param>
	/// <param name="Serialize">
	/// Persist event binding through serialization.
	/// If this is false, registration is transient and will have to be re-executed after game load.
	/// </param>
	public void Register(IEventHandler Handler, int ID, int Order = 0, bool Serialize = false)
	{
		int num = IndexOf(ID);
		if (num >= 0)
		{
			Slots[num].Value.Require(Handler, Order, Serialize);
			return;
		}
		List list = List.Get(this, ID);
		list.Add(Handler, Order, Serialize);
		Insert(ID, list);
	}

	/// <summary>Unregister a handler from receiving a <see cref="T:XRL.World.MinEvent" />.</summary>
	/// <param name="Handler">An event handler such as <see cref="T:XRL.World.IPart" /> or <see cref="T:XRL.World.Effect" />.</param>
	/// <param name="EventID">The ID of a <see cref="T:XRL.World.MinEvent" />.</param>
	/// <returns><c>true</c> if a matching handler was unregistered; otherwise <c>false</c>.</returns>
	public bool Unregister(IEventHandler Handler, int ID)
	{
		int num = IndexOf(ID);
		if (num == -1)
		{
			return false;
		}
		List value = Slots[num].Value;
		int num2 = value.IndexOf(Handler);
		if (num2 == -1)
		{
			return false;
		}
		if (num2 == 0 && value.Count == 1)
		{
			Remove(ID);
		}
		else
		{
			value.RemoveAt(num2);
		}
		return true;
	}

	/// <summary>Unregister a handler from receiving any MinEvent.</summary>
	/// <param name="Handler">An event handler such as <see cref="T:XRL.World.IPart" /> or <see cref="T:XRL.World.Effect" />.</param>
	/// <returns><c>true</c> if a matching handler was unregistered; otherwise <c>false</c>.</returns>
	/// <remarks>This has to linearly iterate all handlers within the collection, specifying ID is preferred.</remarks>
	public bool Unregister(IEventHandler Handler)
	{
		bool result = false;
		for (int i = 0; i < Length; i++)
		{
			List value = Slots[i].Value;
			if (value == null)
			{
				continue;
			}
			int num = value.IndexOf(Handler);
			if (num != -1)
			{
				if (num == 0 && value.Count == 1)
				{
					Remove(Slots[i].ID);
				}
				else
				{
					value.RemoveAt(num);
				}
				result = true;
			}
		}
		return result;
	}

	public void Clear()
	{
		if (Length > 0)
		{
			for (int i = 0; i < Length && i < Slots.Length; i++)
			{
				Slots[i].Value?.Dispose();
				Slots[i] = default;
			}
			for (int j = 0; j < Size; j++)
			{
				Buckets[j] = 0;
			}
			Next = -1;
			Length = 0;
			Amount = 0;
			Serialized = 0;
		}
	}

	public bool ContainsKey(int ID)
	{
		return IndexOf(ID) >= 0;
	}

	public bool TryGetValue(int ID, out List Handlers)
	{
		int num = IndexOf(ID);
		if (num >= 0)
		{
			Handlers = Slots[num].Value;
			return true;
		}
		Handlers = null;
		return false;
	}

	public int GetRegisteredCount(int ID)
	{
		int num = IndexOf(ID);
		if (num != -1)
		{
			return Slots[num].Value.Count;
		}
		return 0;
	}

	protected int IndexOf(int ID)
	{
		if (Size > 0)
		{
			uint num = (uint)ID % (uint)Size;
			for (int num2 = Buckets[num] - 1; num2 >= 0; num2 = Slots[num2].Next)
			{
				if (Slots[num2].ID == ID)
				{
					return num2;
				}
			}
		}
		return -1;
	}

	protected bool Insert(int ID, List Value, bool ReturnOnDuplicate = false, bool ThrowOnDuplicate = false)
	{
		uint num = 0u;
		int num2 = -1;
		if (Size > 0)
		{
			num = (uint)ID % (uint)Size;
			for (num2 = Buckets[num] - 1; num2 >= 0; num2 = Slots[num2].Next)
			{
				if (Slots[num2].ID == ID)
				{
					if (ThrowOnDuplicate)
					{
						throw new ArgumentException($"Event by ID '{ID}' already registered.");
					}
					if (ReturnOnDuplicate)
					{
						return true;
					}
					Slots[num2].Value = Value;
					return true;
				}
			}
		}
		if (Next >= 0)
		{
			Next = Slots[num2 = Next].Next;
		}
		else
		{
			if (Length == Size)
			{
				Resize(Length * 2);
				num = (uint)ID % (uint)Size;
			}
			num2 = Length++;
		}
		Slots[num2].ID = ID;
		Slots[num2].Next = Buckets[num] - 1;
		Slots[num2].Value = Value;
		Buckets[num] = num2 + 1;
		Amount++;
		return true;
	}

	protected void Resize(int Capacity)
	{
		Capacity = Hash.GetSharedCapacity(Capacity);
		if (Capacity == Size)
		{
			return;
		}
		int[] array = BucketPool.Rent(Capacity);
		Slot[] array2 = SlotPool.Rent(Capacity);
		for (int i = 0; i < Length; i++)
		{
			array2[i] = Slots[i];
			Slots[i] = default;
			if (array2[i].ID != 0)
			{
				uint num = (uint)array2[i].ID % (uint)Capacity;
				array2[i].Next = array[num] - 1;
				array[num] = i + 1;
			}
		}
		BucketPool.Return(Buckets, clearArray: true);
		SlotPool.Return(Slots);
		Buckets = array;
		Slots = array2;
		Size = Capacity;
	}

	public bool Remove(int ID)
	{
		if (Size == 0)
		{
			return false;
		}
		uint num = (uint)ID % (uint)Size;
		int num2 = Buckets[num] - 1;
		int num3 = -1;
		while (num2 >= 0)
		{
			if (Slots[num2].ID == ID)
			{
				if (num3 == -1)
				{
					Buckets[num] = Slots[num2].Next + 1;
				}
				else
				{
					Slots[num3].Next = Slots[num2].Next;
				}
				if (Slots[num2].Value.Serialize)
				{
					Serialized--;
				}
				Slots[num2].Next = Next;
				Slots[num2].ID = 0;
				Slots[num2].Value.Dispose();
				Slots[num2].Value = null;
				Next = num2;
				Amount--;
				return true;
			}
			num3 = num2;
			num2 = Slots[num2].Next;
		}
		return false;
	}

	public bool Dispatch(MinEvent E)
	{
		int num = IndexOf(E.ID);
		if (num != -1)
		{
			return Slots[num].Value.Dispatch(E);
		}
		return true;
	}

	public bool DispatchRange(MinEvent E, int Min = int.MinValue, int Max = int.MaxValue)
	{
		int num = IndexOf(E.ID);
		if (num != -1)
		{
			return Slots[num].Value.DispatchRange(E, Min, Max);
		}
		return true;
	}

	public static void Write(SerializationWriter Writer, EventRegistry Registry)
	{
		if (Registry == null || Registry.Serialized == 0)
		{
			Writer.WriteOptimized(0);
			return;
		}
		Writer.WriteOptimized(Registry.Serialized);
		for (int i = 0; i < Registry.Length; i++)
		{
			if (Registry.Slots[i].ID != 0 && Registry.Slots[i].Value.Serialize)
			{
				Registry.Slots[i].Value.Write(Writer);
			}
		}
	}

	public void Write(SerializationWriter Writer)
	{
		Write(Writer, this);
	}

	public static void Read(SerializationReader Reader, ref EventRegistry Registry)
	{
		int num = Reader.ReadOptimizedInt32();
		if (num <= 0)
		{
			return;
		}
		if (Registry == null)
		{
			Registry = Get();
		}
		else if (Registry.Size < num)
		{
			Registry.Resize(num);
		}
		for (int i = 0; i < num; i++)
		{
			List list = List.Get(Registry);
			list.Read(Reader);
			if (list.Count == 0)
			{
				list.Dispose();
			}
			else
			{
				Registry.Insert(list.EventID, list);
			}
		}
	}

	public void Read(SerializationReader Reader)
	{
		EventRegistry Registry = this;
		Read(Reader, ref Registry);
	}

	/// <inheritdoc cref="M:XRL.Collections.EventRegistry.List.Clean" />
	public int Clean()
	{
		int num = 0;
		for (int num2 = Length - 1; num2 >= 0; num2--)
		{
			if (Slots[num2].ID != 0)
			{
				int num3 = Slots[num2].Value.Clean();
				if (num3 > 0)
				{
					num += num3;
					if (Slots[num2].Value.Count == 0)
					{
						Remove(Slots[num2].ID);
					}
				}
			}
		}
		return num;
	}

	/// <summary>Return this registry and its rented arrays to their respective pools.</summary>
	public virtual void Dispose()
	{
		if (Size > 0)
		{
			Clear();
			Size = 0;
			SlotPool.Return(ref Slots);
			BucketPool.Return(ref Buckets);
			Return(this);
		}
	}

	~EventRegistry()
	{
		Dispose();
	}
}
