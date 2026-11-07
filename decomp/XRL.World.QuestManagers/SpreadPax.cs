using System;
using System.Collections.Generic;
using System.Globalization;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace XRL.World.QuestManagers;

[Serializable]
public class SpreadPax : IQuestSystem
{
	public List<PaxQuestStep> Steps;

	public string MyQuestID;

	[NonSerialized]
	public static string[] paxPlaces = new string[11]
	{
		"Joppa", "Kyakukya", "Asphalt Mines", "Rusted Archway", "Rustwell", "Red Rock", "Grit Gate", "Six Day Stilt", "Golgotha", "Bethesda Susa",
		"Omonporch"
	};

	[NonSerialized]
	public static string[] paxPlacesExcludeAltStarts = new string[1] { "Joppa" };

	[NonSerialized]
	public static Dictionary<string, string> paxPlacesPreposition = new Dictionary<string, string>
	{
		{ "Rusted Archway", "at" },
		{ "Rustwell", "at" },
		{ "Six Day Stilt", "at" }
	};

	[NonSerialized]
	public static Dictionary<string, string> paxPlacesDisplay = new Dictionary<string, string>
	{
		{ "Asphalt Mines", "the Asphalt Mines" },
		{ "Rusted Archway", "the Rusted Archway" },
		{ "Rustwell", "the Rust Wells" },
		{ "Six Day Stilt", "the Six Day Stilt" }
	};

	[NonSerialized]
	public static Dictionary<string, string> paxPlacesAlias = new Dictionary<string, string> { { "Six Day Stilt", "Stiltgrounds" } };

	[NonSerialized]
	public static string questName = "Spread Klanq around Qud in 4 Ways of Your Choosing";

	private bool hasStepName(string s)
	{
		for (int i = 0; i < Steps.Count; i++)
		{
			if (Steps[i].Name == s)
			{
				return true;
			}
		}
		return false;
	}

	public void Check()
	{
		if (The.Player?.CurrentCell == null)
		{
			return;
		}
		for (int i = 0; i < Steps.Count; i++)
		{
			if (Steps[i].Finished)
			{
				continue;
			}
			if (Steps[i].Target.StartsWith("Underground:"))
			{
				int num = int.Parse(Steps[i].Target.Split(':')[1]) + 10;
				Zone currentZone = The.Player.CurrentZone;
				if (currentZone != null && currentZone.Z >= num)
				{
					The.Game.FinishQuestStep(MyQuestID, Steps[i].Name);
					Steps[i].Finished = true;
				}
			}
			else if (Steps[i].Target.StartsWith("Faction:"))
			{
				string key = Steps[i].Target.Split(':')[1];
				foreach (Cell localAdjacentCell in The.Player.CurrentCell.GetLocalAdjacentCells(3, IncludeSelf: true))
				{
					for (int j = 0; j < localAdjacentCell.Objects.Count; j++)
					{
						AllegianceSet allegianceSet = localAdjacentCell.Objects[j].Brain?.GetBaseAllegiance();
						if (allegianceSet != null && allegianceSet.TryGetValue(key, out var Value) && Value > 0)
						{
							The.Game.FinishQuestStep(MyQuestID, Steps[i].Name);
							Steps[i].Finished = true;
							goto end_IL_01c4;
						}
					}
					continue;
					end_IL_01c4:
					break;
				}
			}
			else if (Steps[i].Target.StartsWith("Item:"))
			{
				string text = Steps[i].Target.Split(':')[1];
				foreach (Cell localAdjacentCell2 in The.Player.CurrentCell.GetLocalAdjacentCells(3, IncludeSelf: true))
				{
					for (int k = 0; k < localAdjacentCell2.Objects.Count; k++)
					{
						if (localAdjacentCell2.Objects[k].Blueprint == text || RandomAltarBaetyl.DisplayNameMatches(localAdjacentCell2.Objects[k].Blueprint, text))
						{
							The.Game.FinishQuestStep(MyQuestID, Steps[i].Name);
							Steps[i].Finished = true;
							goto end_IL_02d8;
						}
					}
					continue;
					end_IL_02d8:
					break;
				}
			}
			else if (Steps[i].Target.StartsWith("Place:") && The.Player.CurrentZone != null)
			{
				string text2 = Steps[i].Target.Split(':')[1];
				string text3 = WorldFactory.Factory.ZoneDisplayName(The.Player.CurrentZone.ZoneID);
				if (text3.Contains(text2, CompareOptions.IgnoreCase) || (paxPlacesAlias.TryGetValue(text2, out var value) && text3.Contains(value, CompareOptions.IgnoreCase)))
				{
					The.Game.FinishQuestStep(MyQuestID, Steps[i].Name);
					Steps[i].Finished = true;
				}
			}
		}
		int num2 = 0;
		for (int l = 0; l < Steps.Count; l++)
		{
			if (Steps[l].Finished)
			{
				num2++;
			}
		}
		if (num2 >= 4)
		{
			The.Game.FinishQuest(MyQuestID);
		}
	}

