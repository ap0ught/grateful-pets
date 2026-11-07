using System;
using System.Collections;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World;

[Serializable]
public class ZonePartCollection : IReadOnlyCollection<ZonePartBlueprint>, IEnumerable<ZonePartBlueprint>, IEnumerable
{
	[Serializable]
	public struct Enumerator : IEnumerator<ZonePartBlueprint>, IEnumerator, IDisposable
	{
		private ZonePartCollection Collection;

		private int Index;

		private ZonePartBlueprint Blueprint;

		public ZonePartBlueprint Current => Blueprint;

		object IEnumerator.Current => Blueprint;

		public Enumerator(ZonePartCollection Collection)
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
			Blueprint = Collection.Members[Index];
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

	public List<ZonePartBlueprint> Members;

	public int Count => Members.Count;

	public ZonePartBlueprint this[int Index] => Members[Index];

	public ZonePartCollection()
	{
		Members = new List<ZonePartBlueprint>();
	}

	public ZonePartCollection(string ZoneID)
	{
		this.ZoneID = ZoneID;
		Members = new List<ZonePartBlueprint>();
	}

	public ZonePartCollection(string ZoneID, int Capacity)
	{
		this.ZoneID = ZoneID;
		Members = new List<ZonePartBlueprint>(Capacity);
	}

	public ZonePartCollection(ZonePartCollection Collection)
	{
		ZoneID = Collection.ZoneID;
		Members = new List<ZonePartBlueprint>(Collection.Members);
	}

	public void Add(ZonePartBlueprint Blueprint)
	{
		Members.Add(Blueprint);
	}

	public void AddRange(ZonePartCollection Collection)
	{
		Members.AddRange(Collection.Members);
	}

	public bool AddUnique(ZonePartBlueprint Member)
	{
		for (int num = Members.Count - 1; num >= 0; num--)
		{
			if (Members[num].Equals(Member))
			{
				return false;
			}
		}
		Add(Member);
		return true;
	}

	public bool Remove(string Name)
	{
		for (int num = Members.Count - 1; num >= 0; num--)
		{
			if (!(Members[num].Name != Name))
			{
				Members.RemoveAt(num);
				return true;
			}
		}
		return false;
	}

	public bool Remove(ZonePartBlueprint Blueprint)
	{
		for (int num = Members.Count - 1; num >= 0; num--)
		{
			if (Members[num].Equals(Blueprint))
			{
				Members.RemoveAt(num);
				return true;
			}
		}
		return false;
	}

	public int RemoveAll(string Name, Predicate<ZonePartBlueprint> Predicate = null)
	{
		int num = 0;
		for (int num2 = Members.Count - 1; num2 >= 0; num2--)
		{
			if (!(Members[num2].Name != Name) && (Predicate == null || Predicate(Members[num2])))
			{
				Members.RemoveAt(num2);
				num++;
			}
		}
		return num;
	}

	public int RemoveAll(Predicate<ZonePartBlueprint> Predicate)
	{
		int num = 0;
		for (int num2 = Members.Count - 1; num2 >= 0; num2--)
		{
			if (Predicate(Members[num2]))
			{
				Members.RemoveAt(num2);
				num++;
			}
		}
		return num;
	}

	public bool ApplyTo(Zone Zone, string Seed = "", bool Force = false)
	{
		int i = 0;
		for (int count = Members.Count; i < count; i++)
		{
			ZonePartBlueprint zonePartBlueprint = Members[i];
			string text = Seed + zonePartBlueprint?.ToString() + i;
			MetricsManager.rngCheckpoint(text);
			Stat.ReseedFrom(text);
			Coach.StartSection(zonePartBlueprint.ToString(), bTrackGarbage: true);
			if (!zonePartBlueprint.ApplyTo(Zone) && !Force)
			{
				MetricsManager.LogError("Part setup failed: " + zonePartBlueprint);
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

	public static void Save(SerializationWriter Writer, ZonePartCollection Collection)
	{
		Writer.Write(Collection.ZoneID);
		Writer.Write(Collection.Members.Count);
		foreach (ZonePartBlueprint member in Collection.Members)
		{
			member.SaveTokenized(Writer);
		}
	}

	public static ZonePartCollection Load(SerializationReader Reader, List<ZonePartBlueprint> Blueprints)
	{
		string zoneID = Reader.ReadString();
		int num = Reader.ReadInt32();
		ZonePartCollection zonePartCollection = new ZonePartCollection(zoneID, num);
		for (int i = 0; i < num; i++)
		{
			int num2 = Reader.ReadInt32();
			if (num2 >= Blueprints.Count)
			{
				Blueprints.Add(ZonePartBlueprint.GetSerialized(Reader, num2));
			}
			zonePartCollection.Add(Blueprints[num2]);
		}
		return zonePartCollection;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<ZonePartBlueprint> IEnumerable<ZonePartBlueprint>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
