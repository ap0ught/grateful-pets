using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Qud.UI;
using XRL.World;

namespace XRL.UI;

[UIView("KeymappingLegacy", false, true, false, "Keybind", "Keymapping", false, 0, false)]
public class KeyMappingUI : IWantsTextConsoleInit
{
	private class KeyNode : ConsoleTreeNode<KeyNode>
	{
		public GameCommand Command;

		public string primaryBinding;

		public string secondaryBinding;

		public KeyNode(GameCommand Command, bool Expand, KeyNode ParentNode)
			: base("", Expand, ParentNode)
		{
			this.Command = Command;
		}

		public KeyNode(string Category, bool Expand, KeyNode ParentNode)
			: base(Category, Expand, ParentNode)
		{
		}
	}

	private static ScreenBuffer Buffer;

	private static TextConsole TextConsole;

	public void Init(TextConsole console, ScreenBuffer buffer)
	{
		Buffer = buffer;
		TextConsole = console;
	}

	private static async Task BuildNodes(List<KeyNode> NodeList, ControlManager.InputDeviceType deviceType)
	{
		await The.UiContext;
		NodeList.Clear();
		foreach (string item in CommandBindingManager.CategoriesInOrder)
		{
			KeyNode keyNode = new KeyNode(item, Expand: true, null);
			NodeList.Add(keyNode);
			foreach (GameCommand item2 in CommandBindingManager.CommandsByCategory[item])
			{
				KeyNode keyNode2 = new KeyNode(item2, Expand: true, keyNode);
				CommandBindingManager.GetCommandBindings(item2.ID, deviceType, out var bind, out var bind2);
				keyNode2.primaryBinding = bind;
				keyNode2.secondaryBinding = bind2;
				NodeList.Add(keyNode2);
			}
		}
	}

