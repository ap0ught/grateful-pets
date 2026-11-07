using System;
using System.Text;
using XRL.Rules;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Running : Effect, ITierInitialized
{
	public bool PenaltyApplied;

	public string MessageName;

	public bool SpringingEffective;

	public int MovespeedBonus;

	public Running()
	{
		DisplayName = "sprinting";
	}

	public Running(int Duration = 0, string DisplayName = "sprinting", string MessageName = "sprinting", bool SpringingEffective = true)
		: this()
	{
		base.Duration = Duration;
		base.DisplayName = DisplayName;
		this.MessageName = MessageName;
		this.SpringingEffective = SpringingEffective;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(20, 50);
		DisplayName = "sprinting";
		MessageName = "sprinting";
	}

	public override int GetEffectType()
	{
		return 218103936;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (!Object.HasSkill("Tactics_Hurdle"))
		{
			stringBuilder.Compound("-5 DV", '\n');
		}
		stringBuilder.Compound($"Moves at {GetMovespeedMultiplier()}X the normal speed. (+{MovespeedBonus} move speed)", '\n');
		if (!IsEnhanced())
		{
			if (Object.HasSkill("Pistol_SlingAndRun"))
			{
				stringBuilder.Compound("Reduced accuracy with missile weapons, except pistols.", '\n');
			}
			else
			{
				stringBuilder.Compound("Reduced accuracy with missile weapons.", '\n');
			}
			stringBuilder.Compound("-10 to hit in melee combat.", '\n').Compound("Is ended by attacking in melee, by effects that interfere with movement, and by most other actions that have action costs, other than using physical mutations.", '\n');
		}
		if (Duration == 9999)
		{
			stringBuilder.Compound("Indefinite duration.", '\n');
		}
		else
		{
			stringBuilder.Compound(Duration + " " + ((Duration == 1) ? "round" : "rounds") + " left.", '\n');
		}
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Running>())
		{
			return false;
		}
		if (!Object.CanChangeMovementMode(MessageName))
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyRunning", "Effect", this)))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_movementBuff");
		StatShifter.DefaultDisplayName = DisplayName;
		Object.MovementModeChanged(MessageName);
		DidX("begin", MessageName, "!");
		if (!Object.HasSkill("Tactics_Hurdle") && Object.HasStat("DV"))
		{
			StatShifter.SetStatShift("DV", -5);
			PenaltyApplied = true;
		}
		RecalcMovespeedBonus();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		DidX("stop", MessageName);
		StatShifter.RemoveStatShifts(Object);
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != SingletonEvent<EndActionEvent>.ID && ID != SingletonEvent<UseEnergyEvent>.ID)
		{
			return ID == PooledEvent<GetToHitModifierEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Actor == Object && E.Checking == "Actor" && E.Melee && !IsEnhanced())
		{
			E.Modifier -= 10;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			if (Object != null && Object.OnWorldMap())
			{
				Duration = 0;
			}
			else
			{
				RecalcMovespeedBonus();
				if (Duration != 9999 && Object.GetIntProperty("SkatesEquipped") <= 0)
				{
					Duration--;
				}
			}
			RecalcMovespeedBonus();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndActionEvent E)
	{
		RecalcMovespeedBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseEnergyEvent E)
	{
		if (E.Type != null && !E.Type.Contains("Movement") && E.Type.Contains("Physical") && !IsEnhanced() && E.Amount >= 500)
		{
			Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginAttack");
		Registrar.Register("BodyPositionChanged");
		Registrar.Register("MovementModeChanged");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			E.RenderEffectIndicator("\u001a", "Tiles2/status_sprinting.bmp", "&W", "W", 35);
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginAttack")
		{
			if (!IsEnhanced())
			{
				Duration = 0;
			}
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			string stringParameter = E.GetStringParameter("To");
			if (stringParameter == "Frozen")
			{
				Object.RemoveEffect(this);
			}
			else if (!IsEnhanced() && (!(stringParameter == "Jumping") || !Object.HasPart<Tactics_Hurdle>()))
			{
				Object.RemoveEffect(this);
			}
			else
			{
				RecalcMovespeedBonus();
			}
		}
		return base.FireEvent(E);
	}

	public static bool IsEnhanced(GameObject Object)
	{
		if (Object == null)
		{
			return false;
		}
		return Object.GetIntProperty("EnhancedSprint") > 0;
	}

	public bool IsEnhanced()
	{
		return IsEnhanced(Object);
	}

	public void RecalcMovespeedBonus()
	{
		if (Duration > 0)
		{
			StatShifter.RemoveStatShift(Object, "MoveSpeed");
			int num = 100 - Object.Stat("MoveSpeed") + 100;
			MovespeedBonus = (int)((float)num * (GetMovespeedMultiplier() - 1f));
			StatShifter.SetStatShift("MoveSpeed", -MovespeedBonus);
		}
	}

	public float GetMovespeedMultiplier()
	{
		return GetMovespeedMultiplier(Object, SpringingEffective);
	}

	public static float GetMovespeedMultiplier(GameObject Object, bool SpringingEffective = true, Templates.StatCollector stats = null)
	{
		float num;
		if (SpringingEffective && Object.HasEffect<Springing>())
		{
			stats?.AddPercentageBonusModifier("Multiplier", 100, "springing effect");
			num = 3f;
		}
		else
		{
			num = 2f;
		}
		Wings part = Object.GetPart<Wings>();
		if (part != null)
		{
			float num2 = part.SprintingMoveSpeedBonus(part.Level);
			stats?.AddPercentageBonusModifier("Multiplier", (int)(num2 * 100f), part.GetDisplayName() + " " + part.GetMutationTerm());
			num *= 1f + part.SprintingMoveSpeedBonus(part.Level);
		}
		stats?.CollectBonusModifiers("Multiplier", 100, "Move speed multiplier");
		stats?.Set("Multipler", (int)((num - 1f) * 100f), num != 2f, (num > 2f) ? 1 : (-1));
		return num;
	}
}
