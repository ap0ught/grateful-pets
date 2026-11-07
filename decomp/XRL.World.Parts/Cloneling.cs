using System;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Cloneling : IPart
{
	public const int CLONES_PER_DRAM = 40;

	public const string INITIAL_CLONES = "10-40";

	public const string REPLICATION_CONTEXT = "Cloneling";

	public static readonly string COMMAND_NAME = "CommandClone";

	public int ClonesLeft;

	public Guid ActivatedAbilityID;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CanDrinkEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		ConsiderCloning();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanDrinkEvent E)
	{
		if (E.Liquid.MilliAmount("cloning") * 40 / 1000 > 0)
		{
			E.CanDrinkThis = true;
		}
		else
		{
			E.CouldDrinkOther = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			AttemptCloning();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ClonesLeft = "10-40".Roll();
		SyncAbility();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (20.in100() && ClonesLeft.in100())
		{
			IInventory dropInventory = ParentObject.GetDropInventory();
			if (dropInventory != null)
			{
				GameObject gameObject = GameObject.Create("Phial");
				LiquidVolume liquidVolume = gameObject.LiquidVolume;
				if (liquidVolume != null)
				{
					liquidVolume.InitialLiquid = "cloning";
					liquidVolume.Volume = 1;
				}
				dropInventory.AddObjectToInventory(gameObject, ParentObject, Silent: false, NoStack: false, FlushTransient: true, null, E);
				DroppedEvent.Send(ParentObject, gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!ParentObject.IsHostileTowards(E.Actor))
		{
			E.AddAction("ResupplyForCloning", "resupply [1 dram of " + LiquidVolume.GetLiquid("cloning").GetName().Strip() + "]", "ResupplyForCloning", null, 'r', FireOnActor: false, -1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ResupplyForCloning")
		{
			if (E.Actor.UseDrams(1, "cloning"))
			{
				ClonesLeft = 40;
				SyncAbility();
				IComponent<GameObject>.XDidYToZ(E.Actor, "resupply", ParentObject, "with " + LiquidVolume.GetLiquid("cloning").GetName());
				PlayWorldSound("Sounds/Interact/sfx_interact_robot_resupply_liquid");
				E.Actor.UseEnergy(1000, "Action Resupply");
				E.RequestInterfaceExit();
			}
			else if (E.Actor.IsPlayer())
			{
				Popup.ShowFail("You do not have 1 dram of " + LiquidVolume.GetLiquid("cloning").GetName() + ".");
			}
		}
		return base.HandleEvent(E);
	}

	public override void Initialize()
	{
		base.Initialize();
		ActivatedAbilityID = AddMyActivatedAbility("Clone", COMMAND_NAME, "Onboard Systems", null, "\u0013", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, -1, null, null, Renderable.UITile("Abilities/abil_clone.bmp", 'r', 'W'));
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.Remove();
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DrinkingFrom");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DrinkingFrom")
		{
			int num = Math.Min(E.GetGameObjectParameter("Container").LiquidVolume.MilliAmount("cloning") * 40 / 1000, 40);
			if (num > ClonesLeft)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("Your cloning capacity is refreshed.");
				}
				ClonesLeft = num;
				SyncAbility();
			}
		}
		return base.FireEvent(E);
	}

	public void SyncAbility()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Clone [").Append(ClonesLeft).Append(" left]");
		SetMyActivatedAbilityDisplayName(ActivatedAbilityID, stringBuilder.ToString());
		if (ClonesLeft <= 0)
		{
			DisableMyActivatedAbility(ActivatedAbilityID);
		}
		else
		{
			EnableMyActivatedAbility(ActivatedAbilityID);
		}
	}

	public bool ConsiderWandering()
	{
		if (ParentObject.IsPlayerControlled())
		{
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		ParentObject.Brain.PushGoal(new WanderRandomly(5));
		return true;
	}

	public bool ConsiderCloning()
	{
		if (ParentObject.IsPlayer())
		{
			return false;
		}
		if (ClonesLeft <= 0)
		{
			ConsiderWandering();
			return false;
		}
		if (!IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			ConsiderWandering();
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		for (int i = 0; i < ParentObject.Brain.Goals.Items.Count; i++)
		{
			if (ParentObject.Brain.Goals.Items[i].GetType().FullName.Contains("Cloneling"))
			{
				return false;
			}
		}
		GameObject randomElement = ParentObject.CurrentZone.FastCombatSquareVisibility(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, 12, ParentObject, CanBeCloned).GetRandomElement();
		if (randomElement == null)
		{
			ConsiderWandering();
			return false;
		}
		if (ParentObject.Brain.Staying && ParentObject.DistanceTo(randomElement) > 1)
		{
			return false;
		}
		ParentObject.Brain.Goals.Clear();
		ParentObject.Brain.PushGoal(new ClonelingGoal(randomElement));
		return true;
	}

	public bool CanBeCloned(GameObject Target)
	{
		if (!Cloning.CanBeCloned(Target, ParentObject, "Cloneling"))
		{
			return false;
		}
		if (!Target.PhaseAndFlightMatches(ParentObject))
		{
			return false;
		}
		return true;
	}

	public bool PerformCloning(GameObject Target)
	{
		if (ClonesLeft <= 0)
		{
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		Cell randomElement = cell.GetLocalEmptyAdjacentCells().GetRandomElement();
		if (randomElement == null)
		{
			return false;
		}
		string geneID = Target.GeneID;
		GameObject gameObject = Target.DeepCopy();
		gameObject.StripContents(KeepNatural: true, Silent: true);
		Statistic stat = gameObject.GetStat("XPValue");
		if (stat != null)
		{
			stat.BaseValue /= 2;
		}
		gameObject.RestorePristineHealth();
		gameObject.RemoveIntProperty("ProperNoun");
		gameObject.RemoveIntProperty("Renamed");
		gameObject.ModIntProperty("IsClone", 1);
		gameObject.ModIntProperty("IsClonelingClone", 1);
		gameObject.SetStringProperty("CloneOf", Target.ID);
		gameObject.SetStringProperty("CloneOfGenes", geneID);
		if (gameObject.Render != null && !Target.HasPropertyOrTag("CloneNoNameChange") && !Target.BaseDisplayName.Contains("clone of"))
		{
			if (Target.HasProperName)
			{
				gameObject.Render.DisplayName = "clone of " + Target.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: true, Short: false, BaseOnly: false, WithIndefiniteArticle: true);
			}
			else
			{
				string text = gameObject.GetBlueprint().DisplayName();
				if (!text.IsNullOrEmpty() && !text.Contains("["))
				{
					gameObject.Render.DisplayName = "clone of " + Grammar.A(text);
				}
				else
				{
					gameObject.Render.DisplayName = "clone of " + Target.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: true, Short: false, BaseOnly: false, WithIndefiniteArticle: true);
				}
			}
		}
		gameObject.Brain?.Mindwipe();
		if (!Achievement.CLONES_30.Achieved && Target.IsPlayer())
		{
			Cloning.QueueAchievementCheck();
		}
		randomElement.AddObject(gameObject);
		gameObject.MakeActive();
		WasReplicatedEvent.Send(Target, ParentObject, gameObject, "Cloneling");
		ReplicaCreatedEvent.Send(gameObject, ParentObject, Target, "Cloneling");
		gameObject.Bloodsplatter(SelfSplatter: false);
		ParentObject.UseEnergy(1000, "Ability Clone");
		CooldownMyActivatedAbility(ActivatedAbilityID, "2d10".Roll());
		ClonesLeft--;
		SyncAbility();
		ParentObject.UseEnergy(1000, "Ability Clone");
		Messaging.XDidYToZ(ParentObject, "produce", "a clone of", Target, "in a flurry of {{C|flashing chrome}} and {{cloning|spurting liquid}}", null, null, null, ParentObject);
		gameObject?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_budding_clone_complete");
		return true;
	}

	public bool AttemptCloning()
	{
		if (ClonesLeft <= 0)
		{
			return ParentObject.Fail("Your onboard systems are out of " + LiquidVolume.GetLiquid("cloning").GetName() + ".");
		}
		if (!IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
		{
			return ParentObject.Fail("Your onboard cloning systems are offline.");
		}
		Cell cell = PickDirection("Clone");
		if (cell == null)
		{
			return false;
		}
		GameObject combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
		if (combatTarget == null)
		{
			return ParentObject.Fail("There is nothing there for you to clone.");
		}
		if (!CanBeCloned(combatTarget))
		{
			if (ParentObject.IsPlayer())
			{
				if (!combatTarget.FlightMatches(ParentObject))
				{
					ParentObject.Fail("You cannot reach " + combatTarget.t() + ".");
				}
				else if (!combatTarget.PhaseMatches(ParentObject))
				{
					ParentObject.Fail("You cannot make contact with " + combatTarget.t() + ".");
				}
				else if (Cloning.CanBeCloned(combatTarget))
				{
					ParentObject.Fail("You cannot clone " + combatTarget.t() + ".");
				}
				else
				{
					ParentObject.Fail(combatTarget.T() + " cannot be cloned.");
				}
			}
			return false;
		}
		if (!PerformCloning(combatTarget))
		{
			return ParentObject.Fail("You fail to clone " + combatTarget.t() + ".");
		}
		return false;
	}
}
