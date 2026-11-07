using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class ModBlinkEscape : IModification
{
	public ModBlinkEscape()
	{
	}

	public ModBlinkEscape(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnEquipper = true;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsTechScannable = true;
		NameForStatus = "EmergencyTeleporter";
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(3, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnCarrier)
		{
			E.Postfix.AppendRules("Whenever you're about to take avoidable damage, there's " + Grammar.A(GetActivationChance()) + "% chance you blink away instead.", GetEventSensitiveAddStatusSummary(E));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (IsObjectActivePartSubject(E.Object))
		{
			CheckBlinkEscape(E.Object, E.Actor, E.Damage);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public static int GetActivationChance(int Tier)
	{
		return 5 + Tier;
	}

	public int GetActivationChance()
	{
		return GetActivationChance(Tier);
	}

	public int CheckBlinkEscape(GameObject source, Damage damage)
	{
		int num = 0;
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				if (CheckBlinkEscape(activePartSubject, source, damage))
				{
					num++;
				}
			}
		}
		else if (CheckBlinkEscape(GetActivePartFirstSubject(), source, damage))
		{
			num++;
		}
		return num;
	}

	public bool CheckBlinkEscape(GameObject who, GameObject source, Damage damage)
	{
		if (who == null)
		{
			return false;
		}
		if (who.OnWorldMap())
		{
			return false;
		}
		if (damage.HasAttribute("Unavoidable"))
		{
			return false;
		}
		if (!IComponent<GameObject>.CheckRealityDistortionUsability(who, null, who, ParentObject))
		{
			return false;
		}
		if (!GetActivationChance().in100() || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (source != null && source.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Fate intervenes and you deal no damage to " + who.t() + ".");
		}
		damage.Amount = 0;
		who.RandomTeleport(Swirl: true);
		return true;
	}
}
