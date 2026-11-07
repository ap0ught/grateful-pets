using System;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Domination : BaseMutation
{
	public const string COMMAND_NAME = "CommandDominateCreature";

	public const string METEMPSYCHOSIS_COUNT_KEY = "MetempsychosisCount";

	public const string METEMPSYCHOSIS_FLAG = "MetempsychosisFlag";

	public const string METEMPSYCHOSIS_ORIGINAL_PLAYER_BODY_FLAG = "MetempsychosisOriginalPlayerBodyFlag";

	public bool RealityDistortionBased;

	public GameObject Target;

	public Domination()
	{
		Type = "Mental";
	}

	public override void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopy(Source, CopyEffects, CopyID, MapInv);
		Target = null;
	}

	public override string GetDescription()
	{
		return "You garrote an adjacent creature's mind and control its actions while your own body lies dormant.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("Mental attack versus creature with a mind\n" + "Success roll: {{rules|mutation rank}} or Ego mod (whichever is higher) + character level + 1d8 VS. Defender MA + character level\n", "Range: 1\n"), "Duration: {{rules|", GetDuration(Level).ToString(), "}} rounds\n"), "Cooldown: 75 rounds");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		int num = Math.Max(ParentObject.StatMod("Ego"), Level) + ParentObject.GetStat("Level").Value;
		if (num == 0)
		{
			stats.Set("Attack", "1d8", !stats.mode.Contains("ability"));
		}
		else if (num > 0)
		{
			stats.Set("Attack", "1d8+" + num, !stats.mode.Contains("ability"));
		}
		else
		{
			stats.Set("Attack", "1d8" + num, !stats.mode.Contains("ability"));
		}
		stats.Set("Duration", GetDuration(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID && ID != PooledEvent<BeforeAITakingActionEvent>.ID && (ID != SingletonEvent<BeginTakeActionEvent>.ID || !RealityDistortionBased) && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (GameObject.Validate(ref Target))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAITakingActionEvent E)
	{
		if (GameObject.Validate(ref Target))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (RealityDistortionBased && GameObject.Validate(ref Target) && (!CheckMyRealityDistortionUsability() || !IComponent<GameObject>.CheckRealityDistortionAccessibility(Target, null, ParentObject, null, this)))
		{
			BreakDomination();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		InterruptDomination();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandDominateCreature")
		{
			AttemptDomination(E.Target, E.TargetCell, E);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ChainInterruptDomination");
		Registrar.Register("DominationBroken");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DominationBroken")
		{
			BreakDomination();
		}
		else if (E.ID == "ChainInterruptDomination" && GameObject.Validate(ref Target) && !Target.FireEvent("InterruptDomination"))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Dominate Creature", "CommandDominateCreature", "Mental Mutations", GetDescription(), "\u0003", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	private bool IsOurDominationEffect(Dominated FX)
	{
		return FX.Dominator == ParentObject;
	}

	public Dominated GetDominationEffect(GameObject Target = null)
	{
		return (Target ?? this.Target)?.GetEffect<Dominated>(IsOurDominationEffect);
	}

	public int GetDuration(int Level)
	{
		return 100 * (Level + 1);
	}

	public int GetCooldown(int Level)
	{
		return 75;
	}

	public bool BreakDomination()
	{
		if (!GameObject.Validate(ref Target))
		{
			return false;
		}
		Dominated dominationEffect = GetDominationEffect();
		if (dominationEffect != null && !dominationEffect.BeingRemovedBySource)
		{
			dominationEffect.BeingRemovedBySource = true;
			Target.RemoveEffect(dominationEffect);
		}
		if (Target.OnWorldMap())
		{
			Target.PullDown();
		}
		Target = null;
		The.Game.Player.Body = ParentObject;
		IComponent<GameObject>.ThePlayer.Target = null;
		if (ParentObject.IsPlayer())
		{
			Popup.Show("{{r|Your domination is broken!}}");
			CheckMetempsychosis(ParentObject);
		}
		ParentObject.SmallTeleportSwirl(null, "&M");
		ParentObject.Brain.Goals.Clear();
		CooldownMyActivatedAbility(ActivatedAbilityID, 75);
		Sidebar.UpdateState();
		return true;
	}

	public static void Metempsychosis(GameObject Subject, bool FromOriginalPlayerBody = false)
	{
		if (Subject.IsPlayer())
		{
			int intGameState = The.Game.GetIntGameState("MetempsychosisCount");
			Popup.Show("{{r|Your mind is stranded here.}}");
			JournalAPI.AddAccomplishment("Your mind was stranded inside " + Subject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: false, Short: false, BaseOnly: false, WithIndefiniteArticle: true) + ".", null, null, null, "general", MuralCategory.Generic, MuralWeight.Nil, null, -1L);
			The.Game.SetIntGameState("MetempsychosisCount", intGameState + 1);
			The.Game.PlayerName = Subject.Render.DisplayName;
			Subject.SetIntProperty("Renamed", 1);
			Subject.Brain.Factions = "";
			Subject.Brain.Allegiance.Clear();
			Subject.Brain.Allegiance["Player"] = 100;
			Subject.Brain.FactionFeelings.Clear();
			Subject.RemovePart<GivesRep>();
		}
		else
		{
			Subject.SetIntProperty("MetempsychosisFlag", 1);
			if (FromOriginalPlayerBody)
			{
				Subject.SetIntProperty("MetempsychosisOriginalPlayerBodyFlag", 1);
			}
		}
	}

	public static void CheckMetempsychosis(GameObject Subject)
	{
		if (Subject.GetIntProperty("MetempsychosisFlag") > 0)
		{
			Subject.RemoveIntProperty("MetempsychosisFlag");
			bool fromOriginalPlayerBody = false;
			if (Subject.GetIntProperty("MetempsychosisOriginalPlayerBodyFlag") > 0)
			{
				Subject.RemoveIntProperty("MetempsychosisOriginalPlayerBodyFlag");
				fromOriginalPlayerBody = true;
			}
			Metempsychosis(Subject, fromOriginalPlayerBody);
		}
	}

	public void InterruptDomination()
	{
		if (GameObject.Validate(ref Target))
		{
			Target.FireEvent("InterruptDomination");
		}
	}

	public bool AttemptDomination(GameObject Target = null, Cell TargetCell = null, IEvent FromEvent = null)
	{
		if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
		{
			RealityStabilized.ShowGenericInterdictMessage(ParentObject);
			return false;
		}
		if (TargetCell == null)
		{
			TargetCell = Target?.CurrentCell ?? PickDirection(ForAttack: true, "Dominate");
		}
		if (TargetCell == null)
		{
			return false;
		}
		if (RealityDistortionBased)
		{
			Event obj = Event.New("InitiateRealityDistortionTransit");
			obj.SetParameter("Object", ParentObject);
			obj.SetParameter("Mutation", this);
			obj.SetParameter("Cell", TargetCell);
			if (!ParentObject.FireEvent(obj, FromEvent) || !TargetCell.FireEvent(obj, FromEvent))
			{
				return false;
			}
		}
		ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_mental_generic_activate");
		string FailureMessage = null;
		if (Target != null)
		{
			if (ProcessTarget(Target, ref FailureMessage))
			{
				return true;
			}
		}
		else
		{
			foreach (GameObject item in TargetCell.GetObjectsWithPart("Combat"))
			{
				if (ProcessTarget(item, ref FailureMessage))
				{
					return true;
				}
			}
		}
		if (!FailureMessage.IsNullOrEmpty())
		{
			ParentObject.Fail(FailureMessage);
		}
		return false;
	}

	public bool Dominate(MentalAttackEvent E)
	{
		GameObject defender = E.Defender;
		if (E.Penetrations > 0)
		{
			if (E.Defender.ApplyEffect(new Dominated(ParentObject, E.Magnitude)))
			{
				defender.SmallTeleportSwirl(null, "&M");
				DidXToY("take", "control of", defender, null, "!");
				Target = defender;
				The.Game.Player.Body = defender;
				IComponent<GameObject>.ThePlayer.Target = null;
				return true;
			}
			ParentObject.Fail("Something prevents you from dominating " + defender.t() + ".");
		}
		else
		{
			IComponent<GameObject>.XDidYToZ(defender, "resist", ParentObject, "domination", "!", null, null, null, ParentObject, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			defender.AddOpinion<OpinionDominate>(ParentObject);
		}
		return false;
	}

	private bool ProcessTarget(GameObject Target, ref string FailureMessage)
	{
		if (!Target.HasStat("Level"))
		{
			return false;
		}
		if (Target.Brain == null)
		{
			FailureMessage = "There seems to be no mind in " + Target.t() + " to dominate.";
			return false;
		}
		if (Target.HasCopyRelationship(ParentObject))
		{
			FailureMessage = "You can't dominate " + ParentObject.itself + "!";
			return false;
		}
		if (GetDominationEffect(Target) != null)
		{
			FailureMessage = "You can't dominate someone you are already dominating.";
			return false;
		}
		if (Target.HasEffect<Dominated>())
		{
			FailureMessage = "You can't do that.";
			return false;
		}
		if (!Target.FireEvent("CanApplyDomination") || !CanApplyEffectEvent.Check(Target, "Domination"))
		{
			FailureMessage = Target.Does("do") + " not have a consciousness you can make psychic contact with.";
			return false;
		}
		if (!Target.CheckInfluence(ref FailureMessage, By: ParentObject, Type: Name))
		{
			if (FailureMessage.IsNullOrEmpty())
			{
				FailureMessage = "Nothing happens.";
			}
			return false;
		}
		int attackModifier = ParentObject.Stat("Level") + Math.Max(ParentObject.StatMod("Ego"), Level);
		int num = Target.Stat("Level");
		if (Options.SifrahPsychicCombat)
		{
			attackModifier = ParentObject.Stat("Level") + ParentObject.StatMod("Ego") + Level;
			num = Target.Stat("Level") + Stats.GetCombatMA(Target);
			PsychicCombatSifrah psychicCombatSifrah = new PsychicCombatSifrah(Target, "Domination", attackModifier / 5, num / 5, "dominating " + Target.an());
			psychicCombatSifrah.Play(Target);
			attackModifier = attackModifier * (psychicCombatSifrah.Performance + 50) / 100;
		}
		PerformMentalAttack(Dominate, ParentObject, Target, null, "Domination", "1d8", 1, GetDuration(Level), int.MinValue, attackModifier, num);
		UseEnergy(1000, "Mental Mutation Domination");
		CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
		return true;
	}
}
