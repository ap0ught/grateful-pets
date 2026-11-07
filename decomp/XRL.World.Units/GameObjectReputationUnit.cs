using System;

namespace XRL.World.Units;

[Serializable]
public class GameObjectReputationUnit : GameObjectUnit
{
	public string Faction;

	public string Type;

	public int Value;

	public bool Silent;

	[NonSerialized]
	private Faction _Entry;

	public Faction Entry
	{
		get
		{
			return _Entry ?? (_Entry = Factions.GetIfExists(Faction));
		}
		set
		{
			Faction = (_Entry = value)?.Name;
		}
	}

	public override void Apply(GameObject Object)
	{
		The.Game.PlayerReputation.Modify(Entry, Value, null, null, Type, Silent);
	}

	public override void Remove(GameObject Object)
	{
		The.Game.PlayerReputation.Modify(Entry, -Value, null, null, Type, Silent);
	}

	public override void Reset()
	{
		base.Reset();
		Faction = null;
		Value = 0;
		_Entry = null;
		Silent = false;
	}

	public override string GetDescription(bool Inscription = false)
	{
		if (Entry != null)
		{
			return Event.FinalizeString(Event.NewStringBuilder().AppendSigned(Value).Append(" reputation with ")
				.Append(Entry.GetFormattedName()));
		}
		return base.GetDescription(Inscription);
	}
}
