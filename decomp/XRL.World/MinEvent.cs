using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Genkit;
using UnityEngine;

namespace XRL.World;

public abstract class MinEvent : IEvent
{
	public enum Cache
	{
		None,
		Pool,
		Singleton
	}

	public delegate void EventPoolReset();

	public delegate int EventPoolCount();

	public const int CASCADE_NONE = 0;

	/// <summary>Allow cascade to equipped objects.</summary>
	public const int CASCADE_EQUIPMENT = 1;

	/// <summary>Allow cascade to inventory objects.</summary>
	public const int CASCADE_INVENTORY = 2;

	/// <summary>Allow cascade to slotted game objects, e.g. socketed energy cells.</summary>
	public const int CASCADE_SLOTS = 4;

	/// <summary>Allow cascade to component game objects, e.g. mine explosives.</summary>
	public const int CASCADE_COMPONENTS = 8;

	/// <summary>Prevent cascade to equipped thrown weapon.</summary>
	public const int CASCADE_EXCEPT_THROWN_WEAPON = 16;

	/// <summary>Prevent cascade to zone cells and their contents.</summary>
	public const int CASCADE_STOP_AT_ZONE = 32;

	/// <summary>Prevent cascade beyond registry.</summary>
	public const int CASCADE_STOP_AT_REGISTRY = 64;

	/// <summary>Allow cascade on world map.</summary>
	public const int CASCADE_WORLD_MAP_INVENTORY = 128;

	/// <summary>Event is presumed desired by game objects and should skip to dispatch.</summary>
	public const int CASCADE_DESIRED_OBJECT = 256;

	/// <summary>Event is presumed desired by game systems and should skip to dispatch.</summary>
	public const int CASCADE_DESIRED_SYSTEM = 512;

	public const int CASCADE_ALL = 15;

	public static readonly int CascadeLevel = 0;

	public int ID;

	public bool InterfaceExit;

	public static Dictionary<int, Type> EventTypes = new Dictionary<int, Type>();

	public static bool SuppressThreadWarning = false;

	private static List<EventPoolReset> EventPoolResets = new List<EventPoolReset>();

	private static Dictionary<string, EventPoolCount> EventPoolCounts = new Dictionary<string, EventPoolCount>();

	private static bool Initialized;

	public static bool UIHold = false;

	public int GetID()
	{
		return ID;
	}

