using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class SpindleNegotiation : IPart
{
	public bool bQualified;

	public bool bArrived;

	public bool bNegotiated;

	public bool bChaosed;

	public bool bRemovedDelegates;

	public long TimeArrived;

	public List<string> DelegateFactions = new List<string>();

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (The.Game.HasFinishedQuestStep("The Earl of Omonporch", "Secure the Spindle") && !bRemovedDelegates)
		{
			Zone currentZone = ParentObject.CurrentZone;
			for (int i = 0; i < currentZone.Height; i++)
			{
				for (int j = 0; j < currentZone.Width; j++)
				{
					Cell cell = currentZone.GetCell(j, i);
					foreach (GameObject item in cell.GetObjectsWithPart("Brain"))
					{
						if (item.HasIntProperty("IsDelegate"))
						{
							cell.RemoveObject(item);
						}
					}
				}
			}
			bRemovedDelegates = true;
		}
		if (!bArrived && bQualified && Calendar.TotalTimeTicks - TimeArrived >= 3600)
		{
			bArrived = true;
			ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelArrived";
			XRLCore.Core.Game.SetIntGameState("DelegationOn", 1);
			AddDelegates();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginSpindleNegotiation");
		base.Register(Object, Registrar);
	}

	public List<string> GetTop4Factions()
	{
		List<string> list = new List<string>();
		int num = 0;
		List<string> factionNames = Factions.GetFactionNames();
		factionNames.Sort(new FactionRepComparerRandomSortEqual());
		int num2 = 0;
		while (num < 4)
		{
			if (Factions.Get(factionNames[num2]).Visible)
			{
				list.Add(factionNames[num2]);
				num++;
			}
			num2++;
		}
		return list;
	}

	public GameObject GetDelegateForFaction(string Faction)
	{
		GameObject gameObject = GameObject.Create("Delegate");
		gameObject.AddPart(new DelegateSpawner(Faction));
		return gameObject;
	}

	public void AddDelegates()
	{
		List<Cell> list = new List<Cell>();
		ParentObject.Physics.CurrentCell.GetConnectedSpawnLocations(4, list);
		if (list.Count < 4)
		{
			List<Cell> emptyCells = ParentObject.Physics.CurrentCell.ParentZone.GetEmptyCells();
			for (int i = 0; i < emptyCells.Count; i++)
			{
				if (list.Count >= 4)
				{
					break;
				}
				list.Add(emptyCells[i]);
			}
		}
		Algorithms.RandomShuffleInPlace(list, Stat.Rand);
		int num = 0;
		foreach (string top4Faction in GetTop4Factions())
		{
			GameObject delegateForFaction = GetDelegateForFaction(top4Faction);
			list[num].AddObject(delegateForFaction);
			num++;
			DelegateFactions.Add(top4Faction);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginSpindleNegotiation")
		{
			if (bChaosed)
			{
				Popup.Show("That was awful!");
				return true;
			}
			if (bNegotiated)
			{
				Popup.Show("Your friends may lease the Spindle, as we agreed.");
				return true;
			}
			if (bArrived)
			{
				string[] array = new string[5];
				List<string> list = ((DelegateFactions.Count >= 4) ? DelegateFactions : GetTop4Factions());
				List<string> list2 = new List<string>();
				for (int i = 0; i < list.Count; i++)
				{
					list2.Add(Faction.GetFormattedName(list[i]));
				}
				array[0] = "Share the burden across all allies. [-{{C|50}} reputation with each attending faction]";
				array[1] = "Share the burden between two allies. [-{{C|100}} reputation with two attending factions of your choice]";
				array[2] = "Spare one faction of all obligation by betraying a second faction and selling their secrets to Asphodel. [-{{C|800}} with the betrayed faction, +{{C|200}} reputation with the spared faction + a faction heirloom]";
				array[3] = "Invoke the Chaos Spiel. [????????, +{{C|300}} reputation with {{C|highly entropic beings}}]";
				array[4] = "Take time to weigh the options.";
				char[] hotkeys = new char[5] { 'a', 'b', 'c', 'd', 'e' };
				string text = "";
				for (int j = 0; j < list.Count; j++)
				{
					text = text + list[j] + ", ";
				}
				int num = Popup.PickOption("", "The First Council of Omonporch has begun. Choose how to appease Asphodel.", "", "Sounds/UI/ui_notification", array, hotkeys, null, null, null, null, null, 1, 75);
				if (num == 4 || num < 0)
				{
					return true;
				}
				switch (num)
				{
				case 0:
					Popup.Show("The pact is struck. The Barathrumites may lease control of the Spindle, and all the attending factions owe a debt to Asphodel.");
					foreach (string item in list)
					{
						The.Game.PlayerReputation.Modify(item, -50, "Quest");
					}
					XRLCore.Core.Game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
					bNegotiated = true;
					ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelDone";
					return true;
				case 1:
				{
					int num4 = Popup.PickOption("", "Choose a faction to share the burden. [-{{C|100}} reputation]", "", "Sounds/UI/ui_notification", list2.ToArray(), null, null, null, null, null, null, 1);
					string text7 = list[num4];
					if (num4 < 0)
					{
						return true;
					}
					list2.Remove(Faction.GetFormattedName(text7));
					list.Remove(text7);
					int num5 = Popup.PickOption("", "Choose a faction to share the burden. [-{{C|100}} reputation]", "", "Sounds/UI/ui_notification", list2.ToArray(), null, null, null, null, null, null, 1);
					if (num5 < 0)
					{
						return true;
					}
					string faction = list[num5];
					Popup.Show("The pact is struck. The Barathrumites may lease control of the Spindle, and the chosen factions owe a debt to Asphodel.");
					XRLCore.Core.Game.PlayerReputation.Modify(text7, -100, "Quest");
					The.Game.PlayerReputation.Modify(faction, -100, "Quest");
					XRLCore.Core.Game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
					bNegotiated = true;
					ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelDone";
					return true;
				}
				case 2:
				{
					int num2 = Popup.PickOption("", "Choose a faction to betray. [-{{C|800}} reputation]", "", "Sounds/UI/ui_notification", list2.ToArray(), null, null, null, null, null, null, 1);
					string text5 = list[num2];
					if (num2 < 0)
					{
						return true;
					}
					list2.Remove(Faction.GetFormattedName(text5));
					list.Remove(text5);
					int num3 = Popup.PickOption("", "Choose a faction to spare from obligation to Asphodel. [+{{C|200}} reputation and a faction heirloom]", "", "Sounds/UI/ui_notification", list2.ToArray(), null, null, null, null, null, null, 1);
					if (num3 < 0)
					{
						return true;
					}
					string text6 = list[num3];
					GameObject heirloom = Factions.Get(text6).GetHeirloom();
					Popup.Show("The pact is struck. The Barathrumites may lease control the Spindle.");
					Popup.Show("The delegate for " + Faction.GetFormattedName(text6) + " says, 'Live and drink, " + IComponent<GameObject>.ThePlayer.formalAddressTerm + ". We won't forget this.'");
					The.Game.PlayerReputation.Modify(text6, 200, "Quest");
					IComponent<GameObject>.ThePlayer.ReceiveObject(heirloom);
					Popup.Show("The delegate for " + Faction.GetFormattedName(text6) + " gives you " + heirloom.an() + "!");
					Popup.Show("The delegate for " + Faction.GetFormattedName(text5) + " says, 'Betrayer! May you choke on your own spittle! We won't forget this.'");
					The.Game.PlayerReputation.Modify(text5, -800, "Quest");
					XRLCore.Core.Game.FinishQuestStep("The Earl of Omonporch", "Secure the Spindle");
					bNegotiated = true;
					ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelDone";
					return true;
				}
				case 3:
				{
					Popup.Show("You ponder how best to sow chaos with your words.");
					The.Game.PlayerReputation.Modify("Entropic", GivesRep.VaryRep(300), "Quest");
					List<string> top4Factions = GetTop4Factions();
					top4Factions.Add("Flowers");
					top4Factions.Add("Consortium");
					string text2 = "";
					string text3 = "";
					bool flag = false;
					for (int k = 0; k < 3; k++)
					{
						while (!flag)
						{
							text2 = top4Factions.GetRandomElement();
							text3 = top4Factions.GetRandomElement();
							if (!text2.Equals(text3))
							{
								flag = true;
							}
						}
						flag = false;
						string text4 = ((text3 == "Entropic") ? GenerateFriendOrFoe_HEB.getHateReason() : GenerateFriendOrFoe.getHateReason());
						Popup.Show("You yell, 'I cannot believe {{C|" + Faction.GetFormattedName(text2) + "}} don't despise {{C|" + Faction.GetFormattedName(text3) + "}} for " + text4 + ".'");
						Popup.Show("Due to your revelation, " + Faction.GetFormattedName(text2) + " change their opinion of " + Faction.GetFormattedName(text3) + ".");
						Factions.Get(text2).SetFactionFeeling(text3, -100);
						The.Game.PlayerReputation.Modify(text2, GivesRep.VaryRep(200), "Quest");
						The.Game.PlayerReputation.Modify(text3, GivesRep.VaryRep(-200), "Quest");
					}
					bChaosed = true;
					ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelChaos";
					XRLCore.Core.Game.SetIntGameState("DelegationOn", 0);
					Popup.Show("Asphodel yells, '{{R|You ruined the First Council of Omonporch, you barbaric lout!}}'");
					ParentObject.Brain.AddOpinion<OpinionChaosSpiel>(IComponent<GameObject>.ThePlayer);
					ParentObject.Brain.FactionFeelings["Player"] = -100;
					foreach (GameObject item2 in ParentObject.CurrentZone.FindObjectsWithPart("Calming"))
					{
						item2.RemovePart<Calming>();
					}
					AchievementManager.SetAchievement("ACH_CHAOS_SPIEL");
					Achievement.CHAOS_SPIEL.Unlock();
					return true;
				}
				}
			}
			else
			{
				if (!HasEnoughFriends())
				{
					Popup.Show("You don't have enough allied factions. Come back when you're favored by {{C|4}} or more factions.");
					return true;
				}
				ParentObject.GetPart<ConversationScript>().ConversationID = "AsphodelWaiting";
				if (TimeArrived == 0L)
				{
					TimeArrived = Calendar.TotalTimeTicks;
				}
				int num6 = Math.Max(1, 3 - (int)Math.Ceiling((float)(Calendar.TotalTimeTicks - TimeArrived) / 1200f));
				Popup.Show("The council will be convened! Come back in " + num6 + " " + ((num6 == 1) ? "day" : "days") + ".");
				bQualified = true;
			}
		}
		return base.FireEvent(E);
	}

	public bool HasEnoughFriends()
	{
		List<string> top4Factions = GetTop4Factions();
		if (The.Game.PlayerReputation.Get(top4Factions[3]) < RuleSettings.REPUTATION_LIKED)
		{
			return false;
		}
		return true;
	}
}
