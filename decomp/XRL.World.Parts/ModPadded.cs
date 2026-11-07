using System;
using XRL.Language;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ModPadded : IModification
{
	public ModPadded()
	{
	}

	public ModPadded(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "Padding";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IModification.CheckWornSlot(Object, "Head");
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyIfComplex(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("padded");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageIncrease += 50;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "ApplyDazed");
		E.Actor.RegisterPartEvent(this, "ApplyStun");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "ApplyDazed");
		E.Actor.UnregisterPartEvent(this, "ApplyStun");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("GetShakeItOffChance");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyDazed" || E.ID == "ApplyStun")
		{
			if (GetProtectionChance().in100())
			{
				GameObject equipped = ParentObject.Equipped;
				if (equipped != null && IComponent<GameObject>.Visible(equipped) && (equipped.IsPlayer() || equipped == IComponent<GameObject>.ThePlayer?.Target))
				{
					char color = ColorCoding.ConsequentialColorChar(equipped);
					if (E.ID == "ApplyStun")
					{
						equipped?.ParticleText("Resisted stun!", color);
					}
					else if (E.ID == "ApplyDazed")
					{
						equipped.ParticleText("Resisted daze!", color);
					}
					IComponent<GameObject>.AddPlayerMessage(equipped.Poss("padding") + " softened the blow.", color);
				}
			}
		}
		else if (E.ID == "GetShakeItOffChance")
		{
			E.SetParameter("Chance", E.GetIntParameter("Chance") * 2);
		}
		return base.FireEvent(E);
	}

	public static int GetProtectionChance(int Tier)
	{
		return 10;
	}

	public int GetProtectionChance()
	{
		return GetProtectionChance(Tier);
	}

	public static string GetDescription(int Tier)
	{
		return "Padded: This item grants " + Grammar.AOrAnBeforeNumber(GetProtectionChance(Tier)) + " " + GetProtectionChance(Tier) + "% chance to prevent daze or stun effects, and it doubles the chance for the wearer to shake off daze and stun effects with the Shake It Off skill power.";
	}
}
