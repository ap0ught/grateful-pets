using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Pyrokinesis : BaseMutation
{
	public static readonly int DEFAULT_RANGE = 8;

	public static readonly int DEFAULT_DURATION = 3;

	public static readonly int DEFAULT_RADIUS = 1;

	public static readonly string COMMAND_NAME = "CommandPyrokinesis";

	public static readonly string FIELD_BLUEPRINT = "Shimmering Heat";

	public int Duration = DEFAULT_DURATION;

	public int Range = DEFAULT_RANGE;

	public int Radius = DEFAULT_RADIUS;

	public static int GroupID;

	public Pyrokinesis()
	{
		Type = "Mental";
	}

	public static void CollectProxyStats(Templates.StatCollector stats, int Level)
	{
		bool flag = stats.mode.Contains("ability");
		stats.Set("DamageRound1", $"{Level}d3 / 2", !flag);
		stats.Set("DamageRound2", $"{Level}d4 / 2", !flag);
		stats.Set("DamageRound3", $"{Level}d6 / 2", !flag);
		stats.Set("Area", "3x3");
		stats.Set("Range", DEFAULT_RANGE);
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		base.CollectStats(stats, Level);
		CollectProxyStats(stats, Level);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 50);
		stats.Set("Intro", (base.Level == Level) ? "Toasts affected area over 3 rounds, dealing damage and setting things on fire\n" : "{{rules|Increased toast temperature intensity}}\n");
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= Range && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && !E.Target.IsInStasis())
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && !Cast(this))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You heat a nearby area with your mind.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		if (Level == base.Level)
		{
			text = "Toasts affected area over " + Duration.Things("round") + "\n";
		}
		else
		{
			text = ((Level <= base.Level) ? "{{rules|Decreased toast temperature intensity}}\n" : "{{rules|Increased toast temperature intensity}}\n");
		}
		text = text + "Range: " + Range + "\n";
		int num = Radius * 2 + 1;
		text = text + "Area: " + num + "x" + num + "\n";
		for (int i = 1; i <= Duration; i++)
		{
			text = text + "Round " + i + " damage: {{rules|" + Level + "d" + GetDamageDieSize(i) + "}} divided by 2\n";
		}
		return text + "Cooldown: 50 rounds";
	}

	public static int GetDamageDieSize(int Round)
	{
		return Round switch
		{
			1 => 3, 
			2 => 4, 
			_ => 6, 
		};
	}

	public static List<GameObject> Pyro(GameObject Actor, int Level, List<Cell> Cells, int? Duration = null, int Phase = 0, bool Dependent = false)
	{
		int duration = Duration ?? DEFAULT_DURATION;
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		Actor?.PlayWorldSound("Sounds/Abilities/sfx_ability_pyrokinesis_active");
		Cells.ShuffleInPlace().Sort((Cell a, Cell b) => b.GetTotalDistance(Cells).CompareTo(a.GetTotalDistance(Cells)));
		if (Phase == 0)
		{
			Phase = Actor?.GetPhase() ?? 1;
		}
		List<GameObject> list = new List<GameObject>(Cells.Count);
		int nextGroupID = GetNextGroupID();
		PyroZone pyroZone = null;
		foreach (Cell Cell in Cells)
		{
			GameObject gameObject = GameObject.Create(FIELD_BLUEPRINT);
			if (gameObject.TryGetPart<PyroZone>(out var Part))
			{
				Part.Level = Level;
				Part.Owner = Actor;
				Part.Duration = duration;
				Part.GroupID = nextGroupID;
				if (Dependent)
				{
					Part.DependsOn = Actor?.ID;
				}
				if (pyroZone == null)
				{
					Part.Control = true;
					pyroZone = Part;
				}
			}
			else if (Dependent && Actor != null)
			{
				gameObject.RequirePart<ExistenceSupport>().SupportedBy = Actor;
			}
			XRL.World.Capabilities.Phase.carryOver(Phase, gameObject);
			list.Add(Cell.AddObject(gameObject));
			if (!Cell.IsVisible())
			{
				GameObject player = The.Player;
				if (player == null || !player.WithinSensePsychicRange(Cell))
				{
					continue;
				}
			}
			scrapBuffer.Goto(Cell.X, Cell.Y);
			scrapBuffer.Write("&" + XRL.World.Capabilities.Phase.getRandomHeatColor(Phase) + "*");
			textConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(25);
		}
		pyroZone?.Started();
		return list;
	}

	public static bool Cast(Pyrokinesis Mutation = null, string Level = "5-6", GameObject Actor = null, int? Range = null, int? Radius = null, int Phase = 0)
	{
		if (Actor == null)
		{
			Actor = Mutation?.ParentObject ?? The.Player;
		}
		bool flag = false;
		if (Mutation == null)
		{
			flag = true;
			Mutation = new Pyrokinesis();
			Mutation.Level = Level.RollCached();
			Mutation.Range = Range ?? DEFAULT_RANGE;
			Mutation.Radius = Radius ?? DEFAULT_RADIUS;
			Mutation.ParentObject = Actor;
		}
		if (Actor.OnWorldMap())
		{
			if (!flag)
			{
				Actor.Fail("You cannot do that on the world map.");
			}
			return false;
		}
		List<Cell> list = Mutation.PickBurst(Mutation.Radius, Mutation.Range, Locked: false, AllowVis.OnlyVisible, "Toast");
		if (list == null || list.Count == 0)
		{
			return false;
		}
		foreach (Cell item in list)
		{
			if (item.DistanceTo(Actor) - 1 > Mutation.Range)
			{
				Actor.Fail("That is out of range! (" + Mutation.Range.Things("square") + ")");
				return false;
			}
		}
		Mutation.UseEnergy(1000, "Mental Mutation Pyrokinesis");
		Mutation.CooldownMyActivatedAbility(Mutation.ActivatedAbilityID, 50);
		Pyro(Actor, Mutation.Level, list, Mutation.Duration);
		return true;
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Toast", COMMAND_NAME, "Mental Mutations", null, "*");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public static int GetNextGroupID()
	{
		if (GroupID == 0)
		{
			foreach (Zone value in The.ZoneManager.CachedZones.Values)
			{
				for (int i = 0; i < value.Height; i++)
				{
					for (int j = 0; j < value.Width; j++)
					{
						Cell cell = value.GetCell(j, i);
						if (cell == null)
						{
							continue;
						}
						int k = 0;
						for (int count = cell.Objects.Count; k < count; k++)
						{
							GameObject gameObject = cell.Objects[k];
							if (gameObject.Blueprint == FIELD_BLUEPRINT && gameObject.TryGetPart<PyroZone>(out var Part) && Part.GroupID >= GroupID)
							{
								GroupID = Part.GroupID;
							}
						}
					}
				}
			}
		}
		return ++GroupID;
	}
}
