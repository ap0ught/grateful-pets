using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[HasGameBasedStaticCache]
public class DroidScrambler : IPart
{
	public static HashSet<DroidScrambler> Scramblers = new HashSet<DroidScrambler>();

	public static Dictionary<string, List<string>> Scrambled = new Dictionary<string, List<string>>();

	public static long ScrambledCheck = -1L;

	[GameBasedCacheInit]
	public static void ResetCache()
	{
		Scramblers.Clear();
		Scrambled.Clear();
		ScrambledCheck = -1L;
	}

	public override void Attach()
	{
		Scramblers.Add(this);
	}

	public static void CheckScramblingFactions(long TurnNumber)
	{
		if (ScrambledCheck >= TurnNumber - 5)
		{
			return;
		}
		ScrambledCheck = TurnNumber;
		Scrambled.Clear();
		if (Scramblers.IsNullOrEmpty())
		{
			return;
		}
		List<DroidScrambler> list = null;
		foreach (DroidScrambler scrambler in Scramblers)
		{
			GameObject Object = scrambler.ParentObject;
			if (!GameObject.Validate(ref Object))
			{
				if (list == null)
				{
					list = new List<DroidScrambler>();
				}
				list.Add(scrambler);
				continue;
			}
			GameObject Object2 = Object.Holder;
			if (!GameObject.Validate(ref Object2))
			{
				continue;
			}
			string text = Object2.CurrentZone?.ZoneID;
			if (text.IsNullOrEmpty() || Object.HasEffect(typeof(Broken)) || Object.HasEffect(typeof(Rusted)) || Object.HasEffect(typeof(ElectromagneticPulsed)))
			{
				continue;
			}
			string scrambledFaction = GetScrambledFaction(Object2);
			if (!scrambledFaction.IsNullOrEmpty())
			{
				if (!Scrambled.TryGetValue(text, out var value))
				{
					Scrambled[text] = new List<string> { scrambledFaction };
				}
				else if (!value.Contains(scrambledFaction))
				{
					value.Add(scrambledFaction);
				}
			}
		}
		if (list != null)
		{
			Scramblers.ExceptWith(list);
		}
	}

	public static string GetScrambledFaction(GameObject Object)
	{
		if (Object == null)
		{
			return null;
		}
		Object = Object.Brain?.GetFinalLeader() ?? Object;
		if (!Object.IsPlayer())
		{
			return Object.GetPrimaryFaction();
		}
		return "Player";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != TakenEvent.ID && ID != DroppedEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(TakenEvent E)
	{
		ScrambledCheck = -1L;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		ScrambledCheck = -1L;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		ScrambledCheck = -1L;
		return base.HandleEvent(E);
	}
}
