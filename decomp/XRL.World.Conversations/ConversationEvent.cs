using System;
using System.Collections.Generic;
using Genkit;
using UnityEngine;

namespace XRL.World.Conversations;

public abstract class ConversationEvent
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ParameterAttribute : Attribute, IComparable<ParameterAttribute>
	{
		public object Default;

		public bool Reference;

		public bool Output;

		public bool Required;

		public bool Exclude;

		public Type Input;

		public bool Get;

		public int CompareTo(ParameterAttribute Other)
		{
			if (Other == null)
			{
				if (Required || Reference || Output)
				{
					return -1;
				}
				return 0;
			}
			if (Output)
			{
				if (Other.Output)
				{
					return 0;
				}
				if (Other.Reference)
				{
					return 1;
				}
				if (Other.Required)
				{
					return 1;
				}
			}
			else if (Reference)
			{
				if (Other.Reference)
				{
					return 0;
				}
				if (Other.Required)
				{
					return 1;
				}
				if (Other.Output)
				{
					return -1;
				}
			}
			else if (Required)
			{
				if (!Other.Required)
				{
					return -1;
				}
				if (Other.Reference)
				{
					return -1;
				}
				if (Other.Output)
				{
					return -1;
				}
			}
			return 0;
		}
	}

	public enum Action
	{
		Custom,
		Send,
		Get,
		Check
	}

	public enum Instantiation
	{
		Custom,
		Pooling,
		Stack,
		Singleton
	}

	public delegate void EventPoolReset();

	public delegate int EventPoolCount();

	public readonly int ID;

	[Parameter(Required = true)]
	public IConversationElement Element;

	public static Dictionary<int, Type> EventTypes = new Dictionary<int, Type>();

	private static List<EventPoolReset> EventPoolResets = new List<EventPoolReset>();

	private static Dictionary<string, EventPoolCount> EventPoolCounts = new Dictionary<string, EventPoolCount>();

	public static int AllocateID()
	{
		return 0;
	}

	public static int RegisterEvent<T>(string Seed = null, EventPoolCount Count = null, EventPoolReset Reset = null)
	{
		return RegisterEvent(typeof(T), Seed, Count, Reset);
	}

	public static int RegisterEvent(Type Event, string Seed = null, EventPoolCount Count = null, EventPoolReset Reset = null)
	{
		string name = Event.Name;
		int num = (int)Hash.FNV1A32(Seed ?? name);
		if (num == 0)
		{
			throw new ArgumentException("ID hash is zero.");
		}
		if (!EventTypes.TryAdd(num, Event))
		{
			Type type = EventTypes[num];
			if (!(type == Event))
			{
				throw new ArgumentException("ID hash conflict between conversation events " + name + " and " + type.Name + ".");
			}
			Debug.LogError("Duplicate conversation event registration for " + name + ".");
		}
		if (Count != null && !EventPoolCounts.TryAdd(name, Count))
		{
			Debug.LogError("Duplicate pool retrieval registration for " + name + ".");
		}
		if (Reset != null)
		{
			EventPoolResets.Add(Reset);
		}
		return num;
	}

	public ConversationEvent()
	{
	}

	public ConversationEvent(int ID)
	{
		this.ID = ID;
	}

	public virtual bool HandlePartDispatch(IConversationPart Part)
	{
		Debug.LogError("Base HandlePartDispatch called for " + GetType().Name);
		return true;
	}

	public virtual void Reset()
	{
		Element = null;
	}

	public static void RegisterPoolReset(EventPoolReset R)
	{
		EventPoolResets.Add(R);
	}

	public static void RegisterPoolCount(string ClassName, EventPoolCount C)
	{
		if (EventPoolCounts.ContainsKey(ClassName))
		{
			Debug.LogError("duplicate pool retrieval registration for " + ClassName);
		}
		else
		{
			EventPoolCounts.Add(ClassName, C);
		}
	}

	public static void ResetPools()
	{
		foreach (EventPoolReset eventPoolReset in EventPoolResets)
		{
			eventPoolReset();
		}
	}

	protected static void ResetTo<T>(T Event, List<T> Pool, ref int Counter) where T : ConversationEvent
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

	protected static T FromPool<T>(ref List<T> Pool, ref int Counter, int MaxPool = 8192) where T : ConversationEvent, new()
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
}
