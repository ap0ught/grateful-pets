using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GrandfatherHorn : IActivePart
{
	public int Cooldown;

	public string CooldownTime = "100-200";

	public string Power = "1d8";

	public string Duration = "2d4+1";

	public string HarmonyTag = "Cervine";

	public string DisharmonyDescription = "DisharmoniousUser";

	public GrandfatherHorn()
	{
		WorksOnEquipper = true;
	}

	public override bool SameAs(IPart p)
	{
		GrandfatherHorn grandfatherHorn = p as GrandfatherHorn;
		if (grandfatherHorn.Cooldown != Cooldown)
		{
			return false;
		}
		if (grandfatherHorn.CooldownTime != CooldownTime)
		{
			return false;
		}
		if (grandfatherHorn.Power != Power)
		{
			return false;
		}
		if (grandfatherHorn.Duration != Duration)
		{
			return false;
		}
		if (grandfatherHorn.HarmonyTag != HarmonyTag)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveItemListEvent.ID && ID != SingletonEvent<EndTurnEvent>.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveItemListEvent E)
	{
		if (Cooldown <= 0 && E.Actor == ParentObject.Equipped && E.Distance < 10 && IsInHarmony(E.Actor) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && Stat.Random(1, 6) == 6)
		{
			E.Add("BlowGrandfatherHorn", 1, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Cooldown > 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Cooldown--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (Cooldown <= 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.AddAction("Blow", "blow", "BlowGrandfatherHorn", null, 'b');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "BlowGrandfatherHorn" && Cooldown <= 0 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null)
			{
				ParentObject.PlayWorldSound("Sounds/Interact/sfx_interact_grandfatherHorn_blow");
				ParentObject.Soundwave();
				foreach (GameObject item in cell.ParentZone.FastFloodVisibility(cell.X, cell.Y, 10, "Brain", ParentObject))
				{
					if (!IsInHarmony(item))
					{
						PerformMentalAttack(Terrified.OfAttacker, E.Actor, item, null, "Fear GrandfatherHorn", Power, 2, Duration.RollCached());
					}
				}
			}
			Cooldown = Stat.Roll(CooldownTime);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AdjustWeaponScore");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AdjustWeaponScore" && IsInHarmony(E.GetGameObjectParameter("User")))
		{
			E.SetParameter("Score", E.GetIntParameter("Score") + 10);
		}
		return base.FireEvent(E);
	}

	public bool IsInHarmony(GameObject who)
	{
		if (who == null)
		{
			return false;
		}
		if (!who.HasTagOrProperty(HarmonyTag))
		{
			return who.HasSkill("Customs_Sharer");
		}
		return true;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return !IsInHarmony(ParentObject.Equipped);
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return DisharmonyDescription;
	}
}
