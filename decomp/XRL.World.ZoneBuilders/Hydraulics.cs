using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World.ZoneBuilders;

public class Hydraulics : ZoneBuilderSandbox
{
	public string DamageChance;

	public string DamageIsBreakageChance = "50";

	public string ConduitBlueprint = "GlassHydraulicPipe";

	public string ConduitModPart = "ModPiping";

	public string MissingConsumers;

	public string MissingProducers;

	public string MissingConsumerChargeRate = "50|100|200|500|1000|2000|1d20x500";

	public string MissingProducerChargeRate = "2000|4000|8000|1d20x1000";

	public string MissingConsumerLocations;

	public string MissingProducerLocations;

	public string Liquid;

	public string LiquidTable = "HydraulicFluid";

	public int FinalDamageChance;

	public int FinalDamageIsBreakageChance;

	public int FinalMissingConsumers;

	public int FinalMissingProducers;

	public int Noise;

	public bool PreferWalls = true;

	public bool AvoidWalls;

	public bool ShowPathfinding;

	public bool ShowPathWeights;

	public List<Location2D> FinalMissingConsumerLocations;

	public List<Location2D> FinalMissingProducerLocations;

	[NonSerialized]
	private static List<GameObject> consumers = new List<GameObject>(16);

	[NonSerialized]
	private static List<GameObject> producers = new List<GameObject>(8);

	[NonSerialized]
	private static List<GameObject> conduits = new List<GameObject>(32);

	[NonSerialized]
	private static List<GameObject> all = new List<GameObject>(32);

	[NonSerialized]
	private static List<GameObject> missingConsumers = new List<GameObject>(16);

	[NonSerialized]
	private static List<GameObject> missingProducers = new List<GameObject>(8);

	[NonSerialized]
	private static int missingConsumerIndex = 0;

	[NonSerialized]
	private static int missingProducerIndex = 0;

	private int HydraulicProducerRate(GameObject obj)
	{
		int intProperty = obj.GetIntProperty("HydraulicProducerRate", -1);
		if (intProperty >= 0)
		{
			return intProperty;
		}
		return obj.QueryCharge(LiveOnly: false, 0L);
	}

	private int HydraulicConsumerRate(GameObject obj)
	{
		int intProperty = obj.GetIntProperty("HydraulicConsumerRate", -1);
		if (intProperty >= 0)
		{
			return intProperty;
		}
		int num = 0;
		foreach (IPart parts in obj.PartsList)
		{
			if (parts is IPoweredPart)
			{
				IPoweredPart poweredPart = parts as IPoweredPart;
				num += poweredPart.GetActiveChargeUse();
			}
		}
		return num;
	}

