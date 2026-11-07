using System;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsStasisEntangler : IPoweredPart
{
	public int BaseCooldown = 200;

	public int BaseRealityStabilizationPenetration = 30;

	public string BaseDuration = "15";

	public string CommandID;

	public Guid ActivatedAbilityID = Guid.Empty;

	public CyberneticsStasisEntangler()
	{
		ChargeUse = 0;
		WorksOnImplantee = true;
		NameForStatus = "StasisEntangler";
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID, ParentObject?.Implantee), GetCooldown());
		stats.Set("Duration", GetDuration(), !stats.mode.Contains("ability"));
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.ID && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject?.Implantee);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		if (CommandID == null)
		{
			CommandID = Guid.NewGuid().ToString();
		}
		ActivatedAbilityID = E.Implantee.AddActivatedAbility("Stasis Entangler", CommandID, "Cybernetics", null, "é", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, "CommandStasisEntangler");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == CommandID && !ActivateStasisEntangler(E.Actor, E.Target, E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		E.Description = GetBehaviorDescription();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool ActivateStasisEntangler(GameObject Actor, GameObject Target = null, IEvent FromEvent = null)
	{
		if (!GameObject.Validate(ref Actor))
		{
			return false;
		}
		if (!IsObjectActivePartSubject(Actor))
		{
			return false;
		}
		if (Actor.OnWorldMap())
		{
			return Actor.Fail("You cannot do that on the world map.");
		}
		Zone currentZone = Actor.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		if (!Actor.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			return false;
		}
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return Actor.Fail(ParentObject.Does("are") + " " + GetStatusPhrase() + ".");
		}
		Cell cell = null;
		if (Target == null)
		{
			cell = Actor.Physics.PickDestinationCell(9999, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Stasis Entangler");
			if (cell == null)
			{
				return false;
			}
			Target = cell.GetCombatTarget(Actor, IgnoreFlight: true, IgnoreAttackable: true, IgnorePhase: false, 5, null, null, null, Actor, null, AllowInanimate: false);
			if (Target == null)
			{
				return Actor.Fail("There is no one there for you to entangle.");
			}
		}
		if (cell == null)
		{
			cell = Target.CurrentCell;
		}
		int computePower = GetAvailableComputePowerEvent.GetFor(this);
		int realityStabilizationPenetration = GetRealityStabilizationPenetration(computePower);
		Event obj = Event.New("InitiateRealityDistortionTransit");
		obj.SetParameter("Object", Actor);
		obj.SetParameter("Cell", cell);
		obj.SetParameter("Device", ParentObject);
		obj.SetParameter("RealityStabilizationPenetration", realityStabilizationPenetration);
		if (!Actor.FireEvent(obj, FromEvent) || !cell.FireEvent(obj, FromEvent))
		{
			return false;
		}
		Actor.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown(computePower));
		Actor.UseEnergy(1000, "Cybernetics StasisEntangler");
		DeployToCells(currentZone, Actor, Target, computePower, realityStabilizationPenetration);
		FromEvent?.RequestInterfaceExit();
		return true;
	}

	private GameObject DeployToCells(Zone Zone, GameObject Actor, GameObject Target, int ComputePower, int RealityStabilizationPenetration)
	{
		GameObject gameObject = null;
		int num = 0;
		int num2 = 0;
		Phase.carryOverPrep(Actor, out var FX, out var FX2);
		_ = Look._TextConsole;
		The.Core.RenderBase();
		for (int i = 0; i < Zone.Height; i++)
		{
			for (int j = 0; j < Zone.Width; j++)
			{
				Cell cell = Zone.GetCell(j, i);
				bool flag = false;
				bool flag2 = false;
				int k = 0;
				for (int count = cell.Objects.Count; k < count; k++)
				{
					GameObject gameObject2 = cell.Objects[k];
					if (gameObject2 == Actor || gameObject2 == Target)
					{
						flag2 = true;
						break;
					}
					if (gameObject2.IsCreature)
					{
						flag = true;
					}
				}
				if (flag && !flag2 && IComponent<GameObject>.CheckRealityDistortionAccessibility(null, cell, Actor, ParentObject, null, RealityStabilizationPenetration))
				{
					GameObject gameObject3 = GameObject.Create("Stasisfield");
					gameObject3.GetPart<Forcefield>().Creator = Actor;
					gameObject3.AddPart(new Temporary(GetDuration(ComputePower) + 1));
					Phase.carryOver(Actor, gameObject3, FX, FX2);
					cell.AddObject(gameObject3);
					gameObject = gameObject3;
					num++;
					if (cell.IsVisible())
					{
						num2++;
						The.Core.RenderBase();
					}
				}
			}
		}
		if (num2 == 1)
		{
			IComponent<GameObject>.XDidY(gameObject, "appear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
		}
		else if (num2 >= 6)
		{
			IComponent<GameObject>.AddPlayerMessage(gameObject.ShortDisplayName.Pluralize().Capitalize() + " appear all around.");
		}
		else if (num2 >= 0)
		{
			IComponent<GameObject>.AddPlayerMessage("Several " + gameObject.ShortDisplayName.Pluralize() + " appear nearby.");
		}
		return gameObject;
	}

	public int GetCooldown(int ComputePower)
	{
		int num = BaseCooldown;
		if (ComputePower != 0)
		{
			num = Math.Max(num * (100 - ComputePower / 2) / 100, num / 2);
		}
		return num;
	}

	public int GetCooldown()
	{
		return GetCooldown(GetAvailableComputePowerEvent.GetFor(this));
	}

	public int GetDuration(int ComputePower)
	{
		return BaseDuration.RollCached();
	}

	public int GetDuration()
	{
		return GetDuration(GetAvailableComputePowerEvent.GetFor(this));
	}

	public int GetMinDuration(int ComputePower)
	{
		return BaseDuration.RollMinCached();
	}

	public int GetMinDuration()
	{
		return GetMinDuration(GetAvailableComputePowerEvent.GetFor(this));
	}

	public int GetMaxDuration(int ComputePower)
	{
		return BaseDuration.RollMaxCached();
	}

	public int GetMaxDuration()
	{
		return GetMaxDuration(GetAvailableComputePowerEvent.GetFor(this));
	}

	public int GetRealityStabilizationPenetration(int ComputePower)
	{
		return BaseRealityStabilizationPenetration;
	}

	public int GetRealityStabilizationPenetration()
	{
		return GetRealityStabilizationPenetration(GetAvailableComputePowerEvent.GetFor(this));
	}

	public string GetBehaviorDescription()
	{
		int computePower = GetAvailableComputePowerEvent.GetFor(this);
		int cooldown = GetCooldown(computePower);
		int minDuration = GetMinDuration(computePower);
		int maxDuration = GetMaxDuration(computePower);
		return "Activated. Cooldown " + cooldown + ".\nChoose one creature in sight. All creatures in the zone other than you and it, excepting creatures in the same square as one of you, are put in stasis for " + ((minDuration == maxDuration) ? minDuration.Things("turn") : (minDuration + "-" + maxDuration + " turns")) + ".";
	}
}
