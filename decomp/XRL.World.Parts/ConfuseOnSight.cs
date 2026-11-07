using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ConfuseOnSight : IPart
{
	public int Chance = 100;

	public int Strength = 25;

	public string Duration = "10-15";

	public int Level = 10;

	public string NameForChecks = "Confusion";

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (ParentObject.IsNowhere())
		{
			return;
		}
		string primaryFaction = ParentObject.GetPrimaryFaction();
		Zone.ObjectEnumerator enumerator = ParentObject.CurrentZone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.IsCombatObject() && current != ParentObject && !current.HasEffect(typeof(Confused)) && !current.IsMemberOfFaction(primaryFaction) && current.HasLOSTo(ParentObject) && Chance.in100() && current.FireEvent("ApplyAttackConfusion") && !current.MakeSave("Willpower", Strength, null, null, NameForChecks, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				current.ApplyEffect(new Confused(Duration.RollCached(), Level, Level + 2, NameForChecks));
			}
		}
	}
}
