using System;
using System.Collections;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World;

[Serializable]
public class ZoneBuilderCollection : IReadOnlyList<ZoneBuilderBlueprint>, IEnumerable<ZoneBuilderBlueprint>, IEnumerable, IReadOnlyCollection<ZoneBuilderBlueprint>
{
	[Serializable]
	public struct Enumerator : IEnumerator<ZoneBuilderBlueprint>, IEnumerator, IDisposable
	{
		private ZoneBuilderCollection Collection;

		private int Index;

		private ZoneBuilderBlueprint Blueprint;

		public ZoneBuilderBlueprint Current => Blueprint;

		object IEnumerator.Current => Blueprint;

		public Enumerator(ZoneBuilderCollection Collection)
		{
			this.Collection = Collection;
			Index = 0;
			Blueprint = null;
		}

		public void Dispose()
		{
			Collection = null;
			Blueprint = null;
		}

		public bool MoveNext()
		{
			if (Index >= Collection.Members.Count)
			{
				Index = Collection.Members.Count + 1;
				Blueprint = null;
				return false;
			}
			Blueprint = Collection.Members[Index].Blueprint;
			Index++;
			return true;
		}

		void IEnumerator.Reset()
		{
			Index = 0;
			Blueprint = null;
		}
	}

	public string ZoneID;

	public List<OrderedBuilderBlueprint> Members;

	public int Count => Members.Count;

	public ZoneBuilderBlueprint this[int Index] => Members[Index].Blueprint;

	public ZoneBuilderCollection()
	{
		Members = new List<OrderedBuilderBlueprint>();
	}

	public ZoneBuilderCollection(string ZoneID)
	{
		this.ZoneID = ZoneID;
		Members = new List<OrderedBuilderBlueprint>();
	}

	public ZoneBuilderCollection(string ZoneID, int Capacity)
	{
		this.ZoneID = ZoneID;
		Members = new List<OrderedBuilderBlueprint>(Capacity);
	}

	public ZoneBuilderCollection(ZoneBuilderCollection Collection)
	{
		ZoneID = Collection.ZoneID;
		Members = new List<OrderedBuilderBlueprint>(Collection.Members);
	}

	public void Add(OrderedBuilderBlueprint Member)
	{
		if (Member.Blueprint == null)
		{
			MetricsManager.LogError("Attempting to add null zone builder to collection");
			return;
		}
		int num = Members.Count;
		int num2 = num - 1;
		while (num2 >= 0 && Member.Priority < Members[num2].Priority)
		{
			num = num2;
			num2--;
		}
		Members.Insert(num, Member);
	}

	public void Add(ZoneBuilderBlueprint Blueprint, int Priority)
	{
		Add(new OrderedBuilderBlueprint(Blueprint, Priority));
	}

	public void AddRange(IEnumerable<ZoneBuilderBlueprint> Blueprints, int Priority)
	{
		foreach (ZoneBuilderBlueprint Blueprint in Blueprints)
		{
			Add(Blueprint, Priority);
		}
	}

	public void AddRange(ZoneBuilderCollection Collection)
	{
		foreach (OrderedBuilderBlueprint member in Collection.Members)
		{
			Add(member);
		}
	}

