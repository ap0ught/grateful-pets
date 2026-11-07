using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Wintellect.PowerCollections;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SporePuffer : BaseMutation
{
	public int Chance = 100;

	public int EnergyCost = 1000;

	public int nCooldown;

	public string PuffObject = "1";

	public string ColorString = "&G";

	public static List<string> InfectionList = new List<string> { "FungalSporeGasLuminous", "FungalSporeGasPuff", "FungalSporeGasWax", "FungalSporeGasMumbles" };

	public static List<string> InfectionObjectList = new List<string> { "LuminousInfection", "PuffInfection", "WaxInfection", "MumblesInfection" };

	public static List<string> PufferList = new List<string> { "FungusPuffer1", "FungusPuffer2", "FungusPuffer3", "FungusPuffer4" };

	public override string GetLevelText(int Level)
	{
		return "You puff with the best of them.\n";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == GetAdjacentNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (PuffObject.Length == 1)
		{
			Stat.ReseedFrom("PufferType");
			string[] array = Algorithms.RandomShuffle(InfectionList);
			PuffObject = array[Convert.ToInt32(PuffObject)];
		}
		if (!ParentObject.IsPlayer())
		{
			UseEnergy(EnergyCost, "Fungus Puff");
		}
		nCooldown--;
		if (nCooldown <= 0 && Chance > 0 && (Chance >= 100 || Stat.Random(1, 100) < Chance))
		{
			bool flag = false;
			List<Cell> localAdjacentCells = ParentObject.Physics.CurrentCell.GetLocalAdjacentCells();
			if (localAdjacentCells != null)
			{
				foreach (Cell item in localAdjacentCells)
				{
					if (!item.HasObjectWithPart("Brain"))
					{
						continue;
					}
					foreach (GameObject item2 in item.GetObjectsWithPart("Brain"))
					{
						if (ParentObject.Brain == null || !ParentObject.Brain.IsAlliedTowards(item2))
						{
							flag = true;
							break;
						}
					}
				}
			}
			if (flag)
			{
				ParentObject.ParticleBlip("&W*", 10, 0L);
				for (int i = 0; i < localAdjacentCells.Count; i++)
				{
					Gas part = localAdjacentCells[i].AddObject(PuffObject).GetPart<Gas>();
					part.ColorString = ColorString;
					part.Creator = ParentObject;
				}
				nCooldown = 20 + Stat.Random(1, 6);
			}
		}
		if (EnergyCost != 0 && !ParentObject.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (!ParentObject.IsAlliedTowards(E.Actor))
		{
			E.MinWeight(97);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplySpores");
		Registrar.Register("VillageInit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplySpores")
		{
			return false;
		}
		if (E.ID == "VillageInit" && ParentObject.GetBlueprint().DescendsFrom("FungusPuffer"))
		{
			char value = ColorUtility.FindLastForeground(ColorString) ?? 'M';
			if (!ParentObject.Render.ColorString.Contains(value))
			{
				ParentObject.Render.DetailColor = value.ToString();
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ColorString = GO.Render.ColorString;
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
