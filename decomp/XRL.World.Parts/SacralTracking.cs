using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class SacralTracking : IPart
{
	[NonSerialized]
	private List<WorshipTracking> WorshipTracking;

	[NonSerialized]
	private List<WorshipTracking> BlasphemyTracking;

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteComposite(WorshipTracking);
		Writer.WriteComposite(BlasphemyTracking);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		WorshipTracking = Reader.ReadCompositeList<WorshipTracking>();
		BlasphemyTracking = Reader.ReadCompositeList<WorshipTracking>();
	}

	public override bool SameAs(IPart Part)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BlasphemyPerformedEvent.ID)
		{
			return ID == WorshipPerformedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(WorshipPerformedEvent E)
	{
		Track(ref WorshipTracking, E.Being);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BlasphemyPerformedEvent E)
	{
		Track(ref BlasphemyTracking, E.Being);
		return base.HandleEvent(E);
	}

	private void Track(ref List<WorshipTracking> List, Worshippable Being)
	{
		if (Being == null)
		{
			return;
		}
		if (List == null)
		{
			List = new List<WorshipTracking>();
		}
		WorshipTracking worshipTracking = null;
		foreach (WorshipTracking item in List)
		{
			if (item.Faction == Being.Faction && worshipTracking == null && item.Name == Being.Name)
			{
				worshipTracking = item;
			}
		}
		if (worshipTracking == null)
		{
			worshipTracking = new WorshipTracking();
			worshipTracking.Name = Being.Name;
			worshipTracking.Faction = Being.Faction;
			worshipTracking.First = The.CurrentTurn;
			List.Add(worshipTracking);
		}
		worshipTracking.Times++;
		worshipTracking.Last = The.CurrentTurn;
	}

	public bool HasWorshipped(string Faction, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in WorshipTracking)
		{
			if (item.Faction == Faction && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWorshipped(Worshippable Being, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in WorshipTracking)
		{
			if (item.Name == Being.Name && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWorshippedInName(string Name, string Faction = null, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in WorshipTracking)
		{
			if (item.Name == Name && (Faction.IsNullOrEmpty() || item.Faction == Faction) && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWorshippedBySpec(string Spec, string ContextFaction = null)
	{
		if (Spec.IsNullOrEmpty())
		{
			return HasWorshipped(ContextFaction);
		}
		string value = null;
		string value2 = null;
		int result = 0;
		if (Spec.Contains("=") && Spec.Contains(";"))
		{
			Dictionary<string, string> dictionary = Spec.CachedDictionaryExpansion();
			dictionary.TryGetValue("Name", out value);
			dictionary.TryGetValue("Factions", out value2);
			if (value2 == "*context*")
			{
				value2 = ContextFaction;
			}
			if (dictionary.TryGetValue("WithinTurns", out var value3))
			{
				int.TryParse(value3, out result);
			}
		}
		else
		{
			value = Spec;
		}
		if (!value.IsNullOrEmpty())
		{
			return HasWorshippedInName(value, value2, result);
		}
		return HasWorshipped(value2, result);
	}

	public List<WorshipTracking> GetWorshipTracking()
	{
		return WorshipTracking;
	}

	public bool HasBlasphemed(string Faction, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in BlasphemyTracking)
		{
			if (item.Faction == Faction && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasBlasphemed(Worshippable Being, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in BlasphemyTracking)
		{
			if (item.Name == Being.Name && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasBlasphemedAgainstName(string Name, string Faction = null, int WithinTurns = 0)
	{
		foreach (WorshipTracking item in BlasphemyTracking)
		{
			if (item.Name == Name && (Faction.IsNullOrEmpty() || item.Faction == Faction) && (WithinTurns == 0 || item.Last >= The.CurrentTurn - WithinTurns))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasBlasphemedBySpec(string Spec, string ContextFaction = null)
	{
		if (Spec.IsNullOrEmpty())
		{
			return HasBlasphemed(ContextFaction);
		}
		string value = null;
		string value2 = null;
		int result = 0;
		if (Spec.Contains("=") && Spec.Contains(";"))
		{
			Dictionary<string, string> dictionary = Spec.CachedDictionaryExpansion();
			dictionary.TryGetValue("Name", out value);
			dictionary.TryGetValue("Factions", out value2);
			if (value2 == "*context*")
			{
				value2 = ContextFaction;
			}
			if (dictionary.TryGetValue("WithinTurns", out var value3))
			{
				int.TryParse(value3, out result);
			}
		}
		else
		{
			value = Spec;
		}
		if (!value.IsNullOrEmpty())
		{
			return HasBlasphemedAgainstName(value, value2, result);
		}
		return HasBlasphemed(value2, result);
	}

	public List<WorshipTracking> GetBlasphemyTracking()
	{
		return BlasphemyTracking;
	}
}