	public bool AddUnique(OrderedBuilderBlueprint Member)
	{
		if (Member.Blueprint == null)
		{
			MetricsManager.LogError("Attempting to add null zone builder to collection");
			return false;
		}
		int num = Members.Count;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			if (Member.Priority < Members[num2].Priority)
			{
				num = num2;
			}
			if (Members[num2].Blueprint.Equals(Member.Blueprint))
			{
				return false;
			}
		}
		Members.Insert(num, Member);
		return true;
	}

	public bool AddUnique(ZoneBuilderBlueprint Blueprint, int Priority)
	{
		return AddUnique(new OrderedBuilderBlueprint(Blueprint, Priority));
	}

	public bool Remove(string Class)
	{
		for (int num = Members.Count - 1; num >= 0; num--)
		{
			if (!(Members[num].Blueprint.Class != Class))
			{
				Members.RemoveAt(num);
				return true;
			}
		}
		return false;
	}

	public bool Remove(ZoneBuilderBlueprint Blueprint)
	{
		for (int num = Members.Count - 1; num >= 0; num--)
		{
			if (Members[num].Blueprint.Equals(Blueprint))
			{
				Members.RemoveAt(num);
				return true;
			}
		}
		return false;
	}

	public int RemoveAll(string Class, Predicate<ZoneBuilderBlueprint> Predicate = null)
	{
		int num = 0;
		for (int num2 = Members.Count - 1; num2 >= 0; num2--)
		{
			if (!(Members[num2].Blueprint.Class != Class) && (Predicate == null || Predicate(Members[num2].Blueprint)))
			{
				Members.RemoveAt(num2);
				num++;
			}
		}
		return num;
	}

	public int RemoveAll(Predicate<ZoneBuilderBlueprint> Predicate)
	{
		int num = 0;
		for (int num2 = Members.Count - 1; num2 >= 0; num2--)
		{
			if (Predicate(Members[num2].Blueprint))
			{
				Members.RemoveAt(num2);
				num++;
			}
		}
		return num;
	}

	public bool ApplyTo(Zone Zone, string Seed = "", bool Force = false)
	{
		bool flag = false;
		int i = 0;
		for (int count = Members.Count; i < count; i++)
		{
			if (Members[i].Priority < 0)
			{
				flag = true;
			}
			else if (flag)
			{
				break;
			}
			ZoneBuilderBlueprint blueprint = Members[i].Blueprint;
			string text = Seed + blueprint?.ToString() + i;
			MetricsManager.rngCheckpoint(text);
			Stat.ReseedFrom(text);
			Coach.StartSection(blueprint.ToString(), bTrackGarbage: true);
			if (!blueprint.ApplyTo(Zone) && !Force)
			{
				MetricsManager.LogError("Builder failed: " + blueprint);
				Coach.EndSection();
				return false;
			}
			Coach.EndSection();
		}
		return true;
	}

	public void Clear()
	{
		Members.Clear();
	}

	public static void Save(SerializationWriter Writer, ZoneBuilderCollection Collection)
	{
		Writer.Write(Collection.ZoneID);
		if (Collection.Members == null)
		{
			MetricsManager.LogError("Zone builder collection members was null.");
			Writer.WriteOptimized(0);
			return;
		}
		int count = Collection.Members.Count;
		Writer.WriteOptimized(count);
		for (int i = 0; i < count; i++)
		{
			OrderedBuilderBlueprint orderedBuilderBlueprint = Collection.Members[i];
			Writer.WriteOptimized(orderedBuilderBlueprint.Priority);
			if (orderedBuilderBlueprint.Blueprint == null)
			{
				MetricsManager.LogError("Zone builder member was null.");
				Writer.WriteOptimized(-1);
			}
			else
			{
				orderedBuilderBlueprint.Blueprint.SaveTokenized(Writer);
			}
		}
	}

	public static ZoneBuilderCollection Load(SerializationReader Reader, List<ZoneBuilderBlueprint> Blueprints)
	{
		string zoneID = Reader.ReadString();
		int num = Reader.ReadOptimizedInt32();
		ZoneBuilderCollection zoneBuilderCollection = new ZoneBuilderCollection(zoneID, num);
		for (int i = 0; i < num; i++)
		{
			int priority = Reader.ReadOptimizedInt32();
			int num2 = Reader.ReadOptimizedInt32();
			if (num2 < 0)
			{
				continue;
			}
			if (num2 >= Blueprints.Count)
			{
				Blueprints.Add(ZoneBuilderBlueprint.GetSerialized(Reader, num2));
				if (Blueprints[num2] == null)
				{
					MetricsManager.LogError("Deserialized null zone builder");
				}
			}
			ZoneBuilderBlueprint zoneBuilderBlueprint = Blueprints[num2];
			if (zoneBuilderBlueprint != null)
			{
				zoneBuilderCollection.Add(zoneBuilderBlueprint, priority);
			}
		}
		return zoneBuilderCollection;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<ZoneBuilderBlueprint> IEnumerable<ZoneBuilderBlueprint>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
