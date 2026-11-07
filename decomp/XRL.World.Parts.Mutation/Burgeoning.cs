using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Burgeoning : BaseMutation
{
	public const int PRIME_PLANT_CHANCE = 50;

	public bool QudzuOnly;

	[NonSerialized]
	public static string[] ColorList = new string[6] { "&R", "&G", "&B", "&M", "&Y", "&W" };

	public Burgeoning()
	{
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetOffensiveAbilityListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 8 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.HasLOSTo(E.Target, IncludeSolid: false))
		{
			E.Add("CommandBurgeoning");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandBurgeoning");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You cause plants to spontaneously grow in a nearby area, hindering your enemies.";
	}

	public override string GetLevelText(int Level)
	{
		int num = 115 - 10 * Level;
		if (num < 5)
		{
			num = 5;
		}
		string text = "";
		text += "Range: 8\n";
		text += "Area: 3x3 + growth into adjacent tiles\n";
		text = text + "Cooldown: {{rules|" + num + "}} rounds\n";
		if (Level != base.Level)
		{
			text += "More powerful plants summoned\n";
		}
		return text + "+200 reputation with {{w|the Consortium of Phyta}}";
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Area", "3x3");
		stats.Set("Range", "8");
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public static string GetRandomRainbowColor()
	{
		return ColorList.GetRandomElement();
	}

	public static void GrowPlant(Cell C, bool bFriendly, GameObject Owner, int Level, bool QudzuOnly, string primePlant = null)
	{
		if (C.ParentZone.IsActive() && C.IsVisible())
		{
			TextConsole.LoadScrapBuffers();
			ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
			XRLCore.Core.RenderMapToBuffer(TextConsole.ScrapBuffer);
			scrapBuffer.Goto(C.X, C.Y);
			scrapBuffer.Write(GetRandomRainbowColor() + "\a");
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(50);
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			scrapBuffer.Goto(C.X, C.Y);
			scrapBuffer.Write(GetRandomRainbowColor() + "\u000f");
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(25);
			if (Stat.Random(0, 1) == 0)
			{
				XRLCore.ParticleManager.AddSinusoidal(GetRandomRainbowColor() + "\r", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999, 0L);
			}
			else
			{
				XRLCore.ParticleManager.AddSinusoidal(GetRandomRainbowColor() + "\u000e", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999, 0L);
			}
		}
		GameObject gameObject = ((primePlant == null) ? GameObject.Create(GetPlantByTier(Level, QudzuOnly)) : GameObject.Create(primePlant));
		Brain brain = gameObject.Brain;
		gameObject.IsTrifling = true;
		if (brain != null)
		{
			if (bFriendly)
			{
				brain.SetAlliedLeader<AllySummon>(Owner, 0, Silent: true);
			}
			else
			{
				brain.Allegiance.Add("GerminatedPlants", 100);
			}
			gameObject.MakeActive();
			if (gameObject.HasStat("XPValue"))
			{
				gameObject.GetStat("XPValue").BaseValue = 0;
			}
		}
		C.AddObject(gameObject);
		C.PlayWorldSound("sfx_creature_appear_summon");
		gameObject.PlayWorldSoundTag("AmbientIdleSound");
	}

	public static void PlantSummoning(List<Cell> Cells, bool bFriendly, GameObject Owner, int Level, bool bQudzuOnly)
	{
		List<Cell> list = new List<Cell>(32);
		string plantByTier = GetPlantByTier(Level);
		Owner?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_burgeoning_plantGrow");
		foreach (Cell Cell in Cells)
		{
			if (!Cell.IsOccluding())
			{
				if (50.in100())
				{
					GrowPlant(Cell, bFriendly, Owner, Level, bQudzuOnly, plantByTier);
				}
				else
				{
					GrowPlant(Cell, bFriendly, Owner, Level, bQudzuOnly);
				}
			}
			foreach (Cell adjacentCell in Cell.GetAdjacentCells())
			{
				if (!list.CleanContains(adjacentCell) && !Cells.CleanContains(adjacentCell))
				{
					list.Add(adjacentCell);
				}
			}
		}
		foreach (Cell item in list)
		{
			if (!item.IsOccluding() && 20.in100())
			{
				if (50.in100())
				{
					GrowPlant(item, bFriendly, Owner, Level, bQudzuOnly, plantByTier);
				}
				else
				{
					GrowPlant(item, bFriendly, Owner, Level, bQudzuOnly);
				}
			}
		}
	}

	public static string GetPlantByTier(int Level, bool bQudzuOnly = false)
	{
		int num = (int)(Math.Ceiling((float)Level / 2f) + (double)Stat.Random(-1, 2));
		if (num < 1)
		{
			num = 1;
		}
		if (num > 9)
		{
			num = 9;
		}
		GameObject gameObject = ((!bQudzuOnly) ? GameObject.Create(PopulationManager.RollOneFrom("PlantSummoning" + num).Blueprint) : GameObject.Create("Qudzu"));
		return gameObject.Blueprint;
	}

	public bool Burgeon()
	{
		List<Cell> list = PickBurst(1, 8, Locked: false, AllowVis.OnlyVisible, "Burgeoning");
		if (list == null)
		{
			return false;
		}
		foreach (Cell item in list)
		{
			if (item.DistanceTo(ParentObject) > 9)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("That is out of range! (8 squares)");
				}
				return false;
			}
		}
		UseEnergy(1000, "Mental Mutation Burgeoning");
		PlantSummoning(list, bFriendly: true, ParentObject, Level, QudzuOnly);
		int cooldown = GetCooldown(Level);
		CooldownMyActivatedAbility(ActivatedAbilityID, cooldown);
		DidX("impel", "plants to burgeon from the ground", "!", null, null, ParentObject);
		return true;
	}

	public int GetCooldown(int Level)
	{
		return Math.Max(115 - 10 * Level, 5);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandBurgeoning" && !Burgeon())
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
		ActivatedAbilityID = AddMyActivatedAbility("Burgeoning", "CommandBurgeoning", "Mental Mutations", null, "\r");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
