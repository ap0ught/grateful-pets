using System;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Robot : IPart
{
	public bool EMPable = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != BeforeDeathRemovalEvent.ID && ID != BeforeApplyDamageEvent.ID && ID != CanApplyEffectEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != PooledEvent<GetMutationTermEvent>.ID && ID != PooledEvent<GetScanTypeEvent>.ID && ID != PooledEvent<MutationsSubjectToEMPEvent>.ID && ID != PooledEvent<IsMutantEvent>.ID && ID != PooledEvent<IsSensableAsPsychicEvent>.ID && ID != PooledEvent<RespiresEvent>.ID)
		{
			return ID == PooledEvent<TransparentToEMPEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetScanTypeEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.ScanType = Scanning.Scan.Tech;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsMutantEvent E)
	{
		E.IsMutant = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		E.Sensable = false;
		return false;
	}

	public override bool HandleEvent(GetMutationTermEvent E)
	{
		E.Term = "module";
		E.Color = "C";
		return false;
	}

	public override bool HandleEvent(MutationsSubjectToEMPEvent E)
	{
		if (EMPable)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RespiresEvent E)
	{
		return false;
	}

	public override bool HandleEvent(TransparentToEMPEvent E)
	{
		if (EMPable)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction += 25;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && (E.Damage.HasAttribute("Poison") || E.Damage.HasAttribute("Asphyxiation")))
		{
			NotifyTargetImmuneEvent.Send(E.Weapon, E.Object, E.Actor, E.Damage, this);
			E.Damage.Amount = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		GameObject gameObject = null;
		int chance = 5;
		if (!ParentObject.HasPart<ExtradimensionalLoot>())
		{
			if (IComponent<GameObject>.ThePlayer.HasSkill("Tinkering_Scavenger"))
			{
				chance = 35;
			}
			if (chance.in100())
			{
				gameObject = GameObjectFactory.create(PopulationManager.RollOneFrom("Scrap " + Tier.Constrain(ParentObject.Stat("Level") / 5)).Blueprint);
			}
		}
		if (gameObject != null && gameObject.IsReal)
		{
			IInventory dropInventory = ParentObject.GetDropInventory();
			if (dropInventory != null)
			{
				dropInventory.AddObjectToInventory(gameObject, null, Silent: false, NoStack: false, FlushTransient: true, null, E);
				DroppedEvent.Send(ParentObject, gameObject);
			}
			else
			{
				gameObject.Obliterate();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("circuitry", 2);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AIMovingTowardsTarget");
		Registrar.Register("ApplyEMP");
		Registrar.Register("ApplyingTonic");
		Registrar.Register("CanApplyAshPoison");
		Registrar.Register("CanApplySpores");
		Registrar.Register("CanApplyTonic");
		Registrar.Register("HasPowerConnectors");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyingTonic" || E.ID == "CanApplyTonic")
		{
			if (E.HasFlag("ShowMessage"))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Subject");
				gameObjectParameter.Fail(string.Concat(str2: E.GetGameObjectParameter("Tonic").t(int.MaxValue, null, null, AsIfKnown: false, Single: true), str0: gameObjectParameter2.Does("are"), str1: " robotic and cannot be affected by ", str3: "."));
			}
			return false;
		}
		if (E.ID == "CanApplyTonic" || E.ID == "CanApplyAshPoison" || E.ID == "CanApplySpores")
		{
			return false;
		}
		if (E.ID == "HasPowerConnectors")
		{
			return false;
		}
		if (E.ID == "AIMovingTowardsTarget")
		{
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Target");
			if (gameObjectParameter3 != null && gameObjectParameter3.HasProperty("RobotStop") && !ParentObject.MakeSave("Willpower", 18, null, null, "RobotStop Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, gameObjectParameter3))
			{
				DidX("come", "to a complete stop");
				ParentObject.ParticleBlip("&rX", 10, 0L);
				return false;
			}
		}
		else if (E.ID == "ApplyEMP" && EMPable)
		{
			Brain brain = ParentObject.Brain;
			brain.Goals.Clear();
			brain.PushGoal(new Dormant(E.GetIntParameter("Duration")));
		}
		return base.FireEvent(E);
	}

	private bool Check(IEffectCheckEvent E)
	{
		if (E.Name == "AshPoison" || E.Name == "CardiacArrest" || E.Name == "Confusion" || E.Name == "CyberneticRejectionSyndrome" || E.Name == "Disease" || E.Name == "DiseaseOnset" || E.Name == "Poison" || E.Name == "PoisonGasPoison" || E.Name == "ShatterMentalArmor" || E.Name == "ToxicConfusion")
		{
			return false;
		}
		return true;
	}
}
