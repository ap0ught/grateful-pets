using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.UI;

public class SkillsAndPowersScreen : BaseScreen, IScreen
{
	public static List<SPNode> Nodes;

	public static bool HasAnyPower(GameObject GO, SkillEntry Skill)
	{
		foreach (PowerEntry value in Skill.Powers.Values)
		{
			if (GO.HasPart(value.Class))
			{
				return true;
			}
		}
		return false;
	}

	private static int SkillNodePos(SkillEntry Skill)
	{
		if (Nodes != null)
		{
			int i = 0;
			for (int count = Nodes.Count; i < count; i++)
			{
				if (Nodes[i].Skill == Skill)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private static int PowerNodePos(PowerEntry Power)
	{
		if (Nodes != null)
		{
			int i = 0;
			for (int count = Nodes.Count; i < count; i++)
			{
				if (Nodes[i].Power == Power)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private static SPNode OldSkillNode(List<SPNode> OldNodes, SkillEntry Skill)
	{
		if (OldNodes != null)
		{
			int i = 0;
			for (int count = OldNodes.Count; i < count; i++)
			{
				if (OldNodes[i].Skill == Skill)
				{
					return OldNodes[i];
				}
			}
		}
		return null;
	}

	private static SPNode OldPowerNode(List<SPNode> OldNodes, PowerEntry Power)
	{
		if (OldNodes != null)
		{
			int i = 0;
			for (int count = OldNodes.Count; i < count; i++)
			{
				if (OldNodes[i].Power == Power)
				{
					return OldNodes[i];
				}
			}
		}
		return null;
	}

	private static void AddSkillNodes(GameObject GO, SkillEntry Skill, List<SPNode> OldNodes, bool Expand)
	{
		SPNode sPNode = null;
		int num = SkillNodePos(Skill);
		XRLGame game = The.Game;
		if (game != null && game.HasBooleanGameState("UI_Expand_Skill_" + Skill.Name))
		{
			Expand = The.Game?.GetBooleanGameState("UI_Expand_Skill_" + Skill.Name) ?? Expand;
		}
		if (num == -1)
		{
			num = Nodes.Count;
			sPNode = OldSkillNode(OldNodes, Skill) ?? new SPNode(Skill, null, Expand, null);
			Nodes.Add(sPNode);
		}
		else
		{
			sPNode = Nodes[num];
		}
		int num2 = num;
		foreach (PowerEntry value in Skill.Powers.Values)
		{
			if (Skill.Hidden && !GO.HasSkill(value.Class))
			{
				continue;
			}
			int num3 = PowerNodePos(value);
			if (num3 == -1)
			{
				SPNode sPNode2 = OldPowerNode(OldNodes, value);
				SPNode item = sPNode2 ?? new SPNode(null, value, Expand: true, sPNode);
				if (num2 >= Nodes.Count - 1)
				{
					num3 = Nodes.Count;
					Nodes.Add(item);
				}
				else
				{
					num3 = num2 + 1;
					Nodes.Insert(num3, item);
				}
				if ((sPNode2 == null && !sPNode.Expand) & Expand)
				{
					sPNode.Expand = true;
				}
			}
			num2 = num3;
		}
	}

	public static void SelectNode(SPNode node, GameObject GO)
	{
		SkillEntry skillEntry = null;
		PowerEntry powerEntry = null;
		string name;
		string text;
		string text2;
		int cost;
		if (node.Power == null)
		{
			name = node.Skill.Name;
			text = node.Skill.Class;
			text2 = "skill";
			cost = node.Skill.Cost;
			skillEntry = node.Skill;
		}
		else if (!GO.HasSkill(node.ParentNode.Skill.Class))
		{
			if (Popup.ShowYesNoCancel("You do not have the skill associated with that power. Would you like to purchase the required skill?") != DialogResult.Yes)
			{
				return;
			}
			name = node.ParentNode.Skill.Name;
			text = node.ParentNode.Skill.Class;
			text2 = "skill";
			cost = node.ParentNode.Skill.Cost;
			skillEntry = node.ParentNode.Skill;
		}
		else
		{
			name = node.Power.Name;
			text = node.Power.Class;
			text2 = "power";
			cost = node.Power.Cost;
			powerEntry = node.Power;
		}
		if (GO.HasPart(text))
		{
			Popup.Show("You already have that " + text2 + ".");
		}
		else if ((powerEntry != null && powerEntry.IsSkillInitiatory) || (skillEntry != null && skillEntry.Initiatory))
		{
			Popup.Show("You must be initiated into this " + text2 + " in order to learn it.");
		}
		else if (GO.Stat("SP") < cost)
		{
			Popup.Show("You don't have enough skill points to buy that " + text2 + "!");
		}
		else
		{
			if ((powerEntry != null && !powerEntry.MeetsRequirements(GO, ShowPopup: true)) || (skillEntry != null && !skillEntry.MeetsRequirements(GO, ShowPopup: true)))
			{
				return;
			}
			string text3 = "XRL.World.Parts.Skill." + text;
			Type type = ModManager.ResolveType(text3);
			if (type == null)
			{
				Popup.Show("No implementation for " + text3);
				return;
			}
			if (text2 != "power")
			{
				foreach (PowerEntry value in skillEntry.Powers.Values)
				{
					if (value.Cost == 0 && !value.MeetsAttributeMinimum(GO, ShowPopup: true))
					{
						return;
					}
				}
			}
			if (Popup.ShowYesNo("Are you sure you want to buy " + name + " for {{C|" + cost + "}} sp?") == DialogResult.Yes)
			{
				BaseSkill newSkill = Activator.CreateInstance(type) as BaseSkill;
				GO.GetPart<Skills>().AddSkill(newSkill);
				GO.GetStat("SP").Penalty += cost;
				if (node.Skill != null)
				{
					node.Expand = true;
				}
				if (!string.IsNullOrEmpty(name))
				{
					MetricsManager.LogEvent("Gameplay:Skill:Purchase:" + name);
				}
			}
		}
	}

	public static void BuildNodes(GameObject GO, bool Rebuild = false)
	{
		List<SPNode> nodes = Nodes;
		if (Rebuild || Nodes == null)
		{
			Nodes = new List<SPNode>();
		}
		if (GO != null)
		{
			foreach (SkillEntry value in SkillFactory.Factory.SkillList.Values)
			{
				bool flag = GO.HasSkill(value.Class);
				if ((!value.Hidden || flag) && (flag || HasAnyPower(GO, value)))
				{
					AddSkillNodes(GO, value, nodes, Expand: true);
				}
			}
		}
		if (GO == null)
		{
			return;
		}
		foreach (SkillEntry value2 in SkillFactory.Factory.SkillList.Values)
		{
			bool flag2 = GO.HasSkill(value2.Class);
			if ((!value2.Hidden || flag2) && !flag2 && !HasAnyPower(GO, value2))
			{
				AddSkillNodes(GO, value2, nodes, Expand: false);
			}
		}
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("SkillsAndPowers");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		bool flag = false;
		Keys keys = Keys.None;
		int num = 0;
		int num2 = 0;
		BuildNodes(GO, Rebuild: true);
		int num3 = 3;
		foreach (SPNode node in Nodes)
		{
			string text = null;
			text = node.Description;
			if (text != null)
			{
				TextBlock textBlock = new TextBlock(text.Replace("\r\n", "\n"), 73, 12);
				if (textBlock.Lines.Count > num3)
				{
					num3 = textBlock.Lines.Count;
				}
			}
		}
		string text2 = "< {{W|7}} Tinkering | Character {{W|9}} >";
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			text2 = "< {{W|" + ControlManager.getCommandInputDescription("Page Left", mapGlyphs: false) + "}} Tinkering | Character {{W|" + ControlManager.getCommandInputDescription("Page Right", mapGlyphs: false) + "}} >";
		}
		while (!flag)
		{
			int num5;
			while (true)
			{
				Event.ResetPool();
				scrapBuffer.Clear();
				scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				scrapBuffer.Goto(13, 0);
				scrapBuffer.Write("[ {{W|Buy Skills}} - {{C|" + GO.Stat("SP") + "}}sp remaining ]");
				scrapBuffer.Goto(79 - ColorUtility.StripFormatting(text2).Length, 24);
				scrapBuffer.Write(text2);
				scrapBuffer.Goto(60, 0);
				scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Cancel", mapGlyphs: false) + "}} to exit ");
				if (num2 < num)
				{
					num = num2;
				}
				int num4 = 2;
				int i = num;
				num5 = 0;
				while (num4 <= 22 - num3 && i <= Nodes.Count)
				{
					for (; i < Nodes.Count && !Nodes[i].Visible; i++)
					{
					}
					if (i < Nodes.Count)
					{
						if (Nodes[i].Skill != null)
						{
							scrapBuffer.Goto(4, num4);
							if (Nodes[i].Expand)
							{
								scrapBuffer.Write("[-] ");
							}
							else
							{
								scrapBuffer.Write("[+] ");
							}
							bool flag2 = HasAnyPower(GO, Nodes[i].Skill);
							if (GO.HasPart(Nodes[i].Skill.Class))
							{
								scrapBuffer.Write("{{W|" + Nodes[i].Skill.Name + "}}");
							}
							else if (Nodes[i].Skill.Initiatory)
							{
								scrapBuffer.Write("{{w|" + Nodes[i].Skill.Name + "}}");
							}
							else if (flag2)
							{
								if (Nodes[i].Skill.Cost <= GO.Stat("SP"))
								{
									scrapBuffer.Write("[{{C|" + Nodes[i].Skill.Cost + "}}sp] {{W|" + Nodes[i].Skill.Name + "}}");
								}
								else
								{
									scrapBuffer.Write("[{{R|" + Nodes[i].Skill.Cost + "}}sp] {{W|" + Nodes[i].Skill.Name + "}}");
								}
							}
							else if (Nodes[i].Skill.Cost <= GO.Stat("SP"))
							{
								scrapBuffer.Write("[{{C|" + Nodes[i].Skill.Cost + "}}sp] {{w|" + Nodes[i].Skill.Name + "}}");
							}
							else
							{
								scrapBuffer.Write("[{{R|" + Nodes[i].Skill.Cost + "}}sp] {{w|" + Nodes[i].Skill.Name + "}}");
							}
						}
						else
						{
							scrapBuffer.Goto(4, num4);
							string text3 = "";
							PowerEntry power = Nodes[i].Power;
							if (GO.HasPart(power.Class))
							{
								scrapBuffer.Write(" - {{G|" + power.Name + "}}");
							}
							else
							{
								if (power.Requires != null)
								{
									foreach (string item in power.Requires.CachedCommaExpansion())
									{
										string text4 = item;
										bool flag3 = false;
										if (SkillFactory.Factory.TryGetFirstEntry(item, out var Entry))
										{
											if (power.IsSkillInitiatory)
											{
												int num6 = power.ParentSkill.PowerList.IndexOf(power);
												if (num6 > 0 && power.ParentSkill.PowerList[num6 - 1] == Entry)
												{
													continue;
												}
											}
											text4 = Entry.Name;
											flag3 = GO.HasSkill(item);
										}
										else if (MutationFactory.HasMutation(item))
										{
											text4 = MutationFactory.GetMutationEntryByName(item).Name;
											flag3 = GO.HasPart(item);
										}
										text3 = ((!flag3) ? (text3 + ", {{R|" + text4 + "}}") : (text3 + ", {{G|" + text4 + "}}"));
									}
								}
								if (power.Exclusion != null)
								{
									foreach (string item2 in power.Exclusion.CachedCommaExpansion())
									{
										string text5 = item2;
										bool flag4 = false;
										if (SkillFactory.Factory.TryGetFirstEntry(item2, out var Entry2))
										{
											text5 = Entry2.Name;
											flag4 = !GO.HasSkill(item2);
										}
										else if (MutationFactory.HasMutation(item2))
										{
											text5 = MutationFactory.GetMutationEntryByName(item2).Name;
											flag4 = !GO.HasPart(item2);
										}
										text3 = ((!flag4) ? (text3 + ", Ex: {{R|" + text5 + "}}") : (text3 + ", Ex: {{g|" + text5 + "}}"));
									}
								}
								scrapBuffer.Write(" - ");
								scrapBuffer.Write(power.Render(GO));
								scrapBuffer.Write(text3);
							}
						}
						num5 = i;
					}
					if (i == num2)
					{
						scrapBuffer.Goto(2, num4);
						scrapBuffer.Write("{{Y|>}}");
					}
					num4++;
					i++;
				}
				if (num2 <= num5 || num >= Nodes.Count)
				{
					break;
				}
				for (int j = num + 1; j < Nodes.Count; j++)
				{
					if (Nodes[j].Visible)
					{
						num = j;
						break;
					}
				}
			}
			int num7 = 0;
			int num8 = 0;
			for (int k = 0; k < Nodes.Count; k++)
			{
				if (Nodes[k].Visible)
				{
					num7++;
					num8 = k;
				}
			}
			scrapBuffer.Goto(2, 24);
			if (Nodes[num2].Power != null && GO.HasPart(Nodes[num2].Power.Class))
			{
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					scrapBuffer.Write(" [{{K|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}-Buy] ");
				}
				else
				{
					scrapBuffer.Write(" [{{W|8}}-Up {{W|2}}-Down {{W|4}}-Collapse {{W|6}}-Expand {{K|Space}}-Buy] ");
				}
			}
			else if (Nodes[num2].Skill != null && GO.HasPart(Nodes[num2].Skill.Class))
			{
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					scrapBuffer.Write(" [{{K|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}-Buy] ");
				}
				else
				{
					scrapBuffer.Write(" [{{W|8}}-Up {{W|2}}-Down {{W|4}}-Collapse {{W|6}}-Expand {{K|Space}}-Buy] ");
				}
			}
			else if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				scrapBuffer.Write(" [{{W|" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "}}-Buy] ");
			}
			else
			{
				scrapBuffer.Write(" [{{W|8}}-Up {{W|2}}-Down {{W|4}}-Collapse {{W|6}}-Expand {{W|Space}}-Buy] ");
			}
			string text6 = "";
			string description = Nodes[num2].Description;
			text6 = ((Nodes[num2].Skill == null) ? Nodes[num2].Power.Name : Nodes[num2].Skill.Name);
			if (num != 0)
			{
				scrapBuffer.Goto(4, 1);
				scrapBuffer.Write("{{W|<More...>}}");
			}
			if (num5 != num8)
			{
				scrapBuffer.Goto(4, 19);
				scrapBuffer.Write("{{W|<More...>}}");
			}
			TextBlock textBlock2 = new TextBlock(description.Replace("\r\n", "\n"), 73, 12);
			int num9 = 24 - num3;
			scrapBuffer.SingleBox(0, num9 - 1, 79, num9 - 1, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			scrapBuffer.Fill(1, num9, 78, 23, 32, ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
			scrapBuffer.Goto(4, num9 - 1);
			scrapBuffer.Goto(0, num9 - 1);
			scrapBuffer.Write(195);
			scrapBuffer.Goto(79, num9 - 1);
			scrapBuffer.Write(180);
			scrapBuffer.Goto(4, num9 - 1);
			scrapBuffer.Write("{{W|" + text6 + "}}");
			for (int l = 0; l < textBlock2.Lines.Count; l++)
			{
				scrapBuffer.Goto(2, num9 + l);
				scrapBuffer.Write(textBlock2.Lines[l]);
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			keys = Keyboard.getvk(MapDirectionToArrows: true);
			if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && Keyboard.RawCode != Keys.Prior && Keyboard.RawCode != Keys.Next))
			{
				flag = true;
			}
			if (keys == Keys.NumPad2)
			{
				for (int m = num2 + 1; m < Nodes.Count; m++)
				{
					if (Nodes[m].Visible)
					{
						num2 = m;
						break;
					}
				}
			}
			if (keys == Keys.NumPad8)
			{
				for (int num10 = num2 - 1; num10 >= 0; num10--)
				{
					if (Nodes[num10].Visible)
					{
						num2 = num10;
						break;
					}
				}
			}
			if (keys == Keys.Prior)
			{
				if (num2 == num)
				{
					int n = 0;
					for (int num11 = 21 - num3; n < num11; n++)
					{
						if (num2 <= 0)
						{
							break;
						}
						for (int num12 = num2 - 1; num12 >= 0; num12--)
						{
							if (Nodes[num12].Visible)
							{
								num2 = num12;
								break;
							}
						}
					}
				}
				else
				{
					num2 = num;
				}
			}
			if (keys == Keys.Next)
			{
				if (num2 == num5)
				{
					int num13 = 0;
					for (int num14 = 21 - num3; num13 < num14; num13++)
					{
						if (num2 >= Nodes.Count)
						{
							break;
						}
						for (int num15 = num2 + 1; num15 < Nodes.Count; num15++)
						{
							if (Nodes[num15].Visible)
							{
								num2 = num15;
								break;
							}
						}
					}
				}
				else
				{
					num2 = num5;
				}
			}
			if (keys == Keys.NumPad4)
			{
				if (Nodes[num2].Skill != null)
				{
					Nodes[num2].Expand = false;
				}
				else
				{
					Nodes[num2].ParentNode.Expand = false;
					for (int num16 = num2 - 1; num16 >= 0; num16--)
					{
						if (Nodes[num16].Visible)
						{
							num2 = num16;
							break;
						}
					}
				}
			}
			if (keys == Keys.OemMinus || keys == Keys.Subtract || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Negative"))
			{
				foreach (SPNode node2 in Nodes)
				{
					node2.Expand = false;
				}
				for (int num17 = num2 - 1; num17 >= 0; num17--)
				{
					if (Nodes[num17].Visible)
					{
						num2 = num17;
						break;
					}
				}
			}
			if (keys == Keys.Oemplus || keys == Keys.Add || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Positive"))
			{
				foreach (SPNode node3 in Nodes)
				{
					node3.Expand = true;
				}
			}
			if (keys == Keys.NumPad6)
			{
				Nodes[num2].Expand = true;
			}
			if (keys == Keys.OemQuestion)
			{
				if (Nodes[num2].Skill != null)
				{
					Popup.Show(Nodes[num2].Skill.GetFormattedDescription());
				}
				else
				{
					Popup.Show(Nodes[num2].Power.GetFormattedDescription());
				}
			}
			if (keys == Keys.Space || keys == Keys.Enter)
			{
				SelectNode(Nodes[num2], GO);
				BuildNodes(GO);
				SkillEntry skill = Nodes[num2].Skill;
				if (skill != null && Nodes[num2].Skill != skill)
				{
					for (int num18 = 0; num18 < Nodes.Count; num18++)
					{
						if (Nodes[num18].Skill == skill)
						{
							num2 = num18;
							if (num2 < num)
							{
								num = num2;
							}
							break;
						}
					}
				}
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5 || keys == Keys.NumPad9 || keys == Keys.NumPad7)
			{
				flag = true;
			}
		}
		GameManager.Instance.PopGameView();
		if (keys == Keys.NumPad7 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Left"))
		{
			return ScreenReturn.Previous;
		}
		if (keys == Keys.NumPad9 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Right"))
		{
			return ScreenReturn.Next;
		}
		return ScreenReturn.Exit;
	}
}
