using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XRL.Collections;

namespace XRL.World.AI;

public sealed class AllegianceSet : StringMap<int>
{
	public int SourceID;

	public AllegianceSet Previous;

	public IAllyReason Reason;

	public int Flags;

	public bool Hostile
	{
		get
		{
			return (Flags & 1) == 1;
		}
		set
		{
			Flags = (value ? (Flags | 1) : (Flags & -2));
		}
	}

	public bool Calm
	{
		get
		{
			return (Flags & 2) == 2;
		}
		set
		{
			Flags = (value ? (Flags | 2) : (Flags & -3));
		}
	}

	public int TotalWeight
	{
		get
		{
			int num = 0;
			for (int i = 0; i < Length; i++)
			{
				if (Slots[i].Key != null)
				{
					num += Slots[i].Value;
				}
			}
			return num;
		}
	}

	public int GetBaseFeeling(GameObject Object, IDictionary<string, int> Override = null)
	{
		float num = (float)TotalWeight * 1f;
		float num2 = 0f;
		if (num > 0f)
		{
			for (int i = 0; i < Length; i++)
			{
				if (Slots[i].Key != null)
				{
					num2 += (float)Factions.GetFeelingFactionToObject(Slots[i].Key, Object, Override) * ((float)Slots[i].Value / num);
				}
			}
		}
		return Mathf.RoundToInt(num2);
	}

	public int GetBaseFeeling(Faction Faction, IDictionary<string, int> Override = null)
	{
		float num = (float)TotalWeight * 1f;
		float num2 = 0f;
		if (num > 0f)
		{
			for (int i = 0; i < Length; i++)
			{
				if (Slots[i].Key != null)
				{
					num2 = ((Override == null || !Override.TryGetValue(Slots[i].Key, out var value)) ? (num2 + (float)Faction.GetFeelingTowardsFaction(Slots[i].Key) * ((float)Slots[i].Value / num)) : (num2 + (float)value * ((float)Slots[i].Value / num)));
				}
			}
		}
		return Mathf.RoundToInt(num2);
	}

	public void Copy(AllegianceSet Set)
	{
		Clear();
		Flags = Set.Flags;
		foreach (KeyValuePair<string, int> item in Set)
		{
			Add(item.Key, item.Value);
		}
	}

	public void AppendTo(StringBuilder Builder)
	{
		Builder.Append('(');
		bool flag = true;
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				var (value, value2) = enumerator.Current;
				if (!flag)
				{
					Builder.Append(", ");
				}
				Builder.Append(value).Append('-').Append(value2);
				flag = false;
			}
		}
		Builder.Append(')');
	}

	public override void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(SourceID);
		Writer.Write(Reason);
		Writer.WriteOptimized(Flags);
		Writer.WriteOptimized(Amount);
		for (int i = 0; i < Length; i++)
		{
			if (Slots[i].Key != null)
			{
				Writer.WriteOptimized(Slots[i].Key);
				Writer.WriteOptimized(Slots[i].Value);
			}
		}
		Writer.WriteComposite(Previous);
	}

	public override void Read(SerializationReader Reader)
	{
		SourceID = Reader.ReadOptimizedInt32();
		Reason = (IAllyReason)Reader.ReadComposite();
		Flags = Reader.ReadOptimizedInt32();
		int num = Reader.ReadOptimizedInt32();
		Resize(num);
		for (int i = 0; i < num; i++)
		{
			InsertInternal(Reader.ReadOptimizedString(), Reader.ReadOptimizedInt32());
		}
		Previous = Reader.ReadComposite<AllegianceSet>();
	}

	public string GetDebugInternalsEntry()
	{
		StringBuilder sB = Event.NewStringBuilder();
		for (int i = 0; i < Length; i++)
		{
			if (Slots[i].Key != null)
			{
				sB.Compound(Slots[i].Key, "; ");
				sB.Compound(Slots[i].Value, ": ");
			}
		}
		return Event.FinalizeString(sB);
	}

	public void ClearSlots()
	{
		base.Clear();
	}

	public override void Clear()
	{
		base.Clear();
		SourceID = 0;
		Previous = null;
		Reason = null;
		Flags = 0;
	}
}
