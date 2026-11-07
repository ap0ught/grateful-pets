using System.Collections.Generic;

namespace XRL.World.AI;

public sealed class PartyCollection : Dictionary<int, PartyMember>, IComposite
{
	public int this[GameObject Object]
	{
		get
		{
			return base[Object.BaseID].Flags;
		}
		set
		{
			if (TryGetValue(Object.BaseID, out var value2))
			{
				value2.Flags = value;
				base[Object.BaseID] = value2;
			}
			else
			{
				Add(Object, value);
			}
		}
	}

	public void Read(SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			int key = Reader.ReadOptimizedInt32();
			GameObjectReference reference = Reader.ReadGameObjectReference();
			int flags = Reader.ReadOptimizedInt32();
			Add(key, new PartyMember(reference, flags));
		}
	}

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Count);
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			var (value, partyMember2) = enumerator.Current;
			Writer.WriteOptimized(value);
			Writer.Write(partyMember2.Reference);
			Writer.WriteOptimized(partyMember2.Flags);
		}
	}

	public void Add(GameObject Object, int Flags)
	{
		Add(Object.BaseID, new PartyMember(Object.Reference(), Flags));
	}

	public bool TryAdd(GameObject Object)
	{
		return TryAdd(Object, 0);
	}

	public bool TryAdd(GameObject Object, int Flags)
	{
		if (ContainsKey(Object.BaseID))
		{
			return false;
		}
		Add(Object, Flags);
		return true;
	}
}
