using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Cysharp.Text;
using Qud.UI;
using XRL.Core;
using XRL.World;

namespace XRL.UI;

public class OptionsUI
{
	private class OptionNode : ConsoleTreeNode<OptionNode>
	{
		public GameOption Option;

		public override bool Visible
		{
			get
			{
				if (base.Visible)
				{
					return Option?.Requires?.RequirementsMet ?? true;
				}
				return false;
			}
		}

		public OptionNode(GameOption Option, bool Expand, OptionNode ParentNode)
			: base("", Expand, ParentNode)
		{
			this.Option = Option;
		}

		public OptionNode(string Category, bool Expand, OptionNode ParentNode)
			: base(Category, Expand, ParentNode)
		{
		}

		public string GetValue()
		{
			if (Option == null)
			{
				return null;
			}
			return Options.GetOption(Option.ID);
		}
	}

	public static HashSet<GameOption> RestartOptions = new HashSet<GameOption>();

	private static void BuildNodes(List<OptionNode> NodeList)
	{
		NodeList.Clear();
		foreach (string key in Options.OptionsByCategory.Keys)
		{
			OptionNode optionNode = new OptionNode(key, Expand: true, null);
			NodeList.Add(optionNode);
			foreach (GameOption item in Options.OptionsByCategory[key])
			{
				NodeList.Add(new OptionNode(item, Expand: true, optionNode));
			}
		}
	}

