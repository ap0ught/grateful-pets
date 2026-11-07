using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class CooldownOnStep : IPart
{
	public string ClusterSize = "1";

	public string CooldownDamage = "1d10";

	public bool NeedsToBeHidden = true;

	public override bool SameAs(IPart Part)
	{
		CooldownOnStep cooldownOnStep = Part as CooldownOnStep;
		if (cooldownOnStep.ClusterSize != ClusterSize)
		{
			return false;
		}
		if (cooldownOnStep.CooldownDamage != CooldownDamage)
		{
			return false;
		}
		if (cooldownOnStep.NeedsToBeHidden != NeedsToBeHidden)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (ClusterSize != "1")
		{
			List<Cell> list = new List<Cell>(ParentObject.CurrentCell.GetLocalEmptyAdjacentCells()).ShuffleInPlace();
			int num = ClusterSize.RollCached();
			for (int i = 0; i < num && i < list.Count; i++)
			{
				if (15.in100())
				{
					list[i].AddObject(ParentObject.Blueprint);
					continue;
				}
				GameObject gameObject = GameObject.Create(ParentObject.Blueprint);
				if (gameObject.TryGetPart<CooldownOnStep>(out var Part))
				{
					Part.ClusterSize = "1";
				}
				list[i].AddObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object?.ActivatedAbilities != null && ParentObject.IsHostileTowards(E.Object) && ParentObject.PhaseAndFlightMatches(E.Object))
		{
			Hidden part = ParentObject.GetPart<Hidden>();
			if (!NeedsToBeHidden || !part.Found)
			{
				part?.Reveal();
				DidXToY("prick", E.Object, "with " + ParentObject.its + " neuronal thorns", null, null, null, null, E.Object);
				ActivatedAbilities activatedAbilities = E.Object.ActivatedAbilities;
				if (activatedAbilities?.AbilityByGuid != null)
				{
					int cooldown = CooldownDamage.RollCached();
					foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
					{
						value.AddScaledCooldown(cooldown);
					}
				}
				E.Object.Splatter("^B!");
			}
		}
		else if (E.Object != null && E.Object.IsPlayer() && !ParentObject.IsHostileTowards(E.Object))
		{
			ParentObject.GetPart<Hidden>()?.Reveal();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
