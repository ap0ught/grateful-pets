using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class FabricateFromSelf : IPoweredPart
{
	public string FabricateBlueprint = "PhysicalObject";

	public string BatchSize = "1d6";

	public string HitpointsPer = "2d4";

	public string Cooldown = "5d10";

	public string FabricateVerb = "fabricate";

	public string FabricateAlternateSource;

	public int EnergyCost = 1000;

	public int HitpointsThreshold;

	public int AIHitpointsThreshold;

	public bool AIUseForAmmo;

	public bool AIUseOffensively;

	public bool AIUseDefensively;

	public bool AIUsePassively;

	public bool AIUseForThrowing;

	public bool Continuous;

	public Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private string _AbilityDescription;

	[NonSerialized]
	private GameObject _Sample;

	private GameObject Sample
	{
		get
		{
			if (_Sample == null)
			{
				_Sample = GameObject.CreateSample(FabricateBlueprint);
			}
			return _Sample;
		}
	}

	private string AbilityDescription
	{
		get
		{
			if (_AbilityDescription == null)
			{
				_AbilityDescription = "Fabricate " + Grammar.Pluralize(Sample.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true));
			}
			return _AbilityDescription;
		}
	}

	public FabricateFromSelf()
	{
		ChargeUse = 1000;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		FabricateFromSelf fabricateFromSelf = p as FabricateFromSelf;
		if (fabricateFromSelf.FabricateBlueprint != FabricateBlueprint)
		{
			return false;
		}
		if (fabricateFromSelf.BatchSize != BatchSize)
		{
			return false;
		}
		if (fabricateFromSelf.HitpointsPer != HitpointsPer)
		{
			return false;
		}
		if (fabricateFromSelf.Cooldown != Cooldown)
		{
			return false;
		}
		if (fabricateFromSelf.FabricateVerb != FabricateVerb)
		{
			return false;
		}
		if (fabricateFromSelf.FabricateAlternateSource != FabricateAlternateSource)
		{
			return false;
		}
		if (fabricateFromSelf.EnergyCost != EnergyCost)
		{
			return false;
		}
		if (fabricateFromSelf.HitpointsThreshold != HitpointsThreshold)
		{
			return false;
		}
		if (fabricateFromSelf.AIHitpointsThreshold != AIHitpointsThreshold)
		{
			return false;
		}
		if (fabricateFromSelf.AIUseForAmmo != AIUseForAmmo)
		{
			return false;
		}
		if (fabricateFromSelf.AIUseOffensively != AIUseOffensively)
		{
			return false;
		}
		if (fabricateFromSelf.AIUseDefensively != AIUseDefensively)
		{
			return false;
		}
		if (fabricateFromSelf.AIUsePassively != AIUsePassively)
		{
			return false;
		}
		if (fabricateFromSelf.AIUseForThrowing != AIUseForThrowing)
		{
			return false;
		}
		if (fabricateFromSelf.Continuous != Continuous)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return Continuous;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (Continuous && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Activate(Automatic: true);
		}
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("FabricateVerb", FabricateVerb);
		string text = Sample.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true);
		if (BatchSize != "1")
		{
			text = Grammar.Pluralize(text);
			stats.Set("An", "");
		}
		else
		{
			stats.Set("An", Grammar.IndefiniteArticleShouldBeAn(text) ? "an " : "a ");
		}
		stats.Set("FabricateBlueprint", text);
		stats.Set("FabricateSource", string.IsNullOrEmpty(FabricateAlternateSource) ? "the substance of your body" : FabricateAlternateSource);
		stats.Set("BatchSize", BatchSize);
		stats.Set("HitpointsPer", HitpointsPer);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), Cooldown);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetDefensiveAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID && ID != AIGetPassiveAbilityListEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetPassiveAbilityListEvent E)
	{
		if (AIUsePassively && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (AIHitpointsThreshold <= 0 || E.Actor.Stat("Hitpoints") >= AIHitpointsThreshold) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Add("ActivateFabricateFromSelf");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetDefensiveAbilityListEvent E)
	{
		if (AIUseDefensively && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (AIHitpointsThreshold <= 0 || E.Actor.Stat("Hitpoints") >= AIHitpointsThreshold) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Add("ActivateFabricateFromSelf");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if ((AIUseOffensively || AIUseForAmmo || AIUseForThrowing) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (AIHitpointsThreshold <= 0 || E.Actor.Stat("Hitpoints") >= AIHitpointsThreshold) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			bool flag = false;
			if (!flag && AIUseOffensively)
			{
				flag = true;
			}
			if (!flag && AIUseForAmmo)
			{
				List<GameObject> missileWeapons = E.Actor.GetMissileWeapons();
				if (missileWeapons != null && missileWeapons.Count > 0)
				{
					bool flag2 = false;
					int num = 0;
					foreach (GameObject item in missileWeapons)
					{
						MagazineAmmoLoader part = item.GetPart<MagazineAmmoLoader>();
						if (part != null && !part.AmmoPart.IsNullOrEmpty() && Sample.HasPart(part.AmmoPart))
						{
							flag2 = true;
							int num2 = ((part.Ammo == null) ? 100 : (100 - part.Ammo.Count * 100 / part.MaxAmmo));
							if (num2 > num)
							{
								num = num2;
							}
						}
					}
					if (flag2 && num > 0)
					{
						flag = true;
					}
				}
			}
			if (!flag && AIUseForThrowing)
			{
				BodyPart firstBodyPart = E.Actor.GetFirstBodyPart("Thrown Weapon");
				if (firstBodyPart != null && firstBodyPart.Equipped == null)
				{
					flag = true;
				}
			}
			if (flag)
			{
				E.Add("ActivateFabricateFromSelf");
			}
		}
		return base.HandleEvent(E);
	}

	public override void Initialize()
	{
		ActivatedAbilityID = AddMyActivatedAbility(AbilityDescription, "ActivateFabricateFromSelf", "Tinkering", null, "ö");
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ActivateFabricateFromSelf");
		base.Register(Object, Registrar);
	}

	private bool Activate(bool Automatic = false)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ParentObject.IsPlayer() && !Automatic)
			{
				Popup.ShowFail("Nothing happens.");
			}
			return false;
		}
		if (HitpointsThreshold > 0 && ParentObject.Stat("Hitpoints") < HitpointsThreshold)
		{
			if (ParentObject.IsPlayer() && !Automatic)
			{
				Popup.ShowFail("Your health is too weak to do that.");
			}
			return false;
		}
		int num = BatchSize.RollCached();
		if (num < 1)
		{
			if (ParentObject.IsPlayer() && !Automatic)
			{
				Popup.ShowFail("Nothing happens.");
			}
			return false;
		}
		GameObject gameObject = GameObject.CreateUnmodified(FabricateBlueprint);
		if (num > 1 && gameObject.Stacker != null && gameObject.CanGenerateStacked())
		{
			gameObject.Stacker.StackCount = num;
			ParentObject.ReceiveObject(gameObject);
		}
		else
		{
			ParentObject.ReceiveObject(gameObject);
			for (int i = 1; i < num; i++)
			{
				gameObject = GameObject.CreateUnmodified(FabricateBlueprint);
				ParentObject.ReceiveObject(gameObject);
			}
		}
		ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_fabricate");
		if (Visible())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.Does(FabricateVerb) + " " + ((num == 1) ? gameObject.an() : (Grammar.Cardinal(num) + " " + Sample.GetPluralName())) + " from " + (FabricateAlternateSource.IsNullOrEmpty() ? ("the substance of " + ParentObject.its + " body") : FabricateAlternateSource) + ".");
		}
		if (AIUseForThrowing && !ParentObject.IsPlayer())
		{
			BodyPart firstBodyPart = ParentObject.GetFirstBodyPart("Thrown Weapon");
			if (firstBodyPart != null && firstBodyPart.Equipped == null)
			{
				ParentObject.FireEvent(Event.New("CommandEquipObject", "Object", gameObject, "BodyPart", firstBodyPart));
			}
		}
		int num2 = 0;
		for (int j = 0; j < num; j++)
		{
			num2 += HitpointsPer.RollCached();
		}
		if (num2 > 0)
		{
			ParentObject.TakeDamage(num2, Owner: ParentObject, Message: "from using " + (FabricateAlternateSource.IsNullOrEmpty() ? (ParentObject.GetPronounProvider().PossessiveAdjective + "body") : FabricateAlternateSource) + " as raw materials.", Attributes: "Fabrication");
		}
		ConsumeCharge();
		if (EnergyCost > 0)
		{
			ParentObject.UseEnergy(EnergyCost, "Physical Ability Fabricate");
		}
		if (!Cooldown.IsNullOrEmpty())
		{
			int num3 = Cooldown.RollCached();
			if (num3 > 0)
			{
				CooldownMyActivatedAbility(ActivatedAbilityID, num3);
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ActivateFabricateFromSelf" && !Activate())
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
