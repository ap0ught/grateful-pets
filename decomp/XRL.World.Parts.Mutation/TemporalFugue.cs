using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TemporalFugue : BaseMutation
{
	public virtual string CommandID => "CommandTemporalFugue";

	public virtual string AbilityClass => "Mental Mutations";

	public virtual bool IsRealityDistortionBased => true;

	public virtual bool AffectedByWillpower => true;

	public virtual int AIMaxDistance => 2;

	public TemporalFugue()
	{
		Type = "Mental";
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= AIMaxDistance && GameObject.Validate(E.Target) && !E.Actor.HasStringProperty("FugueCopy") && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (!IsRealityDistortionBased || CheckMyRealityDistortionAdvisability()) && E.Actor.CanBeReplicated(E.Actor, null, Temporary: true) && (E.Actor.HasTagOrProperty("AIAbilityIgnoreCon") || E.Target.Con(E.Actor) > Stat.Random(-10, -5)))
		{
			E.Add(CommandID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == CommandID)
		{
			if (!PerformTemporalFugue(E))
			{
				return false;
			}
			UseEnergy(1000, "Mental Mutation TemporalFugue");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("time", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You quickly pass back and forth through time creating multiple copies of yourself.";
	}

	public static int GetTemporalFugueDuration(int Level)
	{
		return 20 + 2 * (Level / 2);
	}

	public int GetTemporalFugueDuration()
	{
		return GetTemporalFugueDuration(Level);
	}

	public static int GetTemporalFugueCopies(int Level)
	{
		return (Level - 1) / 2 + 1;
	}

	public int GetTemporalFugueCopies()
	{
		return GetTemporalFugueCopies(Level);
	}

	public int GetCooldown()
	{
		return GetCooldown(Level);
	}

	public virtual int GetCooldown(int Level)
	{
		return 200;
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("" + "Duration: {{rules|" + GetTemporalFugueDuration(Level) + "}} rounds\n", "Copies: {{rules|", GetTemporalFugueCopies(Level).ToString(), "}}\n"), "Cooldown: {{rules|", GetCooldown(Level).ToString(), "}} rounds");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		bool flag = stats.mode.Contains("ability");
		stats.Set("Duration", GetTemporalFugueDuration(Level), !flag);
		stats.Set("Copies", GetTemporalFugueCopies(Level), !flag);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public static bool PerformTemporalFugue(GameObject Actor, GameObject Subject = null, GameObject Source = null, TemporalFugue Mutation = null, IEvent TriggeringEvent = null, bool Involuntary = false, bool IsRealityDistortionBased = true, int? Duration = null, int? Copies = null, int HostileCopyChance = 0, string Context = "Temporal Fugue", string FriendlyCopyColorString = null, string HostileCopyColorString = null, string FriendlyCopyPrefix = null, string HostileCopyPrefix = null)
	{
		if (Subject == null)
		{
			Subject = Actor;
		}
		if (Source == null)
		{
			Source = Actor;
		}
		if (Subject.HasStringProperty("FugueCopy"))
		{
			Actor.Fail(Subject.Does("are") + " too tenuously anchored in this time to be temporally duplicated in it.");
			return false;
		}
		Cell cell = Subject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (cell.OnWorldMap())
		{
			Actor.Fail("You may not perform temporal fugue on the world map.");
			return false;
		}
		if (!Subject.CanBeReplicated(Actor, Context, Temporary: true))
		{
			Actor.Fail("It is impossible to duplicate " + Subject.t() + ".");
			return false;
		}
		if (IsRealityDistortionBased)
		{
			Event e = ((Mutation == null) ? Event.New("InitiateRealityDistortionLocal", "Actor", Actor, "Object", Subject, "Source", Source ?? Actor) : Event.New("InitiateRealityDistortionLocal", "Actor", Actor, "Object", Subject, "Mutation", Mutation));
			if (!Actor.FireEvent(e, TriggeringEvent))
			{
				return false;
			}
		}
		Subject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_temporalFugue_copy", 0.5f, 0f, Combat: true);
		int num = 0;
		int num2;
		if (Duration.HasValue)
		{
			num2 = Duration.Value;
		}
		else
		{
			num2 = ((Mutation == null) ? Stat.Random(20, 40) : (Mutation.GetTemporalFugueDuration() + 1));
		}
		int num3 = (Copies.HasValue ? Copies.Value : (Mutation?.GetTemporalFugueCopies() ?? Stat.Random(2, 4)));
		Mutation?.CooldownMyActivatedAbility(Mutation.ActivatedAbilityID, Mutation.GetCooldown());
		int intProperty = Actor.GetIntProperty("MentalCloneMultiplier");
		if (intProperty > 0)
		{
			num3 *= intProperty;
		}
		List<Cell> adjacentCells = cell.GetAdjacentCells(2);
		List<Cell> list = null;
		List<Cell> list2 = null;
		foreach (Cell item in adjacentCells.ShuffleInPlace())
		{
			if (!item.IsSpawnable())
			{
				continue;
			}
			int navigationWeightFor = item.GetNavigationWeightFor(Subject);
			if (navigationWeightFor >= 95)
			{
				continue;
			}
			if (navigationWeightFor >= 70)
			{
				if (list2 == null)
				{
					list2 = new List<Cell>();
				}
				list2.Add(item);
				continue;
			}
			if (navigationWeightFor >= 30 || !item.IsEmpty())
			{
				if (list == null)
				{
					list = new List<Cell>();
				}
				list.Add(item);
				continue;
			}
			GameObject gameObject = Subject;
			GameObject source = Source;
			if (CreateFugueCopyOf(Actor, gameObject, item, source, IsRealityDistortionBased, num2, HostileCopyChance, Context, FriendlyCopyColorString, HostileCopyColorString, FriendlyCopyPrefix, HostileCopyPrefix, Mutation) == null || ++num < num3)
			{
				continue;
			}
			list = null;
			list2 = null;
			break;
		}
		if (list != null)
		{
			foreach (Cell item2 in list)
			{
				GameObject gameObject2 = Subject;
				GameObject source = Source;
				if (CreateFugueCopyOf(Actor, gameObject2, item2, source, IsRealityDistortionBased, num2, HostileCopyChance, Context, FriendlyCopyColorString, HostileCopyColorString, FriendlyCopyPrefix, HostileCopyPrefix, Mutation) != null && ++num >= num3)
				{
					list2 = null;
					break;
				}
			}
		}
		if (list2 != null)
		{
			foreach (Cell item3 in list2)
			{
				GameObject gameObject3 = Subject;
				GameObject source = Source;
				GameObject source2 = source;
				int duration = num2;
				if (CreateFugueCopyOf(Actor, gameObject3, item3, source2, IsRealityDistortionBased, duration, HostileCopyChance, Context, FriendlyCopyColorString, HostileCopyColorString, FriendlyCopyPrefix, HostileCopyPrefix, Mutation) != null && ++num >= num3)
				{
					break;
				}
			}
		}
		if (num < num3)
		{
			foreach (Cell connectedSpawnLocation in Subject.CurrentCell.GetConnectedSpawnLocations(num3 - num))
			{
				GameObject gameObject4 = Subject;
				GameObject source = Source;
				if (CreateFugueCopyOf(Actor, gameObject4, connectedSpawnLocation, source, IsRealityDistortionBased, num2, HostileCopyChance, Context, FriendlyCopyColorString, HostileCopyColorString, FriendlyCopyPrefix, HostileCopyPrefix, Mutation) != null && ++num >= num3)
				{
					break;
				}
			}
		}
		IComponent<GameObject>.XDidY(Actor, "blur", "through spacetime");
		IComponent<GameObject>.XDidY(Actor, "multiply");
		return true;
	}

	public virtual bool PerformTemporalFugue(IEvent TriggeringEvent = null)
	{
		return PerformTemporalFugue(ParentObject, ParentObject, null, this, TriggeringEvent);
	}

	public static GameObject CreateFugueCopyOf(GameObject Actor, GameObject Object, Cell TargetCell, GameObject Source = null, bool IsRealityDistortionBased = true, int Duration = 20, int HostileCopyChance = 0, string Context = "Temporal Fugue", string FriendlyCopyColorString = null, string HostileCopyColorString = null, string FriendlyCopyPrefix = null, string HostileCopyPrefix = null, IPart Mutation = null)
	{
		if (Source == null)
		{
			Source = Actor;
		}
		if (Object.HasStringProperty("FugueCopy"))
		{
			return null;
		}
		if (IsRealityDistortionBased)
		{
			if (!IComponent<GameObject>.CheckRealityDistortionAccessibility(null, TargetCell, Actor, Source, Mutation))
			{
				return null;
			}
		}
		if (!Object.CanBeReplicated(Actor, Context, Temporary: true))
		{
			return null;
		}
		GameObject gameObject = Object.DeepCopy();
		if (gameObject.HasStat("XPValue"))
		{
			gameObject.GetStat("XPValue").BaseValue = 0;
		}
		if (Object.IsPlayer())
		{
			gameObject.SetStringProperty("PlayerCopy", "true");
			gameObject.SetStringProperty("PlayerCopyDescription", "one of your " + Context + " clones");
			if (Object.IsOriginalPlayerBody())
			{
				if (Object.HasPart<ConversationScript>())
				{
					gameObject.RemovePart<Chat>();
					gameObject.AddPart(new Chat("*You make small talk with =player.reflexive=."));
				}
				gameObject.SetStringProperty("OriginalPlayerCopy", "true");
			}
		}
		gameObject.SetStringProperty("FugueCopy", Object.ID);
		gameObject.SetStringProperty("CloneOf", Object.ID);
		gameObject.RemoveStringProperty("OriginalPlayerBody");
		if (HostileCopyChance.in100())
		{
			if (gameObject.Brain != null)
			{
				gameObject.Brain.PartyLeader = null;
				gameObject.Brain.Staying = false;
				gameObject.Brain.Passive = false;
				gameObject.Brain.Goals.Clear();
				gameObject.Brain.Target = Object;
			}
			if (!HostileCopyColorString.IsNullOrEmpty())
			{
				gameObject.Render.ColorString = HostileCopyColorString;
			}
			if (!HostileCopyPrefix.IsNullOrEmpty())
			{
				gameObject.Render.DisplayName = HostileCopyPrefix + " " + gameObject.Render.DisplayName;
			}
		}
		else
		{
			AIReplica aIReplica = gameObject.RequirePart<AIReplica>();
			aIReplica.OriginalID = Object.ID;
			aIReplica.ForceAllied = true;
			if (gameObject.Brain != null)
			{
				gameObject.Brain.Goals.Clear();
				gameObject.IsTrifling = true;
				gameObject.Brain.SetPartyLeader(Object, 0, Transient: false, Silent: true);
			}
			if (!FriendlyCopyColorString.IsNullOrEmpty())
			{
				gameObject.Render.ColorString = FriendlyCopyColorString;
			}
			if (!FriendlyCopyPrefix.IsNullOrEmpty())
			{
				gameObject.Render.DisplayName = FriendlyCopyPrefix + " " + gameObject.Render.DisplayName;
			}
		}
		Temporary.AddHierarchically(gameObject, Duration, "*fugue");
		gameObject.FireEvent("TemporalFugueCopied");
		TargetCell.AddObject(gameObject);
		gameObject.MakeActive();
		gameObject.FugueVFX();
		WasReplicatedEvent.Send(Object, Actor, gameObject, Context, Temporary: true);
		ReplicaCreatedEvent.Send(gameObject, Actor, Object, Context, Temporary: true);
		if (!Achievement.CLONES_30.Achieved && Object.IsPlayer())
		{
			Cloning.QueueAchievementCheck();
		}
		return gameObject;
	}

	public bool PerformTemporalFugue(Event TriggeringEvent = null)
	{
		return PerformTemporalFugue(ParentObject, null, null, this, TriggeringEvent);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == CommandID)
		{
			if (!PerformTemporalFugue(E))
			{
				return false;
			}
			UseEnergy(1000, "Mental Mutation TemporalFugue");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown());
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility(GetDisplayName(), CommandID, AbilityClass, null, "\u0013", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
