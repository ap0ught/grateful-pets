using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TimeDilation : BaseMutation
{
	public const int DEFAULT_RANGE = 9;

	public const int DEFAULT_DURATION = 15;

	public static readonly string COMMAND_NAME = "CommandTimeDilation";

	public int Duration;

	public int Range = 9;

	public int Frame;

	public TimeDilation()
	{
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 10 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && !E.Target.IsInStasis() && CheckMyRealityDistortionAdvisability())
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.Fail("You cannot do that on the world map.");
			}
			if (!ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
			{
				return false;
			}
			Duration = 16;
			PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_timeDilation_activate", 0.5f, 0f, Combat: true);
			ApplyField();
			IComponent<GameObject>.XDidYToZ("Time", "begins", "to distort around", ParentObject, null, null, null, null, Source: ParentObject, ColorAsGoodFor: ParentObject);
			UseEnergy(1000, "Mental Mutation TimeDilation");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Duration > 0)
		{
			Duration--;
			if (Duration > 0)
			{
				ApplyField();
			}
			else
			{
				PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_timeDilation_deactivate", 0.5f, 0f, Combat: true);
			}
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
		return "You distort time around your person in order to slow down your enemies.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("Creatures within " + Range + " tiles are slowed according to how close they are to you.\n", "Distance 1: creatures receive a {{rules|", ((int)(CalculateQuicknessPenaltyMultiplier(1.0, Range, Level) * 100.0)).ToString(), "%}} Quickness penalty\n"), "Distance 4: creatures receive a {{rules|", ((int)(CalculateQuicknessPenaltyMultiplier(4.0, Range, Level) * 100.0)).ToString(), "%}} Quickness penalty\n"), "Distance 7: creatures receive a {{rules|", ((int)(CalculateQuicknessPenaltyMultiplier(7.0, Range, Level) * 100.0)).ToString(), "%}} Quickness penalty\n"), "Duration: 15 rounds\n"), "Cooldown: ", GetCooldown(Level).ToString(), " rounds");
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Range", Range);
		stats.Set("QuicknessPenalty1", (int)(CalculateQuicknessPenaltyMultiplier(1.0, Range, Level) * 100.0));
		stats.Set("QuicknessPenalty4", (int)(CalculateQuicknessPenaltyMultiplier(4.0, Range, Level) * 100.0));
		stats.Set("QuicknessPenalty7", (int)(CalculateQuicknessPenaltyMultiplier(7.0, Range, Level) * 100.0));
		stats.Set("Duration", 15);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public static double CalculateQuicknessPenaltyMultiplier(double Distance, int Range, int MutationLevel)
	{
		return Math.Pow((double)(float)Range - Distance, 2.0) * (double)(0.0005f * (float)MutationLevel + 0.0085f);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			Cell cell = ParentObject.CurrentCell;
			Frame--;
			if (Frame < 0)
			{
				Frame = 180;
				for (int i = 0; i < Stat.RandomCosmetic(1, 3); i++)
				{
					float num = (float)Stat.RandomCosmetic(4, 14) / 3f;
					for (int j = 0; j < 360; j++)
					{
						The.ParticleManager.Add("@", cell.X, cell.Y, (float)Math.Sin((double)(float)j * 0.017) / num, (float)Math.Cos((double)(float)j * 0.017) / num);
					}
				}
			}
		}
		return true;
	}

	public static int GetCooldown(int Level)
	{
		return 150;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Time Dilation", "CommandTimeDilation", "Mental Mutations", null, "ä", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public static void ApplyField(GameObject src, int Range = 9, bool Independent = false, int Duration = 15, int Level = 1, IPart Mutation = null)
	{
		if (!src.IsValid() || src.OnWorldMap())
		{
			return;
		}
		Cell cell = src.CurrentCell;
		if (cell == null)
		{
			return;
		}
		foreach (GameObject item in cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, Range, "Combat", src, VisibleToPlayerOnly: false, IncludeWalls: true))
		{
			if (item == src || !item.FireEvent("CanApplyTimeDilated") || !IComponent<GameObject>.CheckRealityDistortionAccessibility(item, null, src, null, Mutation))
			{
				continue;
			}
			if (Independent)
			{
				double num = src.RealDistanceTo(item);
				if (num <= (double)Range)
				{
					double speedPenaltyMultiplier = CalculateQuicknessPenaltyMultiplier(num, Range, Level);
					item.ApplyEffect(new TimeDilatedIndependent(Duration, speedPenaltyMultiplier));
					IComponent<GameObject>.XDidY(src, "begin", "to distort around " + src.the + src.ShortDisplayNameWithoutTitles, null, "Time");
				}
			}
			else
			{
				item.ApplyEffect(new TimeDilated(src));
			}
		}
	}

	public void ApplyField()
	{
		ApplyField(ParentObject, Range, Independent: false, 15, 1, this);
	}
}
