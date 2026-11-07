using System;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ReflectShame : BaseMutation
{
	/// <summary>Shamed effect duration.</summary>
	public string Duration = "20-30";

	public bool Friendly;

	/// <summary>Affected target radius.</summary>
	public int Radius = 5;

	public ReflectShame()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("glass", 1);
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You reflect the shameful countenance of nearby creatures.";
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ShameNearby();
	}

	public void ShameNearby()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return;
		}
		foreach (GameObject item in cell.ParentZone.FastCombatSquareVisibility(cell.X, cell.Y, Radius, ParentObject, IsValidTarget))
		{
			PerformMentalAttack(Shame, ParentObject, item, null, "Shame ReflectShame", "1d8", 4, Stat.RollCached(Duration), int.MinValue, ParentObject.StatMod("Ego"));
		}
	}

	public bool Shame(MentalAttackEvent E)
	{
		if (E.Penetrations >= 1 && CanApplyEffectEvent.Check<Shamed>(E.Defender, E.Magnitude) && E.Defender.ApplyEffect(new Shamed(E.Magnitude)))
		{
			Messaging.XDidYToZ(E.Defender, "are", "shamed by", E.Defender, "reflection", "!", null, null, null, E.Defender, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
			return true;
		}
		return false;
	}

	private bool IsValidTarget(GameObject Object)
	{
		if ((Friendly || Object.IsHostileTowards(ParentObject)) && !Object.HasEffect<Shamed>())
		{
			return Object.HasLOSTo(ParentObject);
		}
		return false;
	}
}