	public static ScreenReturn Show()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		ControlManager.InputDeviceType deviceType = ControlManager.InputDeviceType.Keyboard;
		Keys keys = Keys.None;
		int Index = 0;
		int Index2 = 0;
		int num = 0;
		if (Options.ModernUI)
		{
			GameManager.Instance.PushGameView("Keybinds");
			Event.ResetPool();
			Buffer.Clear();
			Buffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			Buffer.Goto(13, 0);
			Buffer.Write("[ {{w|Control Mapping}} ]");
			Buffer.Goto(3, 10);
			Buffer.Write("Loading...");
			Popup._TextConsole.DrawBuffer(Buffer);
			ControlManager.ResetInput();
			_ = SingletonWindowBase<KeybindsScreen>.instance.KeybindsMenu().Result;
			GameManager.Instance.PopGameView(bHard: true);
			return ScreenReturn.Exit;
		}
		GameManager.Instance.PushGameView("Keymapping");
		List<KeyNode> list = new List<KeyNode>();
		BuildNodes(list, deviceType).Wait();
		while (!flag)
		{
			int num3;
			int i;
			while (true)
			{
				Event.ResetPool();
				Buffer.Clear();
				Buffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				Buffer.Goto(13, 0);
				Buffer.Write("[ {{w|Control Mapping}} ]");
				Buffer.Goto(50, 0);
				Buffer.Write(" {{W|ESC}} - Exit ");
				int num2 = 2;
				num3 = 0;
				ConsoleTreeNode<KeyNode>.NextVisible(list, ref Index);
				if (Index2 < Index)
				{
					Index = Index2;
				}
				i = Index;
				while (num2 <= 21 && i <= list.Count)
				{
					for (; i < list.Count && list[i].ParentNode != null && !list[i].ParentNode.Expand; i++)
					{
					}
					if (i < list.Count)
					{
						if (list[i].Command == null)
						{
							Buffer.Goto(4, num2);
							if (list[i].Expand)
							{
								Buffer.Write("[-] ");
							}
							else
							{
								Buffer.Write("[+] ");
							}
							Buffer.Write("{{C|" + list[i].Category + "}}");
						}
						else
						{
							Buffer.Goto(4, num2);
							if (i == Index2)
							{
								Buffer.Write("  {{Y|" + list[i].Command.DisplayText + "}}");
							}
							else
							{
								Buffer.Write("  " + list[i].Command.DisplayText);
							}
							Buffer.Goto(35, num2);
							if (!string.IsNullOrEmpty(list[i].primaryBinding))
							{
								if (num == 0 && i == Index2)
								{
									Buffer.Write("{{keybind|" + list[i].primaryBinding + "}}");
								}
								else
								{
									Buffer.Write("{{dark keybind|" + list[i].primaryBinding + "}}");
								}
							}
							else if (num == 0 && i == Index2)
							{
								Buffer.Write("{{y|<none>}}");
							}
							else
							{
								Buffer.Write("{{K|<none>}}");
							}
							Buffer.Goto(60, num2);
							if (!string.IsNullOrEmpty(list[i].secondaryBinding))
							{
								if (num == 1 && i == Index2)
								{
									Buffer.Write("{{keybind|" + list[i].secondaryBinding + "}}");
								}
								else
								{
									Buffer.Write("{{dark keybind|" + list[i].secondaryBinding + "}}");
								}
							}
							else if (num == 1 && i == Index2)
							{
								Buffer.Write("{{y|<none>}}");
							}
							else
							{
								Buffer.Write("{{K|<none>}}");
							}
						}
						num3 = i;
					}
					if (i == Index2)
					{
						Buffer.Goto(2, num2);
						Buffer.Write("{{Y|>}}");
					}
					num2++;
					i++;
				}
				if (flag4)
				{
					Index2 = num3;
					flag4 = false;
					continue;
				}
				Buffer.Goto(2, 24);
				Buffer.Write(" [{{W|Space}}-Assign] ");
				Buffer.Goto(17, 24);
				Buffer.Write(" [{{W|" + ControlManager.getCommandInputDescription("CmdDelete") + "}}-Delete binding] ");
				Buffer.Goto(32, 24);
				Buffer.Write(" [{{W|" + ControlManager.getCommandInputDescription("CmdHelp") + "}}-Load defaults] ");
				if (Index != 0)
				{
					Buffer.Goto(4, 1);
					Buffer.Write("{{W|<More...>}}");
				}
				if (i < list.Count)
				{
					Buffer.Goto(4, 22);
					Buffer.Write("{{W|<More...>}}");
				}
				if (Index2 > num3 && Index < list.Count)
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
			}
			TextConsole.DrawBuffer(Buffer);
			keys = Keyboard.getvk(MapDirectionToArrows: false);
			if (keys == Keys.NumPad2)
			{
				ConsoleTreeNode<KeyNode>.NextVisible(list, ref Index2, 1);
			}
			if (keys == Keys.NumPad8)
			{
				ConsoleTreeNode<KeyNode>.PrevVisible(list, ref Index2, -1);
			}
			if (keys == Keys.Next)
			{
				if (num3 == Index2 && i < list.Count)
				{
					ConsoleTreeNode<KeyNode>.NextVisible(list, ref Index2, 1);
					Index = Math.Min(Index2, list.Count - 20);
					flag4 = true;
				}
				else
				{
					Index2 = num3;
				}
			}
			if (keys == Keys.Prior)
			{
				if (Index == Index2 && Index > 0)
				{
					ConsoleTreeNode<KeyNode>.PrevVisible(list, ref Index2, -1);
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
				list.ForEach((KeyNode n) =>
				{
					n.Expand = false;
				});
				ConsoleTreeNode<KeyNode>.PrevVisible(list, ref Index2);
			}
			if (keys == Keys.Oemplus || keys == Keys.Add || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Positive"))
			{
				list.ForEach((KeyNode n) =>
				{
					n.Expand = true;
				});
			}
			if (keys == Keys.NumPad4)
			{
				if (list[Index2].Category != "")
				{
					list[Index2].Expand = false;
				}
				else if (num == 1)
				{
					num = 0;
				}
				else
				{
					list[Index2].ParentNode.Expand = false;
					ConsoleTreeNode<KeyNode>.PrevVisible(list, ref Index2);
				}
			}
			if (keys == Keys.NumPad6)
			{
				if (list[Index2].Category != "")
				{
					list[Index2].Expand = true;
				}
				else
				{
					num = 1;
				}
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdDelete" && list[Index2].Command != null && list[Index2].Command.ID != "CmdSystemMenu")
			{
				if ((num == 0 && string.IsNullOrEmpty(list[Index2].primaryBinding)) || (num == 1 && string.IsNullOrEmpty(list[Index2].secondaryBinding)))
				{
					continue;
				}
				if (!list[Index2].Command.CanRemoveBinding())
				{
					Popup.Show("Can not remove the last binding for " + Markup.Color("C", list[Index2].Command.DisplayText) + ".");
					continue;
				}
				if (Popup.ShowYesNo("Are you sure you want to clear this binding for {{C|" + list[Index2].Command.DisplayText + "}}?") == DialogResult.Yes)
				{
					CommandBindingManager.RemoveCommandBindingAsync(list[Index2].Command.ID, num).Wait();
				}
				flag2 = true;
			}
			if ((Keyboard.RawCode == Keys.F1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdHelp")) && Popup.ShowYesNo("Are you sure you want to override your bindings with the default?") == DialogResult.Yes)
			{
				CommandBindingManager.RestoreDefaultsAsync().Wait();
				flag2 = true;
			}
			if ((Keyboard.RawCode == Keys.Space || keys == Keys.Enter) && list[Index2].Command != null && list[Index2].Command.ID != "CmdSystemMenu")
			{
				ScreenBuffer.GetScrapBuffer1();
				Popup.RenderBlock("Press control to bind to {{C|" + list[Index2].Command.DisplayText + "}}", "");
				KeybindsScreen.HandleRebindAsync(list[Index2].Command, num, deviceType).Wait();
				flag2 = true;
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5)
			{
				if (flag2)
				{
					if (Popup.ShowYesNo("Would you like to save your changes?") == DialogResult.Yes)
					{
						CommandBindingManager.SaveCurrentKeymapAsync().Wait();
					}
					else
					{
						CommandBindingManager.LoadCurrentKeymapAsync(restoreLayers: true).Wait();
					}
				}
				flag = true;
			}
			if (flag2)
			{
				BuildNodes(list, deviceType).Wait();
			}
		}
		GameManager.Instance.PopGameView(bHard: true);
		return ScreenReturn.Exit;
	}
}
