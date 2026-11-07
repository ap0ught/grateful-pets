using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ConsoleLib.Console;
using Genkit;
using HistoryKit;
using JoppaTutorial;
using Qud.API;
using Sheeter;
using UnityEngine;
using XRL.Annals;
using XRL.Core;
using XRL.Language;
using XRL.Liquids;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Anatomy;
using XRL.World.Conversations.Parts;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using XRL.World.QuestManagers;
using XRL.World.Quests;
using XRL.World.Quests.GolemQuest;
using XRL.World.Skills.Cooking;
using XRL.World.Tinkering;
using XRL.World.ZoneBuilders;
using XRL.World.ZoneParts;

namespace XRL.World.Capabilities;

public static class Wishing
{
	private static List<StringBuilder> memtest = new List<StringBuilder>();

	public static void HandleWish(GameObject who, string Wish)
	{
		if (WishManager.HandleWish(Wish))
		{
			return;
		}
		XRLGame game = The.Game;
		if (Wish.StartsWith("conv:"))
		{
			GameObject gameObject = null;
			string[] array = Wish.Split(':');
			ConversationUI.HaveConversation(Speaker: (array.Length < 3) ? GameObjectFactory.Factory.CreateObject("BaseHumanoid") : GameObjectFactory.Factory.CreateObject(array[2]), ConversationID: array[1]);
			return;
		}
		if (Wish.StartsWith("startquest:"))
		{
			The.Game.StartQuest(Wish.Split(':')[1]);
			return;
		}
		if (Wish.StartsWith("completequest:"))
		{
			The.Game.CompleteQuest(Wish.Split(':')[1]);
			return;
		}
		if (Wish.StartsWith("questdebug"))
		{
			while (true)
			{
				List<string> list = new List<string>();
				List<Quest> list2 = new List<Quest>();
				foreach (Quest value9 in The.Game.Quests.Values)
				{
					if (!The.Game.FinishedQuests.ContainsKey(value9.ID))
					{
						list2.Add(value9);
						list.Add(value9.Name);
					}
				}
				int num = Popup.PickOption("<Quest Debug>", null, "", "Sounds/UI/ui_notification", list.ToArray(), null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
				if (num < 0)
				{
					break;
				}
				while (true)
				{
					list = new List<string> { "info", "complete", "steps" };
					int num2 = Popup.PickOption("Debug: " + list2[num].Name, null, "", "Sounds/UI/ui_notification", list.ToArray(), null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
					if (num2 == 0)
					{
						Popup.Show(list2[num].ToString());
						break;
					}
					if (num2 == 1)
					{
						game.CompleteQuest(list2[num].ID);
						break;
					}
					if (num2 != 2)
					{
						break;
					}
					list = new List<string>();
					List<QuestStep> list3 = new List<QuestStep>();
					foreach (KeyValuePair<string, QuestStep> item in list2[num].StepsByID)
					{
						list3.Add(item.Value);
						list.Add(item.Key);
					}
					int num3 = Popup.PickOption("Pick step from " + list2[num].Name, null, "", "Sounds/UI/ui_notification", list.ToArray(), null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
					if (num3 < 0)
					{
						continue;
					}
					int num4;
					while (true)
					{
						list = new List<string> { "info", "finish" };
						num4 = Popup.PickOption("Debug step " + list3[num3], null, "", "Sounds/UI/ui_notification", list.ToArray(), null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
						if (num4 != 0)
						{
							break;
						}
						Popup.Show(list3[num3].ToString());
					}
					if (num4 == 1)
					{
						game.FinishQuestStep(list2[num].ID, list3[num3].ID);
					}
				}
			}
			return;
		}
		if (Wish.StartsWith("finishallquests"))
		{
			foreach (KeyValuePair<string, Quest> quest in The.Game.Quests)
			{
				quest.Value.Finish();
			}
			return;
		}
		if (Wish.StartsWith("finishqueststep:"))
		{
			The.Game.FinishQuestStep(Wish.Split(':')[1], Wish.Split(':')[2]);
			return;
		}
		if (Wish.StartsWith("geno:"))
		{
			Zone currentZone = who.CurrentZone;
			string cmp = Wish.Split(':')[1];
			for (int i = 0; i < currentZone.Height; i++)
			{
				for (int j = 0; j < currentZone.Width; j++)
				{
					foreach (GameObject item2 in currentZone.GetCell(j, i).GetObjectsWithPart("Physics"))
					{
						if (item2.Blueprint.EqualsNoCase(cmp))
						{
							item2.Obliterate();
						}
						else if (item2.DisplayNameOnlyStripped.EqualsNoCase(cmp))
						{
							item2.Obliterate();
						}
						else if (item2.DisplayNameStripped.EqualsNoCase(cmp))
						{
							item2.Obliterate();
						}
					}
				}
			}
			return;
		}
		if (Wish.StartsWith("deathgeno:"))
		{
			Zone currentZone2 = who.CurrentZone;
			string cmp2 = Wish.Split(':')[1];
			for (int k = 0; k < currentZone2.Height; k++)
			{
				for (int l = 0; l < currentZone2.Width; l++)
				{
					foreach (GameObject item3 in currentZone2.GetCell(l, k).GetObjectsWithPart("Physics"))
					{
						if (item3.Blueprint.EqualsNoCase(cmp2))
						{
							item3.Die(who, "wished dead");
						}
						else if (item3.DisplayNameOnlyStripped.EqualsNoCase(cmp2))
						{
							item3.Die(who, "wished dead");
						}
						else if (item3.DisplayNameStripped.EqualsNoCase(cmp2))
						{
							item3.Die(who, "wished dead");
						}
					}
				}
			}
			return;
		}
		if (Wish == "destroy")
		{
			The.Player.Physics.PickDestinationCell(9999, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Destroy which object?")?.GetHighestRenderLayerObject().Destroy();
			return;
		}
		if (Wish == "obliterate")
		{
			The.Player.Physics.PickDestinationCell(9999, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Obliterate which object?")?.GetHighestRenderLayerObject().Obliterate();
			return;
		}
		if (Wish == "showcharset")
		{
			string[] array2 = new string[255];
			for (int m = 1; m <= 255; m++)
			{
				string s = ((m != 10) ? (((char)m).ToString() ?? "") : "(newline)");
				array2[m - 1] = m.ToString().PadLeft(3, 'ÿ') + ": " + ConsoleLib.Console.ColorUtility.EscapeFormatting(s);
			}
			Popup.PickOption("charset", null, "", "Sounds/UI/ui_notification", array2, null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
			return;
		}
		if (Wish == "die")
		{
			The.Player.Die(The.Player, "wished dead");
			return;
		}
		if (Wish == "collapseinterior")
		{
			InteriorZone.Active?.ParentObject?.Die(who, "wished dead");
			return;
		}
		if (Wish.StartsWith("allaggroxp"))
		{
			int num5 = 0;
			Zone parentZone = who.CurrentCell.ParentZone;
			for (int n = 0; n < parentZone.Height; n++)
			{
				for (int num6 = 0; num6 < parentZone.Width; num6++)
				{
					foreach (GameObject item4 in parentZone.GetCell(num6, n).GetObjectsWithPart("Brain"))
					{
						if (item4.Brain.IsHostileTowards(The.Player))
						{
							num5 += item4.AwardXPTo(The.Player, ForKill: true, null, MockAward: true);
						}
					}
				}
			}
			MessageQueue.AddPlayerMessage("Total xp: " + num5);
			return;
		}
		if (Wish.StartsWith("testhero:"))
		{
			GameObject gameObject2 = GameObjectFactory.Factory.CreateObject(Wish.Split(':')[1]);
			HeroMaker.MakeHero(gameObject2);
			who.CurrentCell.GetCellFromDirection("E").AddObject(gameObject2).MakeActive();
			return;
		}
		if (Wish.StartsWith("testwarden:"))
		{
			GameObject gameObject3 = GameObjectFactory.Factory.CreateObject(Wish.Split(':')[1]);
			HeroMaker.MakeHero(gameObject3);
			string text = HistoricStringExpander.ExpandString("<spice.villages.warden.introDialog.!random>");
			gameObject3.SetIntProperty("SuppressSimpleConversation", 1);
			ConversationsAPI.addSimpleConversationToObject(gameObject3, text, "Live and drink.", null, null, null, true);
			TakeOnRoleEvent.Send(gameObject3, "Warden");
			gameObject3.RequirePart<Interesting>();
			gameObject3.SetIntProperty("VillageWarden", 1);
			gameObject3.SetIntProperty("NamedVillager", 1);
			gameObject3.Brain.Mobile = true;
			gameObject3.Brain.Factions = "";
			gameObject3.Brain.Allegiance.Clear();
			gameObject3.Brain.Allegiance.Add("Wardens", 100);
			gameObject3.Brain.Allegiance.Hostile = false;
			gameObject3.Brain.Allegiance.Calm = true;
			who.CurrentCell.GetCellFromDirection("E").AddObject(gameObject3).MakeActive();
			return;
		}
		if (Wish.StartsWith("animatedhero"))
		{
			GameObject anAnimatedObject = EncountersAPI.GetAnAnimatedObject();
			HeroMaker.MakeHero(anAnimatedObject);
			who.CurrentCell.GetCellFromDirection("E").AddObject(anAnimatedObject).MakeActive();
			return;
		}
		if (Wish.StartsWith("showstringproperty:"))
		{
			string text2 = Wish.Split(':')[1];
			if (who.HasStringProperty(text2))
			{
				Popup.Show(who.GetStringProperty(text2));
			}
			else
			{
				Popup.Show("no string property '" + text2 + "' found");
			}
			return;
		}
		if (Wish.StartsWith("setintproperty "))
		{
			string[] array3 = Wish.Split(' ');
			who.SetIntProperty(array3[1], Convert.ToInt32(array3[2]));
			return;
		}
		if (Wish.StartsWith("pushgameview "))
		{
			string newView = Wish.Split(' ')[1];
			GameManager.Instance.PushGameView(newView);
			return;
		}
		if (Wish.StartsWith("showintproperty:"))
		{
			string text3 = Wish.Split(':')[1];
			if (who.HasIntProperty(text3))
			{
				Popup.Show(who.GetIntProperty(text3).ToString());
			}
			else
			{
				Popup.Show("no int property '" + text3 + "' found");
			}
			return;
		}
		if (Wish == "fire")
		{
			Cell cell = who.Physics.PickDirection("Fire");
			if (cell != null)
			{
				GameObject gameObject4 = cell.GetCombatTarget(who, IgnoreFlight: true) ?? cell.GetCombatTarget(who, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
				if (gameObject4 != null && gameObject4.Physics != null)
				{
					gameObject4.Physics.Temperature = gameObject4.Physics.FlameTemperature + 200;
				}
			}
			return;
		}
		if (Wish == "zap")
		{
			Cell cell2 = who.Physics.PickDirection("Zap");
			if (cell2 != null)
			{
				who.Discharge(cell2, 3, 0, "5d6", null, who, who);
			}
			return;
		}
		if (Wish == "gas")
		{
			foreach (Cell cell13 in The.PlayerCell.ParentZone.GetCells())
			{
				cell13.AddObject("MiasmaticAsh200");
			}
			return;
		}
		if (Wish == "gas2")
		{
			foreach (Cell cell14 in The.PlayerCell.ParentZone.GetCells())
			{
				cell14.AddObject("GlitterGas80");
			}
			return;
		}
		if (Wish == "gas3")
		{
			foreach (Cell cell15 in The.PlayerCell.ParentZone.GetCells())
			{
				cell15.AddObject("SaltyWaterExtraDeepPool");
			}
			return;
		}
		if (Wish == "gates")
		{
			List<List<string>> list4 = The.Game.GetObjectGameState("JoppaWorldTeleportGate2Rings") as List<List<string>>;
			List<List<string>> list5 = The.Game.GetObjectGameState("JoppaWorldTeleportGate3Rings") as List<List<string>>;
			List<List<string>> list6 = The.Game.GetObjectGameState("JoppaWorldTeleportGate4Rings") as List<List<string>>;
			List<string> list7 = The.Game.GetObjectGameState("JoppaWorldTeleportGateSecants") as List<string>;
			List<string> list8 = The.Game.GetObjectGameState("JoppaWorldTeleportGateZones") as List<string>;
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (list4 != null && list4.Count > 0)
			{
				stringBuilder.Compound("2-rings:\n", "\n");
				foreach (List<string> item5 in list4)
				{
					stringBuilder.Append('\n').Append(The.ZoneManager.GetZoneProperty(item5[0], "TeleportGateName") ?? "").Append(' ')
						.Append(item5[0])
						.Append(The.ZoneManager.HasZoneProperty(item5[0], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item5[0], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item5[0], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item5[0]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item5[1], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item5[1])
						.Append(The.ZoneManager.HasZoneProperty(item5[1], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item5[1], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item5[1], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item5[1]))
						.Append('\n');
				}
			}
			else
			{
				stringBuilder.Compound("No 2-rings found!\n", '\n');
			}
			if (list5 != null && list5.Count > 0)
			{
				stringBuilder.Compound("3-rings:\n", "\n");
				foreach (List<string> item6 in list5)
				{
					stringBuilder.Append('\n').Append(The.ZoneManager.GetZoneProperty(item6[0], "TeleportGateName") ?? "").Append(' ')
						.Append(item6[0])
						.Append(The.ZoneManager.HasZoneProperty(item6[0], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item6[0], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item6[0], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item6[0]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item6[1], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item6[1])
						.Append(The.ZoneManager.HasZoneProperty(item6[1], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item6[1], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item6[1], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item6[1]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item6[2], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item6[2])
						.Append(The.ZoneManager.HasZoneProperty(item6[2], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item6[2], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item6[2], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item6[2]))
						.Append('\n');
				}
			}
			else
			{
				stringBuilder.Compound("No 3-rings found!\n", '\n');
			}
			if (list6 != null && list6.Count > 0)
			{
				stringBuilder.Compound("4-rings:\n", "\n");
				foreach (List<string> item7 in list6)
				{
					stringBuilder.Append('\n').Append(The.ZoneManager.GetZoneProperty(item7[0], "TeleportGateName") ?? "").Append(' ')
						.Append(item7[0])
						.Append(The.ZoneManager.HasZoneProperty(item7[0], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item7[0], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item7[0], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item7[0]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item7[1], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item7[1])
						.Append(The.ZoneManager.HasZoneProperty(item7[1], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item7[1], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item7[1], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item7[1]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item7[2], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item7[2])
						.Append(The.ZoneManager.HasZoneProperty(item7[2], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item7[2], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item7[2], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item7[2]))
						.Append('\n')
						.Append(The.ZoneManager.GetZoneProperty(item7[3], "TeleportGateName") ?? "")
						.Append(' ')
						.Append(item7[3])
						.Append(The.ZoneManager.HasZoneProperty(item7[3], "TeleportGateCandidateNameRoot") ? " (" : "")
						.Append(The.ZoneManager.GetZoneProperty(item7[3], "TeleportGateCandidateNameRoot") ?? "")
						.Append(The.ZoneManager.HasZoneProperty(item7[3], "TeleportGateCandidateNameRoot") ? ")" : "")
						.Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item7[3]))
						.Append('\n');
				}
			}
			else
			{
				stringBuilder.Compound("No 4-rings found!\n", '\n');
			}
			if (list7 != null && list7.Count > 0)
			{
				stringBuilder.Compound("Secants:\n", "\n");
				foreach (string item8 in list7)
				{
					stringBuilder.Append('\n').Append(item8).Append(' ')
						.Append(The.ZoneManager.GetZoneDisplayName(item8))
						.Append('\n');
					string text4 = The.ZoneManager.GetZoneProperty(item8, "TeleportGateDestinationZone") as string;
					if (!string.IsNullOrEmpty(text4))
					{
						stringBuilder.Append("to ").Append(text4).Append(' ')
							.Append(The.ZoneManager.GetZoneDisplayName(text4))
							.Append('\n');
					}
					else
					{
						stringBuilder.Append("no destination set\n");
					}
				}
			}
			else
			{
				stringBuilder.Compound("No secants found!\n", '\n');
			}
			if (list8 != null && list8.Count > 0)
			{
				stringBuilder.Compound(list8.Count, "\n").Append(" total gate zones in the above:\n\n");
				foreach (string item9 in list8)
				{
					stringBuilder.Append(item9).Append(' ').Append(The.ZoneManager.GetZoneDisplayName(item9))
						.Append('\n');
				}
			}
			else
			{
				stringBuilder.Compound("No gate zones found!\n", '\n');
			}
			Popup.Show(stringBuilder.ToString());
			return;
		}
		if (Wish == "sparks")
		{
			who.Sparksplatter();
			return;
		}
		if (Wish == "groundliquid")
		{
			Cell currentCell = The.Player.CurrentCell;
			if (currentCell != null)
			{
				Popup.Show("[" + (currentCell.GroundLiquid ?? "null") + "]");
			}
			return;
		}
		if (Wish == "testmarkup")
		{
			MessageQueue.AddPlayerMessage(Markup.Transform("{{blue|blue blue blue blue blue blue blue blue blue blue blue {{rainbow|rainbow rainbow {{random|random1}} {{random|random2}} rainbow rainbow rainbow}} blue blue}} gray gray"));
			return;
		}
		if (Wish == "showcooldownminima")
		{
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			int num7 = 20;
			int num8 = 10;
			while (num7 <= 600)
			{
				int num9 = ActivatedAbilities.MinimumValueForCooldown(num7);
				stringBuilder2.Append(num7).Append(": ").Append(num9)
					.Append(" (")
					.Append((int)Math.Round((double)num9 * 100.0 / (double)num7, MidpointRounding.AwayFromZero))
					.Append("%)\n");
				num7 += num8;
				num8 += 10;
			}
			Popup.Show(stringBuilder2.ToString());
			return;
		}
		if (Wish.StartsWith("zoomnodes"))
		{
			MessageQueue.AddPlayerMessage(The.Game.GetIntGameState("zoomnodes").ToString());
			return;
		}
		if (Wish.StartsWith("find:"))
		{
			string text5 = Wish.Split(':')[1];
			StringBuilder stringBuilder3 = Event.NewStringBuilder();
			List<GameObject> objects = who.Physics.CurrentCell.ParentZone.GetObjects(text5);
			if (objects.Count == 0)
			{
				stringBuilder3.Append("no ").Append(text5).Append(" found in zone");
			}
			else
			{
				foreach (GameObject item10 in objects)
				{
					stringBuilder3.Append(item10.Physics.CurrentCell.X).Append(' ').Append(item10.Physics.CurrentCell.Y)
						.Append('\n');
				}
			}
			Popup.Show(stringBuilder3.ToString());
			return;
		}
		if (Wish == "testcardinal")
		{
			StringBuilder stringBuilder4 = Event.NewStringBuilder();
			for (int num10 = 0; num10 <= 150; num10++)
			{
				stringBuilder4.Append(num10).Append(": ").Append(Grammar.Cardinal(num10))
					.Append('\n');
			}
			Popup.Show(stringBuilder4.ToString());
			return;
		}
		if (Wish == "testordinal")
		{
			StringBuilder stringBuilder5 = Event.NewStringBuilder();
			for (int num11 = 0; num11 <= 150; num11++)
			{
				stringBuilder5.Append(num11).Append(": ").Append(Grammar.Ordinal(num11))
					.Append('\n');
			}
			Popup.Show(stringBuilder5.ToString());
			return;
		}
		if (Wish == "testpets")
		{
			bool flag = false;
			foreach (GameObjectBlueprint item11 in GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint bp) => bp.HasTag("Creature") && !bp.HasTag("BaseObject") && !bp.HasTag("ExcludeFromDynamicEncounters") && !bp.HasTag("ExcludeFromVillagePopulations") && !bp.HasTag("Merchant")))
			{
				GameObject gameObject5 = GameObjectFactory.Factory.CreateObject(item11.Name);
				if (gameObject5.DisplayName.StartsWith("["))
				{
					Popup.Show(gameObject5.Blueprint + ": " + gameObject5.DisplayName);
					flag = true;
				}
			}
			if (!flag)
			{
				Popup.Show("No problems found.");
			}
			return;
		}
		if (Wish == "testobjects")
		{
			bool flag2 = false;
			foreach (GameObjectBlueprint item12 in GameObjectFactory.Factory.BlueprintList.Where((GameObjectBlueprint bp) => !bp.HasTag("BaseObject") && !bp.HasTag("ExcludeFromDynamicEncounters")))
			{
				GameObject gameObject6 = GameObjectFactory.Factory.CreateObject(item12.Name);
				if (gameObject6.DisplayName.StartsWith("["))
				{
					Popup.Show(gameObject6.Blueprint + ": " + gameObject6.DisplayName);
					flag2 = true;
				}
			}
			if (!flag2)
			{
				Popup.Show("No problems found.");
			}
			return;
		}
		if (Wish == "showgenders")
		{
			List<Gender> all = Gender.GetAll();
			StringBuilder stringBuilder6 = Event.NewStringBuilder();
			for (int num12 = 0; num12 < all.Count; num12++)
			{
				stringBuilder6.Length = 0;
				stringBuilder6.Append(num12 + 1).Append('/').Append(all.Count)
					.Append("\n\n");
				all[num12].GetSummary(stringBuilder6);
				Popup.Show(stringBuilder6.ToString());
			}
			return;
		}
		if (Wish == "showmygender")
		{
			Popup.Show(who.GetGender().GetSummary());
			return;
		}
		if (Wish == "showpronounsets")
		{
			List<PronounSet> all2 = PronounSet.GetAll();
			StringBuilder stringBuilder7 = Event.NewStringBuilder();
			for (int num13 = 0; num13 < all2.Count; num13++)
			{
				stringBuilder7.Length = 0;
				stringBuilder7.Append(num13 + 1).Append('/').Append(all2.Count)
					.Append("\n\n");
				all2[num13].GetSummary(stringBuilder7);
				Popup.Show(stringBuilder7.ToString());
			}
			return;
		}
		if (Wish == "powergrid")
		{
			new PowerGrid().BuildZone(who.Physics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "powergriddebug")
		{
			PowerGrid powerGrid = new PowerGrid();
			powerGrid.ShowPathfinding = true;
			powerGrid.ShowPathWeights = true;
			powerGrid.BuildZone(who.Physics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "powergridruin")
		{
			PowerGrid powerGrid2 = new PowerGrid();
			powerGrid2.DamageChance = "20-40";
			powerGrid2.DamageIsBreakageChance = "30-80";
			powerGrid2.MissingConsumers = "2-15";
			powerGrid2.MissingProducers = "2-5";
			powerGrid2.Noise = ((Stat.Random(0, 1) == 0) ? Stat.Random(0, 10) : Stat.Random(0, 80));
			powerGrid2.BuildZone(who.Physics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "hydraulics")
		{
			new Hydraulics().BuildZone(who.Physics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "1hp")
		{
			Statistic stat = The.Player.GetStat("Hitpoints");
			stat.Penalty += stat.Value - 1;
			return;
		}
		if (Wish == "ambientstabilization")
		{
			AmbientStabilization ambientStabilization = The.Player.CurrentZone.RequirePart<AmbientStabilization>();
			Popup.Show("Have ambient stabilization at strength " + ambientStabilization.Strength + ((ambientStabilization.ParentZone == The.Player.CurrentZone) ? " with coherent zone" : " with zone mismatch") + ".");
			return;
		}
		if (Wish == "showworshippables")
		{
			string text6 = "";
			foreach (Worshippable worshippable in Factions.GetWorshippables())
			{
				text6 = text6 + worshippable.ToString() + "\n\n";
			}
			Popup.Show(text6);
			return;
		}
		if (Wish == "showworship")
		{
			string text7 = "";
			foreach (WorshipTracking item13 in The.Game.PlayerReputation.GetWorshipTracking())
			{
				text7 = text7 + item13.ToString() + "\n\n";
			}
			Popup.Show(text7);
			return;
		}
		if (Wish == "showblasphemy")
		{
			string text8 = "";
			foreach (WorshipTracking item14 in The.Game.PlayerReputation.GetBlasphemyTracking())
			{
				text8 = text8 + item14.ToString() + "\n\n";
			}
			Popup.Show(text8);
			return;
		}
		if (Wish == "zonenamedata")
		{
			Zone currentZone3 = The.Player.CurrentZone;
			Popup.Show("DisplayName: " + currentZone3.DisplayName + "\nBaseDisplayName: " + currentZone3.BaseDisplayName + "\nReferenceDisplayName: " + currentZone3.ReferenceDisplayName + "\nNameContext: " + currentZone3.NameContext + "\nHasProperName: " + currentZone3.HasProperName + "\nDefiniteArticle: " + currentZone3.DefiniteArticle + "\nIndefiniteArticle: " + currentZone3.IndefiniteArticle + "\nIncludeContextInZoneDisplay: " + currentZone3.IncludeContextInZoneDisplay + "\nIncludeStratumInZoneDisplay: " + currentZone3.IncludeStratumInZoneDisplay + "\nNamedByPlayer: " + currentZone3.NamedByPlayer + "\n");
			return;
		}
		if (Wish == "zoneparts")
		{
			Zone currentZone4 = The.Player.CurrentZone;
			if (currentZone4.Parts == null)
			{
				Popup.Show("Zone has null parts list.");
				return;
			}
			if (currentZone4.Parts.Count == 0)
			{
				Popup.Show("Zone has no parts.");
				return;
			}
			string text9 = "";
			int num14 = 0;
			for (int count = currentZone4.Parts.Count; num14 < count; num14++)
			{
				text9 = text9 + "\n" + currentZone4.Parts[num14].Name;
			}
			Popup.Show(text9);
			return;
		}
		if (Wish == "hydraulicsmetal")
		{
			Hydraulics hydraulics = new Hydraulics();
			hydraulics.ConduitBlueprint = "MetalHydraulicPipe";
			hydraulics.BuildZone(who.Physics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "hydraulicsplastic")
		{
			Hydraulics hydraulics2 = new Hydraulics();
			hydraulics2.ConduitBlueprint = "PlasticHydraulicPipe";
			hydraulics2.BuildZone(who.Physics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "hydraulicsglass")
		{
			Hydraulics hydraulics3 = new Hydraulics();
			hydraulics3.ConduitBlueprint = "GlassHydraulicPipe";
			hydraulics3.BuildZone(who.Physics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "hydraulicsruin")
		{
			Hydraulics hydraulics4 = new Hydraulics();
			hydraulics4.DamageChance = "10-20";
			hydraulics4.DamageIsBreakageChance = "30-80";
			hydraulics4.MissingConsumers = "1-8";
			hydraulics4.MissingProducers = "1-3";
			hydraulics4.Noise = ((Stat.Random(0, 1) == 0) ? Stat.Random(0, 10) : Stat.Random(0, 80));
			hydraulics4.BuildZone(who.Physics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "mechpower")
		{
			new MechanicalPower().BuildZone(who.Physics.CurrentCell.ParentZone);
			return;
		}
		if (Wish == "showrandomfaction")
		{
			Popup.Show(Factions.GetRandomFaction().Name);
			return;
		}
		if (Wish == "showrandomoldfaction")
		{
			Popup.Show(Factions.GetRandomOldFaction().Name);
			return;
		}
		if (Wish == "showrandomfactionexceptbeasts")
		{
			Popup.Show(Factions.GetRandomFaction("Beasts").Name);
			return;
		}
		if (Wish == "showrandomfactionshortname")
		{
			Popup.Show(Factions.GetRandomFaction((Faction f) => f.Name.Length <= 8).Name);
			return;
		}
		if (Wish == "teststringbuilder")
		{
			Action<StringBuilder, string> action = (StringBuilder sb, string text36) =>
			{
				if (!sb.Contains(text36))
				{
					Popup.Show("'" + sb.ToString() + "' should contain '" + text36 + "' but doesn't");
				}
			};
			Action<StringBuilder, string> action2 = (StringBuilder sb, string text36) =>
			{
				if (sb.Contains(text36))
				{
					Popup.Show("'" + sb.ToString() + "' shouldn't contain '" + text36 + "' but does");
				}
			};
			StringBuilder arg = new StringBuilder("abcde");
			action(arg, "a");
			action(arg, "b");
			action(arg, "c");
			action(arg, "d");
			action(arg, "e");
			action2(arg, "f");
			action(arg, "ab");
			action(arg, "bc");
			action(arg, "cd");
			action(arg, "de");
			action2(arg, "aa");
			action2(arg, "ac");
			action2(arg, "ad");
			action2(arg, "ae");
			action2(arg, "af");
			action2(arg, "ea");
			action2(arg, "ba");
			action2(arg, "bb");
			action2(arg, "bd");
			action2(arg, "be");
			action2(arg, "bf");
			action2(arg, "ca");
			action2(arg, "cb");
			action2(arg, "cc");
			action2(arg, "ce");
			action2(arg, "cf");
			action2(arg, "da");
			action2(arg, "db");
			action2(arg, "dc");
			action2(arg, "dd");
			action2(arg, "df");
			action2(arg, "eb");
			action2(arg, "ec");
			action2(arg, "ed");
			action2(arg, "ee");
			action2(arg, "ef");
			action(arg, "abc");
			action(arg, "bcd");
			action(arg, "cde");
			action2(arg, "def");
			action(arg, "abcd");
			action(arg, "bcde");
			action2(arg, "cdef");
			action(arg, "abcde");
			action2(arg, "abcdef");
			StringBuilder arg2 = new StringBuilder("abcdee");
			action(arg2, "ab");
			action(arg2, "bc");
			action(arg2, "cd");
			action(arg2, "de");
			action(arg2, "ee");
			action2(arg2, "ef");
			action(arg2, "abc");
			action(arg2, "bcd");
			action(arg2, "dee");
			action2(arg2, "deee");
			action2(arg2, "def");
			action(arg2, "abcd");
			action(arg2, "bcde");
			action(arg2, "cdee");
			action2(arg2, "cdef");
			action(arg2, "abcde");
			action(arg2, "bcdee");
			action2(arg2, "bcdef");
			action2(arg2, "bcdeee");
			action2(arg2, "cdeee");
			Popup.Show("Done.");
			return;
		}
		if (Wish == "testzoneparse")
		{
			string zoneID = who.CurrentZone.ZoneID;
			bool value = ZoneID.Parse(zoneID, out var World, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY, out var ZoneZ);
			string text10 = ZoneID.Assemble(World, ParasangX, ParasangY, ZoneX, ZoneY, ZoneZ);
			StringBuilder stringBuilder8 = Event.NewStringBuilder();
			stringBuilder8.Append("ZoneID: ").Append(zoneID).Append('\n')
				.Append("Parse result: ")
				.Append(value)
				.Append('\n')
				.Append("Components: ")
				.Append(World)
				.Append(' ')
				.Append(ParasangX)
				.Append(' ')
				.Append(ParasangY)
				.Append(' ')
				.Append(ZoneX)
				.Append(' ')
				.Append(ZoneY)
				.Append(' ')
				.Append(ZoneZ)
				.Append('\n')
				.Append("Match on reassemble: ")
				.Append(text10 == zoneID);
			Popup.Show(stringBuilder8.ToString());
			return;
		}
		if (Wish == "topevents")
		{
			Event.ShowTopEvents();
			return;
		}
		if (Wish == "achnotify")
		{
			foreach (AchievementInfo value10 in AchievementManager.State.Achievements.Values)
			{
				if (value10.Progress != null)
				{
					value10.NotifyProgress();
				}
				value10.NotifyUnlock();
			}
			return;
		}
		if (Wish == "statnotify")
		{
			foreach (AchievementInfo value11 in AchievementManager.State.Achievements.Values)
			{
				if (value11.Progress != null)
				{
					value11.NotifyProgress();
				}
			}
			return;
		}
		if (Wish == "testrig")
		{
			Mutations part = who.GetPart<Mutations>();
			part.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Clairvoyance)), 12);
			part.AddMutation((BaseMutation)Activator.CreateInstance(typeof(Teleportation)), 12);
			AddSkill("Survival");
			AddSkill("Survival_Trailblazer");
			return;
		}
		if (Wish == "testhero")
		{
			GameObject gameObject7 = GameObjectFactory.Factory.CreateObject("Scrapbot");
			HeroMaker.MakeHero(gameObject7);
			who.Physics.CurrentCell.GetCellFromDirection("E").AddObject(gameObject7);
			return;
		}
		if (Wish == "reload")
		{
			MinEvent.SuppressThreadWarning = true;
			GameManager.Instance.uiQueue.awaitTask(() =>
			{
				The.Core.HotloadConfiguration();
				GameManager.Instance.SetActiveLayersForNavCategory("Adventure");
			});
			MinEvent.SuppressThreadWarning = false;
			MessageQueue.AddPlayerMessage("Hotload complete.");
			return;
		}
		if (Wish == "xy")
		{
			Popup.Show(who.CurrentCell.X + ", " + who.CurrentCell.Y);
			return;
		}
		if (Wish == "rebuildfull")
		{
			The.Game.SetIntGameState("WorldSeed", Stat.RandomCosmetic(1, 2147483646));
			The.ZoneManager.RebuildActiveZone(Wish == "flushandrebuild");
			return;
		}
		if (Wish == "rebuild" || Wish == "flushandrebuild")
		{
			The.ZoneManager.RebuildActiveZone(Wish == "flushandrebuild");
			return;
		}
		if (Wish == "popuptest")
		{
			Popup.ShowYesNo("Test1");
			Popup.ShowYesNo("Test2");
			Popup.ShowYesNo("Test3");
			return;
		}
		if (Wish == "nanoterm")
		{
			Cell currentCell2 = who.Physics.CurrentCell;
			currentCell2.GetCellFromDirection("E").AddObject("Nanowall1W");
			currentCell2.GetCellFromDirection("E").GetCellFromDirection("E").GetCellFromDirection("E")
				.AddObject("Nanowall1E");
			currentCell2.GetCellFromDirection("S").GetCellFromDirection("E").AddObject("Nanowall2W");
			currentCell2.GetCellFromDirection("S").GetCellFromDirection("E").GetCellFromDirection("E")
				.AddObject("CyberneticsFabTerminal");
			currentCell2.GetCellFromDirection("S").GetCellFromDirection("E").GetCellFromDirection("E")
				.GetCellFromDirection("E")
				.AddObject("Nanowall2E");
			currentCell2.GetCellFromDirection("S").GetCellFromDirection("S").GetCellFromDirection("E")
				.AddObject("Nanowall3W");
			currentCell2.GetCellFromDirection("S").GetCellFromDirection("S").GetCellFromDirection("E")
				.GetCellFromDirection("E")
				.AddObject("ArmNook");
			currentCell2.GetCellFromDirection("S").GetCellFromDirection("S").GetCellFromDirection("E")
				.GetCellFromDirection("E")
				.GetCellFromDirection("E")
				.AddObject("Nanowall3E");
			return;
		}
		if (Wish == "thirsty")
		{
			who.GetPart<Stomach>().Water = 0;
			return;
		}
		if (Wish == "hungry")
		{
			Stomach part2 = who.GetPart<Stomach>();
			part2.CookingCounter = part2.CalculateCookingIncrement();
			return;
		}
		if (Wish == "famished")
		{
			Stomach part3 = who.GetPart<Stomach>();
			part3.CookingCounter = part3.CalculateCookingIncrement() * 2;
			return;
		}
		if (Wish == "what")
		{
			_ = who.Physics.CurrentCell;
			{
				foreach (GameObject @object in who.Physics.CurrentCell.Objects)
				{
					MessageQueue.AddPlayerMessage(@object.Blueprint);
				}
				return;
			}
		}
		if (Wish == "where")
		{
			Cell currentCell3 = who.Physics.CurrentCell;
			MessageQueue.AddPlayerMessage(currentCell3.X + "," + currentCell3.Y + " in " + currentCell3.ParentZone.ZoneID);
			return;
		}
		if (Wish == "bordertest")
		{
			for (int num15 = 0; num15 < 10000; num15++)
			{
				Popup._ScreenBuffer.ThickSingleBox(Stat.RandomCosmetic(-1000, 1000), Stat.RandomCosmetic(-1000, 1000), Stat.RandomCosmetic(-1000, 1000), Stat.RandomCosmetic(-1000, 1000), ConsoleLib.Console.ColorUtility.MakeColor(ConsoleLib.Console.ColorUtility.Bright(TextColor.Black), TextColor.Black));
			}
			return;
		}
		if (Wish == "curefungus")
		{
			foreach (BodyPart part6 in who.Body.GetParts())
			{
				if (part6.Equipped != null && part6.Equipped.HasTag("FungalInfection"))
				{
					part6.Equipped.Destroy();
				}
			}
			return;
		}
		if (Wish == "cureironshank")
		{
			if (who.TryGetEffect<IronshankOnset>(out var Effect))
			{
				Effect.Duration = 0;
			}
			if (who.TryGetEffect<Ironshank>(out var Effect2))
			{
				Effect2.Duration = 0;
			}
		}
		else if (Wish == "cureglotrot")
		{
			if (who.TryGetEffect<GlotrotOnset>(out var Effect3))
			{
				Effect3.Duration = 0;
			}
			if (who.TryGetEffect<Glotrot>(out var Effect4))
			{
				Effect4.Duration = 0;
			}
		}
		else if (Wish == "glotrotonset")
		{
			who.ApplyEffect(new GlotrotOnset());
		}
		else if (Wish == "glotrot")
		{
			who.ApplyEffect(new Glotrot());
		}
		else if (Wish == "glotrotfinal")
		{
			who.ApplyEffect(new Glotrot());
			who.GetEffect<Glotrot>().Stage = 3;
		}
		else if (Wish == "ironshankonset")
		{
			who.ApplyEffect(new IronshankOnset());
		}
		else if (Wish == "ironshank")
		{
			who.ApplyEffect(new Ironshank());
		}
		else if (Wish == "monochromeonset")
		{
			who.ApplyEffect(new MonochromeOnset());
		}
		else if (Wish == "monochrome")
		{
			who.ApplyEffect(new Monochrome());
		}
		else if (Wish == "monochrome")
		{
			who.ApplyEffect(new Monochrome());
		}
		else if (Wish == "glotrotonset")
		{
			who.ApplyEffect(new GlotrotOnset());
		}
		else if (Wish == "resetgreyscale")
		{
			GameManager.Instance.GreyscaleLevel = 0;
		}
		else if (Wish == "mazetest")
		{
			Keys keys = Popup.ShowBlock("1) random maze\n2) recursive backtrack maze");
			if (keys == Keys.D1)
			{
				RandomMaze.Generate(80, 25, Stat.Random(0, 2147483646)).Test(bWait: true);
			}
			if (keys == Keys.D2)
			{
				RecursiveBacktrackerMaze.Generate(80, 25, bShow: true, Stat.Random(0, 2147483646)).Test(bWait: true);
			}
		}
		else if (Wish == "tunneltest")
		{
			do
			{
				TunnelMaker tunnelMaker = new TunnelMaker(5, 3, Stat.Random(0, 2).ToString(), Stat.Random(0, 2).ToString(), "NES");
				tunnelMaker.CreateTunnel();
				ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
				for (int num16 = 0; num16 < tunnelMaker.Height; num16++)
				{
					for (int num17 = 0; num17 < tunnelMaker.Width; num17++)
					{
						scrapBuffer.Goto(num17, num16);
						if (tunnelMaker.Map[num17, num16] == "")
						{
							scrapBuffer.Write(".");
						}
						if (tunnelMaker.Map[num17, num16].Contains("N") && tunnelMaker.Map[num17, num16].Contains("S"))
						{
							scrapBuffer.Write(186);
						}
						if (tunnelMaker.Map[num17, num16].Contains("E") && tunnelMaker.Map[num17, num16].Contains("W"))
						{
							scrapBuffer.Write(205);
						}
						if (tunnelMaker.Map[num17, num16].Contains("E") && tunnelMaker.Map[num17, num16].Contains("N"))
						{
							scrapBuffer.Write(200);
						}
						if (tunnelMaker.Map[num17, num16].Contains("W") && tunnelMaker.Map[num17, num16].Contains("N"))
						{
							scrapBuffer.Write(188);
						}
						if (tunnelMaker.Map[num17, num16].Contains("E") && tunnelMaker.Map[num17, num16].Contains("S"))
						{
							scrapBuffer.Write(201);
						}
						if (tunnelMaker.Map[num17, num16].Contains("W") && tunnelMaker.Map[num17, num16].Contains("S"))
						{
							scrapBuffer.Write(187);
						}
						if (tunnelMaker.Map[num17, num16] == "N")
						{
							scrapBuffer.Write(208);
						}
						if (tunnelMaker.Map[num17, num16] == "S")
						{
							scrapBuffer.Write(210);
						}
						if (tunnelMaker.Map[num17, num16] == "E")
						{
							scrapBuffer.Write(198);
						}
						if (tunnelMaker.Map[num17, num16] == "W")
						{
							scrapBuffer.Write(181);
						}
					}
				}
				Popup._TextConsole.DrawBuffer(scrapBuffer);
			}
			while (Keyboard.getch() != 120);
		}
		else if (Wish.StartsWith("rebuildbody:"))
		{
			string text11 = Wish.Split(':')[1];
			if (!who.Body.Rebuild(text11))
			{
				Popup.Show("Failed to rebuild body as " + text11);
			}
		}
		else if (Wish == "bodyparttypes")
		{
			StringBuilder stringBuilder9 = Event.NewStringBuilder();
			who.Body.TypeDump(stringBuilder9);
			Popup.Show(stringBuilder9.ToString());
		}
		else if (Wish.StartsWith("xpmul:"))
		{
			The.Core.XPMul = (float)Convert.ToDouble(Wish.Split(':')[1]);
		}
		else if (Wish.StartsWith("xp:"))
		{
			Popup.Suppress = true;
			who.AwardXP(Convert.ToInt32(Wish.Split(':')[1]));
			Popup.Suppress = false;
		}
		else if (Wish.StartsWith("xpverbose:"))
		{
			who.AwardXP(Convert.ToInt32(Wish.Split(':')[1]));
		}
		else
		{
			if (Wish == "cleaneffects")
			{
				if (who.Effects == null)
				{
					return;
				}
				{
					foreach (Effect effect in who.Effects)
					{
						effect.Duration = 0;
					}
					return;
				}
			}
			if (Wish == "clean")
			{
				if (who.Effects != null)
				{
					foreach (Effect effect2 in who.Effects)
					{
						effect2.Duration = 0;
					}
				}
				who.Statistics["Strength"].Penalty = 0;
				who.Statistics["Agility"].Penalty = 0;
				who.Statistics["Intelligence"].Penalty = 0;
				who.Statistics["Toughness"].Penalty = 0;
				who.Statistics["Willpower"].Penalty = 0;
				who.Statistics["Ego"].Penalty = 0;
				who.Statistics["Speed"].Penalty = 0;
			}
			else if (Wish == "websplat")
			{
				for (int num18 = 0; num18 < 8; num18++)
				{
					Cryobarrio1.Websplat(Stat.Random(0, 79), Stat.Random(0, 24), who.Physics.CurrentCell.ParentZone, "PhaseWeb");
				}
			}
			else if (Wish == "sultantomb1")
			{
				The.ZoneManager.SetActiveZone("JoppaWorld.53.3.0.2.6");
				who.SystemMoveTo(The.ZoneManager.ActiveZone.GetCell(39, 14));
				The.ZoneManager.ProcessGoToPartyLeader();
			}
			else if (Wish == "sultantomb6")
			{
				The.ZoneManager.SetActiveZone("JoppaWorld.53.3.1.0.1");
				who.SystemMoveTo(The.ZoneManager.ActiveZone.GetCell(39, 14));
				The.ZoneManager.ProcessGoToPartyLeader();
			}
			else if (Wish == "stage2" || Wish == "stage3a" || Wish == "stage3" || Wish == "stage4" || Wish == "stage5" || Wish == "stage6" || Wish == "stage8" || Wish.StartsWith("stage9") || Wish == "stage10" || Wish == "stage11" || Wish == "tombbeta" || Wish == "fastforwardtomb" || Wish == "tombbetastart" || Wish == "tombbetaend" || Wish == "tombbetainside" || Wish == "reefjump" || Wish == "pregolem" || Wish == "golemstart" || Wish == "posttomb" || Wish == "startmoonstair" || Wish == "postgolem")
			{
				Popup.Suppress = true;
				ItemNaming.Suppress = true;
				who.AwardXP(15000);
				game.CompleteQuest("Fetch Argyve a Knickknack");
				game.CompleteQuest("Fetch Argyve Another Knickknack");
				game.CompleteQuest("Weirdwire Conduit... Eureka!");
				game.StartQuest("A Canticle for Barathrum");
				XRL.World.Parts.Physics physics = who.Physics;
				if (Wish == "stage2")
				{
					The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.1.10");
					who.SystemMoveTo(The.ZoneManager.ActiveZone.GetCell(0, 0));
					The.ZoneManager.ProcessGoToPartyLeader();
					who.ReceiveObjectsFromPopulation("Junk 1", Stat.Random(1, 4));
					who.ReceiveObjectsFromPopulation("Meds 1", Stat.Random(1, 4));
				}
				if (Wish == "stage3")
				{
					who.ReceiveObject("Steel Plate Mail");
					who.ReceiveObject("Steel Buckler");
					who.ReceiveObject("Steel Boots");
					who.ReceiveObject("Steel Gauntlets");
					if (who.HasSkill("Cudgel"))
					{
						who.ReceiveObject("Cudgel3");
					}
					if (who.HasSkill("ShortBlades"))
					{
						who.ReceiveObject("Dagger3");
					}
					if (who.HasSkill("LongBlades"))
					{
						who.ReceiveObject("Long Sword3");
					}
					if (who.HasSkill("Axe"))
					{
						who.ReceiveObject("Battle Axe3");
					}
					who.ReceiveObjectsFromPopulation("Junk 1", Stat.Random(1, 4));
					who.ReceiveObjectsFromPopulation("Junk 2", Stat.Random(1, 3));
					who.ReceiveObjectsFromPopulation("Meds 1", Stat.Random(1, 3));
					who.ReceiveObjectsFromPopulation("Meds 2", Stat.Random(1, 2));
				}
				if (Wish == "stage4")
				{
					who.ReceiveObject("Carbide Plate Armor");
					who.ReceiveObject("Steel Buckler");
					who.ReceiveObject("Chain Coif");
					who.ReceiveObject("Steel Boots");
					who.ReceiveObject("Steel Gauntlets");
					who.ReceiveObject("MasterworkCarbine");
					who.ReceiveObject("UbernostrumTonic");
					who.ReceiveObject("Sowers_Seed", 12);
					who.ReceiveObject("Fixit Spray");
					who.ReceiveObject("SalveTonic", 6);
					who.ReceiveObject("Floating Glowsphere");
					who.ReceiveObject("Ironweave Cloak");
					if (who.HasSkill("Cudgel"))
					{
						who.ReceiveObject("Cudgel4");
					}
					if (who.HasSkill("ShortBlades"))
					{
						who.ReceiveObject("Dagger4");
					}
					if (who.HasSkill("LongBlades"))
					{
						who.ReceiveObject("Long Sword4");
					}
					if (who.HasSkill("Axe"))
					{
						who.ReceiveObject("Battle Axe4");
					}
					who.ReceiveObjectsFromPopulation("Junk 3", 2);
					who.ReceiveObjectsFromPopulation("Junk 4", 2);
					who.ReceiveObjectsFromPopulation("Meds 3", 3);
					who.ReceiveObjectsFromPopulation("Meds 4", 2);
					who.ReceiveObjectsFromPopulation("Artifact 3", 3);
					who.ReceiveObjectsFromPopulation("Artifact 4", 2);
				}
				who.ReceiveObject("Droid Scrambler");
				who.ReceiveObject("Joppa Recoiler");
				who.ReceiveObject("Borderlands Revolver");
				who.ReceiveObject("Lead Slug", 500);
				if (Wish == "stage3a")
				{
					who.AwardXP(15000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.StartQuest("Decoding the Signal");
					game.FinishedQuestStep("Decoding the Signal~Decode the Signal");
					The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.13");
					The.ZoneManager.ActiveZone.GetCell(33, 16).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
					GritGateScripts.OpenRank1Doors();
				}
				if (Wish == "stage3")
				{
					who.AwardXP(15000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.13");
					The.ZoneManager.ActiveZone.GetCell(33, 16).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
					GritGateScripts.OpenRank1Doors();
				}
				if (Wish == "stage4")
				{
					who.AwardXP(75000);
					The.ZoneManager.SetActiveZone("JoppaWorld");
					The.ZoneManager.ActiveZone.GetCell(25, 3).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
				}
				if (Wish == "stage5")
				{
					who.AwardXP(210000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("Decoding the Signal");
					game.StartQuest("The Earl of Omonporch");
					game.FinishQuestStep("The Earl of Omonporch", "Travel to Omonporch");
					game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
					The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.13");
					The.ZoneManager.ActiveZone.GetCell(31, 21).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
				}
				if (Wish == "stage6")
				{
					who.AwardXP(210000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("The Earl of Omonporch");
					game.SetIntGameState("ForcePostEarlSpawn", 1);
					game.CompleteQuest("A Call to Arms");
					game.CompleteQuest("The Assessment");
					game.StartQuest("Pax Klanq, I Presume?");
					The.Player.ReceiveObject("BarathrumKey");
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank1Doors();
					The.ZoneManager.SetActiveZone("JoppaWorld");
					The.ZoneManager.ActiveZone.GetCell(48, 19).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
				}
				if (Wish == "stage8" || Wish == "tombbetastart")
				{
					The.Player.ReceivePopulation("TombSupply");
					who.AwardXP(240000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("The Earl of Omonporch");
					game.SetIntGameState("ForcePostEarlSpawn", 1);
					game.CompleteQuest("A Call to Arms");
					game.CompleteQuest("The Assessment");
					game.CompleteQuest("Pax Klanq, I Presume?");
					The.Player.ReceiveObject("BarathrumKey");
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
					The.ZoneManager.SetActiveZone("JoppaWorld");
					The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.14");
					The.ZoneManager.ActiveZone.GetCell(18, 8).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
				}
				if (Wish == "stage9")
				{
					The.Player.ReceivePopulation("TombSupply");
					who.AwardXP(240000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("The Earl of Omonporch");
					game.SetIntGameState("ForcePostEarlSpawn", 1);
					game.CompleteQuest("A Call to Arms");
					game.CompleteQuest("Pax Klanq, I Presume?");
					game.StartQuest("Tomb of the Eaters");
					The.Player.ReceiveObject("BarathrumKey");
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
					The.ZoneManager.SetActiveZone("JoppaWorld");
					The.ZoneManager.ActiveZone.GetCell(53, 4).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
				}
				if (Wish == "stage9b" || Wish == "stage9c" || Wish == "stage9d" || Wish == "stage9e" || Wish == "stage9f" || Wish == "stage9u" || Wish == "stage10")
				{
					The.Player.ReceivePopulation("TombSupply");
					who.AwardXP(240000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("The Earl of Omonporch");
					game.SetIntGameState("ForcePostEarlSpawn", 1);
					game.CompleteQuest("A Call to Arms");
					game.CompleteQuest("Pax Klanq, I Presume?");
					game.StartQuest("Tomb of the Eaters");
					game.FinishQuestStep("Tomb of the Eaters", "Recover the Mark of Death");
					game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
					game.FinishQuestStep("Tomb of the Eaters", "Enter the Tomb of the Eaters");
					The.Player.ToggleMarkOfDeath();
					MessageQueue.AddPlayerMessage("Mark of death now " + The.Player.HasMarkOfDeath());
					The.Player.ReceiveObject("BarathrumKey");
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
					The.ZoneManager.SetActiveZone("JoppaWorld");
					if (Wish == "stage9b")
					{
						The.ZoneManager.SetActiveZone("JoppaWorld.53.3.0.2.9");
					}
					if (Wish == "stage9c")
					{
						The.ZoneManager.SetActiveZone("JoppaWorld.53.3.2.2.9");
					}
					if (Wish == "stage9d")
					{
						The.ZoneManager.SetActiveZone("JoppaWorld.53.3.1.2.9");
					}
					if (Wish == "stage9e")
					{
						The.ZoneManager.SetActiveZone("JoppaWorld.53.3.0.2.8");
					}
					if (Wish == "stage9f")
					{
						The.ZoneManager.SetActiveZone("JoppaWorld.53.3.2.0.0");
					}
					if (Wish == "stage9u")
					{
						The.ZoneManager.SetActiveZone("JoppaWorld.53.3.0.2.12");
					}
					The.ZoneManager.ActiveZone.GetCell(45, 24).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
				}
				if (Wish == "tombbeta" || Wish == "tombbetainside" || Wish == "tombbetaend" || Wish == "fastforwardtomb")
				{
					The.Player.ReceivePopulation("TombSupply");
					who.AwardXP(240000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("The Earl of Omonporch");
					game.SetIntGameState("ForcePostEarlSpawn", 1);
					game.CompleteQuest("A Call to Arms");
					game.CompleteQuest("Pax Klanq, I Presume?");
					game.StartQuest("Tomb of the Eaters");
					game.FinishQuestStep("Tomb of the Eaters", "Recover the Mark of Death");
					game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
					game.FinishQuestStep("Tomb of the Eaters", "Enter the Tomb of the Eaters");
					The.Player.ToggleMarkOfDeath();
					MessageQueue.AddPlayerMessage("Mark of death now " + The.Player.HasMarkOfDeath());
					The.Player.ReceiveObject("BarathrumKey");
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
					The.ZoneManager.SetActiveZone("JoppaWorld");
					if (Wish == "tombbetaend")
					{
						The.ZoneManager.SetActiveZone("JoppaWorld.53.3.1.0.0");
						The.ZoneManager.ActiveZone.GetCell(45, 12).AddObject(physics.ParentObject);
					}
					else
					{
						The.ZoneManager.SetActiveZone("JoppaWorld.53.4.0.0.11");
						The.ZoneManager.ActiveZone.GetCell(45, 0).AddObject(physics.ParentObject);
					}
					The.ZoneManager.ProcessGoToPartyLeader();
				}
				if (Wish == "stage11" || Wish == "reefjump")
				{
					The.Player.ReceivePopulation("ReefBetaSupply");
					who.AwardXP(350000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("The Earl of Omonporch");
					game.SetIntGameState("ForcePostEarlSpawn", 1);
					game.CompleteQuest("A Call to Arms");
					game.CompleteQuest("Pax Klanq, I Presume?");
					game.StartQuest("Tomb of the Eaters");
					game.FinishQuestStep("Tomb of the Eaters", "Recover the Mark of Death");
					game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
					game.FinishQuestStep("Tomb of the Eaters", "Enter the Tomb of the Eaters");
					game.FinishQuestStep("Tomb of the Eaters", "Ascend the Tomb and Cross into Brightsheol");
					game.FinishQuestStep("Tomb of the Eaters", "Disable the Spindle's Magnetic Field");
					game.FinishQuestStep("Tomb of the Eaters", "Return to Grit Gate");
					The.Player.ReceiveObject("BarathrumKey");
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
					The.ZoneManager.SetActiveZone("JoppaWorld");
					GritGateScripts.PromoteToJourneyfriend();
					ThinWorld.ReturnBody(The.Player);
					game.SetBooleanGameState("Recame", Value: true);
				}
				if (Wish == "pregolem" || Wish == "posttomb")
				{
					The.Player.ReceivePopulation("ReefBetaSupply");
					who.AwardXP(600000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("The Earl of Omonporch");
					game.SetIntGameState("ForcePostEarlSpawn", 1);
					game.CompleteQuest("A Call to Arms");
					game.CompleteQuest("Pax Klanq, I Presume?");
					game.StartQuest("Tomb of the Eaters");
					game.FinishQuestStep("Tomb of the Eaters", "Recover the Mark of Death");
					game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
					game.FinishQuestStep("Tomb of the Eaters", "Enter the Tomb of the Eaters");
					game.FinishQuestStep("Tomb of the Eaters", "Ascend the Tomb and Cross into Brightsheol");
					game.FinishQuestStep("Tomb of the Eaters", "Disable the Spindle's Magnetic Field");
					game.SetBooleanGameState("Recame", Value: true);
					The.Player.ReceiveObject("BarathrumKey");
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
					The.ZoneManager.SetActiveZone("JoppaWorld");
					The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.14");
					The.ZoneManager.ActiveZone.GetCell(18, 8).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
				}
				if (Wish == "golemstart" || Wish == "startmoonstair")
				{
					The.Player.ReceivePopulation("ReefBetaSupply");
					who.AwardXP(600000);
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("The Earl of Omonporch");
					game.SetIntGameState("ForcePostEarlSpawn", 1);
					game.CompleteQuest("A Call to Arms");
					game.CompleteQuest("Pax Klanq, I Presume?");
					game.CompleteQuest("Tomb of the Eaters");
					game.SetBooleanGameState("Recame", Value: true);
					game.StartQuest("The Golem");
					The.Player.ReceiveObject("BarathrumKey");
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
					The.ZoneManager.SetActiveZone("JoppaWorld");
					The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.14");
					The.ZoneManager.ActiveZone.GetCell(18, 8).AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
				}
				if (Wish == "postgolem")
				{
					game.CompleteQuest("A Canticle for Barathrum");
					game.CompleteQuest("Decoding the Signal");
					game.CompleteQuest("More Than a Willing Spirit");
					game.CompleteQuest("The Earl of Omonporch");
					game.SetIntGameState("ForcePostEarlSpawn", 1);
					game.CompleteQuest("A Call to Arms");
					game.CompleteQuest("Pax Klanq, I Presume?");
					game.CompleteQuest("Tomb of the Eaters");
					int amount = Leveler.GetXPForLevel(37) - The.Player.GetStatValue("XP");
					The.Player.AwardXP(amount);
					if (!game.GetBooleanGameState("Recame"))
					{
						BodyPart body = The.Player.Body.GetBody();
						bool? dynamic = true;
						body.AddPart("Floating Nearby", 0, null, null, null, null, null, null, null, null, null, null, null, null, null, dynamic);
						game.SetBooleanGameState("Recame", Value: true);
					}
					The.Player.ReceiveObject("BarathrumKey");
					GritGateScripts.PromoteToJourneyfriend();
					GritGateScripts.OpenRank0Doors();
					GritGateScripts.OpenRank1Doors();
					GritGateScripts.OpenRank2Doors();
					The.ZoneManager.SetActiveZone("JoppaWorld");
					The.ZoneManager.SetActiveZone("JoppaWorld.22.14.1.0.14");
					Cell cell3 = The.ZoneManager.ActiveZone.GetCell(18, 8);
					Cell cell4 = The.ZoneManager.ActiveZone.GetCell(21, 10);
					cell3.AddObject(physics.ParentObject);
					The.ZoneManager.ProcessGoToPartyLeader();
					if (!The.Game.HasFinishedQuest("The Golem"))
					{
						try
						{
							GolemQuestSelection.PlaceFinalMound(cell4);
							The.Game.CompleteQuest("The Golem");
						}
						catch (Exception x)
						{
							MetricsManager.LogException("Postgolem", x);
						}
					}
				}
				foreach (GameObject item15 in who.GetInventoryAndEquipment())
				{
					item15.MakeUnderstood();
				}
				who.Brain?.PerformEquip();
				Popup.Suppress = false;
				ItemNaming.Suppress = false;
			}
			else if (Wish == "golemquest:soup")
			{
				The.Player.ReceiveObject(GameObjectFactory.Factory.CreateObject("Phial", (GameObject gameObject23) =>
				{
					LiquidVolume liquidVolume = gameObject23.LiquidVolume;
					liquidVolume.InitialLiquid = "proteangunk-1000";
					liquidVolume.MaxVolume = 20;
					liquidVolume.StartVolume = "20";
				}));
			}
			else if (Wish == "reequip")
			{
				who.Brain.PerformReequip();
			}
			else
			{
				if (Wish == "maket2" || Wish == "worldbmp")
				{
					return;
				}
				if (Wish == "garbagetest")
				{
					for (int num19 = 0; num19 < 1000000; num19++)
					{
						MessageQueue.AddPlayerMessage("garbage");
					}
					return;
				}
				if (Wish == "playeronly")
				{
					The.Game.ActionManager.ActionQueue.Clear();
					The.Game.ActionManager.ActionQueue.Enqueue(who);
					The.Game.ActionManager.ActionQueue.Enqueue(null);
					MessageQueue.AddPlayerMessage("Removed everyone but the player from the action queue.");
					return;
				}
				if (Wish.StartsWith("confusion"))
				{
					string[] array4 = Wish.Split(':');
					int duration = ((array4.Length >= 2) ? int.Parse(array4[1]) : 10);
					int num20 = ((array4.Length >= 3) ? int.Parse(array4[2]) : 5);
					who.ApplyEffect(new XRL.World.Effects.Confused(duration, num20, num20 + 2));
					return;
				}
				if (Wish.StartsWith("roll:"))
				{
					Popup.Show(Stat.Roll(Wish.Split(':')[1]).ToString());
					return;
				}
				if (Wish.StartsWith("rollmin:"))
				{
					Popup.Show(Stat.RollMin(Wish.Split(':')[1]).ToString());
					return;
				}
				if (Wish.StartsWith("rollmax:"))
				{
					Popup.Show(Stat.RollMax(Wish.Split(':')[1]).ToString());
					return;
				}
				if (Wish.StartsWith("rollcached:"))
				{
					Popup.Show(Stat.RollCached(Wish.Split(':')[1]).ToString());
					return;
				}
				if (Wish.StartsWith("rollmincached:"))
				{
					Popup.Show(Stat.RollMinCached(Wish.Split(':')[1]).ToString());
					return;
				}
				if (Wish.StartsWith("rollmaxcached:"))
				{
					Popup.Show(Stat.RollMaxCached(Wish.Split(':')[1]).ToString());
					return;
				}
				if (Wish.StartsWith("godown:"))
				{
					int n2 = int.Parse(Wish.Split(':')[1]);
					Zone zoneFromDirection = who.CurrentZone.GetZoneFromDirection("D", n2);
					Point2D pos2D = who.CurrentCell.Pos2D;
					who.Physics.CurrentCell.RemoveObject(who);
					zoneFromDirection.GetCell(pos2D).AddObject(who);
					The.ZoneManager.SetActiveZone(zoneFromDirection);
					The.ZoneManager.ProcessGoToPartyLeader();
					return;
				}
				if (Wish == "sherlock")
				{
					game.CompleteQuest("Find Eskhind");
					game.StartQuest("Kith and Kin");
					JournalAPI.Observations.Where((JournalObservation o) => o.Has("hindrenclue") && o.Has("free")).ToList().Shuffle()
						.Take(5)
						.ToList()
						.ForEach((JournalObservation o) =>
						{
							JournalAPI.RevealObservation(o);
						});
					JournalAPI.Observations.Where((JournalObservation o) => o.Has("hindrenclue") && o.Attributes.Any((string p) => p.StartsWith("motive:"))).ToList().Shuffle()
						.Take(5)
						.ToList()
						.ForEach((JournalObservation o) =>
						{
							JournalAPI.RevealObservation(o);
						});
					KithAndKinGameState.Instance.foundClue();
					if (The.ActiveZone.FindObject("Neelahind") == null)
					{
						who.GetCurrentCell().GetCellFromDirection("NW").AddObject("Neelahind");
					}
					return;
				}
				if (Wish == "revealobservations")
				{
					Popup.Suppress = true;
					JournalAPI.Observations.ForEach((JournalObservation o) =>
					{
						JournalAPI.RevealObservation(o);
					});
					Popup.Suppress = false;
					return;
				}
				if (Wish == "revealmapnotes")
				{
					Popup.Suppress = true;
					JournalAPI.MapNotes.ForEach((JournalMapNote o) =>
					{
						JournalAPI.RevealMapNote(o);
					});
					Popup.Suppress = false;
					return;
				}
				if (Wish == "calm")
				{
					The.Core.Calm = !The.Core.Calm;
					MessageQueue.AddPlayerMessage("Calm now " + The.Core.Calm);
					return;
				}
				if (Wish == "minime")
				{
					EvilTwin.CreateEvilTwin(who, "Mini", null, "{{c|You sense a diminutive presence nearby.}}", "&K", null, null, MakeExtras: true, "It's miniature you.");
					return;
				}
				if (Wish == "blink")
				{
					who.TeleportTo(who.Physics.PickDestinationCell(9999, AllowVis.OnlyExplored, Locked: false, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Teleport"), 0);
					return;
				}
				if (Wish == "license")
				{
					MessageQueue.AddPlayerMessage("License tier now " + who.ModIntProperty("CyberneticsLicenses", 20));
					return;
				}
				if (Wish.StartsWith("license:"))
				{
					int num21 = int.Parse(Wish.Split(':')[1]);
					if (num21 != 0)
					{
						MessageQueue.AddPlayerMessage("License tier now " + who.ModIntProperty("CyberneticsLicenses", num21));
					}
					return;
				}
				if (Wish == "impl")
				{
					who.ImplosionSplat();
					return;
				}
				if (Wish.StartsWith("impl:"))
				{
					int num22 = int.Parse(Wish.Split(':')[1]);
					if (num22 != 0)
					{
						who.ImplosionSplat(num22);
					}
					return;
				}
				if (Wish == "gainmp")
				{
					Popup.Show(who.GainMP(1).ToString());
					return;
				}
				if (Wish.StartsWith("gainmp:"))
				{
					int num23 = int.Parse(Wish.Split(':')[1]);
					if (num23 != 0)
					{
						Popup.Show(who.GainMP(num23).ToString());
					}
					return;
				}
				if (Wish == "bits")
				{
					who.RequirePart<BitLocker>().AddAllBits(20);
					return;
				}
				if (Wish.StartsWith("bits:"))
				{
					int num24 = int.Parse(Wish.Split(':')[1]);
					if (num24 > 0)
					{
						who.RequirePart<BitLocker>().AddAllBits(num24);
					}
					return;
				}
				if (Wish == "smartass")
				{
					Popup.Suppress = true;
					try
					{
						AddSkill("Tinkering");
						AddSkill("Tinkering_GadgetInspector");
						AddSkill("Tinkering_Repair");
						AddSkill("Tinkering_ReverseEngineer");
						AddSkill("Tinkering_Scavenger");
						AddSkill("Tinkering_Disassemble");
						AddSkill("Tinkering_LayMine");
						AddSkill("Tinkering_DeployTurret");
						AddSkill("Tinkering_Tinker1");
						AddSkill("Tinkering_Tinker2");
						AddSkill("Tinkering_Tinker3");
						foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
						{
							if (!TinkerData.KnownRecipes.CleanContains(tinkerRecipe))
							{
								TinkerData.KnownRecipes.Add(tinkerRecipe);
							}
						}
						return;
					}
					finally
					{
						Popup.Suppress = false;
					}
				}
				if (Wish == "cloacasurprise")
				{
					CookingDomainSpecial_UnitSlogTransform.ApplyTo(who);
					return;
				}
				if (Wish == "crystaldelight")
				{
					CookingDomainSpecial_UnitCrystalTransform.ApplyTo(who);
					return;
				}
				if (Wish == "showrecipes")
				{
					StringBuilder stringBuilder10 = Event.NewStringBuilder();
					foreach (TinkerData tinkerRecipe2 in TinkerData.TinkerRecipes)
					{
						stringBuilder10.Append("DisplayName: ").Append(tinkerRecipe2.DisplayName ?? "NULL").Append("\nBlueprint: ")
							.Append(tinkerRecipe2.Blueprint ?? "NULL")
							.Append("\nCategory: ")
							.Append(tinkerRecipe2.Category ?? "NULL")
							.Append("\nType: ")
							.Append(tinkerRecipe2.Type ?? "NULL")
							.Append("\nTier: ")
							.Append(tinkerRecipe2.Tier)
							.Append("\nCost: ")
							.Append(tinkerRecipe2.Cost)
							.Append("\nIngredient: ")
							.Append(tinkerRecipe2.Ingredient ?? "NULL")
							.Append("\nDescriptionLineCount: ")
							.Append(tinkerRecipe2.DescriptionLineCount)
							.Append("\n\n");
					}
					Popup.Show(stringBuilder10.ToString());
					return;
				}
				if (Wish == "showmodrecipes")
				{
					StringBuilder stringBuilder11 = Event.NewStringBuilder();
					foreach (TinkerData tinkerRecipe3 in TinkerData.TinkerRecipes)
					{
						if (tinkerRecipe3.Type == "Mod")
						{
							stringBuilder11.Append("DisplayName: ").Append(tinkerRecipe3.DisplayName ?? "NULL").Append("\nBlueprint: ")
								.Append(tinkerRecipe3.Blueprint ?? "NULL")
								.Append("\nCategory: ")
								.Append(tinkerRecipe3.Category ?? "NULL")
								.Append("\nType: ")
								.Append(tinkerRecipe3.Type ?? "NULL")
								.Append("\nTier: ")
								.Append(tinkerRecipe3.Tier)
								.Append("\nCost: ")
								.Append(tinkerRecipe3.Cost)
								.Append("\nIngredient: ")
								.Append(tinkerRecipe3.Ingredient ?? "NULL")
								.Append("\nDescriptionLineCount: ")
								.Append(tinkerRecipe3.DescriptionLineCount)
								.Append("\n\n");
						}
					}
					Popup.Show(stringBuilder11.ToString());
					return;
				}
				if (Wish == "findduplicaterecipes")
				{
					Dictionary<string, int> dictionary = new Dictionary<string, int>();
					foreach (TinkerData tinkerRecipe4 in TinkerData.TinkerRecipes)
					{
						string blueprint = tinkerRecipe4.Blueprint;
						string key = (blueprint.StartsWith("[") ? blueprint : GameObjectFactory.Factory.CreateSampleObject(blueprint).Render.DisplayName);
						if (dictionary.ContainsKey(key))
						{
							dictionary[key]++;
						}
						else
						{
							dictionary.Add(key, 1);
						}
					}
					StringBuilder stringBuilder12 = Event.NewStringBuilder();
					bool flag3 = false;
					foreach (string key2 in dictionary.Keys)
					{
						if (dictionary[key2] > 1)
						{
							stringBuilder12.Append(key2).Append(" (").Append(dictionary[key2])
								.Append(")\n");
							flag3 = true;
						}
					}
					Popup.Show(flag3 ? stringBuilder12.ToString() : "no duplicate recipes found");
					return;
				}
				if (Wish == "cluber")
				{
					AddSkill("Cudgel");
					AddSkill("Cudgel_Expertise");
					AddSkill("Cudgel_Backswing");
					AddSkill("Cudgel_Bludgeon");
					AddSkill("Cudgel_ChargingStrike");
					AddSkill("Cudgel_Conk");
					AddSkill("Cudgel_Slam");
					AddSkill("Cudgel_SmashUp");
					return;
				}
				if (Wish == "fencer")
				{
					AddSkill("LongBlades");
					AddSkill("LongBladesDuelingStance");
					AddSkill("LongBladesImprovedAggressiveStance");
					AddSkill("LongBladesImprovedDefensiveStance");
					AddSkill("LongBladesImprovedDuelistStance");
					AddSkill("LongBladesDuelingStance");
					AddSkill("LongBladesLunge");
					AddSkill("LongBladesProficiency");
					AddSkill("LongBladesSwipe");
					AddSkill("LongBladesDeathblow");
					return;
				}
				if (Wish == "axer")
				{
					AddSkill("Axe");
					AddSkill("Axe_Expertise");
					AddSkill("Axe_Cleave");
					AddSkill("Cudgel_ChargingStrike");
					AddSkill("Axe_Dismember");
					AddSkill("Axe_HookAndDrag");
					AddSkill("Axe_Decapitate");
					AddSkill("Axe_Berserk");
					return;
				}
				if (Wish == "sblader")
				{
					AddSkill("ShortBlades");
					AddSkill("ShortBlades_Expertise");
					AddSkill("ShortBlades_Hobble");
					AddSkill("ShortBlades_Jab");
					AddSkill("ShortBlades_Bloodletter");
					AddSkill("ShortBlades_Shank");
					AddSkill("ShortBlades_PointedCircle");
					AddSkill("ShortBlades_Rejoinder");
					return;
				}
				if (Wish == "wandermode")
				{
					foreach (Faction item16 in Factions.Loop())
					{
						if (The.Game.PlayerReputation.Get(item16) < 0)
						{
							The.Game.PlayerReputation.Set(item16, 0);
						}
					}
					return;
				}
				if (Wish == "skillpoints")
				{
					who.Statistics["SP"].BaseValue = 20000;
					return;
				}
				if (Wish == "traveler")
				{
					foreach (Faction item17 in Factions.Loop())
					{
						game.PlayerReputation.Set(item17, 0);
					}
					return;
				}
				if (Wish == "togglementalshields")
				{
					MentalShield.Disabled = !MentalShield.Disabled;
				}
				else if (Wish == "trip")
				{
					who.ApplyEffect(new Prone());
				}
				else if (Wish == "pro")
				{
					who.Statistics["Strength"].BaseValue = 40;
					who.Statistics["Intelligence"].BaseValue = 40;
					who.Statistics["Ego"].BaseValue = 40;
					who.Statistics["Agility"].BaseValue = 40;
					who.Statistics["Toughness"].BaseValue = 40;
					who.Statistics["Willpower"].BaseValue = 40;
				}
				else if (Wish == "where?")
				{
					MessageQueue.AddPlayerMessage(who.CurrentZone.ZoneID);
					XRLCore.SetClipboard(who.CurrentZone.ZoneID);
				}
				else if (Wish.StartsWith("factionrep") || Wish.StartsWith("reputation"))
				{
					string[] array5 = Wish.Split(' ');
					if (array5.Length > 3 || Wish.Contains(":"))
					{
						array5 = Wish.Split(':');
					}
					game.PlayerReputation.Modify(array5[1], Convert.ToInt32(array5[2]), "Wish");
				}
				else
				{
					if (Wish == "memtest")
					{
						return;
					}
					if (Wish == "leadslugs")
					{
						who.ReceiveObject("Lead Slug", 10000);
						return;
					}
					if (Wish == "pistoltest")
					{
						who.ReceiveObject("Lead Slug", 10000);
						who.ReceiveObject("Chain Pistol", 2);
						AddSkill("Akimbo");
						AddSkill("Steady Hand");
						return;
					}
					if (Wish.StartsWith("regionalize"))
					{
						new RegionPopulator().BuildZone(who.Physics.CurrentCell.ParentZone);
						return;
					}
					if (Wish.StartsWith("smartitem:"))
					{
						WishResult wishResult = WishSearcher.SearchForBlueprint(Wish);
						GameObject gameObject8 = GameObjectFactory.Factory.CreateObject(wishResult.Result, 0, 0, null, null, null, "Wish");
						gameObject8.AddPart(new SmartItem());
						who.Physics.CurrentCell.AddObject(gameObject8);
						return;
					}
					if (Wish.StartsWith("seed:"))
					{
						int value2 = Convert.ToInt32(Wish.Split(':')[1]);
						game.SetIntGameState("WorldSeed", value2);
						return;
					}
					if (Wish.StartsWith("setintgamestate:"))
					{
						string[] array6 = Wish.Split(':');
						int value3 = ((array6.Length <= 2) ? 1 : int.Parse(array6[2]));
						The.Game.SetIntGameState(array6[1], value3);
						return;
					}
					if (Wish.StartsWith("setboolgamestate:"))
					{
						string[] array7 = Wish.Split(':');
						bool value4 = array7.Length <= 2 || bool.Parse(array7[2]);
						The.Game.SetBooleanGameState(array7[1], value4);
						return;
					}
					if (Wish == "glowcrust")
					{
						FungalSporeInfection.ApplyFungalInfection(who, "LuminousInfection");
						return;
					}
					if (Wish == "mumblemouth")
					{
						FungalSporeInfection.ApplyFungalInfection(who, "MumblesInfection");
						return;
					}
					if (Wish == "waxflab")
					{
						FungalSporeInfection.ApplyFungalInfection(who, "WaxInfection");
						return;
					}
					if (Wish == "testmadness")
					{
						who.Physics.CurrentCell.ParentZone.FindClosestObjectWithPart(who, "Brain", ExploredOnly: true, IncludeSelf: false).Brain.PushGoal(new PaxKlanqMadness());
						return;
					}
					if (Wish == "randomitems")
					{
						for (int num25 = 0; num25 < 20; num25++)
						{
							who.GetCurrentCell().AddObject(EncountersAPI.GetAnItem());
						}
						return;
					}
					if (Wish.StartsWith("sultantest:"))
					{
						string[] collection = Wish.Split(':')[1].Split(',');
						Zone zone = The.ZoneManager.GetZone("JoppaWorld");
						ZoneManager zoneManager = The.ZoneManager;
						History sultanHistory = The.Game.sultanHistory;
						Cell cell5 = zone.GetCell(who.GetCurrentCell().Pos2D);
						SultanDungeonArgs sultanDungeonArgs = new SultanDungeonArgs();
						HistoricEntity historicEntity = sultanHistory.CreateEntity(sultanHistory.currentYear);
						historicEntity.ApplyEvent(new InitializeRegion(5));
						HistoricEntitySnapshot currentSnapshot = historicEntity.GetCurrentSnapshot();
						foreach (string item18 in new List<string>(currentSnapshot.properties.Keys))
						{
							if (item18 != "name" && item18 != "newName" && item18 != "period")
							{
								currentSnapshot.properties.Remove(item18);
							}
						}
						currentSnapshot.listProperties.Clear();
						currentSnapshot.listProperties.Add("testAttributes", new List<string>());
						currentSnapshot.listProperties["testAttributes"].AddRange(collection);
						sultanDungeonArgs.UpdateFromEntity(currentSnapshot);
						string property = currentSnapshot.GetProperty("name");
						The.Game.SetObjectGameState("sultanDungeonArgs_" + property, sultanDungeonArgs);
						Vector2i vector2i = new Vector2i(cell5.X, cell5.Y);
						string text12 = Grammar.MakeTitleCase(property);
						int num26 = 10;
						for (int num27 = 0; num27 < num26; num27++)
						{
							HistoricEntity historicEntity2 = sultanHistory.CreateEntity(sultanHistory.currentYear);
							historicEntity2.ApplyEvent(new InitializeLocation(property, 5));
							string property2 = historicEntity2.GetCurrentSnapshot().GetProperty("name");
							string zoneID2 = "JoppaWorld." + vector2i.x + "." + vector2i.y + ".1.1." + (num27 + 10);
							if (num27 == 0)
							{
								zoneManager.SetZoneName(zoneID2, text12, null, null, null, null, Proper: true);
							}
							else
							{
								zoneManager.SetZoneName(zoneID2, "liminal floor", text12);
							}
							string text13 = "";
							if (num27 < num26 - 1)
							{
								text13 += "D";
							}
							if (num27 > 0)
							{
								text13 += "U";
							}
							zoneManager.ClearZoneBuilders(zoneID2);
							zoneManager.SetZoneProperty(zoneID2, "SkipTerrainBuilders", true);
							zoneManager.AddZoneMidBuilder(zoneID2, "SultanDungeon", "locationName", property2, "regionName", property, "stairs", text13);
						}
						return;
					}
					if (Wish == "sultanreveal")
					{
						Popup.Suppress = true;
						ItemNaming.Suppress = true;
						Zone zone2 = The.ZoneManager.GetZone("JoppaWorld");
						for (int num28 = 0; num28 < 25; num28++)
						{
							for (int num29 = 0; num29 < 80; num29++)
							{
								zone2.GetCell(num29, num28).GetFirstObjectWithPart("TerrainTravel").FireEvent("SultanReveal");
							}
						}
						Popup.Suppress = false;
						ItemNaming.Suppress = false;
						return;
					}
					if (Wish == "villagereveal")
					{
						Popup.Suppress = true;
						ItemNaming.Suppress = true;
						Zone zone3 = The.ZoneManager.GetZone("JoppaWorld");
						for (int num30 = 0; num30 < 25; num30++)
						{
							for (int num31 = 0; num31 < 80; num31++)
							{
								zone3.GetCell(num31, num30).GetFirstObjectWithPart("TerrainTravel").FireEvent("VillageReveal");
							}
						}
						Popup.Suppress = false;
						ItemNaming.Suppress = false;
						return;
					}
					if (Wish == "zonebuilders")
					{
						IEnumerable<ZoneBuilderBlueprint> buildersFor = The.ZoneManager.GetBuildersFor(The.ZoneManager.ActiveZone);
						StringBuilder stringBuilder13 = new StringBuilder();
						foreach (ZoneBuilderBlueprint item19 in buildersFor)
						{
							stringBuilder13.Append(item19.Class);
							if (item19.Parameters != null && item19.Parameters.Count > 0)
							{
								stringBuilder13.Append(" [");
								foreach (KeyValuePair<string, object> parameter in item19.Parameters)
								{
									if (stringBuilder13[stringBuilder13.Length - 1] != '[')
									{
										stringBuilder13.Append(", ");
									}
									stringBuilder13.Append(parameter.Key).Append(": ");
									stringBuilder13.Append(parameter.Value.ToString());
								}
								stringBuilder13.Append(']');
							}
							stringBuilder13.Append('\n');
						}
						Popup.Show(stringBuilder13.ToString());
						return;
					}
					if (Wish == "zoneconnections")
					{
						Zone activeZone = The.ActiveZone;
						ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer1();
						scrapBuffer2.RenderBase();
						foreach (ZoneConnection item20 in activeZone.EnumerateConnections())
						{
							if (item20 is CachedZoneConnection cachedZoneConnection)
							{
								scrapBuffer2.WriteAt(item20.X, item20.Y, "{{R|" + cachedZoneConnection.TargetDirection + "}}");
							}
							else
							{
								scrapBuffer2.WriteAt(item20.X, item20.Y, "{{M|X}}");
							}
						}
						scrapBuffer2.Draw();
						Keyboard.getch();
						return;
					}
					if (Wish == "freezezones")
					{
						foreach (Zone value12 in The.ZoneManager.CachedZones.Values)
						{
							if (!value12.IsActive())
							{
								value12.LastActive = 0L;
							}
						}
						The.ZoneManager.CheckCached();
						return;
					}
					if (Wish == "clearfrozen")
					{
						The.ZoneManager.ClearFrozen();
						return;
					}
					if (Wish == "sultanhistory")
					{
						foreach (JournalSultanNote sultanNote in JournalAPI.GetSultanNotes())
						{
							sultanNote.Reveal(null, Silent: true);
						}
						return;
					}
					if (Wish == "sultanquests")
					{
						HistoricEntityList entitiesWherePropertyEquals = The.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan");
						for (int num32 = 0; num32 < entitiesWherePropertyEquals.entities.Count; num32++)
						{
							for (int num33 = 0; num33 < entitiesWherePropertyEquals.entities[num32].events.Count; num33++)
							{
								entitiesWherePropertyEquals.entities[num32].events[num33].Reveal();
							}
						}
						return;
					}
					if (Wish == "reveal1sultanhistory")
					{
						string text14 = null;
						{
							foreach (JournalSultanNote sultanNote2 in JournalAPI.GetSultanNotes())
							{
								if (text14 == null)
								{
									text14 = sultanNote2.SultanID;
								}
								if (sultanNote2.SultanID == text14)
								{
									sultanNote2.Reveal();
								}
							}
							return;
						}
					}
					if (Wish == "revealallsecrets")
					{
						foreach (IBaseJournalEntry allNote in JournalAPI.GetAllNotes())
						{
							allNote.Reveal(null, Silent: true);
						}
						return;
					}
					if (Wish == "glass")
					{
						History sultanHistory2 = The.Game.sultanHistory;
						HistoricEntityList entitiesWithProperty = sultanHistory2.GetEntitiesWithProperty("itemType");
						for (int num34 = 0; num34 < entitiesWithProperty.entities.Count; num34++)
						{
							HistoricEntitySnapshot currentSnapshot2 = entitiesWithProperty.entities[num34].GetCurrentSnapshot();
							UnityEngine.Debug.Log("New historic relic: " + currentSnapshot2.GetProperty("name"));
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(currentSnapshot2, Stat.Random(1, 8)));
						}
						List<string> list9 = new List<string>();
						list9.Add("glass");
						sultanHistory2.GetEntitiesWherePropertyEquals("type", "region").GetRandomElement().GetCurrentSnapshot();
						List<string> list10 = new List<string>();
						list10.Add(list9.GetRandomElement());
						{
							foreach (string type3 in RelicGenerator.GetTypes())
							{
								who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(type3, Stat.Random(1, 8), null, list10));
							}
							return;
						}
					}
					if (Wish.StartsWith("findmod:"))
					{
						string[] array8 = Wish.Split(':');
						ModInfo modBySpec = ModManager.GetModBySpec(array8[1]);
						if (modBySpec == null)
						{
							Popup.Show("No mod found for " + array8[1]);
							return;
						}
						string text15 = "Found mod";
						List<string> list11 = new List<string>();
						if (modBySpec.ID.IsNullOrEmpty())
						{
							list11.Add("no ID");
						}
						else
						{
							text15 = text15 + " " + modBySpec.ID;
						}
						if (modBySpec.WorkshopInfo != null && modBySpec.WorkshopInfo.WorkshopId != 0)
						{
							text15 = text15 + " workshop ID " + modBySpec.WorkshopInfo.WorkshopId;
						}
						else
						{
							list11.Add("no workshop ID");
						}
						if (modBySpec.IsScripting)
						{
							list11.Add("scripting");
						}
						if (modBySpec.Path != null)
						{
							text15 = text15 + " in " + modBySpec.Path;
						}
						else
						{
							list11.Add("no file path");
						}
						if (list11.Count > 0)
						{
							text15 = text15 + " (" + string.Join(", ", list11.ToArray()) + ")";
						}
						Popup.Show(text15);
						return;
					}
					if (Wish.StartsWith("randomrelic:"))
					{
						string[] array9 = Wish.Split(':');
						for (int num35 = 0; num35 < Convert.ToInt32(array9[1]); num35++)
						{
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(Stat.Random(1, 8), RandomName: true));
						}
						return;
					}
					if (Wish == "sultanrelics")
					{
						HistoricEntityList entitiesWithProperty2 = The.Game.sultanHistory.GetEntitiesWithProperty("itemType");
						for (int num36 = 0; num36 < entitiesWithProperty2.entities.Count; num36++)
						{
							HistoricEntitySnapshot currentSnapshot3 = entitiesWithProperty2.entities[num36].GetCurrentSnapshot();
							UnityEngine.Debug.Log("New historic relic: " + currentSnapshot3.GetProperty("name"));
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(currentSnapshot3));
						}
						return;
					}
					if (Wish == "relic")
					{
						History sultanHistory3 = The.Game.sultanHistory;
						HistoricEntityList entitiesWithProperty3 = sultanHistory3.GetEntitiesWithProperty("itemType");
						for (int num37 = 0; num37 < entitiesWithProperty3.entities.Count; num37++)
						{
							HistoricEntitySnapshot currentSnapshot4 = entitiesWithProperty3.entities[num37].GetCurrentSnapshot();
							UnityEngine.Debug.Log("New historic relic: " + currentSnapshot4.GetProperty("name"));
							who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(currentSnapshot4));
						}
						List<string> list12 = new List<string>();
						list12.Add("chance");
						list12.Add("might");
						list12.Add("scholarship");
						list12.Add("time");
						list12.Add("ice");
						list12.Add("stars");
						list12.Add("salt");
						list12.Add("jewels");
						list12.Add("glass");
						list12.Add("circuitry");
						list12.Add("travel");
						sultanHistory3.GetEntitiesWherePropertyEquals("type", "region").GetRandomElement().GetCurrentSnapshot();
						List<string> list13 = new List<string>();
						list13.Add(list12.GetRandomElement());
						{
							foreach (string type4 in RelicGenerator.GetTypes())
							{
								who.GetCurrentCell().AddObject(RelicGenerator.GenerateRelic(type4, Stat.Random(1, 8), null, list13));
							}
							return;
						}
					}
					if (Wish == "shownamingchances")
					{
						StringBuilder stringBuilder14 = Event.NewStringBuilder();
						Dictionary<GameObject, int> chances = ItemNaming.GetNamingChances(The.Player);
						List<GameObject> list14 = Event.NewGameObjectList();
						list14.AddRange(chances.Keys);
						list14.Sort((GameObject a, GameObject b) => chances[b].CompareTo(chances[a]));
						foreach (GameObject item21 in list14)
						{
							stringBuilder14.Append(chances[item21]).Append(": ").Append(item21.DisplayName)
								.Append("&y\n");
						}
						Popup.Show(stringBuilder14.ToString());
						return;
					}
					if (Wish == "whatami")
					{
						MessageQueue.AddPlayerMessage("I am a: " + who.Blueprint);
						return;
					}
					if (Wish == "cool")
					{
						The.Core.cool = !The.Core.cool;
						The.ActionManager.TickAbilityCooldowns(9999999);
						MessageQueue.AddPlayerMessage("Coolmode now " + The.Core.cool);
						return;
					}
					if (Wish.StartsWith("stat:"))
					{
						string[] array10 = Wish.Split(':');
						who.Statistics[array10[1]].BaseValue += Convert.ToInt32(array10[2]);
						MessageQueue.AddPlayerMessage("Added " + Convert.ToInt32(array10[2]) + " to " + array10[1] + "'s BaseValue");
						return;
					}
					if (Wish.StartsWith("statbonus:"))
					{
						string[] array11 = Wish.Split(':');
						who.Statistics[array11[1]].Bonus += Convert.ToInt32(array11[2]);
						MessageQueue.AddPlayerMessage("Added " + Convert.ToInt32(array11[2]) + " to " + array11[1] + "'s Bonus");
						return;
					}
					if (Wish.StartsWith("statpenality:") || Wish.StartsWith("statpenalty:"))
					{
						string[] array12 = Wish.Split(':');
						who.Statistics[array12[1]].Penalty += Convert.ToInt32(array12[2]);
						MessageQueue.AddPlayerMessage("Added " + Convert.ToInt32(array12[2]) + " to " + array12[1] + "'s Penalty");
						return;
					}
					if (Wish == "testearl")
					{
						string[] array13 = new string[5];
						List<string> list15 = new List<string>();
						list15.Add("Dogs");
						list15.Add("Cannibals");
						list15.Add("Dromad");
						list15.Add("Girsh");
						List<string> list16 = new List<string>();
						for (int num38 = 0; num38 < list15.Count; num38++)
						{
							list16.Add(Faction.GetFormattedName(list15[num38]));
						}
						array13[0] = "Share the burden across all allies. [-&C50&y reputation with each attending faction]";
						array13[1] = "Share the burden between two allies. [-&C100&y reputation with two attending factions of your choice]";
						array13[2] = "Spare one faction of all obligation by betraying a second faction and selling their secrets to Asphodel. [-&C800&y with the betrayed faction, +&C200&y reputation with the spared faction + a faction heirloom]";
						array13[3] = "Invoke the Chaos Spiel. [????????, +&C300&y reputation with &Chighly &Centropic &Cbeings&y]";
						array13[4] = "Take time to weigh the options.";
						char[] hotkeys = new char[5] { 'a', 'b', 'c', 'd', 'e' };
						string text16 = "";
						for (int num39 = 0; num39 < list15.Count; num39++)
						{
							text16 = text16 + list15[num39] + ", ";
						}
						Popup.PickOption("", "The First Council of Omonporch has begun. Choose how to appease Asphodel.", "", "Sounds/UI/ui_notification", array13, hotkeys, null, null, null, null, null, 1, 75);
						return;
					}
					if (Wish == "bookfuck")
					{
						for (int num40 = 0; num40 < 10; num40++)
						{
							GameObject gameObject9 = GameObjectFactory.Factory.CreateObject("StandaloneMarkovBook");
							StringBuilder stringBuilder15 = new StringBuilder();
							gameObject9.GetPart<MarkovBook>().GeneratePages();
							stringBuilder15.Append("{\\rtf1\\ansi\r\n\r\n    \\pgbrdrt\r\n    \\brdrart1\r\n    \\pgbrdrb\r\n    \\brdrart1\r\n    \\pgbrdrl\r\n    \\brdrart1\r\n    \\pgbrdrr\r\n    \\brdrart1\r\n\r\n    {\\fonttbl\\f0\\froman Georgia;}\\f0\\pard");
							stringBuilder15.Append("{\\pard\\qc\\fs36 ");
							stringBuilder15.AppendLine(gameObject9.DisplayNameOnlyStripped);
							stringBuilder15.Append(" \\par}");
							stringBuilder15.AppendLine();
							stringBuilder15.AppendLine("\\par");
							foreach (BookPage page in gameObject9.GetPart<MarkovBook>().Pages)
							{
								stringBuilder15.AppendLine(page.FullText.Replace("\n", " ").Replace("\r", ""));
								stringBuilder15.AppendLine("\\par\\fs24");
							}
							stringBuilder15.Append("{\\footer\\pard\\qc\\fs18 Using Predictive Text to Generate Lore in {\\pard\\qc\\fs18\\i Caves of Qud} \\par}");
							stringBuilder15.Append("}");
							File.WriteAllText(DataManager.SavePath("book_" + num40 + ".rtf"), stringBuilder15.ToString());
						}
						return;
					}
					if (Wish == "bookfuckonefile")
					{
						StringBuilder stringBuilder16 = new StringBuilder();
						stringBuilder16.Append("{\\rtf1\\ansi\r\n\r\n    \\pgbrdrt\r\n    \\brdrart1\r\n    \\pgbrdrb\r\n    \\brdrart1\r\n    \\pgbrdrl\r\n    \\brdrart1\r\n    \\pgbrdrr\r\n    \\brdrart1\r\n\r\n    {\\fonttbl\\f0\\froman Georgia;}\\f0\\pard");
						for (int num41 = 0; num41 < 300; num41++)
						{
							GameObject gameObject10 = GameObjectFactory.Factory.CreateObject("StandaloneMarkovBook");
							gameObject10.GetPart<MarkovBook>().GeneratePages();
							stringBuilder16.Append("{\\pard\\qc\\fs36 ");
							stringBuilder16.AppendLine(gameObject10.DisplayNameOnlyStripped);
							stringBuilder16.Append(" \\par}");
							stringBuilder16.AppendLine();
							stringBuilder16.AppendLine("\\par");
							foreach (BookPage page2 in gameObject10.GetPart<MarkovBook>().Pages)
							{
								stringBuilder16.AppendLine(page2.FullText.Replace("\n", " ").Replace("\r", ""));
								stringBuilder16.AppendLine("\\par\\fs24");
							}
							stringBuilder16.Append("{\\footer\\pard\\qc\\fs18 Using Predictive Text to Generate Lore in {\\pard\\qc\\fs18\\i Caves of Qud} \\par}");
							stringBuilder16.Append("\\pard \\insrsid \\page \\par");
						}
						stringBuilder16.Append("}");
						File.WriteAllText(DataManager.SavePath("bigbook.rtf"), stringBuilder16.ToString());
						return;
					}
					if (Wish == "clearfactionmembership")
					{
						who.Brain.Allegiance.Clear();
						MessageQueue.AddPlayerMessage("Cleared faction membership");
						return;
					}
					if (Wish == "allbox")
					{
						GameObject gameObject11 = GameObjectFactory.Factory.CreateObject("Chest");
						Inventory inventory = gameObject11.Inventory;
						foreach (string key3 in GameObjectFactory.Factory.Blueprints.Keys)
						{
							if (GameObjectFactory.Factory.Blueprints[key3].InheritsFrom("Item"))
							{
								inventory.AddObject(GameObjectFactory.Factory.CreateObject(key3));
							}
						}
						who.GetCurrentCell().GetCellFromDirection("N").AddObject(gameObject11);
						return;
					}
					if (Wish == "fungone")
					{
						who.RemoveEffect<FungalSporeInfection>();
						who.RemoveEffect<SporeCloudPoison>();
						return;
					}
					if (Wish == "test437")
					{
						StringBuilder stringBuilder17 = new StringBuilder();
						char c = '\0';
						for (int num42 = 0; num42 < 16; num42++)
						{
							for (int num43 = 0; num43 < 16; num43++)
							{
								stringBuilder17.Append(c);
								c = (char)(c + 1);
							}
							stringBuilder17.AppendLine();
						}
						Popup.Show(stringBuilder17.ToString());
						return;
					}
					if (Wish == "spend")
					{
						who.RandomlySpendPoints();
						return;
					}
					if (Wish == "playerlevelmob")
					{
						who.GetCurrentCell().GetCellFromDirection("NW").AddObject(EncountersAPI.GetCreatureAroundPlayerLevel());
						return;
					}
					if (Wish == "auditblueprints")
					{
						foreach (GameObjectBlueprint blueprint2 in GameObjectFactory.Factory.BlueprintList)
						{
							if (blueprint2.HasTag("BaseObject") && blueprint2.HasTag("ExcludeFromDynamicEncounters") && blueprint2.GetTag("ExcludeFromDynamicEncounters") == "*noinherit")
							{
								Popup.Show(blueprint2.Name + " has BaseObject and ExcludeFromDynamicEncounters with *noinherit");
							}
							string text17 = blueprint2.DisplayName();
							if (text17 != null && (text17.StartsWith("[") || text17.EndsWith("]")) && !blueprint2.HasTag("BaseObject") && !blueprint2.HasTag("ExcludeFromDynamicEncounters"))
							{
								Popup.Show(blueprint2.Name + " has display name " + text17 + " and neither BaseObject nor ExcludeFromDynamicEncounters");
							}
						}
						return;
					}
					if (Wish == "auditrenderlayers")
					{
						foreach (GameObjectBlueprint blueprint3 in GameObjectFactory.Factory.BlueprintList)
						{
							if (blueprint3.GetPartParameter("Physics", "Solid", Default: false) && !blueprint3.InheritsFrom("Terrain"))
							{
								int partParameter = blueprint3.GetPartParameter("Render", "RenderLayer", -1);
								if (partParameter < 6)
								{
									if (partParameter == -1)
									{
										Popup.Show(blueprint3.Name + " is solid and has unparseable RenderLayer");
									}
									else
									{
										Popup.Show(blueprint3.Name + " is solid and has RenderLayer " + partParameter);
									}
								}
							}
						}
						return;
					}
					if (Wish == "findfarmers")
					{
						string text18 = "";
						foreach (GameObjectBlueprint blueprint4 in GameObjectFactory.Factory.BlueprintList)
						{
							if (!blueprint4.Tags.ContainsKey("BaseObject"))
							{
								GamePartBlueprint part4 = blueprint4.GetPart("Render");
								if (part4 != null && part4.TryGetParameter<string>("Tile", out var Value) && Value == "Assets_Content_Textures_Creatures_sw_farmer.bmp")
								{
									text18 = text18 + blueprint4.Name + "\n";
								}
							}
						}
						Popup.Show(text18);
						return;
					}
					if (Wish.StartsWith("dismember:"))
					{
						string text19 = Wish.Split(':')[1];
						Body body2 = who.Body;
						BodyPart bodyPart = body2.GetPartByName(text19) ?? body2.GetPartByDescription(text19) ?? body2.GetPartByName(text19.ToLower()) ?? body2.GetPartByDescription(Grammar.MakeTitleCase(text19));
						if (bodyPart == null)
						{
							Popup.Show("Could not find body part by name or description: " + text19);
						}
						else if (!bodyPart.IsSeverable())
						{
							if (bodyPart.Integral)
							{
								Popup.Show("Your " + bodyPart.Name + " " + (bodyPart.Plural ? "are" : "is") + " an integral part of your body and " + (bodyPart.Plural ? "are" : "is") + " not dismemberable.");
							}
							else if (bodyPart.DependsOn != null || bodyPart.RequiresType != null)
							{
								Popup.Show("Your " + bodyPart.Name + " " + (bodyPart.Plural ? "are" : "is") + " not directly dismemberable, instead being lost when other body parts are dismembered.");
							}
							else
							{
								Popup.Show("Your " + bodyPart.Name + " " + (bodyPart.Plural ? "are" : "is") + " not dismemberable.");
							}
						}
						else
						{
							Axe_Dismember.Dismember(who, who, null, bodyPart, null, null, "sfx_characterTrigger_dismember", assumeDecapitate: true, suppressDecapitate: false, weaponActing: false, UsePopups: true);
						}
						return;
					}
					if (Wish == "clearach!!!")
					{
						if (Popup.ShowYesNo("Are you sure you want to RESET ALL YOUR ACHIEVEMENTS?") == DialogResult.Yes)
						{
							AchievementManager.Reset();
						}
						return;
					}
					if (Wish.StartsWith("regeneratedefaultequipment"))
					{
						who?.Body?.RegenerateDefaultEquipment();
						return;
					}
					if (Wish.StartsWith("cooktestunits:"))
					{
						GameObject gameObject12 = who;
						ProceduralCookingEffect proceduralCookingEffect = ProceduralCookingEffect.CreateJustUnits(new List<string>(Wish.Split(':')[1].Split(',')));
						proceduralCookingEffect.Init(gameObject12);
						gameObject12.ApplyEffect(proceduralCookingEffect);
						return;
					}
					if (Wish.StartsWith("cooktestfull:"))
					{
						GameObject gameObject13 = who;
						string[] array14 = Wish.Split(':')[1].Split(',');
						ProceduralCookingEffectWithTrigger proceduralCookingEffectWithTrigger = ProceduralCookingEffect.CreateBaseAndTriggeredAction(array14[0], array14[1], array14[2]);
						proceduralCookingEffectWithTrigger.Init(gameObject13);
						gameObject13.ApplyEffect(proceduralCookingEffectWithTrigger);
						return;
					}
					if (Wish == "purgeobjectcache!")
					{
						The.ZoneManager.CachedObjects.Clear();
						MessageQueue.AddPlayerMessage("Purged object cache.");
						return;
					}
					if (Wish.StartsWith("unequip:"))
					{
						string requiredPart = Wish.Split(':')[1];
						GameObject gameObject14 = who;
						GameObject equipped = gameObject14.Body.GetPartByName(requiredPart).Equipped;
						gameObject14.Body.GetPartByName(requiredPart)._Equipped = null;
						gameObject14.Inventory.AddObject(equipped);
						return;
					}
					if (Wish == "objtest")
					{
						GameObject gameObject15 = null;
						Stopwatch stopwatch = Stopwatch.StartNew();
						for (int num44 = 0; num44 < 100; num44++)
						{
							gameObject15 = GameObjectFactory.Factory.CreateObject("Tam");
						}
						stopwatch.Stop();
						Stopwatch stopwatch2 = Stopwatch.StartNew();
						for (int num45 = 0; num45 < 100; num45++)
						{
							gameObject15 = gameObject15.DeepCopy();
						}
						stopwatch2.Stop();
						UnityEngine.Debug.Log("MS create: " + stopwatch.Elapsed);
						UnityEngine.Debug.Log("MS clone: " + stopwatch2.Elapsed);
						return;
					}
					if (Wish == "memcheck")
					{
						MessageQueue.AddPlayerMessage("Before Total Memory: " + GC.GetTotalMemory(forceFullCollection: false));
						MessageQueue.AddPlayerMessage("After Total Memory: " + GC.GetTotalMemory(forceFullCollection: true));
						return;
					}
					if (Wish == "makevillage")
					{
						Cell currentCell4 = who.Physics.CurrentCell;
						Zone parentZone2 = who.Physics.CurrentCell.ParentZone;
						who.Physics.CurrentCell.RemoveObject(who);
						try
						{
							Village village = new Village();
							History sultanHistory4 = The.Game.sultanHistory;
							sultanHistory4.currentYear = 1000 + Stat.Random(400, 900);
							HistoricEntity historicEntity3 = sultanHistory4.CreateEntity(sultanHistory4.currentYear);
							historicEntity3.ApplyEvent(new InitializeVillage(new string[14]
							{
								"BananaGrove", "BaroqueRuins", "DesertCanyon", "Flowerfields", "Hills", "Jungle", "LakeHinnom", "MoonStair", "Mountains", "PalladiumReef",
								"Ruins", "Saltdunes", "Saltmarsh", "Water"
							}.GetRandomElement()));
							village.villageEntity = historicEntity3;
							village.BuildZone(parentZone2);
						}
						catch (Exception exception)
						{
							UnityEngine.Debug.LogException(exception);
						}
						currentCell4.AddObject(who);
						return;
					}
					if (Wish == "villageprops")
					{
						MessageQueue.AddPlayerMessage("Listing villageEntity if one exists...");
						{
							foreach (ZoneBuilderBlueprint item22 in The.ZoneManager.GetBuildersFor(who.GetCurrentCell().ParentZone))
							{
								if (item22.Class == "Village")
								{
									HistoricEntitySnapshot currentSnapshot5 = (item22.Parameters["villageEntity"] as HistoricEntity).GetCurrentSnapshot();
									MessageQueue.AddPlayerMessage(currentSnapshot5.ToString());
									UnityEngine.Debug.Log(currentSnapshot5.ToString());
								}
							}
							return;
						}
					}
					if (Wish.StartsWith("listtags:"))
					{
						string text20 = Wish.Split(':')[1];
						MessageQueue.AddPlayerMessage("Listing the tags of " + text20 + "...");
						{
							foreach (KeyValuePair<string, string> tag in GameObjectFactory.Factory.Blueprints[text20].Tags)
							{
								MessageQueue.AddPlayerMessage(tag.Key + " = " + tag.Value);
							}
							return;
						}
					}
					if (Wish.StartsWith("goto:"))
					{
						string zoneID3 = Wish.Split(':')[1];
						Zone zone4 = The.ZoneManager.GetZone(zoneID3);
						Point2D pos2D2 = who.Physics.CurrentCell.Pos2D;
						who.Physics.CurrentCell.RemoveObject(who.Physics.ParentObject);
						zone4.GetCell(pos2D2).AddObject(who);
						The.ZoneManager.SetActiveZone(zone4);
						The.ZoneManager.ProcessGoToPartyLeader();
						return;
					}
					if (Wish.StartsWith("revealsecret:"))
					{
						JournalMapNote mapNote = JournalAPI.GetMapNote(Wish.Split(':')[1]);
						if (mapNote != null && !mapNote.Revealed)
						{
							JournalAPI.RevealMapNote(mapNote);
						}
						return;
					}
					if (Wish == "sheeter")
					{
						global::Sheeter.Sheeter.MonsterSheeter();
						return;
					}
					if (Wish == "factionsheeter")
					{
						global::Sheeter.Sheeter.FactionSheeter();
						return;
					}
					if (Wish.StartsWith("removepart:"))
					{
						string text21 = Wish.Split(':')[1];
						who.RemovePart(text21);
						MessageQueue.AddPlayerMessage("Removed part " + text21 + " from player body.");
						return;
					}
					if (Wish.StartsWith("spawn:"))
					{
						string[] array15 = Wish.Split(':');
						for (int num46 = 0; num46 < 16; num46++)
						{
							The.Game.ActionManager.AddActiveObject(who.CurrentCell.ParentZone.GetCells((Cell cell12) => !cell12.Explored).ShuffleInPlace().GetRandomElement()
								.AddObject(array15[1]));
						}
						return;
					}
					if (Wish.StartsWith("othowander1"))
					{
						OthoWander1.begin();
						return;
					}
					if (Wish == "reshephgospel")
					{
						JournalAPI.GetNotesForResheph().ForEach((JournalSultanNote g) =>
						{
							g.Reveal();
						});
						return;
					}
					if (Wish == "markofdeath")
					{
						The.Player.ToggleMarkOfDeath();
						MessageQueue.AddPlayerMessage("Mark of death now " + The.Player.HasMarkOfDeath());
						return;
					}
					if (Wish == "markofdeath?")
					{
						MessageQueue.AddPlayerMessage("Mark of death is: " + The.Game.GetStringGameState("MarkOfDeath"));
						if (The.Player.HasMarkOfDeath())
						{
							MessageQueue.AddPlayerMessage("HAS mark of death");
						}
						else
						{
							MessageQueue.AddPlayerMessage("DOES NOT HAVE mark of death");
						}
						return;
					}
					if (Wish == "goclam")
					{
						Cell randomElement = The.Game.RequireSystem(() => new ClamSystem()).GetClamZone().GetCells()
							.GetRandomElement();
						The.Player.SetLongProperty("ClamTeleportTurn", The.Game.Turns);
						The.Player.TeleportTo(randomElement, 0);
						return;
					}
					if (Wish == "hydropon")
					{
						The.Player.ZoneTeleport(The.Game.GetStringGameState("HydroponZoneID"));
						return;
					}
					if (Wish == "hollowtree")
					{
						The.Player.ZoneTeleport(The.Game.GetStringGameState("HollowTreeZoneId"));
						return;
					}
					if (Wish == "gritgate")
					{
						The.Player.ZoneTeleport("JoppaWorld.22.14.1.0.13");
						GritGateScripts.OpenRank0Doors();
						return;
					}
					if (Wish == "thinworld")
					{
						GameObject player = The.Player;
						player.GetCurrentCell().GetCellFromDirection("NW").AddObject("SultanSarcophagusWPeriod6");
						ThinWorld.TransitToThinWorld(player.GetCurrentCell().GetCellFromDirection("N").AddObject("SultanSarcophagusEPeriod6"));
						return;
					}
					if (Wish == "thinworldx")
					{
						GameObject player2 = The.Player;
						player2.GetCurrentCell().GetCellFromDirection("NW").AddObject("SultanSarcophagusWPeriod6");
						ThinWorld.TransitToThinWorld(player2.GetCurrentCell().GetCellFromDirection("N").AddObject("SultanSarcophagusEPeriod6"), express: true);
						return;
					}
					if (Wish == "returntoqud")
					{
						ThinWorld.ReturnToQud();
						return;
					}
					if (Wish == "somethinggoeswrong")
					{
						ThinWorld.SomethingGoesWrong(The.Player);
						return;
					}
					if (Wish == "sultanmuralwalltest")
					{
						foreach (Cell cell16 in The.Player.Physics.CurrentCell.ParentZone.GetCells())
						{
							if (cell16.Y % 2 == 0)
							{
								cell16.ClearWalls();
								cell16.AddObject("SultanMuralWall");
							}
						}
						return;
					}
					if (Wish == "sultanmuralwalltest2")
					{
						Zone parentZone3 = The.Player.Physics.CurrentCell.ParentZone;
						for (int num47 = 0; num47 < 12; num47++)
						{
							Cell cell6 = parentZone3.GetCell(num47, 0);
							cell6.ClearWalls();
							cell6.AddObject("SultanMuralWall");
						}
						return;
					}
					if (Wish == "zonetier")
					{
						MessageQueue.AddPlayerMessage(The.Player.CurrentCell.ParentZone.NewTier.ToString());
						return;
					}
					if (Wish == "exception")
					{
						int num48 = 0;
						_ = 10 / num48;
						return;
					}
					if (Wish == "iamconfused")
					{
						XRLCore.player.ApplyEffect(new XRL.World.Effects.Confused(80, 10));
						return;
					}
					if (Wish == "clearprefs")
					{
						GameManager.Instance.uiQueue.queueTask(() =>
						{
							PlayerPrefs.DeleteAll();
						});
						MessageQueue.AddPlayerMessage("Player prefs cleared");
						return;
					}
					if (Wish.StartsWith("placeobjecttest:"))
					{
						string[] array16 = Wish.Split(':');
						if (GameObjectFactory.Factory.CreateObject(array16[1]) != null)
						{
							for (int num49 = 0; num49 < 200; num49++)
							{
								ZoneBuilderSandbox.PlaceObject(array16[1], The.Player.CurrentZone);
							}
						}
						return;
					}
					if (Wish == "allloved")
					{
						foreach (Faction item23 in Factions.Loop())
						{
							The.Game.PlayerReputation.Set(item23, 2000);
						}
						MessageQueue.AddPlayerMessage("Factionrep set to all loved.");
						return;
					}
					if (Wish == "allhated")
					{
						foreach (Faction item24 in Factions.Loop())
						{
							The.Game.PlayerReputation.Set(item24, -2000);
						}
						MessageQueue.AddPlayerMessage("Factionrep set to all hated.");
						return;
					}
					if (Wish == "cloacasurprise")
					{
						new CloacaSurprise().ApplyEffectsTo(The.Player);
						return;
					}
					if (Wish == "cryotube")
					{
						PossibleCryotube possibleCryotube = new PossibleCryotube();
						possibleCryotube.Chance = 10000;
						possibleCryotube.BuildZone(The.Player.CurrentZone);
						return;
					}
					if (Wish.StartsWith("redrock:"))
					{
						string zoneID4 = "JoppaWorld.11.20.1.1." + Wish.Split(':')[1];
						The.Player.SystemMoveTo(The.ZoneManager.GetZone(zoneID4).GetCell(40, 24));
						return;
					}
					if (Wish == "ensurevoids")
					{
						ZoneBuilderSandbox.EnsureAllVoidsConnected(The.Player.CurrentZone);
						return;
					}
					if (Wish == "ydfreehold")
					{
						string zoneID5 = "JoppaWorld.67.17.1.1.10";
						The.Player.SystemMoveTo(The.ZoneManager.GetZone(zoneID5).GetCell(40, 23));
						return;
					}
					if (Wish == "eyn")
					{
						string zoneID6 = "JoppaWorld.76.5.1.1.10";
						The.Player.SystemMoveTo(The.ZoneManager.GetZone(zoneID6).GetCell(40, 14));
						return;
					}
					if (Wish == "moonstair")
					{
						string zoneID7 = "JoppaWorld.75.13.1.1.10";
						The.Player.SystemMoveTo(The.ZoneManager.GetZone(zoneID7).GetCell(40, 24));
						return;
					}
					if (Wish == "moonstair2")
					{
						string zoneID8 = "JoppaWorld.76.5.1.1.11";
						The.Player.SystemMoveTo(The.ZoneManager.GetZone(zoneID8).GetCell(31, 9));
						return;
					}
					if (Wish == "delaytest")
					{
						The.Player.PlayWorldSound("Sounds/Grenade/sfx_grenade_flashbang_explode", 0.5f, 0f, Combat: false, 4f);
						CombatJuice.playWorldSound(The.Player, "Sounds/Grenade/sfx_grenade_flashbang_explode", 0.5f, 0f, 0f, 3f);
						return;
					}
					if (Wish == "classreport")
					{
						HashSet<string> hashSet = new HashSet<string>();
						string text22 = "";
						foreach (GameObjectBlueprint blueprint5 in GameObjectFactory.Factory.BlueprintList)
						{
							_ = blueprint5.Name == "BaseReptile";
							string text23 = blueprint5.GetTag("Class").ToLower();
							string text24 = blueprint5.GetTag("Species").ToLower();
							if (!string.IsNullOrEmpty(text23) && !hashSet.Contains(text23))
							{
								text22 = text22 + ", " + text23;
								hashSet.Add(text23);
							}
							if (!string.IsNullOrEmpty(text24) && !hashSet.Contains(text24))
							{
								text22 = text22 + ", " + text24;
								hashSet.Add(text24);
							}
						}
						UnityEngine.Debug.Log(text22);
						return;
					}
					if (Wish == "findimportant")
					{
						foreach (GameObject item25 in The.ZoneManager.ActiveZone.FindObjects((GameObject o) => o.IsImportant()).ToList())
						{
							MessageQueue.AddPlayerMessage(item25.DisplayName + " at " + item25.CurrentCell.ToString());
						}
						{
							foreach (GameObject item26 in The.ZoneManager.ActiveZone.FindObjects((GameObject o) => o.HasObjectInInventory((GameObject gameObject23) => gameObject23.IsImportant())))
							{
								item26.ForeachInventoryAndEquipment((GameObject gameObject23) =>
								{
									if (gameObject23.IsImportant())
									{
										MessageQueue.AddPlayerMessage(gameObject23.DisplayName + " at " + gameObject23.CurrentCell.ToString());
									}
								});
							}
							return;
						}
					}
					if (Wish == "fetchimportant")
					{
						List<GameObject> gos = new List<GameObject>();
						foreach (GameObject item27 in The.ZoneManager.ActiveZone.FindObjects((GameObject o) => o.IsImportant()).ToList())
						{
							gos.Add(item27);
						}
						foreach (GameObject item28 in The.ZoneManager.ActiveZone.FindObjects((GameObject o) => o.HasObjectInInventory((GameObject gameObject23) => gameObject23.IsImportant())))
						{
							item28.ForeachInventoryAndEquipment((GameObject gameObject23) =>
							{
								if (gameObject23.IsImportant())
								{
									gos.Add(gameObject23);
								}
							});
						}
						gos.ForEach((GameObject go) =>
						{
							go.DirectMoveTo(The.Player.CurrentCell);
						});
						return;
					}
					if (Wish == "sfxtest")
					{
						SoundManager.PlaySound("Sounds/Missile/Fires/Rifles/sfx_missile_laserRifle_fire", 0f, 1f, 1f, SoundRequest.SoundEffectType.FullPanRightToLeft);
						return;
					}
					if (Wish.StartsWith("disablelayer:"))
					{
						ControlManager.DisableLayer(Wish.Split(':')[1]);
						return;
					}
					if (Wish.StartsWith("enablelayer:"))
					{
						ControlManager.EnableLayer(Wish.Split(':')[1]);
						return;
					}
					if (Wish == "testpax")
					{
						PaxInfectLimb.Infect(The.Player);
						SpreadPax.StartQuest();
						return;
					}
					if (Wish == "everycachedquestitem")
					{
						foreach (KeyValuePair<string, GameObject> cachedObject in The.Game.ZoneManager.CachedObjects)
						{
							GameObject value5 = cachedObject.Value;
							if (value5.IsImportant())
							{
								The.Player.ReceiveObject(value5);
							}
						}
						return;
					}
					if (Wish.StartsWith("rapid:"))
					{
						Leveler.RapidAdvancement(Convert.ToInt32(Wish.Split(':')[1]), The.Player);
						return;
					}
					if (Wish == "unexplore")
					{
						The.Player.CurrentZone.UnexploreAll();
						The.Player.CurrentZone.GetObjects().ForEach((GameObject o) =>
						{
							o.RemoveProperty("Autoexplored");
						});
						return;
					}
					if (Wish.StartsWith("deathpopuptest:"))
					{
						string[] array17 = Wish.Split(":");
						Popup.WaitNewPopupMessage(array17[1], null, null, null, null, null, 0, afterRender: new Renderable(null, array17[2], "f", "&W", array17[3], array17[4][0]), contextTitle: array17[5], contextRender: null, showContextFrame: false);
						return;
					}
					if (Wish == "deathpopuptest")
					{
						Popup.WaitNewPopupMessage("You were immolated by a dawnglider.", null, null, null, null, null, 0, "You died.", null, new Renderable(null, "Mutations/flaming_ray.bmp", "f", "&W", "&W", 'R'), showContextFrame: false);
						return;
					}
					if (Wish.StartsWith("deathtestcategory:"))
					{
						foreach (GameObject object2 in The.Player.CurrentZone.GetObjects())
						{
							if (object2.IsCombatObject() && !object2.IsPlayer())
							{
								object2.Die(null, null, null, null, Accidental: false, null, null, Force: false, AlwaysUsePopups: false, null, null, Wish.Split(':')[1]);
							}
						}
						return;
					}
					if (Wish == "deathtest:decapitated")
					{
						foreach (GameObject object3 in The.Player.CurrentZone.GetObjects())
						{
							if (object3.IsCombatObject() && !object3.IsPlayer())
							{
								string tile = object3.Render.Tile;
								string tileForegroundColor = object3.Render.GetTileForegroundColor();
								char detailColor = object3.Render.getDetailColor();
								Cell currentCell5 = object3.CurrentCell;
								object3.Die();
								CombatJuice.playPrefabAnimation(currentCell5.Location, "Deaths/DeathVFXDecapitated", null, tile + ";" + tileForegroundColor + ";" + detailColor);
							}
						}
						return;
					}
					if (Wish == "deathtestlased")
					{
						Popup.WaitNewPopupMessage("You were lased to death by a crypt sitter.", null, null, null, null, null, 0, "You died.", null, new Renderable(null, "Mutations/light_manipulation.bmp", "f", "&R", "&R", 'Y'), showContextFrame: false);
						return;
					}
					if (Wish == "deathtest:gem")
					{
						List<GameObject> list17 = (from o in The.Player.CurrentZone.GetObjects()
							where o.IsCombatObject() && !o.IsPlayer()
							select o).ToList();
						list17.Sort((GameObject a, GameObject b) => a.DistanceTo(The.Player) - b.DistanceTo(The.Player));
						using List<GameObject>.Enumerator enumerator3 = list17.GetEnumerator();
						if (enumerator3.MoveNext())
						{
							GameObject current51 = enumerator3.Current;
							string tile2 = current51.Render.Tile;
							string tileForegroundColor2 = current51.Render.GetTileForegroundColor();
							char detailColor2 = current51.Render.getDetailColor();
							Cell currentCell6 = current51.CurrentCell;
							current51.Die();
							GameObject gameObject16 = GameObject.Create(PopulationManager.RollOneFrom("Gemstones").Blueprint);
							currentCell6.AddObject(gameObject16);
							CombatJuice.playPrefabAnimation(gameObject16, "Deaths/DeathVFXTransmuted", gameObject16.ID, tile2 + ";" + tileForegroundColor2 + ";" + detailColor2 + ";" + gameObject16.Render.Tile + ";" + gameObject16.Render.GetTileForegroundColor() + ";" + gameObject16.Render.getDetailColor());
						}
						return;
					}
					if (Wish == "ignoreme")
					{
						The.Core.IgnoreMe = !The.Core.IgnoreMe;
						MessageQueue.AddPlayerMessage("ignore now " + The.Core.IgnoreMe);
						return;
					}
					if (Wish.StartsWith("pan:"))
					{
						GameManager.Instance.uiQueue.queueTask(() =>
						{
							GameManager.Instance.cinematicPan(Wish.Split(':')[1]);
						});
						return;
					}
					if (Wish.StartsWith("gosecret:"))
					{
						string secretID = Wish.Split(':')[1];
						string zoneID9 = ((from baseJournalEntry in JournalAPI.GetAllNotes()
							where baseJournalEntry.ID == secretID
							select baseJournalEntry).First() as JournalMapNote).ZoneID;
						The.Player.SystemLongDistanceMoveTo(The.ZoneManager.GetZone(zoneID9).GetCell(40, 24));
						return;
					}
					if (Wish == "go:shuglair")
					{
						string zoneID10 = ((from baseJournalEntry in JournalAPI.GetAllNotes()
							where baseJournalEntry.ID == "$shugruithlair"
							select baseJournalEntry).First() as JournalMapNote).ZoneID;
						The.Player.SystemLongDistanceMoveTo(The.ZoneManager.GetZone(zoneID10).GetCell(40, 24));
						return;
					}
					if (Wish == "go:shug")
					{
						string zoneID11 = ((from baseJournalEntry in JournalAPI.GetAllNotes()
							where baseJournalEntry.ID == "$shugruithmouth"
							select baseJournalEntry).First() as JournalMapNote).ZoneID;
						The.Player.SystemLongDistanceMoveTo(The.ZoneManager.GetZone(zoneID11).GetCell(40, 24));
						return;
					}
					if (Wish == "go:agolgot")
					{
						string zoneID12 = JournalAPI.MapNotes.Find((JournalMapNote journalMapNote) => journalMapNote.ID == "$agolgotlair").ZoneID;
						The.Player.SystemLongDistanceMoveTo(The.ZoneManager.GetZone(zoneID12).GetCell(40, 24));
						return;
					}
					if (Wish == "go:bethsaida")
					{
						string zoneID13 = JournalAPI.MapNotes.Find((JournalMapNote journalMapNote) => journalMapNote.ID == "$bethsaidalair").ZoneID;
						The.Player.SystemLongDistanceMoveTo(The.ZoneManager.GetZone(zoneID13).GetCell(40, 24));
						return;
					}
					if (Wish == "go:rermadon")
					{
						string zoneID14 = JournalAPI.MapNotes.Find((JournalMapNote journalMapNote) => journalMapNote.ID == "$rermadonlair").ZoneID;
						The.Player.SystemLongDistanceMoveTo(The.ZoneManager.GetZone(zoneID14).GetCell(40, 24));
						return;
					}
					if (Wish == "go:qas" || Wish == "go:qon" || Wish == "go:qasqon")
					{
						string zoneID15 = JournalAPI.MapNotes.Find((JournalMapNote journalMapNote) => journalMapNote.ID == "$qasqonlair").ZoneID;
						The.Player.SystemLongDistanceMoveTo(The.ZoneManager.GetZone(zoneID15).GetCell(40, 24));
						return;
					}
					if (Wish == "techscan")
					{
						The.Player.ModIntProperty("TechScannerEquipped", 1);
						return;
					}
					if (Wish == "bioscan")
					{
						The.Player.ModIntProperty("BioScannerEquipped", 1);
						return;
					}
					if (Wish == "structurescan")
					{
						The.Player.ModIntProperty("StructureScannerEquipped", 1);
						return;
					}
					if (Wish == "assassins")
					{
						PsychicHunterSystem.CreateExtradimensionalSoloHunters(The.Player.CurrentZone, 24);
						return;
					}
					if (Wish == "testacc")
					{
						JournalAPI.AddAccomplishment("Test ACC", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
						return;
					}
					if (Wish == "unloadunusedassets")
					{
						MessageQueue.AddPlayerMessage("Unloading unused assets...");
						GameManager.Instance.uiQueue.queueTask(() =>
						{
							SoundManager.ClearCache();
							Resources.UnloadUnusedAssets();
						});
						return;
					}
					if (Wish == "sbtest")
					{
						MessageQueue.AddPlayerMessage("Starting memtest");
						for (int num50 = 0; num50 < 100000; num50++)
						{
							StringBuilder stringBuilder18 = new StringBuilder();
							for (int num51 = 0; num51 < 1000; num51++)
							{
								stringBuilder18.Append("?");
							}
							stringBuilder18.ToString();
							memtest.Add(stringBuilder18);
						}
						MessageQueue.AddPlayerMessage("Ending memtest");
						return;
					}
					if (Wish == "tut:joppa")
					{
						TutorialManager.AdvanceStep(new ExploreJoppa());
						The.Player.SystemMoveTo(The.ZoneManager.GetZone("JoppaWorld.11.22.1.1.10").GetCell(37, 22));
						return;
					}
					if (Wish == "tut:bear")
					{
						TutorialManager.AdvanceStep(new FightBear());
						The.Player.CurrentCell.GetCellFromDirection("NE").GetCellFromDirection("NE").AddObject("TutorialBear");
						return;
					}
					if (Wish == "tut:clear")
					{
						TutorialManager.AdvanceStep(null);
						return;
					}
					if (Wish == "tut:camp")
					{
						TutorialManager.AdvanceStep(null);
						The.Player.SystemMoveTo(The.ZoneManager.GetZone("JoppaWorld.11.24.1.0.10").GetCell(48, 11));
						TutorialManager.AdvanceStep(new MakeCamp());
						return;
					}
					if (Wish == "tut:worldmap")
					{
						TutorialManager.AdvanceStep(new ExploreWorldMap());
						The.Player.SystemMoveTo(The.ZoneManager.GetZone("JoppaWorld").GetCell(11, 24));
						return;
					}
					if (Wish == "tut:books")
					{
						TutorialManager.AdvanceStep(null);
						The.Player.SystemMoveTo(The.ZoneManager.GetZone("JoppaWorld.11.24.1.0.11").GetCell(38, 17));
						TutorialManager.AdvanceStep(new GetBooks());
						return;
					}
					if (Wish == "clearcooldowns")
					{
						The.Player.GetPart<ActivatedAbilities>().ClearCooldowns();
						return;
					}
					if (Wish == "forcethawcurrent")
					{
						Location2D location = The.PlayerCell.Location;
						The.Player.SystemMoveTo(null);
						ZoneManager.instance.ForceTryThawZone(The.Player.CurrentCell.ParentZone.ZoneID, out var Zone);
						Zone.GetCell(location).AddObject(The.Player);
						Zone.SetActive();
						return;
					}
					if (Wish == "thwomp")
					{
						GameManager.Instance.ThinWorldDistorting = GameManager.Instance.MAX_THIN_WORLD_DISTORTING;
						return;
					}
					if (Wish == "renderdelay")
					{
						The.Core.RenderDelay(5000);
						return;
					}
					if (Wish.StartsWith("controllerfont:"))
					{
						Options.SetOption("OptionControllerFont", Wish.Split(':')[1]);
						return;
					}
					if (Wish == "rendertest")
					{
						GameManager.Instance.uiQueue.queueTask(() =>
						{
							The.Core.RenderBase();
						});
						return;
					}
					if (Wish == "idkfa" || Wish == "godmode")
					{
						The.Core.IDKFA = !The.Core.IDKFA;
						MessageQueue.AddPlayerMessage("Godmode now " + The.Core.IDKFA);
						return;
					}
					if (Wish == "allfire")
					{
						The.Core.IDKFA = !The.Core.IDKFA;
						The.Player.CurrentZone.ForeachObject((GameObject o) =>
						{
							if (o.Physics.IsReal)
							{
								o.Physics.Temperature = o.Physics.FlameTemperature + 100;
							}
						});
						return;
					}
					if (Wish == "navweight")
					{
						Popup.Show(The.Player.CurrentCell.GetNavigationWeightFor(The.Player).ToString());
						return;
					}
					if (Wish == "clearnavcache")
					{
						The.ZoneManager.ActiveZone.FlushNavigationCaches();
						return;
					}
					if (Wish == "eviltwin")
					{
						EvilTwin.CreateEvilTwin(The.Player, EvilTwin.DEFECT_PREFIX, null, Description: EvilTwin.DEFECT_DESCRIPTION, Message: EvilTwin.DEFECT_MESSAGE);
						return;
					}
					if (Wish == "roadtest")
					{
						Zone currentZone5 = The.Player.CurrentZone;
						currentZone5.AddZoneConnection("-", 0, 14, "Road", null);
						currentZone5.AddZoneConnection("-", 79, 14, "Road", null);
						new RoadBuilder().BuildZone(currentZone5);
						return;
					}
					if (Wish == "rivertest")
					{
						Zone currentZone6 = The.Player.CurrentZone;
						currentZone6.AddZoneConnection("-", 40, 0, "RiverMouth", null);
						currentZone6.AddZoneConnection("-", 40, 24, "RiverMouth", null);
						new RiverBuilder().BuildZone(currentZone6);
						return;
					}
					if (Wish == "testendmessage")
					{
						PaxKlanqIPresumeSystem.UnderConstructionMessage();
						return;
					}
					if (Wish == "crossintobright")
					{
						ThinWorld.CrossIntoBrightsheol();
						return;
					}
					if (Wish == "eventtest")
					{
						Stopwatch stopwatch3 = new Stopwatch();
						stopwatch3.Reset();
						stopwatch3.Start();
						EndTurnEvent e = new EndTurnEvent();
						for (int num52 = 0; num52 < 100000; num52++)
						{
							The.Player.HandleEvent(e);
						}
						UnityEngine.Debug.Log("1m events in " + stopwatch3.Elapsed);
						return;
					}
					if (Wish == "shove")
					{
						string direction = PickDirection.ShowPicker("Shove");
						The.Player.GetCurrentCell().GetCellFromDirection(direction).GetObjectsWithPart("Physics")[0].Move(direction, Forced: true);
						return;
					}
					if (Wish == "makestranger")
					{
						string direction2 = PickDirection.ShowPicker("Make Stranger");
						GameObject combatTarget = The.Player.GetCurrentCell().GetCellFromDirection(direction2).GetCombatTarget(The.Player);
						if (combatTarget?.Brain != null)
						{
							combatTarget.Brain.Factions = "Strangers-100";
						}
						return;
					}
					if (Wish == "weather")
					{
						Zone parentZone4 = The.Player.CurrentCell.ParentZone;
						StringBuilder stringBuilder19 = Event.NewStringBuilder();
						stringBuilder19.Append("HasWeather: ").Append(parentZone4.HasWeather).Append('\n')
							.Append("WindSpeed: ")
							.Append(parentZone4.WindSpeed)
							.Append('\n')
							.Append("WindDirections: ")
							.Append(parentZone4.WindDirections)
							.Append('\n')
							.Append("WindDuration: ")
							.Append(parentZone4.WindDuration)
							.Append('\n')
							.Append("CurrentWindSpeed: ")
							.Append(parentZone4.CurrentWindSpeed)
							.Append('\n')
							.Append("CurrentWindDirection: ")
							.Append(parentZone4.CurrentWindDirection)
							.Append('\n')
							.Append("NextWindChange: +")
							.Append(parentZone4.NextWindChange - The.Game.TimeTicks)
							.Append('\n');
						Popup.Show(stringBuilder19.ToString());
						return;
					}
					if (Wish == "fungalvision")
					{
						FungalVisionary.VisionLevel = 1;
						return;
					}
					if (Wish == "glitchtest")
					{
						LiquidWarmStatic.GlitchZone(The.PlayerCell.ParentZone);
						return;
					}
					if (Wish == "masterchef")
					{
						Popup.Suppress = true;
						who.ReceiveObject("Fermented Yuckwheat Stem", 10);
						who.ReceiveObject("Voider Gland Paste", 10);
						who.ReceiveObjectsFromPopulation("Ingredients_MidTiers", 20);
						List<string> list18 = new List<string>
						{
							"Phase Silk", "Starapple Preserves", "Cured Dawnglider Tail", "Pickled Mushrooms", "Fermented Yondercane", "Spark Tick Plasma", "Spine Fruit Jam", "Vinewafer Sheaf", "Goat Jerky", "Beetle Jerky",
							"Pickles", "Fire Ant Gaster Paste", "Voider Gland Paste", "Congealed Shade Oil", "CiderPool"
						};
						for (int num53 = 0; num53 < 6; num53++)
						{
							string randomElement2 = list18.GetRandomElement();
							string randomElement3;
							do
							{
								randomElement3 = list18.GetRandomElement();
							}
							while (randomElement3 == randomElement2);
							string randomElement4;
							do
							{
								randomElement4 = list18.GetRandomElement();
							}
							while (randomElement4 == randomElement2 || randomElement4 == randomElement3);
							CookingGameState.LearnRecipe(CookingRecipe.FromIngredients(new List<GameObject>
							{
								GameObjectFactory.Factory.CreateObject(randomElement2),
								GameObjectFactory.Factory.CreateObject(randomElement3),
								GameObjectFactory.Factory.CreateObject(randomElement4)
							}, null, who.DisplayNameOnlyStripped), The.Player);
						}
						for (int num54 = 0; num54 < 3; num54++)
						{
							string randomElement2 = list18.GetRandomElement();
							string randomElement3;
							do
							{
								randomElement3 = list18.GetRandomElement();
							}
							while (randomElement3 == randomElement2);
							string randomElement4;
							do
							{
								randomElement4 = list18.GetRandomElement();
							}
							while (randomElement4 == randomElement2 || randomElement4 == randomElement3);
							CookingGameState.LearnRecipe(CookingRecipe.FromIngredients(new List<string> { randomElement2, randomElement3 }, null, who.DisplayNameOnlyStripped), The.Player);
						}
						for (int num55 = 0; num55 < 1; num55++)
						{
							string randomElement2 = list18.GetRandomElement();
							string randomElement3;
							do
							{
								randomElement3 = list18.GetRandomElement();
							}
							while (randomElement3 == randomElement2);
							string randomElement4;
							do
							{
								randomElement4 = list18.GetRandomElement();
							}
							while (randomElement4 == randomElement2 || randomElement4 == randomElement3);
							CookingGameState.LearnRecipe(CookingRecipe.FromIngredients(new List<string> { randomElement2 }, null, who.DisplayNameOnlyStripped), The.Player);
						}
						CookingGameState.LearnRecipe(new HotandSpiny());
						AddSkill("CookingAndGathering");
						AddSkill("CookingAndGathering_Harvestry");
						AddSkill("CookingAndGathering_Butchery");
						AddSkill("CookingAndGathering_Spicer");
						AddSkill("CookingAndGathering_CarbideChef");
						MessageQueue.AddPlayerMessage("Added cooking knowledge.");
						Popup.Suppress = false;
						return;
					}
					if (Wish == "masterchef2")
					{
						Popup.Suppress = true;
						who.ReceiveObject("Fermented Yuckwheat Stem", 10);
						who.ReceiveObject("Voider Gland Paste", 10);
						List<string> list19 = new List<string> { "Starapple", "Bear Jerky", "FluxWaterskin_Ingredient" };
						who.ReceiveObject("Phase Silk", 100);
						who.ReceiveObject("Starapple", 100);
						who.ReceiveObject("Bear Jerky", 100);
						for (int num56 = 0; num56 < 6; num56++)
						{
							string randomElement5 = list19.GetRandomElement();
							string randomElement6;
							do
							{
								randomElement6 = list19.GetRandomElement();
							}
							while (randomElement6 == randomElement5);
							string randomElement7;
							do
							{
								randomElement7 = list19.GetRandomElement();
							}
							while (randomElement7 == randomElement5 || randomElement7 == randomElement6);
							CookingGameState.LearnRecipe(CookingRecipe.FromIngredients(new List<GameObject>
							{
								GameObjectFactory.Factory.CreateObject(randomElement5),
								GameObjectFactory.Factory.CreateObject(randomElement6),
								GameObjectFactory.Factory.CreateObject(randomElement7)
							}, null, who.DisplayNameOnlyStripped), The.Player);
						}
						for (int num57 = 0; num57 < 3; num57++)
						{
							string randomElement5 = list19.GetRandomElement();
							string randomElement6;
							do
							{
								randomElement6 = list19.GetRandomElement();
							}
							while (randomElement6 == randomElement5);
							string randomElement7;
							do
							{
								randomElement7 = list19.GetRandomElement();
							}
							while (randomElement7 == randomElement5 || randomElement7 == randomElement6);
							CookingGameState.LearnRecipe(CookingRecipe.FromIngredients(new List<string> { randomElement5, randomElement6 }, null, who.DisplayNameOnlyStripped), The.Player);
						}
						for (int num58 = 0; num58 < 1; num58++)
						{
							string randomElement5 = list19.GetRandomElement();
							string randomElement6;
							do
							{
								randomElement6 = list19.GetRandomElement();
							}
							while (randomElement6 == randomElement5);
							string randomElement7;
							do
							{
								randomElement7 = list19.GetRandomElement();
							}
							while (randomElement7 == randomElement5 || randomElement7 == randomElement6);
							CookingGameState.LearnRecipe(CookingRecipe.FromIngredients(new List<string> { randomElement5 }, null, who.DisplayNameOnlyStripped), The.Player);
						}
						CookingGameState.LearnRecipe(new HotandSpiny());
						AddSkill("CookingAndGathering");
						AddSkill("CookingAndGathering_Harvestry");
						AddSkill("CookingAndGathering_Butchery");
						AddSkill("CookingAndGathering_Spicer");
						AddSkill("CookingAndGathering_CarbideChef");
						MessageQueue.AddPlayerMessage("Added cooking knowledge.");
						Popup.Suppress = false;
						return;
					}
					if (Wish.StartsWith("findobj:"))
					{
						GameObject gameObject17 = GameObject.FindByID(Wish.Split(':')[1]);
						if (gameObject17 == null)
						{
							Popup.ShowFail("Not found.");
							return;
						}
						gameObject17.GetContext(out var ObjectContext, out var CellContext, out var BodyPartContext);
						if (ObjectContext != null)
						{
							if (BodyPartContext != null)
							{
								Popup.ShowFail(gameObject17.DebugName + " found in " + ObjectContext.DebugName + " body part " + BodyPartContext.Name);
							}
							else
							{
								Popup.ShowFail(gameObject17.DebugName + " found in " + ObjectContext.DebugName);
							}
						}
						else if (CellContext != null)
						{
							Popup.ShowFail(gameObject17.DebugName + " found in " + CellContext.ToString() + " of " + (CellContext.ParentZone?.ZoneID ?? "NULL"));
						}
						else
						{
							Popup.ShowFail(gameObject17.DebugName + " found with no context");
						}
						return;
					}
					if (Wish.StartsWith("gamestate:"))
					{
						string[] array18 = Wish.Split(':');
						if (array18.Length == 2)
						{
							MessageQueue.AddPlayerMessage(array18[1] + "=" + The.Game.GetStringGameState(array18[1]));
						}
						else if (array18[2] == "null")
						{
							The.Game.StringGameState.Set(array18[1], null);
						}
						else
						{
							The.Game.StringGameState.Set(array18[1], array18[2]);
						}
						return;
					}
					if (Wish == "cyber")
					{
						who.GetCurrentCell().GetCellFromDirection("N").AddObject("CyberneticsTerminal2");
						GameObject gameObject18 = who.GetCurrentCell().GetCellFromDirection("NW").AddObject("CyberneticsStationRack");
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("DermalPlating"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("DermalInsulation"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("BiologicalIndexer"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("TechnologicalIndexer"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("NightVision"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("HyperElasticAnkleTendons"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("ParabolicMuscularSubroutine"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("CherubicVisage"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("TranslucentSkin"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("StabilizerArmLocks"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("RapidReleaseFingerFlexors"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("CarbideHandBones"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("Pentaceps"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("MotorizedTreads"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("InflatableAxons"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("NocturnalApex"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("ElectromagneticSensor"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("MatterRecompositer"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("AirCurrentMicrosensor"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("CustomVisage"));
						gameObject18.Inventory.AddObject(GameObjectFactory.Factory.CreateObject("GunRack"));
						foreach (GameObject item29 in gameObject18.Inventory.GetObjectsDirect())
						{
							item29.MakeUnderstood();
						}
						who.ReceiveObject("CyberneticsCreditWedge", 12);
						return;
					}
					if (Wish == "filljournal")
					{
						for (int num59 = 0; num59 < 100; num59++)
						{
							JournalAPI.AddAccomplishment("Afkfasdf ajsd fa fs adkf as dfas dfk asdlf a f asdf a" + Guid.NewGuid().ToString(), null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
						}
						return;
					}
					if (Wish == "wavetilegen")
					{
						GameManager.Instance.uiQueue.queueTask(() =>
						{
							Texture2D texture2D = new Texture2D(80, 25, TextureFormat.ARGB32, mipChain: false)
							{
								filterMode = UnityEngine.FilterMode.Point
							};
							Zone currentZone7 = who.CurrentZone;
							for (int num68 = 0; num68 < currentZone7.Height; num68++)
							{
								for (int num69 = 0; num69 < currentZone7.Width; num69++)
								{
									Cell cell12 = currentZone7.GetCell(num69, num68);
									if (cell12.HasWall())
									{
										texture2D.SetPixel(num69, num68, new Color32(0, 0, 0, byte.MaxValue));
									}
									else if (cell12.HasObjectWithPart("Combat"))
									{
										texture2D.SetPixel(num69, num68, new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
									}
									else if (cell12.HasObjectWithPart("LiquidVolume"))
									{
										texture2D.SetPixel(num69, num68, new Color32(0, 0, byte.MaxValue, byte.MaxValue));
									}
									else if (cell12.HasObjectWithPart("PlantProperties"))
									{
										texture2D.SetPixel(num69, num68, new Color32(0, byte.MaxValue, 0, byte.MaxValue));
									}
									else if (cell12.HasObjectWithPart("Door"))
									{
										texture2D.SetPixel(num69, num68, new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
									}
									else if (cell12.HasObjectWithPart("Furniture"))
									{
										texture2D.SetPixel(num69, num68, new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue));
									}
									else
									{
										texture2D.SetPixel(num69, num68, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
									}
								}
							}
							File.WriteAllBytes(DataManager.SavePath("tilegen.png"), texture2D.EncodeToPNG());
						});
						return;
					}
					if (Wish == "slow")
					{
						who.Statistics["Speed"].BaseValue = 25;
						return;
					}
					if (Wish == "fast")
					{
						who.Statistics["Speed"].BaseValue = 500;
						return;
					}
					if (Wish.StartsWith("testpop"))
					{
						string[] array19 = Wish.Split(' ');
						List<PopulationResult> list20 = ((array19.Length > 2) ? PopulationManager.Generate(array19[1], array19[2], array19[3]) : PopulationManager.Generate(array19[1]));
						MessageQueue.AddPlayerMessage("-- generating " + array19[1] + " ---");
						{
							foreach (PopulationResult item30 in list20)
							{
								MessageQueue.AddPlayerMessage(item30.Blueprint + " x" + item30.Number + ((!string.IsNullOrEmpty(item30.Hint)) ? (" hint:" + item30.Hint) : ""));
							}
							return;
						}
					}
					if (Wish == "maxmod")
					{
						The.Core.CheatMaxMod = !The.Core.CheatMaxMod;
					}
					else if (Wish == "night")
					{
						The.Game.TimeTicks += (10000 - Calendar.CurrentDaySegment) / 10;
					}
					else if (Wish == "day")
					{
						The.Game.TimeTicks += (3250 - Calendar.CurrentDaySegment) / 10;
					}
					else if (Wish == "restock")
					{
						The.ActiveZone.ForeachObject((GameObject gameObject23) =>
						{
							if (gameObject23.TryGetPart<GenericInventoryRestocker>(out var Part))
							{
								Part.PerformRestock();
								Part.LastRestockTick = The.Game.TimeTicks;
							}
						});
					}
					else if (Wish == "degeneratemarkup")
					{
						Popup.Show("Degenerate markup here: {{text}}");
					}
					else if (Wish == "daze")
					{
						The.Player.ApplyEffect(new Dazed(Stat.Random(2, 8)));
					}
					else if (Wish == "stun")
					{
						The.Player.ApplyEffect(new Stun(Stat.Random(2, 8), 30));
					}
					else if (Wish == "bleed")
					{
						game.Player.Body.ApplyEffect(new Bleeding());
					}
					else if (Wish.StartsWith("bleed:"))
					{
						string value6 = Wish.Split(':')[1];
						game.Player.Body.ApplyEffect(new Bleeding("1", Convert.ToInt32(value6)));
					}
					else if (Wish == "holobleed")
					{
						game.Player.Body.ApplyEffect(new HolographicBleeding());
					}
					else if (Wish.StartsWith("holobleed:"))
					{
						string value7 = Wish.Split(':')[1];
						game.Player.Body.ApplyEffect(new HolographicBleeding("1", Convert.ToInt32(value7)));
					}
					else if (Wish == "nosebleed")
					{
						game.Player.Body.ApplyEffect(new Nosebleed());
					}
					else if (Wish.StartsWith("nosebleed:"))
					{
						string value8 = Wish.Split(':')[1];
						game.Player.Body.ApplyEffect(new Nosebleed("1", Convert.ToInt32(value8)));
					}
					else if (Wish == "poison")
					{
						game.Player.Body.ApplyEffect(new Poisoned(30, "1d4", 1));
					}
					else if (Wish == "ill")
					{
						game.Player.Body.ApplyEffect(new Ill(30));
					}
					else if (Wish == "dude")
					{
						GameObject oneCreatureFromZone = The.ZoneManager.GetOneCreatureFromZone(The.ZoneManager.ActiveZone.ZoneID);
						if (oneCreatureFromZone != null)
						{
							Popup.Show(oneCreatureFromZone.DisplayName);
						}
					}
					else if (Wish.StartsWith("effect:"))
					{
						string text25 = Wish.Split(':')[1];
						who.ApplyEffect(Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text25)) as Effect);
					}
					else if (Wish == "lost")
					{
						who.ApplyEffect(new Lost());
					}
					else if (Wish == "notlost")
					{
						who.RemoveEffect<Lost>();
					}
					else if (Wish == "clone")
					{
						string text26 = PickDirection.ShowPicker("Clone");
						if (text26 != null)
						{
							Cell cellFromDirection = who.Physics.CurrentCell.GetCellFromDirection(text26);
							if (cellFromDirection != null)
							{
								GameObject gameObject19 = who.DeepCopy();
								cellFromDirection.AddObject(gameObject19);
							}
						}
					}
					else if (Wish == "copy")
					{
						string text27 = PickDirection.ShowPicker("Copy");
						if (text27 != null)
						{
							Cell cellFromDirection2 = who.Physics.CurrentCell.GetCellFromDirection(text27);
							if (cellFromDirection2 != null)
							{
								GameObject combatTarget2 = cellFromDirection2.GetCombatTarget();
								combatTarget2.Physics.CurrentCell.GetEmptyAdjacentCells().GetRandomElement().AddObject(combatTarget2.DeepCopy());
							}
						}
					}
					else if (Wish == "RandomNoReceiveObject")
					{
						GameObject anObject = EncountersAPI.GetAnObject((GameObjectBlueprint o) => !o.HasTag("Item"));
						who.GetCurrentCell().GetFirstEmptyAdjacentCell().AddObject(anObject);
					}
					else if (Wish == "expand")
					{
						Popup.Show(HistoricStringExpander.ExpandString("<spice.quests.questContext.itemNameMutation.!random>"));
					}
					else if (Wish == "explodingpalm")
					{
						string text28 = PickDirection.ShowPicker("Exploding Palm");
						if (text28 == null)
						{
							return;
						}
						Cell cellFromDirection3 = who.Physics.CurrentCell.GetCellFromDirection(text28);
						if (cellFromDirection3 != null)
						{
							GameObject firstObjectWithPart = cellFromDirection3.GetFirstObjectWithPart("Combat");
							for (int num60 = 0; num60 < 6; num60++)
							{
								Axe_Dismember.Dismember(who, firstObjectWithPart);
							}
							Axe_Decapitate.Decapitate(who, firstObjectWithPart);
						}
					}
					else
					{
						if (Wish == "yoink")
						{
							string text29 = PickDirection.ShowPicker("Swap");
							if (text29 == null)
							{
								return;
							}
							XRL.World.Parts.Physics physics2 = who.Physics;
							Cell cellFromDirection4 = physics2.CurrentCell.GetCellFromDirection(text29);
							if (cellFromDirection4 == null)
							{
								return;
							}
							{
								foreach (GameObject object4 in cellFromDirection4.Objects)
								{
									object4.SystemMoveTo(physics2.currentCell);
								}
								return;
							}
						}
						if (Wish == "swap")
						{
							string text30 = PickDirection.ShowPicker("Swap");
							if (text30 == null)
							{
								return;
							}
							Cell cellFromDirection5 = who.Physics.CurrentCell.GetCellFromDirection(text30);
							if (cellFromDirection5 != null)
							{
								List<GameObject> objectsWithPart = cellFromDirection5.GetObjectsWithPart("Combat");
								if (objectsWithPart.Count > 0)
								{
									game.Player.Body = objectsWithPart[0];
								}
							}
						}
						else if (Wish == "svardymstorm")
						{
							The.Game.GetSystem<SvardymSystem>().BeginStorm();
						}
						else if (Wish == "dismiss")
						{
							string text31 = PickDirection.ShowPicker("Dismiss");
							if (text31 == null)
							{
								return;
							}
							Cell cellFromDirection6 = who.CurrentCell.GetCellFromDirection(text31);
							if (cellFromDirection6 != null)
							{
								GameObject firstObjectWithPart2 = cellFromDirection6.GetFirstObjectWithPart("Brain", (GameObject o) => o != who);
								if (firstObjectWithPart2 != null)
								{
									firstObjectWithPart2.Brain.Goals.Clear();
									firstObjectWithPart2.SetAlliedLeader<AllyWish>(null);
									MessageQueue.AddPlayerMessage(firstObjectWithPart2.Does("are") + " no longer your follower.");
								}
							}
						}
						else if (Wish == "beguile")
						{
							string text32 = PickDirection.ShowPicker("Beguile");
							if (text32 == null)
							{
								return;
							}
							Cell cellFromDirection7 = who.CurrentCell.GetCellFromDirection(text32);
							if (cellFromDirection7 != null)
							{
								GameObject firstObjectWithPart3 = cellFromDirection7.GetFirstObjectWithPart("Brain", (GameObject o) => o != who);
								if (firstObjectWithPart3 != null)
								{
									firstObjectWithPart3.Brain.Goals.Clear();
									firstObjectWithPart3.SetAlliedLeader<AllyWish>(who);
									MessageQueue.AddPlayerMessage(firstObjectWithPart3.Does("are") + " now your follower.");
								}
							}
						}
						else if (Wish == "blueprint")
						{
							string text33 = PickDirection.ShowPicker("Blueprint");
							if (text33 == null)
							{
								return;
							}
							Cell cellFromDirection8 = The.Player.CurrentCell.GetCellFromDirection(text33);
							if (cellFromDirection8 != null)
							{
								GameObject firstObject = cellFromDirection8.GetFirstObject();
								if (firstObject != null)
								{
									Popup.Show(firstObject.Blueprint);
								}
							}
						}
						else if (Wish == "supermutant")
						{
							Type[] types = Assembly.GetExecutingAssembly().GetTypes();
							foreach (Type type in types)
							{
								if (!type.FullName.Contains("Parts.Mutation"))
								{
									continue;
								}
								Mutations part5 = who.GetPart<Mutations>();
								try
								{
									if (Activator.CreateInstance(type) is BaseMutation baseMutation && baseMutation.Name != "FearAura" && baseMutation.GetMutationEntry() != null)
									{
										part5.AddMutation(baseMutation);
									}
								}
								catch
								{
								}
							}
						}
						else if (Wish == "decapitateme")
						{
							Axe_Decapitate.Decapitate(who, who);
						}
						else if (Wish == "optionspop")
						{
							Popup.PickOption("Title only - short", null, "", "Sounds/UI/ui_notification", new string[3] { "Option 1", "Option 2", "Option 3" }, null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
							Popup.PickOption("Title - short - keymap", null, "", "Sounds/UI/ui_notification", new string[3] { "Option 1", "Option 2", "Option 3" }, new char[3] { 'a', 'b', 'c' }, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
							Popup.PickOption("Title - newlines - no respect", null, "", "Sounds/UI/ui_notification", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
							Popup.PickOption("Title - newlines - no respect - keymap", null, "", "Sounds/UI/ui_notification", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, new char[3] { 'a', 'b', 'c' }, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
							Popup.PickOption("Title - intro - newlines - no respect", "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", "", "Sounds/UI/ui_notification", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
							Popup.PickOption("Title - intro - newlines - no respect - spacing", "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", "", "Sounds/UI/ui_notification", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, null, null, null, null, null, null, 1, 60, 0, -1, AllowEscape: true);
							Popup.PickOption("Title - intro - newlines - respect - spacing", "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", "", "Sounds/UI/ui_notification", new string[3] { "Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines" }, null, null, null, null, null, null, 1, 60, 0, -1, AllowEscape: true, RespectOptionNewlines: true);
							Popup.PickOption("Title - intro - newlines - respect - spacing - scrolling", "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", "", "Sounds/UI/ui_notification", new string[16]
							{
								"Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines",
								"Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines", "Option x:\nHas multiple lines"
							}, null, null, null, null, null, null, 1, 60, 0, -1, AllowEscape: true, RespectOptionNewlines: true);
							Popup.PickOption("Title - intro - newlines - respect - scrolling", "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", "", "Sounds/UI/ui_notification", new string[28]
							{
								"Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines",
								"Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines",
								"Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines"
							}, null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true, RespectOptionNewlines: true);
							Popup.PickOption("Title - intro - newlines - no respect - scrolling", "This is a multi line intro of 3 lines\nThe second line &KColors&r something\nbut probably doesnt bleed?", "", "Sounds/UI/ui_notification", new string[28]
							{
								"Option 1:\nHas multiple lines", "Option 2:\nHas multiple lines", "Option 3:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines",
								"Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines",
								"Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines", "Option y:\nHas multiple lines", "Option z:\nHas multiple lines", "Option g:\nHas multiple lines", "Option x:\nHas multiple lines"
							}, null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
						}
						else if (Wish.StartsWith("checkforpart:"))
						{
							string text34 = Wish.Split(':')[1];
							Popup.Show(who.HasPart(text34) ? ("Player has " + text34 + " part") : ("No " + text34 + " part on player"));
						}
						else if (Wish.StartsWith("item:"))
						{
							string[] array20 = Wish.Split(':');
							string result = WishSearcher.SearchForBlueprint(array20[1]).Result;
							int num62 = 1;
							if (array20.Length > 2)
							{
								num62 = Convert.ToInt32(array20[2]);
							}
							Cell cell7 = who.GetCurrentCell();
							if (!array20.Contains("here"))
							{
								cell7 = cell7.GetFirstEmptyAdjacentCell();
							}
							for (int num63 = 0; num63 < num62; num63++)
							{
								cell7.AddObject(GameObjectFactory.Factory.CreateObject(result, 0, 0, null, null, null, "Wish"));
							}
						}
						else if (Wish.StartsWith("understandpartial:"))
						{
							string[] array21 = Wish.Split(':');
							string result2 = WishSearcher.SearchForBlueprint(array21[1]).Result;
							GameObject gameObject20 = GameObjectFactory.Factory.CreateObject(result2, 0, 0, null, null, null, "Wish");
							gameObject20.SetIntProperty("PartiallyUnderstood", 1);
							Cell cell8 = who.GetCurrentCell();
							if (!array21.Contains("here"))
							{
								cell8 = cell8.GetFirstEmptyAdjacentCell();
							}
							cell8.AddObject(gameObject20);
						}
						else if (Wish.StartsWith("factionheirloom:"))
						{
							who.CurrentCell.AddObject(Factions.Get(Wish.Split(':')[1]).GetHeirloom());
						}
						else if (Wish.StartsWith("getboolgamestate:"))
						{
							string[] array22 = Wish.Split(':');
							bool booleanGameState = The.Game.GetBooleanGameState(array22[1]);
							Popup.Show(array22[1] + ": \"" + booleanGameState + "\"");
						}
						else if (Wish.StartsWith("setstringgamestate:"))
						{
							string[] array23 = Wish.Split(':');
							The.Game.SetStringGameState(array23[1], array23[2]);
						}
						else if (Wish.StartsWith("getstringgamestate:"))
						{
							string[] array24 = Wish.Split(':');
							string stringGameState = The.Game.GetStringGameState(array24[1]);
							if (stringGameState == null)
							{
								Popup.Show(array24[1] + ": null");
							}
							else
							{
								Popup.Show(array24[1] + ": \"" + stringGameState + "\"");
							}
						}
						else if (Wish.StartsWith("hasblueprintbeenseen:"))
						{
							string[] array25 = Wish.Split(':');
							bool flag4 = The.Game.HasBlueprintBeenSeen(array25[1]);
							Popup.Show(array25[1] + ": " + (flag4 ? "yes" : "no"));
						}
						else if (Wish.StartsWith("pluralize:"))
						{
							Popup.Show(Grammar.Pluralize(Wish.Split(':')[1]));
						}
						else if (Wish.StartsWith("a:"))
						{
							Popup.Show(Grammar.A(Wish.Split(':')[1]));
						}
						else if (Wish.StartsWith("opinion:"))
						{
							Popup.Show(Factions.Get(Wish.GetDelimitedSubstring(':', 1)).GetFeelingTowardsObject(The.Player).ToString());
						}
						else if (Wish.StartsWith("wordrel:"))
						{
							string Input = Wish.Split(':')[1];
							List<string> relatedWords = WordDataManager.GetRelatedWords(ref Input);
							if (relatedWords == null)
							{
								Popup.Show(Input + ": no results", null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: false, DimBackground: true, LogMessage: false);
							}
							else
							{
								Popup.Show(Input + ": " + string.Join(", ", relatedWords.ToArray()), null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: false, DimBackground: true, LogMessage: false);
							}
						}
						else if (Wish == "clearzone")
						{
							Zone.ObjectEnumerator enumerator29 = The.ActiveZone.IterateObjects().GetEnumerator();
							while (enumerator29.MoveNext())
							{
								GameObject current53 = enumerator29.Current;
								if (!current53.IsPlayer())
								{
									current53.Destroy();
								}
							}
						}
						else if (Wish == "highpools")
						{
							Popup.Show(MinEvent.GetTopPoolCountReport());
						}
						else if (Wish == "enablemarkup")
						{
							Markup.Enabled = true;
						}
						else if (Wish == "giganticme")
						{
							The.Player.IsGiganticCreature = true;
						}
						else if (Wish == "disablemarkup")
						{
							Markup.Enabled = false;
						}
						else
						{
							if (!(Wish != ""))
							{
								return;
							}
							if (Wish == "objdump")
							{
								TextWriter textWriter = new StreamWriter(DataManager.SavePath("ObjectDump.txt"));
								foreach (string key4 in GameObjectFactory.Factory.Blueprints.Keys)
								{
									string text35 = "?";
									GameObject gameObject21 = GameObjectFactory.Factory.CreateObject(key4, -200);
									if (gameObject21.HasPart("Description"))
									{
										text35 = gameObject21.GetPart<Description>()._Short;
									}
									gameObject21.MakeUnderstood();
									string displayNameOnly = gameObject21.DisplayNameOnly;
									textWriter.WriteLine(ConsoleLib.Console.ColorUtility.StripFormatting(displayNameOnly.Substring(0, displayNameOnly.Length - 2).Replace("[", "").Replace("]", "")) + "," + text35.Replace(',', ';'));
								}
								textWriter.Close();
								textWriter.Dispose();
							}
							WishResult wishResult2 = WishSearcher.SearchForWish(Wish);
							if (wishResult2.Type == WishResultType.Quest)
							{
								The.Game.StartQuest(wishResult2.Result);
							}
							else if (wishResult2.Type == WishResultType.Mutation)
							{
								Type type2 = ModManager.ResolveType(wishResult2.Result);
								who.GetPart<Mutations>().AddMutation((BaseMutation)Activator.CreateInstance(type2));
							}
							else if (wishResult2.Type == WishResultType.Blueprint)
							{
								bool flag5 = false;
								int num64 = 1;
								if (char.ToUpper(Wish[Wish.Length - 1]) == 'S')
								{
									num64 = Stat.RandomCosmetic(2, 4);
								}
								for (int num65 = 0; num65 < num64; num65++)
								{
									foreach (Cell adjacentCell in who.CurrentCell.GetAdjacentCells())
									{
										if (adjacentCell.IsEmpty())
										{
											GameObject gameObject22 = GameObjectFactory.Factory.CreateObject(wishResult2.Result, 0, 0, null, null, null, "Wish");
											adjacentCell.AddObject(gameObject22);
											gameObject22.MakeActive();
											flag5 = true;
											break;
										}
									}
								}
								if (!flag5)
								{
									Popup.Show("No adjacent empty squares to create your wish!");
								}
							}
							else
							{
								if (wishResult2.Type != WishResultType.Zone)
								{
									return;
								}
								Zone zone5 = The.ZoneManager.SetActiveZone(wishResult2.Result);
								Cell cell9 = null;
								for (int num66 = zone5.Height - 2; num66 >= 0; num66--)
								{
									for (int num67 = zone5.Width / 2; num67 >= 0; num67--)
									{
										Cell cell10 = zone5.GetCell(num67, num66);
										if (cell10.IsReachable() && cell10.IsEmptyOfSolid())
										{
											cell9 = cell10;
											break;
										}
										Cell cell11 = zone5.GetCell(40 - num67, num66);
										if (cell11.IsReachable() && cell11.IsEmptyOfSolid())
										{
											cell9 = cell11;
											break;
										}
									}
									if (cell9 != null)
									{
										break;
									}
								}
								who.SystemMoveTo(cell9);
								The.ZoneManager.ProcessGoToPartyLeader();
							}
						}
					}
				}
			}
		}
	}

	private static void AddSkill(string Class)
	{
		object obj = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Parts.Skill." + Class));
		The.Player.GetPart<XRL.World.Parts.Skills>().AddSkill(obj as BaseSkill);
	}
}
