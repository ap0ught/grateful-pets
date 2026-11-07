using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.API;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Tinkering;
using XRL.World.ZoneParts;

namespace XRL.World.Conversations.Parts;

public class KithAndKinFinale : IKithAndKinPart
{
	private HindrenQuestOutcome Outcome;

	private string Key;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != EnteredElementEvent.ID && ID != LeftElementEvent.ID && ID != PrepareTextEvent.ID)
		{
			return ID == GetTextElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		Outcome = new HindrenQuestOutcome
		{
			loveState = ((The.Game.GetQuestFinishTime("Love and Fear") > 0) ? "love" : "nolove"),
			thief = Thief,
			circumstance = CircumstanceInfluence,
			motive = MotiveInfluence
		};
		The.Game.SetStringGameState("HindrenMysteryOutcomeClimate", IKithAndKinPart.KeyOf(Outcome.DetermineClimate()));
		The.Game.SetStringGameState("HindrenMysteryOutcomeCircumstance", IKithAndKinPart.KeyOf(Outcome.circumstance));
		The.Game.SetStringGameState("HindrenMysteryOutcomeThief", IKithAndKinPart.KeyOf(Outcome.thief));
		The.Game.SetStringGameState("HindrenMysteryOutcomeHindriarch", IKithAndKinPart.KeyOf(Outcome.CurrentHindriarch()));
		The.Game.RemoveStringGameState("KithAndKinEliminated");
		Key = IKithAndKinPart.KeyOf(Outcome.thief) + IKithAndKinPart.KeyOf(Outcome.climate) + IKithAndKinPart.KeyOf(Outcome.loveState);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftElementEvent E)
	{
		KithAndKinGameState.Instance.getBeyLahZone().AddPart(Outcome);
		The.Game.FinishQuestStep("Kith and Kin", "Accuse the thief");
		The.Game.FinishQuest("Find Eskhind");
		Outcome.ApplyPrologue();
		Reward();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTextElementEvent E)
	{
		E.Selected = E.Visible.FirstOrDefault((ConversationText x) => x.ID == Key) ?? E.Selected;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		string circumstanceInfluence = CircumstanceInfluence;
		string motiveInfluence = MotiveInfluence;
		string value = ((circumstanceInfluence == motiveInfluence) ? circumstanceInfluence : (circumstanceInfluence + " and " + motiveInfluence));
		E.Text.StartReplace().AddReplacer("all.influence", value).AddReplacer("motive.influence", motiveInfluence)
			.AddReplacer("thief.name", ThiefName)
			.Execute();
		return base.HandleEvent(E);
	}

	public void Reward()
	{
		_ = Outcome.climate;
		string motive = Outcome.motive;
		string circumstance = Outcome.circumstance;
		string thief = Outcome.thief;
		List<GameObject> list = new List<GameObject>();
		List<string> list2 = new List<string>();
		bool flag = false;
		switch (thief)
		{
		case "keh":
			switch (motive)
			{
			case "violence":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Dagger3"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Dagger3"));
				}
				break;
			case "illness":
			{
				int num7 = 4;
				if (circumstance == "violence")
				{
					num7 = 8;
				}
				for (int num8 = 0; num8 < num7; num8++)
				{
					list.Add(GameObject.Create(PopulationManager.RollOneFrom("DynamicObjectsTable:Tonics_NonRare").Blueprint));
				}
				break;
			}
			case "craft":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Homoelectric Wrist Warmer"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Homoelectric Wrist Warmer"));
				}
				break;
			case "trade":
			{
				int num5 = 3;
				if (circumstance == "violence")
				{
					num5 = 8;
				}
				if (circumstance == "trade")
				{
					num5 = 6;
					flag = true;
				}
				for (int num6 = 0; num6 < num5; num6++)
				{
					list.Add(GameObject.Create(PopulationManager.RollOneFrom("BooksAndRandomBooks").Blueprint));
				}
				break;
			}
			}
			break;
		case "esk":
			switch (motive)
			{
			case "violence":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Chain Pistol"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Chain Pistol"));
				}
				break;
			case "illness":
				list.Add(GameObjectFactory.Factory.CreateObject("FluxPhial"));
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject("FluxPhial"));
				}
				break;
			case "craft":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Magnetized Boots"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Magnetized Boots"));
				}
				break;
			case "trade":
			{
				int num3 = 3;
				for (int num4 = 0; num4 < num3; num4++)
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints[PopulationManager.RollOneFrom("DynamicObjectsTable:EnergyCells:Tier0-4").Blueprint], 0, 2));
				}
				break;
			}
			}
			break;
		case "kese":
			switch (motive)
			{
			case "violence":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Ari"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Ari"));
				}
				if (circumstance == "craft")
				{
					list2.Add("mod:ModFlaming");
				}
				break;
			case "illness":
				BodyPart.MakeSeveredBodyParts((circumstance == "violence") ? 4 : 2, null, "Face", null, null, null, null, list);
				break;
			case "craft":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Carbide Plate Armor", 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Carbide Plate Armor"));
				}
				break;
			case "trade":
			{
				int num = ((circumstance == "violence") ? 8 : 4);
				for (int num2 = 0; num2 < num; num2++)
				{
					list.Add(GameObject.Create(PopulationManager.RollOneFrom("DynamicObjectsTable:Grenades").Blueprint));
				}
				break;
			}
			}
			break;
		case "kendren":
			switch (motive)
			{
			case "violence":
			{
				List<GameObject> list4 = new List<GameObject>();
				while (list4.Count < 3)
				{
					string blueprint2 = EncountersAPI.GetAnItemBlueprint((GameObjectBlueprint b) => b.Tier == 3 && b.DescendsFrom("MeleeWeapon") && EncountersAPI.IsEligibleForDynamicEncounters(b));
					if (!list4.Any((GameObject o) => o.Blueprint == blueprint2))
					{
						if (circumstance == "violence")
						{
							list4.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints[blueprint2], 0, 2));
						}
						else
						{
							list4.Add(GameObjectFactory.Factory.Blueprints[blueprint2].createOne());
						}
					}
				}
				list4.ForEach((GameObject o) =>
				{
					o.MakeUnderstood();
				});
				list.Add(ConversationsAPI.chooseOneItem(list4, "Choose an item", allowEscape: false));
				break;
			}
			case "illness":
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints["Bio-Scanning Bracelet"], 0, 2));
				}
				else
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Bio-Scanning Bracelet"));
				}
				break;
			case "craft":
			{
				List<GameObject> list3 = new List<GameObject>();
				while (list3.Count < 3)
				{
					string blueprint = EncountersAPI.GetAnItemBlueprint((GameObjectBlueprint b) => b.Tier == 3 && b.DescendsFrom("Armor") && EncountersAPI.IsEligibleForDynamicEncounters(b));
					if (!list3.Any((GameObject o) => o.Blueprint == blueprint))
					{
						if (circumstance == "violence")
						{
							list3.Add(GameObjectFactory.Factory.CreateObject(GameObjectFactory.Factory.Blueprints[blueprint], 0, 2));
						}
						else
						{
							list3.Add(GameObjectFactory.Factory.Blueprints[blueprint].createOne());
						}
					}
				}
				list3.ForEach((GameObject o) =>
				{
					o.MakeUnderstood();
				});
				list.Add(ConversationsAPI.chooseOneItem(list3, "Choose an item", allowEscape: false));
				break;
			}
			case "trade":
				list.Add(GameObjectFactory.Factory.CreateObject("Hoversled"));
				if (circumstance == "violence")
				{
					list.Add(GameObjectFactory.Factory.CreateObject("Hoversled"));
				}
				break;
			}
			break;
		}
		if (circumstance == "illness")
		{
			for (int num9 = 0; num9 < 4; num9++)
			{
				list.Add(GameObject.Create(PopulationManager.RollOneFrom("DynamicObjectsTable:Tonics_NonRare").Blueprint));
			}
		}
		if (circumstance == "trade" && !flag)
		{
			List<GameObject> list5 = new List<GameObject>();
			foreach (GameObject item in list)
			{
				list5.Add(item.DeepCopy(CopyEffects: false, CopyID: true));
			}
			list.AddRange(list5);
		}
		list.ForEach((GameObject o) =>
		{
			o.MakeUnderstood();
		});
		list.Sort((GameObject o1, GameObject o2) => string.Compare(o1.DisplayNameOnlyDirect, o2.DisplayNameOnlyDirect, StringComparison.Ordinal));
		StringBuilder stringBuilder = Event.NewStringBuilder();
		int num10 = 0;
		stringBuilder.AppendLine("In return for your service, you receive:\n");
		foreach (GameObject item2 in list)
		{
			stringBuilder.AppendLine(item2.DisplayName);
			The.Player.ReceiveObject(item2);
			if (circumstance == "craft" && (!(thief == "kese") || !(motive == "violence")))
			{
				if (item2.HasPart<TinkerItem>() && item2.GetPart<TinkerItem>().CanBuild)
				{
					list2.Add(item2.Blueprint);
				}
				else
				{
					num10 = 1;
				}
			}
		}
		if (num10 > 0)
		{
			stringBuilder.AppendLine("failed energy relay x" + num10);
			stringBuilder.AppendLine("fried processing core x" + num10);
			stringBuilder.AppendLine("cracked robotics housing x" + num10 * 2);
			for (int num11 = 0; num11 < num10; num11++)
			{
				The.Player.ReceiveObject("Scrap 1");
			}
			for (int num12 = 0; num12 < num10; num12++)
			{
				The.Player.ReceiveObject("Scrap 2");
			}
			for (int num13 = 0; num13 < num10 * 2; num13++)
			{
				The.Player.ReceiveObject("Scrap 2");
			}
		}
		if (list2.Count > 0)
		{
			foreach (string item3 in list2)
			{
				GameObject gameObject = TinkerData.createDataDisk(item3);
				gameObject.MakeUnderstood();
				The.Player.ReceiveObject(gameObject);
				stringBuilder.AppendLine(gameObject.DisplayName);
			}
		}
		Popup.Show(stringBuilder.ToString());
	}
}