	private List<Location2D> ResolveLocations(string spec)
	{
		if (string.IsNullOrEmpty(spec))
		{
			return null;
		}
		string[] array = spec.Split(';');
		List<Location2D> list = new List<Location2D>(array.Length);
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = array2[i].Split(',');
			list.Add(Location2D.Get(Convert.ToInt32(array3[0]), Convert.ToInt32(array3[1])));
		}
		return list;
	}

	private bool IsProducer(GameObject obj)
	{
		if (obj.HasTagOrProperty("NoHydraulicPower"))
		{
			return false;
		}
		if (obj.HasTagOrProperty("HydraulicPowerConsumer"))
		{
			return false;
		}
		HydraulicPowerTransmission part = obj.GetPart<HydraulicPowerTransmission>();
		if (part != null && part.IsProducer)
		{
			return true;
		}
		if (obj.HasTagOrProperty("HydraulicPowerProducer"))
		{
			return true;
		}
		obj.HasTagOrProperty("HydraulicPowerConsumer");
		return false;
	}

	private bool IsConsumer(GameObject obj)
	{
		if (obj.HasTagOrProperty("NoHydraulicPower"))
		{
			return false;
		}
		HydraulicPowerTransmission part = obj.GetPart<HydraulicPowerTransmission>();
		if (part != null && part.IsConsumer)
		{
			return true;
		}
		if (obj.HasTagOrProperty("HydraulicPowerConsumer"))
		{
			return true;
		}
		obj.HasTagOrProperty("HydraulicPowerProducer");
		return false;
	}

	private bool ValidInstall(GameObject obj)
	{
		if (obj.Takeable)
		{
			return false;
		}
		if (!obj.ConsiderSolid())
		{
			return false;
		}
		if (obj.IsCombat)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(ConduitModPart) && obj.HasPart(ConduitModPart))
		{
			return false;
		}
		if (obj.Brain != null)
		{
			return false;
		}
		if (obj.HasPart<XRL.World.Parts.Temporary>())
		{
			return false;
		}
		if (obj.HasPart<ExistenceSupport>())
		{
			return false;
		}
		return true;
	}

	private string RepresentWeight(int weight)
	{
		string text = weight.ToString();
		if (text.Length == 1)
		{
			return text;
		}
		char c = text[text.Length - 1];
		if (weight < 20)
		{
			return "&Y" + c + "&y";
		}
		if (weight < 30)
		{
			return "&r" + c + "&y";
		}
		if (weight < 40)
		{
			return "&R" + c + "&y";
		}
		if (weight < 50)
		{
			return "&o" + c + "&y";
		}
		if (weight < 60)
		{
			return "&O" + c + "&y";
		}
		if (weight < 70)
		{
			return "&w" + c + "&y";
		}
		if (weight < 80)
		{
			return "&W" + c + "&y";
		}
		if (weight < 90)
		{
			return "&g" + c + "&y";
		}
		if (weight < 100)
		{
			return "&G" + c + "&y";
		}
		if (weight < 110)
		{
			return "&b" + c + "&y";
		}
		if (weight < 120)
		{
			return "&B" + c + "&y";
		}
		if (weight < 130)
		{
			return "&m" + c + "&y";
		}
		if (weight < 140)
		{
			return "&M" + c + "&y";
		}
		return "&K" + c + "&y";
	}

	private GameObject SetUpMissingConsumer(Zone Z)
	{
		GameObject gameObject;
		if (missingConsumerIndex >= missingConsumers.Count)
		{
			gameObject = GameObject.CreateUnmodified("CosmeticObject");
			gameObject.Physics.Solid = true;
			gameObject.SetIntProperty("HydraulicPowerConsumer", 1);
		}
		else
		{
			gameObject = missingConsumers[missingConsumerIndex++];
		}
		if (string.IsNullOrEmpty(MissingConsumerChargeRate))
		{
			gameObject.RemoveIntProperty("HydraulicConsumerRate");
		}
		else
		{
			gameObject.SetIntProperty("HydraulicConsumerRate", MissingConsumerChargeRate.RollCached());
		}
		if (FinalMissingConsumerLocations != null && FinalMissingConsumerLocations.Count > 0)
		{
			Z.GetCell(FinalMissingConsumerLocations[0]).AddObject(gameObject);
			FinalMissingConsumerLocations.RemoveAt(0);
		}
		else
		{
			int num = 0;
			bool flag = false;
			while (!flag && num++ < 10)
			{
				Cell randomCell = Z.GetRandomCell();
				if (randomCell.IsEmpty() && randomCell.HasAdjacentLocalWallCell())
				{
					randomCell.AddObject(gameObject);
					flag = true;
				}
			}
			while (!flag && num++ < 20)
			{
				Cell randomCell2 = Z.GetRandomCell();
				if (randomCell2.IsEmpty())
				{
					randomCell2.AddObject(gameObject);
					flag = true;
				}
			}
			if (!flag)
			{
				Z.GetRandomCell().AddObject(gameObject);
			}
		}
		missingConsumers.Add(gameObject);
		return gameObject;
	}

	private GameObject SetUpMissingProducer(Zone Z)
	{
		GameObject gameObject;
		if (missingProducerIndex >= missingProducers.Count)
		{
			gameObject = GameObject.CreateUnmodified("CosmeticObject");
			gameObject.Physics.Solid = true;
			gameObject.SetIntProperty("HydraulicPowerProducer", 1);
		}
		else
		{
			gameObject = missingProducers[missingProducerIndex++];
		}
		if (string.IsNullOrEmpty(MissingProducerChargeRate))
		{
			gameObject.RemoveIntProperty("HydraulicProducerRate");
		}
		else
		{
			gameObject.SetIntProperty("HydraulicProducerRate", MissingProducerChargeRate.RollCached());
		}
		if (FinalMissingProducerLocations != null && FinalMissingProducerLocations.Count > 0)
		{
			Z.GetCell(FinalMissingProducerLocations[0]).AddObject(gameObject);
			FinalMissingProducerLocations.RemoveAt(0);
		}
		else
		{
			int num = 0;
			bool flag = false;
			Cell randomCell = Z.GetRandomCell();
			if (randomCell.IsEmpty() && randomCell.HasAdjacentLocalWallCell())
			{
				randomCell.AddObject(gameObject);
				flag = true;
			}
			while (!flag && num++ < 20)
			{
				Cell randomCell2 = Z.GetRandomCell();
				if (randomCell2.IsEmpty())
				{
					randomCell2.AddObject(gameObject);
					flag = true;
				}
			}
			if (!flag)
			{
				Z.GetRandomCell().AddObject(gameObject);
			}
		}
		missingProducers.Add(gameObject);
		return gameObject;
	}

	private void CleanUp(List<GameObject> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (GameObject item in list)
		{
			item.CurrentCell?.RemoveObject(item);
			item.Obliterate();
		}
		list.Clear();
	}

	public bool BuildZone(Zone Z)
	{
		consumers.Clear();
		producers.Clear();
		conduits.Clear();
		all.Clear();
		missingConsumerIndex = 0;
		missingProducerIndex = 0;
		if (!string.IsNullOrEmpty(MissingConsumers))
		{
			FinalMissingConsumers = MissingConsumers.RollCached();
		}
		if (!string.IsNullOrEmpty(MissingProducers))
		{
			FinalMissingProducers = MissingProducers.RollCached();
		}
		if (FinalMissingConsumers > 0)
		{
			FinalMissingConsumerLocations = ResolveLocations(MissingConsumerLocations);
		}
		if (FinalMissingProducers > 0)
		{
			FinalMissingProducerLocations = ResolveLocations(MissingProducerLocations);
		}
		for (int i = 0; i < FinalMissingConsumers; i++)
		{
			SetUpMissingConsumer(Z);
		}
		for (int j = 0; j < FinalMissingProducers; j++)
		{
			SetUpMissingProducer(Z);
		}
		using Pathfinder pathfinder = Z.getPathfinder();
		for (int k = 0; k < Z.Height; k++)
		{
			for (int l = 0; l < Z.Width; l++)
			{
				pathfinder.CurrentNavigationMap[l, k] = 40;
			}
		}
		if (Noise > 0)
		{
			for (int m = 0; m < Z.Height; m++)
			{
				for (int n = 0; n < Z.Width; n++)
				{
					pathfinder.CurrentNavigationMap[n, m] += Stat.Random(0, Noise * 2) - Noise;
				}
			}
		}
		for (int num = 0; num < Z.Height; num++)
		{
			for (int num2 = 0; num2 < Z.Width; num2++)
			{
				Cell cell = Z.GetCell(num2, num);
				int num3 = 0;
				GameObject gameObject = null;
				GameObject gameObject2 = null;
				GameObject gameObject3 = null;
				GameObject gameObject4 = null;
				GameObject gameObject5 = null;
				foreach (GameObject @object in cell.Objects)
				{
					if (gameObject == null && @object.IsWall() && !@object.HasPart<ExistenceSupport>())
					{
						gameObject = @object;
					}
					if (gameObject2 == null && @object.HasPart<Door>() && !@object.HasPart<ExistenceSupport>())
					{
						gameObject2 = @object;
					}
					if (IsProducer(@object))
					{
						gameObject3 = @object;
					}
					if (IsConsumer(@object))
					{
						gameObject4 = @object;
					}
					if (@object.HasPart<HydraulicPowerTransmission>())
					{
						gameObject5 = @object;
					}
				}
				if (gameObject3 != null || gameObject4 != null)
				{
					if (gameObject3 != null)
					{
						producers.Add(gameObject3);
					}
					if (gameObject4 != null)
					{
						consumers.Add(gameObject4);
					}
					bool flag = false;
					if (gameObject == null && PreferWalls && !AvoidWalls)
					{
						foreach (Cell localCardinalAdjacentCell in cell.GetLocalCardinalAdjacentCells())
						{
							if (localCardinalAdjacentCell.HasWall())
							{
								for (int num4 = 0; num4 < Z.Width; num4++)
								{
									pathfinder.CurrentNavigationMap[num4, localCardinalAdjacentCell.Y]--;
								}
								for (int num5 = 0; num5 < Z.Height; num5++)
								{
									pathfinder.CurrentNavigationMap[localCardinalAdjacentCell.X, num5]--;
								}
								flag = true;
							}
						}
					}
					if (flag)
					{
						num3 += 40;
					}
					else
					{
						for (int num6 = 0; num6 < Z.Width; num6++)
						{
							pathfinder.CurrentNavigationMap[num6, num]--;
						}
						for (int num7 = 0; num7 < Z.Height; num7++)
						{
							pathfinder.CurrentNavigationMap[num2, num7]--;
						}
						num3 += 8;
					}
				}
				else if (gameObject != null)
				{
					HydraulicPowerTransmission part = gameObject.GetPart<HydraulicPowerTransmission>();
					if (part != null)
					{
						num3 -= 20 + part.ChargeRate / 1000;
						conduits.Add(gameObject);
					}
					else if (AvoidWalls)
					{
						num3 += 30 - Math.Max(gameObject.GetTechTier(), 1) * 2;
					}
					else if (PreferWalls)
					{
						num3 -= 20 + Math.Max(gameObject.GetTechTier(), 1) * 2;
					}
				}
				else if (gameObject2 != null)
				{
					HydraulicPowerTransmission part2 = gameObject2.GetPart<HydraulicPowerTransmission>();
					if (part2 != null)
					{
						num3 -= 18 + part2.ChargeRate / 1000;
						conduits.Add(gameObject2);
					}
					else if (AvoidWalls)
					{
						num3 += 28 - Math.Max(gameObject2.GetTechTier(), 1) * 2;
					}
					else if (PreferWalls)
					{
						num3 -= 18 + Math.Max(gameObject2.GetTechTier(), 1) * 2;
					}
				}
				else if (gameObject5 != null)
				{
					HydraulicPowerTransmission part3 = gameObject5.GetPart<HydraulicPowerTransmission>();
					if (part3 != null)
					{
						conduits.Add(gameObject5);
						num3 -= 20 + part3.ChargeRate / 1000;
					}
					else
					{
						num3 -= 22;
					}
				}
				pathfinder.CurrentNavigationMap[num2, num] += num3;
			}
		}
		if (producers.Count > 0 && consumers.Count > 0)
		{
			for (int num8 = 0; num8 < Z.Height; num8++)
			{
				for (int num9 = 0; num9 < Z.Width; num9++)
				{
					if (pathfinder.CurrentNavigationMap[num9, num8] < 1)
					{
						pathfinder.CurrentNavigationMap[num9, num8] = 1;
					}
				}
			}
			if (ShowPathWeights)
			{
				ScreenBuffer screenBuffer = Popup._ScreenBuffer;
				for (int num10 = 0; num10 < Z.Height; num10++)
				{
					for (int num11 = 0; num11 < Z.Width; num11++)
					{
						screenBuffer.Goto(num11, num10);
						screenBuffer.Write(RepresentWeight(pathfinder.CurrentNavigationMap[num11, num10]));
					}
				}
				XRLCore._Console.DrawBuffer(screenBuffer);
				Keyboard.getch();
			}
			consumers.Sort(new CentralitySorter(Z));
			if (!string.IsNullOrEmpty(DamageChance))
			{
				FinalDamageChance = DamageChance.RollCached();
			}
			if (FinalDamageChance > 0 && !string.IsNullOrEmpty(DamageIsBreakageChance))
			{
				FinalDamageIsBreakageChance = DamageIsBreakageChance.RollCached();
			}
			bool[,] array = new bool[Z.Width, Z.Height];
			foreach (GameObject consumer in consumers)
			{
				Location2D location2D = Location2D.Get(consumer.CurrentCell.X, consumer.CurrentCell.Y);
				GameObject gameObject6 = FindBestConnection(consumer, producers, Z);
				if (gameObject6 == null)
				{
					continue;
				}
				Location2D location2D2 = Location2D.Get(gameObject6.CurrentCell.X, gameObject6.CurrentCell.Y);
				if (!pathfinder.FindPath(location2D, location2D2, ShowPathfinding, CardinalDirectionsOnly: true))
				{
					continue;
				}
				int num12 = 1;
				for (int num13 = pathfinder.Steps.Count - 1; num12 < num13; num12++)
				{
					PathfinderNode pathfinderNode = pathfinder.Steps[num12];
					if (array[pathfinderNode.X, pathfinderNode.Y])
					{
						continue;
					}
					if (pathfinderNode.X == location2D.X && pathfinderNode.Y == location2D.Y)
					{
						Debug.LogError("hydraulic grid trying to apply at consumer location");
						continue;
					}
					if (pathfinderNode.X == location2D2.X && pathfinderNode.Y == location2D2.Y)
					{
						Debug.LogError("hydraulic grid trying to apply at producer location");
						continue;
					}
					Cell cell2 = Z.GetCell(pathfinderNode.X, pathfinderNode.Y);
					if (cell2.HasObjectWithPart("HydraulicPowerTransmission"))
					{
						continue;
					}
					bool flag2 = false;
					GameObject gameObject7 = null;
					GameObject gameObject8 = null;
					if (!string.IsNullOrEmpty(ConduitModPart))
					{
						gameObject7 = cell2.GetFirstObjectWithPropertyOrTag("Wall", ValidInstall) ?? cell2.GetFirstObjectWithPart("Physics", ValidInstall);
					}
					if (gameObject7 != null)
					{
						conduits.Add(gameObject7);
						if (!FinalDamageChance.in100())
						{
							ItemModding.ApplyModification(gameObject7, ConduitModPart);
							if (cell2.ParentZone.Built)
							{
								ZoneManager.PaintWalls(cell2.ParentZone, cell2.X - 1, cell2.Y - 1, cell2.X + 1, cell2.Y + 1);
							}
						}
						else
						{
							array[pathfinderNode.X, pathfinderNode.Y] = true;
						}
						flag2 = true;
					}
					else if (!string.IsNullOrEmpty(ConduitBlueprint))
					{
						bool flag3 = FinalDamageChance.in100();
						bool flag4 = flag3 && FinalDamageIsBreakageChance.in100();
						if (!flag3 | flag4)
						{
							gameObject8 = GameObject.CreateUnmodified(ConduitBlueprint);
							if (flag4)
							{
								gameObject8.ApplyEffect(new Broken());
							}
							cell2.AddObject(gameObject8);
							conduits.Add(gameObject8);
						}
						else
						{
							array[pathfinderNode.X, pathfinderNode.Y] = true;
						}
						flag2 = true;
					}
					if (flag2)
					{
						int weight = pathfinderNode.weight;
						HydraulicPowerTransmission hydraulicPowerTransmission = gameObject7?.GetPart<HydraulicPowerTransmission>() ?? gameObject8?.GetPart<HydraulicPowerTransmission>();
						weight -= ((hydraulicPowerTransmission == null) ? 12 : (10 + hydraulicPowerTransmission.ChargeRate / 500));
						if (weight < 1)
						{
							weight = 1;
						}
						pathfinder.CurrentNavigationMap[pathfinderNode.X, pathfinderNode.Y] = weight;
						pathfinderNode.weight = weight;
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(LiquidTable))
		{
			Liquid = PopulationManager.RollOneFrom(LiquidTable)?.Blueprint;
		}
		if (!string.IsNullOrEmpty(Liquid))
		{
			string text = Liquid;
			if (text.Contains(",") || text.Contains("-"))
			{
				text = text.Split(',')[0].Split('-')[0];
			}
			all.AddRange(producers);
			all.AddRange(consumers);
			all.AddRange(conduits);
			foreach (GameObject item in all)
			{
				HydraulicPowerTransmission part4 = item.GetPart<HydraulicPowerTransmission>();
				if (part4 != null && !string.IsNullOrEmpty(part4.DependsOnLiquid))
				{
					part4.DependsOnLiquid = text;
					LiquidVolume liquidVolume = item.LiquidVolume;
					if (liquidVolume != null)
					{
						liquidVolume.InitialLiquid = Liquid;
					}
				}
			}
		}
		producers.Clear();
		consumers.Clear();
		conduits.Clear();
		all.Clear();
		CleanUp(missingConsumers);
		CleanUp(missingProducers);
		return true;
	}

	private GameObject FindBestConnection(GameObject obj, List<GameObject> connects, Zone Z)
	{
		double num = 999999.0;
		GameObject result = null;
		int centerX = Z.Width / 2;
		int centerY = Z.Height / 2;
		int draw = HydraulicConsumerRate(obj);
		foreach (GameObject connect in connects)
		{
			int num2 = Suitability(obj, connect, centerX, centerY, draw);
			if ((double)num2 < num)
			{
				num = num2;
				result = connect;
			}
		}
		return result;
	}

	private int Suitability(GameObject obj, GameObject connect, int CenterX, int CenterY, int draw)
	{
		int num = XRL.Rules.Geometry.Distance(CenterX, CenterY, connect);
		double num2 = obj.RealDistanceTo(connect);
		double num3 = (double)num / 3.0 + num2;
		if (draw > 0)
		{
			int num4 = HydraulicProducerRate(connect);
			if (draw > num4)
			{
				num3 = ((num4 <= 0) ? (num3 * 100.0) : (num3 * (double)draw / (double)num4));
			}
		}
		return Convert.ToInt32(num3);
	}
}
