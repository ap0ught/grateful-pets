using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class DataDisk : IPart
{
	public TinkerData Data;

	public int TargetTier;

	public int TargetTechTier;

	public int TargetTinkerTier;

	public string TargetBlueprint;

	public string TargetTinkerCategory;

	public string TargetType;

	[NonSerialized]
	public string ObjectName;

	[NonSerialized]
	private static StringBuilder SB = new StringBuilder();

	[NonSerialized]
	private static BitCost Cost = new BitCost();

	public override bool SameAs(IPart p)
	{
		DataDisk dataDisk = p as DataDisk;
		if (dataDisk.Data != Data)
		{
			return false;
		}
		if (dataDisk.TargetTier != TargetTier)
		{
			return false;
		}
		if (dataDisk.TargetTechTier != TargetTechTier)
		{
			return false;
		}
		if (dataDisk.TargetTinkerTier != TargetTinkerTier)
		{
			return false;
		}
		if (dataDisk.TargetBlueprint != TargetBlueprint)
		{
			return false;
		}
		if (dataDisk.TargetTinkerCategory != TargetTinkerCategory)
		{
			return false;
		}
		if (dataDisk.TargetType != TargetType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool CanGenerateStacked()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		SB.Clear();
		if (Data == null)
		{
			if (TargetBlueprint.IsNullOrEmpty())
			{
				ObjectName = "invalid data";
			}
			else
			{
				ObjectName = "invalid blueprint: " + TargetBlueprint;
			}
		}
		else if (Data.Type == "Build")
		{
			try
			{
				if (ObjectName == null)
				{
					if (Data.Blueprint == null)
					{
						ObjectName = "invalid blueprint: " + Data.Blueprint;
					}
					else
					{
						ObjectName = TinkeringHelpers.TinkeredItemShortDisplayName(Data.Blueprint);
					}
				}
			}
			catch (Exception)
			{
				ObjectName = "error:" + Data.Blueprint;
			}
			if (E.AsIfKnown || (E.Understood() && The.Player != null && (The.Player.HasSkill("Tinkering") || Scanning.HasScanningFor(The.Player, Scanning.Scan.Tech))))
			{
				SB.Append(": {{C|").Append(ObjectName).Append("}} <");
				Cost.Clear();
				Cost.Import(TinkerItem.GetBitCostFor(Data.Blueprint));
				ModifyBitCostEvent.Process(The.Player, Cost, "DataDisk");
				Cost.ToStringBuilder(SB);
				SB.Append('>');
			}
		}
		else if (Data.Type == "Mod")
		{
			ObjectName = "[{{W|Item mod}}] - {{C|" + Data.DisplayName + "}}";
			if (E.AsIfKnown || (E.Understood() && The.Player != null && (The.Player.HasSkill("Tinkering") || Scanning.HasScanningFor(The.Player, Scanning.Scan.Tech))))
			{
				SB.Append(": ").Append(ObjectName);
			}
		}
		if (SB.Length > 0)
		{
			E.AddBase(SB.ToString(), 5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Data != null)
		{
			if (Data.Type == "Mod")
			{
				E.Postfix.Append("\nAdds item modification: ").Append(ItemModding.GetModificationDescription(Data.Blueprint, 0));
			}
			else
			{
				GameObject gameObject = GameObject.CreateSample(Data.Blueprint);
				if (gameObject != null)
				{
					TinkeringHelpers.StripForTinkering(gameObject);
					TinkerItem part = gameObject.GetPart<TinkerItem>();
					Description part2 = gameObject.GetPart<Description>();
					E.Postfix.Append("\n{{rules|Creates:}} ");
					if (part != null && part.NumberMade > 1)
					{
						E.Postfix.Append(Grammar.Cardinal(part.NumberMade)).Append(' ').Append(Grammar.Pluralize(gameObject.DisplayNameOnlyDirect));
					}
					else
					{
						E.Postfix.Append(gameObject.DisplayNameOnlyDirect);
					}
					E.Postfix.Append("\n");
					if (part2 != null)
					{
						E.Postfix.Append('\n').Append(part2._Short);
					}
					gameObject.Obliterate();
				}
			}
			E.Postfix.Append("\n\n{{rules|Requires:}} ").Append(GetRequiredSkillHumanReadable());
			if (TinkerData.RecipeKnown(Data))
			{
				E.Postfix.Append("\n\n{{rules|You already know this recipe.}}");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (Data != null && ParentObject.Understood())
		{
			if (Data.Type == "Build")
			{
				E.AddAction("Build", "build", "BuildFromDataDisk", null, 'b', FireOnActor: false, (ParentObject.InInventory == E.Actor) ? 200 : 0);
			}
			E.AddAction("Learn", "learn", "LearnFromDataDisk", null, 'n', FireOnActor: false, (!TinkerData.RecipeKnown(Data)) ? 300 : 0);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "LearnFromDataDisk")
		{
			if (TinkerData.RecipeKnown(Data))
			{
				E.Actor.Fail("You already know that recipe!");
			}
			else if (!E.Actor.HasSkill(GetRequiredSkill()))
			{
				E.Actor.Fail("You don't have the required skill: " + GetRequiredSkillHumanReadable() + "!");
			}
			else
			{
				E.Actor.PlayWorldOrUISound("Sounds/Interact/sfx_interact_dataDisk_learn");
				if (Data.Type == "Mod")
				{
					Popup.Show("You learn the item modification {{W|" + Data.DisplayName + "}}.");
				}
				else
				{
					GameObject gameObject = GameObject.CreateSample(Data.Blueprint);
					gameObject.MakeUnderstood();
					if (E.Actor.IsPlayer())
					{
						Popup.Show("You learn to build " + (gameObject.IsPlural ? gameObject.DisplayNameOnlyDirect : Grammar.Pluralize(gameObject.DisplayNameOnlyDirect)) + ".");
					}
					gameObject.Destroy();
				}
				TinkerData.KnownRecipes.Add(Data);
				ParentObject.Destroy();
			}
		}
		else if (E.Command == "BuildFromDataDisk")
		{
			E.Actor.GetPart<XRL.World.Parts.Skill.Tinkering>();
			BitLocker bitLocker = E.Actor.RequirePart<BitLocker>();
			if (!(Data.Type != "Build"))
			{
				if (!E.Actor.HasSkill(GetRequiredSkill()))
				{
					E.Actor.Fail("You don't have the required skill: " + GetRequiredSkillHumanReadable() + "!");
				}
				else
				{
					string bitCostFor = TinkerItem.GetBitCostFor(Data.Blueprint);
					if (!bitLocker.HasBits(bitCostFor))
					{
						E.Actor.Fail("You don't have the required <" + BitType.GetString(bitCostFor) + "> bits! You have:\n\n" + bitLocker.GetBitsString());
					}
					else
					{
						bool flag = E.Actor.AreHostilesNearby();
						if (flag && E.Actor.FireEvent("CombatPreventsTinkering"))
						{
							E.Actor.Fail("You can't tinker with hostiles nearby!");
							return false;
						}
						if (!E.Actor.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
						{
							return false;
						}
						Inventory inventory = E.Actor.Inventory;
						GameObject gameObject2 = null;
						if (!Data.Ingredient.IsNullOrEmpty())
						{
							string[] array = Data.Ingredient.Split(',');
							foreach (string blueprint in array)
							{
								gameObject2 = inventory.FindObjectByBlueprint(blueprint);
								if (gameObject2 != null)
								{
									break;
								}
							}
							if (gameObject2 == null)
							{
								string text = "";
								array = Data.Ingredient.Split(',');
								foreach (string blueprint2 in array)
								{
									if (text != "")
									{
										text += " or ";
									}
									GameObject gameObject3 = GameObject.CreateSample(blueprint2);
									text += gameObject3.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, true);
									gameObject3.Obliterate();
								}
								E.Actor.Fail("You don't have the required ingredient: " + text + "!");
								goto IL_055a;
							}
						}
						if (!Data.Ingredient.IsNullOrEmpty())
						{
							gameObject2.SplitStack(1, E.Actor);
							if (!inventory.FireEvent(Event.New("CommandRemoveObject", "Object", gameObject2)))
							{
								E.Actor.Fail("You cannot use the ingredient!");
								goto IL_055a;
							}
						}
						bitLocker.UseBits(TinkerItem.GetBitCostFor(Data.Blueprint));
						GameObject gameObject4 = GameObject.CreateSample(Data.Blueprint);
						TinkeringHelpers.ProcessTinkeredItem(gameObject4, E.Actor);
						TinkerItem part = gameObject4.GetPart<TinkerItem>();
						GameObject gameObject5 = null;
						int j = 0;
						for (int num = Math.Max(part.NumberMade, 1); j < num; j++)
						{
							gameObject5 = GameObject.Create(Data.Blueprint);
							TinkeringHelpers.ProcessTinkeredItem(gameObject5, E.Actor);
							inventory.AddObject(gameObject5);
						}
						IComponent<GameObject>.PlayUISound("Sounds/Interact/sfx_interact_dataDisk_build");
						if (part.NumberMade > 1)
						{
							string displayName = gameObject4.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: true, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, true);
							DidX("tinker", "up " + Grammar.Cardinal(part.NumberMade) + " " + Grammar.Pluralize(displayName), "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
						}
						else
						{
							DidXToY("tinker", "up", gameObject5, null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
						}
						E.Actor.UseEnergy(1000, "Skill Tinkering Data Disk Build");
						if (flag)
						{
							E.RequestInterfaceExit();
						}
						gameObject4.Obliterate();
					}
				}
			}
		}
		goto IL_055a;
		IL_055a:
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Data == null)
		{
			if (!TargetBlueprint.IsNullOrEmpty())
			{
				foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
				{
					if (tinkerRecipe.Blueprint == TargetBlueprint)
					{
						Data = tinkerRecipe;
					}
				}
			}
			else
			{
				TinkerData tinkerData = null;
				int num = 0;
				TinkerData data = null;
				int num2 = 0;
				int maximumDataScore = GetMaximumDataScore();
				int num3 = maximumDataScore * 10;
				do
				{
					tinkerData = TinkerData.TinkerRecipes.GetRandomElement();
					int dataScore = GetDataScore(tinkerData);
					if (dataScore > num2)
					{
						data = tinkerData;
						num2 = dataScore;
					}
				}
				while (num2 < maximumDataScore && ++num < num3);
				if (num2 < maximumDataScore)
				{
					foreach (TinkerData item in new List<TinkerData>(TinkerData.TinkerRecipes).ShuffleInPlace())
					{
						int dataScore = GetDataScore(item);
						if (dataScore > num2)
						{
							data = item;
							num2 = dataScore;
							if (num2 >= maximumDataScore)
							{
								break;
							}
						}
					}
				}
				Data = data;
			}
		}
		if (Data != null)
		{
			Commerce part = ParentObject.GetPart<Commerce>();
			if (part != null)
			{
				if (Data.Cost.Contains("M"))
				{
					part.Value = 450.0;
				}
				else if (Data.Cost.Contains("Y"))
				{
					part.Value = 400.0;
				}
				else if (Data.Cost.Contains("W"))
				{
					part.Value = 350.0;
				}
				else if (Data.Cost.Contains("K"))
				{
					part.Value = 300.0;
				}
				else if (Data.Cost.Contains("c"))
				{
					part.Value = 250.0;
				}
				else if (Data.Cost.Contains("b"))
				{
					part.Value = 200.0;
				}
				else if (Data.Cost.Contains("g"))
				{
					part.Value = 150.0;
				}
				else if (Data.Cost.Contains("r"))
				{
					part.Value = 100.0;
				}
				else
				{
					part.Value = 50.0;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public static string GetRequiredSkill(int Tier)
	{
		if (Tier <= 3)
		{
			return "Tinkering_Tinker1";
		}
		if (Tier <= 6)
		{
			return "Tinkering_Tinker2";
		}
		return "Tinkering_Tinker3";
	}

	public string GetRequiredSkill()
	{
		return GetRequiredSkill(Data?.Tier ?? 0);
	}

	public static string GetRequiredSkillHumanReadable(int Tier)
	{
		if (Tier <= 3)
		{
			return "Tinker I";
		}
		if (Tier <= 6)
		{
			return "Tinker II";
		}
		return "Tinker III";
	}

	public string GetRequiredSkillHumanReadable()
	{
		return GetRequiredSkillHumanReadable(Data?.Tier ?? 0);
	}

	private int GetDataScore(TinkerData Data)
	{
		int num = 0;
		if (Data.Type == "Build")
		{
			GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(Data.Blueprint);
			if (blueprintIfExists == null)
			{
				return num;
			}
			if (TargetTier > 0)
			{
				num += 8 - Math.Abs(blueprintIfExists.Tier - TargetTier);
			}
			if (TargetTechTier > 0)
			{
				num += 8 - Math.Abs(blueprintIfExists.TechTier - TargetTechTier);
			}
		}
		if (TargetTinkerTier > 0 && Data.Tier == TargetTinkerTier)
		{
			num += 8 - Math.Abs(Data.Tier - TargetTinkerTier);
		}
		if (!TargetTinkerCategory.IsNullOrEmpty() && Data.Category == TargetTinkerCategory)
		{
			num += 16;
		}
		if (!TargetType.IsNullOrEmpty() && TargetType == Data.Type)
		{
			num += 32;
		}
		return num + 1;
	}

	private int GetMaximumDataScore()
	{
		int num = 1;
		if (TargetTier > 0)
		{
			num += 8;
		}
		if (TargetTechTier > 0)
		{
			num += 8;
		}
		if (TargetTinkerTier > 0)
		{
			num += 8;
		}
		if (!TargetTinkerCategory.IsNullOrEmpty())
		{
			num += 16;
		}
		if (!TargetType.IsNullOrEmpty())
		{
			num += 32;
		}
		return num;
	}
}
