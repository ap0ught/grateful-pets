using System;
using XRL.Language;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class Miner : IPart
{
	public static readonly string COMMAND_NAME = "CommandPlaceMine";

	public int MineCooldown;

	public string Cooldown = "5-10";

	public string MineType;

	public string MineName;

	public string MineTimer = "-1";

	public int MaxMinesPerZone = 15;

	public int Mark = 1;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart Part)
	{
		Miner miner = Part as Miner;
		if (miner.MineCooldown != MineCooldown)
		{
			return false;
		}
		if (miner.Cooldown != Cooldown)
		{
			return false;
		}
		if (miner.MineType != MineType)
		{
			return false;
		}
		if (miner.MineName != MineName)
		{
			return false;
		}
		if (miner.MineTimer != MineTimer)
		{
			return false;
		}
		if (miner.MaxMinesPerZone != MaxMinesPerZone)
		{
			return false;
		}
		if (miner.Mark != Mark)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != SingletonEvent<BeginTakeActionEvent>.ID || ParentObject.IsPlayer()) && ID != PooledEvent<CommandEvent>.ID && (ID != AfterObjectCreatedEvent.ID || !MineType.IsNullOrEmpty()))
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && !IsMyActivatedAbilityCoolingDown(ActivatedAbilityID))
		{
			SetMineOrBomb();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		SetupMinerConfiguration();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		int mineCooldown = MineCooldown;
		if (MineCooldown > 0)
		{
			MineCooldown--;
		}
		if (!ParentObject.IsPlayer() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.FireEvent("CanAIDoIndependentBehavior") && !ParentObject.HasGoal("LayMineGoal") && !ParentObject.HasGoal("WanderRandomly") && !ParentObject.HasGoal("Flee"))
		{
			if (mineCooldown > 0 || ParentObject.CurrentZone.CountObjects("MineShell") >= MaxMinesPerZone)
			{
				GameObject target = ParentObject.Target;
				if (target != null)
				{
					ParentObject.Brain.PushGoal(new Flee(target, Stat.Random(10, 20)));
				}
			}
			else
			{
				Cell randomElement = ParentObject.CurrentZone.GetEmptyReachableCells().GetRandomElement();
				if (randomElement == null)
				{
					ParentObject.Brain.PushGoal(new WanderRandomly(5));
				}
				else
				{
					ParentObject.Brain.Goals.Clear();
					ParentObject.Brain.PushGoal(new LayMineGoal(randomElement.Location, MineType, MineName, MineTimer, 12 + Mark * 3));
				}
				MineCooldown = Cooldown.RollCached();
			}
		}
		return base.HandleEvent(E);
	}

	public void SetMineOrBomb()
	{
		int num = MineTimer.RollCached();
		int num2 = 12 + Mark * 3;
		GameObject gameObject = ((num > 0) ? Tinkering_LayMine.CreateBomb(MineType, ParentObject, num) : Tinkering_LayMine.CreateMine(MineType, ParentObject));
		if (num2 > 0)
		{
			gameObject.RequirePart<Hidden>().Difficulty = num2;
		}
		if (ParentObject.IsPlayer())
		{
			ParentObject.PlayWorldOrUISound("Sounds/Abilities/sfx_ability_lay_mine");
		}
		ParentObject.CurrentCell.AddObject(gameObject);
		ParentObject.UseEnergy(1000);
		CooldownMyActivatedAbility(ActivatedAbilityID, Cooldown.RollCached());
	}

	public void SetupMinerConfiguration()
	{
		if (MineType.IsNullOrEmpty())
		{
			string text = Mark.ToString();
			string populationName = "Explosives " + text;
			int num = 0;
			GameObject gameObject = null;
			string tag;
			do
			{
				if (++num > 1000)
				{
					throw new Exception("cannot generate layable grenade");
				}
				MineType = PopulationManager.RollOneFrom(populationName).Blueprint;
				gameObject = GameObject.CreateSample(MineType);
				tag = gameObject.GetTag("Mark");
			}
			while (!gameObject.HasPart<Tinkering_Layable>() || tag != text || gameObject.HasTag("NoMiners") || Scanning.GetScanTypeFor(gameObject) != Scanning.Scan.Tech);
			string displayName = gameObject.Render.DisplayName;
			int num2 = displayName.IndexOf("grenade");
			if (num2 != -1)
			{
				MineName = displayName.Substring(0, num2);
			}
			else
			{
				MineName = displayName + " ";
			}
			if (!ParentObject.HasProperName)
			{
				if (MineTimer == "-1")
				{
					ParentObject.Render.DisplayName = MineName + "miner mk " + Grammar.GetRomanNumeral(Mark);
				}
				else
				{
					ParentObject.Render.DisplayName = MineName + "bomber mk " + Grammar.GetRomanNumeral(Mark);
				}
			}
			if (GameObject.Validate(ref gameObject))
			{
				gameObject.Obliterate();
			}
		}
		if (ActivatedAbilityID == Guid.Empty)
		{
			ActivatedAbilityID = AddMyActivatedAbility("Lay Mine [" + MineName + "mk " + Grammar.GetRomanNumeral(Mark) + "]", COMMAND_NAME, "Skills", "Lay Mine", "\u0012");
		}
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), Cooldown);
		stats.Set("MineType", MineType);
		stats.Set("MineName", MineName);
		stats.Set("MineTimer", MineTimer);
		stats.Set("MineMark", Grammar.GetRomanNumeral(Mark));
		stats.Set("MineAn", Grammar.IndefiniteArticleShouldBeAn(MineName) ? "an" : "a");
		stats.Set("MineDisplayName", MineName + " mk " + Grammar.GetRomanNumeral(Mark));
	}
}
