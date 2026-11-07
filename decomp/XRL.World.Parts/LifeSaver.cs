using System;

namespace XRL.World.Parts;

[Serializable]
public class LifeSaver : IPoweredPart
{
	public string DefaultActivationMessage;

	public int ChanceVsLethal = 100;

	public string LethalMessage;

	public int ChanceVsMaxHitpointsThreshold = 100;

	public string MaxHitpointsThreshold = "50";

	public string MaxHitpointsThresholdMessage;

	public int ChanceVsCurrentHitpointsThreshold;

	public string CurrentHitpointsThreshold;

	public string CurrentHitpointsThresholdMessage;

	public int MaxUses = 1;

	public bool DestroyWhenUsedUp = true;

	public string DestroyWhenUsedUpMessage;

	public int Used;

	public LifeSaver()
	{
		ChargeUse = 0;
		WorksOnEquipper = true;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool SameAs(IPart p)
	{
		LifeSaver lifeSaver = p as LifeSaver;
		if (lifeSaver.DefaultActivationMessage != DefaultActivationMessage)
		{
			return false;
		}
		if (lifeSaver.ChanceVsLethal != ChanceVsLethal)
		{
			return false;
		}
		if (lifeSaver.LethalMessage != LethalMessage)
		{
			return false;
		}
		if (lifeSaver.ChanceVsMaxHitpointsThreshold != ChanceVsMaxHitpointsThreshold)
		{
			return false;
		}
		if (lifeSaver.MaxHitpointsThreshold != MaxHitpointsThreshold)
		{
			return false;
		}
		if (lifeSaver.MaxHitpointsThresholdMessage != MaxHitpointsThresholdMessage)
		{
			return false;
		}
		if (lifeSaver.ChanceVsCurrentHitpointsThreshold != ChanceVsCurrentHitpointsThreshold)
		{
			return false;
		}
		if (lifeSaver.CurrentHitpointsThreshold != CurrentHitpointsThreshold)
		{
			return false;
		}
		if (lifeSaver.CurrentHitpointsThresholdMessage != CurrentHitpointsThresholdMessage)
		{
			return false;
		}
		if (lifeSaver.MaxUses != MaxUses)
		{
			return false;
		}
		if (lifeSaver.DestroyWhenUsedUp != DestroyWhenUsedUp)
		{
			return false;
		}
		if (lifeSaver.DestroyWhenUsedUpMessage != DestroyWhenUsedUpMessage)
		{
			return false;
		}
		if (lifeSaver.Used != Used)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != ImplantedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "LateBeforeApplyDamage");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "LateBeforeApplyDamage");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "LateBeforeApplyDamage");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "LateBeforeApplyDamage");
		return base.HandleEvent(E);
	}

	public bool CheckLifeSaverActivation(GameObject who, Damage dmg)
	{
		if (who == null)
		{
			return false;
		}
		if (dmg == null)
		{
			return false;
		}
		if (Used >= MaxUses)
		{
			return false;
		}
		if (!IsObjectActivePartSubject(who))
		{
			return false;
		}
		int chance = 0;
		string text = null;
		int hitpoints = who.hitpoints;
		if (dmg.Amount >= hitpoints)
		{
			chance = ChanceVsLethal;
			text = LethalMessage;
		}
		else if (!MaxHitpointsThreshold.IsNullOrEmpty() && dmg.Amount >= who.baseHitpoints * MaxHitpointsThreshold.RollCached() / 100)
		{
			chance = ChanceVsMaxHitpointsThreshold;
			text = MaxHitpointsThresholdMessage;
		}
		else if (!CurrentHitpointsThreshold.IsNullOrEmpty() && dmg.Amount >= hitpoints * CurrentHitpointsThreshold.RollCached() / 100)
		{
			chance = ChanceVsCurrentHitpointsThreshold;
			text = CurrentHitpointsThresholdMessage;
		}
		if (text.IsNullOrEmpty())
		{
			text = DefaultActivationMessage;
		}
		if (!chance.in100())
		{
			return false;
		}
		if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (!text.IsNullOrEmpty())
		{
			IComponent<GameObject>.EmitMessage(who, GameText.VariableReplace(text, who, ParentObject, StripColors: true), ' ', FromDialog: true);
		}
		Used++;
		if (DestroyWhenUsedUp && Used >= MaxUses)
		{
			if (!DestroyWhenUsedUpMessage.IsNullOrEmpty())
			{
				IComponent<GameObject>.EmitMessage(who, GameText.VariableReplace(DestroyWhenUsedUpMessage, who, ParentObject, StripColors: true), ' ', FromDialog: true);
			}
			ParentObject.Destroy();
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LateBeforeApplyDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			Damage dmg = E.GetParameter("Damage") as Damage;
			if (CheckLifeSaverActivation(gameObjectParameter, dmg))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