	public static ScreenReturn Show()
	{
		if (Options.ModernUI)
		{
			ControlManager.ResetInput();
			_ = SingletonWindowBase<OptionsScreen>.instance.OptionsMenu().Result;
		}
		else
		{
			GameManager.Instance.PushGameView("Options");
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			List<OptionNode> list = new List<OptionNode>();
			BuildNodes(list);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			Keys keys = Keys.None;
			int Index = 0;
			int Index2 = 0;
			while (!flag)
			{
				int num2;
				int i;
				while (true)
				{
					Event.ResetPool();
					scrapBuffer.Clear();
					scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
					scrapBuffer.Goto(13, 0);
					scrapBuffer.Write("[ &wGame Options&y ]");
					scrapBuffer.Goto(50, 0);
					if (CapabilityManager.AllowKeyboardHotkeys)
					{
						scrapBuffer.Write(" &WESC&y - Exit ");
					}
					int num = 2;
					num2 = 0;
					ConsoleTreeNode<OptionNode>.NextVisible(list, ref Index);
					if (Index2 < Index)
					{
						Index = Index2;
					}
					i = Index;
					while (num <= 21 && i <= list.Count)
					{
						bool flag5 = false;
						bool flag6 = false;
						string text = null;
						for (; i < list.Count && !list[i].Visible; i++)
						{
						}
						if (i < list.Count)
						{
							if (list[i].Option == null)
							{
								scrapBuffer.Goto(4, num);
								if (list[i].Expand)
								{
									scrapBuffer.Write("[-] ");
								}
								else
								{
									scrapBuffer.Write("[+] ");
								}
								scrapBuffer.Write("&C" + list[i].Category + "&y");
							}
							else
							{
								text = list[i].Option.DisplayText;
								if (list[i].Option.Type == "Checkbox")
								{
									scrapBuffer.Goto(6, num);
									flag6 = true;
									if (Options.GetOption(list[i].Option.ID).EqualsNoCase("Yes"))
									{
										scrapBuffer.Write("[&gû&y] ");
									}
									else
									{
										scrapBuffer.Write("[&g &y] ");
									}
								}
								else if (list[i].Option.Type == "Button")
								{
									scrapBuffer.Goto(6, num);
									scrapBuffer.Write("<&Y");
									scrapBuffer.Write(list[i].Option.DisplayText);
									scrapBuffer.Write("&y>");
								}
								else if (list[i].Option.Type == "Slider")
								{
									int num3 = (Convert.ToInt32(Options.GetOption(list[i].Option.ID)) - list[i].Option.Min) / list[i].Option.Increment;
									int num4 = (list[i].Option.Max - list[i].Option.Min) / list[i].Option.Increment;
									scrapBuffer.Goto(6, num);
									scrapBuffer.Write("[");
									for (int j = 0; j < num3; j++)
									{
										scrapBuffer.Write("#");
									}
									for (int k = num3; k < num4; k++)
									{
										scrapBuffer.Write(" ");
									}
									scrapBuffer.Write("]");
									flag5 = 7 + num4 + text.Length >= 60;
									if (flag5)
									{
										if (num < 21)
										{
											num++;
											scrapBuffer.Goto(8, num);
											flag6 = true;
										}
										else if (i == Index2)
										{
											goto IL_03e2;
										}
									}
									else
									{
										scrapBuffer.Write(" ");
										flag6 = true;
									}
								}
								else if (list[i].Option.Type == "Combo")
								{
									scrapBuffer.Goto(6, num);
									scrapBuffer.Write("[");
									int num5 = 7;
									for (int l = 0; l < list[i].Option.Values.Length; l++)
									{
										string text2 = list[i].Option.Values[l];
										string text3 = list[i].Option.DisplayValues[l];
										num5 += text2.Length + 2;
										scrapBuffer.Write(" ");
										if (text2 == list[i].GetValue())
										{
											scrapBuffer.Write("&W" + text3);
										}
										else
										{
											scrapBuffer.Write("&K" + text3);
										}
										scrapBuffer.Write(" ");
									}
									scrapBuffer.Write("]");
									flag5 = num5 + text.Length >= 60;
									if (flag5)
									{
										if (num < 21)
										{
											num++;
											scrapBuffer.Goto(8, num);
											flag6 = true;
										}
										else if (i == Index2)
										{
											goto IL_0563;
										}
									}
									else
									{
										scrapBuffer.Write(" ");
										flag6 = true;
									}
								}
								else if (list[i].Option.Type == "BigCombo")
								{
									int num6 = 0;
									if (list[i].GetValue() == "*Max")
									{
										num6 = list[i].Option.Values.Length - 1;
									}
									int num7 = list[i].Option.Values.IndexOf(list[i].GetValue());
									if (num7 > -1)
									{
										num6 = num7;
									}
									scrapBuffer.Goto(6, num);
									if (num6 > 0)
									{
										scrapBuffer.Write("&W<< &K[more]  ");
									}
									if (num6 > 0)
									{
										scrapBuffer.Write("&K" + list[i].Option.DisplayValues[num6 - 1]);
									}
									scrapBuffer.Write("  &W" + list[i].Option.Values[num6] + "  ");
									if (num6 < list[i].Option.Values.Length - 1)
									{
										scrapBuffer.Write("  &K" + list[i].Option.DisplayValues[num6 + 1]);
									}
									if (num6 < list[i].Option.Values.Length - 1)
									{
										scrapBuffer.Write("  &K[more] &W>>");
									}
									num++;
									scrapBuffer.Goto(8, num);
									flag6 = true;
								}
							}
							if (flag6)
							{
								if (i == Index2)
								{
									scrapBuffer.Write("&Y" + text);
								}
								else
								{
									scrapBuffer.Write(text);
								}
							}
							num2 = i;
						}
						if (i == Index2)
						{
							scrapBuffer.Goto(2, flag5 ? (num - 1) : num);
							scrapBuffer.Write("&Y>");
						}
						num++;
						i++;
					}
					if (flag4)
					{
						Index2 = num2;
						flag4 = false;
						continue;
					}
					if (CapabilityManager.AllowKeyboardHotkeys)
					{
						scrapBuffer.Goto(2, 24);
						scrapBuffer.Write(" [&WSpace&y-change option] ");
					}
					if (Index != 0)
					{
						scrapBuffer.Goto(4, 1);
						scrapBuffer.Write("&W<More...>");
					}
					if (i < list.Count)
					{
						scrapBuffer.Goto(4, 22);
						scrapBuffer.Write("&W<More...>");
					}
					if (Index2 > num2 && Index < list.Count)
					{
						Index++;
						continue;
					}
					if (!flag3)
					{
						break;
					}
					Index2 = Index;
					flag3 = false;
					continue;
					IL_0563:
					Index++;
					continue;
					IL_03e2:
					Index++;
				}
				Popup._TextConsole.DrawBuffer(scrapBuffer);
				keys = Keyboard.getvk(MapDirectionToArrows: true);
				if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && Keyboard.RawCode != Keys.Prior && Keyboard.RawCode != Keys.Next))
				{
					flag = true;
				}
				if (keys == Keys.NumPad2 && Index2 < list.Count - 1)
				{
					ConsoleTreeNode<OptionNode>.NextVisible(list, ref Index2, 1);
				}
				if (keys == Keys.NumPad8 && Index2 > 0)
				{
					ConsoleTreeNode<OptionNode>.PrevVisible(list, ref Index2, -1);
				}
				if (keys == Keys.Next)
				{
					if (num2 == Index2 && i < list.Count)
					{
						ConsoleTreeNode<OptionNode>.NextVisible(list, ref Index2, 1);
						Index = Math.Min(Index2, list.Count - 20);
						flag4 = true;
					}
					else
					{
						Index2 = num2;
					}
				}
				if (keys == Keys.Prior)
				{
					if (Index == Index2 && Index > 0)
					{
						ConsoleTreeNode<OptionNode>.PrevVisible(list, ref Index2, -1);
						Index = Math.Max(Index2 - 21, 0);
						flag3 = true;
					}
					else
					{
						Index2 = Index;
					}
				}
				if (keys == Keys.OemMinus || keys == Keys.Subtract || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Negative"))
				{
					list.ForEach((OptionNode n) =>
					{
						n.Expand = false;
					});
					ConsoleTreeNode<OptionNode>.PrevVisible(list, ref Index2);
				}
				if (keys == Keys.Oemplus || keys == Keys.Add || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Positive"))
				{
					list.ForEach((OptionNode n) =>
					{
						n.Expand = true;
					});
				}
				OptionNode optionNode = list[Index2];
				GameOption option = optionNode.Option;
				if (keys == Keys.NumPad4 || keys == Keys.Left)
				{
					if (option != null && (option.Type == "Combo" || option.Type == "BigCombo"))
					{
						int num8 = option.Values.IndexOf(optionNode.GetValue());
						num8--;
						if (num8 < 0)
						{
							num8 = option.Values.Length - 1;
						}
						if (num8 >= option.Values.Length)
						{
							num8 = 0;
						}
						if (option.Restart)
						{
							RestartOptions.Add(option);
						}
						Options.SetOption(option.ID, option.Values[num8]);
					}
					else if (option != null && option.Type == "Slider")
					{
						int num9 = Convert.ToInt32(optionNode.GetValue()) - option.Increment;
						if (num9 > option.Max)
						{
							num9 = option.Max;
						}
						if (num9 < option.Min)
						{
							num9 = option.Min;
						}
						if (option.Restart)
						{
							RestartOptions.Add(option);
						}
						Options.SetOption(option.ID, num9.ToStringCached());
					}
					else if (optionNode.Category != "")
					{
						optionNode.Expand = false;
					}
					else
					{
						optionNode.ParentNode.Expand = false;
						ConsoleTreeNode<OptionNode>.PrevVisible(list, ref Index2);
					}
				}
				if (keys == Keys.NumPad6 || keys == Keys.Right)
				{
					if (option != null && (option.Type == "Combo" || option.Type == "BigCombo"))
					{
						int num10 = option.Values.IndexOf(optionNode.GetValue());
						num10++;
						if (num10 < 0)
						{
							num10 = option.Values.Length - 1;
						}
						if (num10 >= option.Values.Length)
						{
							num10 = 0;
						}
						if (option.Restart)
						{
							RestartOptions.Add(option);
						}
						Options.SetOption(option.ID, option.Values[num10]);
					}
					else if (option != null && option.Type == "Slider")
					{
						int num11 = Convert.ToInt32(optionNode.GetValue()) + option.Increment;
						if (num11 > option.Max)
						{
							num11 = option.Max;
						}
						if (num11 < option.Min)
						{
							num11 = option.Min;
						}
						if (option.Restart)
						{
							RestartOptions.Add(option);
						}
						Options.SetOption(option.ID, num11.ToString());
					}
					else
					{
						optionNode.Expand = true;
					}
				}
				if ((keys == Keys.Space || keys == Keys.Enter) && option != null)
				{
					if (option.Restart)
					{
						RestartOptions.Add(option);
					}
					if (option.Type == "Checkbox")
					{
						Options.SetOption(option.ID, (Options.GetOption(option.ID) == "Yes") ? "No" : "Yes");
					}
					if (option.ID == "OptionsPrereleaseInputManager")
					{
						ControlManager.ResetInput();
					}
					if (option.Type == "Button" && option.OnClick.Invoke(null, null) is Task<bool> task)
					{
						task.Wait();
					}
				}
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Keybindings")
				{
					KeyMappingUI.Show();
				}
				if (keys != Keys.Escape && keys != Keys.NumPad5 && (keys != Keys.MouseEvent || !(Keyboard.CurrentMouseEvent.Event == "RightClick")))
				{
					continue;
				}
				if (flag2 && Popup.ShowYesNo("Would you like to save your changes?") == DialogResult.Yes)
				{
					CommandBindingManager.SaveKeymap(CommandBindingManager.CurrentMap, Environment.UserName + ".Keymap2.json");
				}
				try
				{
					if (Options.GetOption("OptionUseTiles") == "Yes")
					{
						Globals.RenderMode = RenderModeType.Tiles;
					}
					else
					{
						Globals.RenderMode = RenderModeType.Text;
					}
					if (Options.GetOption("OptionAnalytics") == "Yes")
					{
						Globals.EnableMetrics = true;
					}
					else
					{
						Globals.EnableMetrics = false;
					}
				}
				catch
				{
				}
				flag = true;
			}
			GameManager.Instance.PopGameView();
		}
		if (RestartOptions.Count != 0)
		{
			using Utf16ValueStringBuilder utf16ValueStringBuilder = ZString.CreateStringBuilder();
			utf16ValueStringBuilder.Append("These options require a game restart to take effect:\n");
			foreach (GameOption restartOption in RestartOptions)
			{
				utf16ValueStringBuilder.Append('\n');
				utf16ValueStringBuilder.Append("{{g|");
				utf16ValueStringBuilder.Append("* ");
				utf16ValueStringBuilder.Append(restartOption.DisplayText);
				utf16ValueStringBuilder.Append("}}");
			}
			RestartOptions.Clear();
			utf16ValueStringBuilder.Append("\n\nDo you want to do so now?");
			if (Popup.ShowYesNo(utf16ValueStringBuilder.ToString()) == DialogResult.Yes)
			{
				GameManager.Restart();
			}
		}
		return ScreenReturn.Exit;
	}
}