	public void Init()
	{
		if (Steps != null)
		{
			return;
		}
		Steps = new List<PaxQuestStep>();
		Stat.ReseedFrom("SpreadPax");
		List<string> list = new List<string>(paxPlaces);
		if (!The.Game.GetStringGameState("embark").Contains("Joppa"))
		{
			list.RemoveAll(paxPlacesExcludeAltStarts.Contains);
		}
		int num = 6;
		while (Steps.Count < num)
		{
			PaxQuestStep paxQuestStep = new PaxQuestStep();
			switch (Stat.Random(1, 4))
			{
			case 1:
				paxQuestStep.Name = "Spread Klanq Deep in the Earth";
				paxQuestStep.Target = "Underground:20";
				paxQuestStep.Text = "Puff Klanq spores at a depth of at least 20 levels.";
				break;
			case 2:
			{
				Faction randomFactionWithAtLeastOneMember = Factions.GetRandomFactionWithAtLeastOneMember((Faction f) => !f.Name.Contains("villagers of"));
				TextInfo textInfo2 = new CultureInfo("en-US", useUserOverride: false).TextInfo;
				paxQuestStep.Name = "Spread Klanq to " + textInfo2.ToTitleCase(randomFactionWithAtLeastOneMember.DisplayName);
				paxQuestStep.Target = "Faction:" + randomFactionWithAtLeastOneMember.Name;
				paxQuestStep.Text = "Puff Klanq spores on a sentient member of the " + randomFactionWithAtLeastOneMember.DisplayName + " faction.";
				break;
			}
			case 3:
			{
				string anObjectBlueprint = EncountersAPI.GetAnObjectBlueprint((GameObjectBlueprint ob) => ob.GetPartParameter("Physics", "IsReal", Default: true) && ob.GetPartParameter("Physics", "Takeable", Default: true) && !ob.HasPart("Brain") && !ob.HasPart("Combat") && !ob.HasTag("NoSparkingQuest"));
				GameObject gameObject = GameObject.CreateSample(anObjectBlueprint);
				TextInfo textInfo = new CultureInfo("en-US", useUserOverride: false).TextInfo;
				paxQuestStep.Name = "Spread Klanq onto " + textInfo.ToTitleCase(gameObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true));
				paxQuestStep.Target = "Item:" + anObjectBlueprint;
				paxQuestStep.Text = "Puff Klanq spores onto " + gameObject.GetReferenceDisplayName(int.MaxValue, null, null, NoColor: false, Stripped: true, ColorOnly: false, WithoutTitles: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true);
				break;
			}
			case 4:
			{
				string randomElement = list.GetRandomElement();
				if (!paxPlacesDisplay.TryGetValue(randomElement, out var value))
				{
					value = randomElement;
				}
				if (!paxPlacesPreposition.TryGetValue(randomElement, out var value2))
				{
					value2 = "in";
				}
				paxQuestStep.Name = "Spread Klanq " + value2 + " " + value;
				paxQuestStep.Target = "Place:" + randomElement;
				paxQuestStep.Text = "Puff Klanq spores in the vicinity of " + value + ".";
				break;
			}
			}
			if (!hasStepName(paxQuestStep.Name))
			{
				Steps.Add(paxQuestStep);
			}
		}
	}

	public override void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
		Registrar.Register(PooledEvent<CommandEvent>.ID);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "ActivatePaxInfection")
		{
			Check();
		}
		return base.HandleEvent(E);
	}

	public static bool StartQuest()
	{
		try
		{
			Quest quest = new Quest();
			SpreadPax spreadPax = The.Game.RequireSystem<SpreadPax>();
			spreadPax.Init();
			quest.ID = Guid.NewGuid().ToString();
			quest.System = spreadPax;
			quest.Name = questName;
			quest.Level = 25;
			quest.Finished = false;
			quest.Accomplishment = "Conspiring with its eponymous mushroom scientist, you spread Klanq throughout Qud.";
			quest.Hagiograph = "Bless the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", when =name= cemented a historic alliance with the godhead Klanq and the two became one! Together the being known as Klanq-=name= puffed the Royal Vapor into every nook and crevice of Qud.";
			quest.HagiographCategory = "DoesSomethingRad";
			quest.StepsByID = new Dictionary<string, QuestStep>();
			spreadPax.MyQuestID = quest.ID;
			for (int i = 0; i < spreadPax.Steps.Count; i++)
			{
				QuestStep questStep = new QuestStep();
				questStep.ID = Guid.NewGuid().ToString();
				questStep.Name = spreadPax.Steps[i].Name;
				questStep.Finished = false;
				questStep.Text = spreadPax.Steps[i].Text;
				questStep.XP = 1500;
				spreadPax.Steps[i].Name = questStep.ID;
				quest.StepsByID.Add(questStep.ID, questStep);
			}
			The.Game.StartQuest(quest, "Pax Klanq");
		}
		catch (Exception x)
		{
			MetricsManager.LogException("SpreadPax.StartQuest", x);
		}
		return true;
	}

	public override void Finish()
	{
		Body body = The.Player.Body;
		if (body == null)
		{
			return;
		}
		foreach (BodyPart item in body.LoopParts())
		{
			if (!(item.Equipped?.Blueprint != "PaxInfection") && item.Equipped.Destroy(null, Silent: true))
			{
				string ordinalName = item.GetOrdinalName();
				string possessiveAdjective = The.Player.GetPronounProvider().PossessiveAdjective;
				Popup.Show("The infected crust of skin on your " + ordinalName + " loosens and breaks away.");
				JournalAPI.AddAccomplishment("To the dismay of fungi everywhere, you cured the Pax Klanq infection on your " + ordinalName + ".", "Bless the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", when =name= dissolved a sham alliance with the treacherous fungus Klanq by eradicating it from " + possessiveAdjective + " " + ordinalName + "!", "While traveling around " + The.Player.CurrentZone.GetTerrainDisplayName() + ", =name= stumbled upon a clan of fungi performing a secret ritual. Because of  " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " <spice.elements." + The.Player.GetMythicDomain() + ".quality.!random>, they furiously rebuked " + The.Player.GetPronounProvider().Objective + " and dissolved the Klanq on " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " " + ordinalName + ".", null, "general", MuralCategory.BodyExperienceNeutral, MuralWeight.Medium, null, -1L);
			}
		}
	}

	public override GameObject GetInfluencer()
	{
		return GameObject.FindByBlueprint("Pax Klanq");
	}
}