	public static void ResetEvents()
	{
		if (EventTypes.IsNullOrEmpty())
		{
			return;
		}
		EventTypes.Clear();
		EventPoolResets.Clear();
		EventPoolCounts.Clear();
		foreach (Type item in ModManager.GetTypesAssignableFrom(typeof(MinEvent), Cache: false))
		{
			if (item.IsAbstract)
			{
				continue;
			}
			Type type = item;
			do
			{
				try
				{
					int count = EventTypes.Count;
					RuntimeHelpers.RunClassConstructor(type.TypeHandle);
					if (EventTypes.Count == count)
					{
						type.TypeInitializer?.Invoke(null, null);
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("MinEventReset:" + item.FullName, x);
				}
				type = type.BaseType;
			}
			while ((object)type != null && (object)type != typeof(MinEvent));
		}
	}

	public static void InitializeEvents()
	{
		if (Initialized)
		{
			return;
		}
		Initialized = true;
		foreach (Type item in ModManager.GetTypesAssignableFrom(typeof(MinEvent), Cache: false))
		{
			if (item.IsAbstract)
			{
				continue;
			}
			Type type = item;
			do
			{
				try
				{
					RuntimeHelpers.RunClassConstructor(type.TypeHandle);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("MinEventInit:" + item.FullName, x);
				}
				type = type.BaseType;
			}
			while ((object)type != null && (object)type != typeof(MinEvent));
		}
	}

	public static Type ResolveEvent(int ID)
	{
		InitializeEvents();
		return EventTypes.GetValue(ID);
	}

	public static int RegisterEvent<T>(string Seed = null, EventPoolCount Count = null, EventPoolReset Reset = null)
	{
		return RegisterEvent(typeof(T), Seed, Count, Reset);
	}

	public static int RegisterEvent(Type Event, string Seed = null, EventPoolCount Count = null, EventPoolReset Reset = null)
	{
		string fullName = Event.FullName;
		int num = (int)Hash.FNV1A32(Seed ?? fullName);
		if (num == 0)
		{
			throw new ArgumentException("ID hash is zero.");
		}
		if (!EventTypes.TryAdd(num, Event))
		{
			Type type = EventTypes[num];
			if (!(type == Event))
			{
				throw new ArgumentException("ID hash conflict between min events " + fullName + " and " + type.Name + ".");
			}
			Debug.LogError("Duplicate min event registration for " + fullName + ".");
		}
		if (Count != null && !EventPoolCounts.TryAdd(fullName, Count))
		{
			Debug.LogError("Duplicate pool retrieval registration for " + fullName + ".");
		}
		if (Reset != null)
		{
			EventPoolResets.Add(Reset);
		}
		return num;
	}

	public static void ResetPools()
	{
		if (EventPoolResets != null)
		{
			for (int i = 0; i < EventPoolResets.Count; i++)
			{
				EventPoolResets[i]();
			}
		}
	}

	protected static void ResetTo<T>(T Event, List<T> Pool, ref int Counter) where T : MinEvent
	{
		if (Pool == null)
		{
			return;
		}
		while (Counter > 0)
		{
			T val = Pool[--Counter];
			val.Reset();
			if (Event == val)
			{
				break;
			}
		}
	}

	protected static T FromPool<T>(ref List<T> Pool, ref int Counter, int MaxPool = 8192) where T : MinEvent, new()
	{
		if (Pool == null)
		{
			Pool = new List<T>();
		}
		if (Counter >= Pool.Count)
		{
			if (Pool.Count >= MaxPool)
			{
				return new T();
			}
			int num = Counter - Pool.Count + 1;
			for (int i = 0; i < num; i++)
			{
				Pool.Add(new T());
			}
		}
		return Pool[Counter++];
	}

	protected static T FromPool<T>(List<T> Pool, ref int Counter, int MaxPool = 8192) where T : MinEvent, new()
	{
		if (Counter >= Pool.Count)
		{
			if (Pool.Count >= MaxPool)
			{
				return new T();
			}
			int num = Counter - Pool.Count + 1;
			for (int i = 0; i < num; i++)
			{
				Pool.Add(new T());
			}
		}
		return Pool[Counter++];
	}

	public virtual void RequestInterfaceExit()
	{
		InterfaceExit = true;
	}

	public bool InterfaceExitRequested()
	{
		return InterfaceExit;
	}

	public virtual void PreprocessChildEvent(IEvent E)
	{
	}

	public virtual void ProcessChildEvent(IEvent E)
	{
		if (E.InterfaceExitRequested())
		{
			InterfaceExit = true;
		}
	}

	public virtual void Reset()
	{
		InterfaceExit = false;
	}

	[Obsolete("Use ModSingletonEvent/ModPooledEvent and IModEventHandler.")]
	public virtual bool WantInvokeDispatch()
	{
		return false;
	}

	public virtual int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool CascadeTo(int Cascade, int Level)
	{
		return (Cascade & Level) != 0;
	}

	public bool CascadeTo(int Level)
	{
		return CascadeTo(GetCascadeLevel(), Level);
	}

	public static string GetTopPoolCountReport(int num = 20)
	{
		List<string> list = new List<string>(EventPoolCounts.Count);
		Dictionary<string, int> Counts = new Dictionary<string, int>(EventPoolCounts.Count);
		foreach (KeyValuePair<string, EventPoolCount> eventPoolCount in EventPoolCounts)
		{
			list.Add(eventPoolCount.Key);
			Counts.Add(eventPoolCount.Key, eventPoolCount.Value());
		}
		list.Sort((string a, string b) =>
		{
			int num4 = Counts[b].CompareTo(Counts[a]);
			return (num4 != 0) ? num4 : a.CompareTo(b);
		});
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int num2 = 0;
		for (int num3 = Math.Min(list.Count, num + 1); num2 < num3; num2++)
		{
			stringBuilder.Append(list[num2]).Append(": ").Append(Counts[list[num2]])
				.Append('\n');
		}
		return stringBuilder.ToString();
	}

	public virtual bool Dispatch(IEventHandler Handler)
	{
		Debug.LogError("base HandlerDispatch called for " + GetType().Name);
		return true;
	}

	public bool ActuateOn(GameObject obj)
	{
		if (obj.WantEvent(GetID(), GetCascadeLevel()))
		{
			return obj.HandleEvent(this);
		}
		return true;
	}
}
