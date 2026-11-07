using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using XRL.Messages;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

[HasGameBasedStaticCache]
public class AbilityManager
{
	public static readonly List<ActivatedAbilityEntry> PlayerAbilities = new List<ActivatedAbilityEntry>();

	public static readonly Dictionary<string, ActivatedAbilityEntry> PlayerAbilitiesByCommand = new Dictionary<string, ActivatedAbilityEntry>();

	public static readonly Dictionary<string, List<ActivatedAbilityEntry>> PlayerAbilitiesByClass = new Dictionary<string, List<ActivatedAbilityEntry>>();

	public static readonly ReaderWriterLockSlim PlayerAbilityLock = new ReaderWriterLockSlim();

	public static AbilityManagerScreen.SortMode sortMode
	{
		get
		{
			return SingletonWindowBase<AbilityManagerScreen>.instance.sortMode;
		}
		set
		{
			SingletonWindowBase<AbilityManagerScreen>.instance.sortMode = value;
		}
	}

	public static void RefreshPlayerAbilities()
	{
		PlayerAbilityLock.EnterWriteLock();
		try
		{
			PlayerAbilities.Clear();
			PlayerAbilitiesByCommand.Clear();
			PlayerAbilitiesByClass.Clear();
			Dictionary<Guid, ActivatedAbilityEntry> dictionary = The.Player?.Abilities?.AbilityByGuid;
			if (dictionary.IsNullOrEmpty())
			{
				return;
			}
			foreach (var (_, activatedAbilityEntry2) in dictionary)
			{
				PlayerAbilities.Add(activatedAbilityEntry2);
				PlayerAbilitiesByCommand.TryAdd(activatedAbilityEntry2.Command, activatedAbilityEntry2);
				if (!PlayerAbilitiesByClass.TryGetValue(activatedAbilityEntry2.Class, out var value))
				{
					value = (PlayerAbilitiesByClass[activatedAbilityEntry2.Class] = new List<ActivatedAbilityEntry>());
				}
				value.Add(activatedAbilityEntry2);
			}
		}
		finally
		{
			PlayerAbilityLock.ExitWriteLock();
		}
	}

	public static bool IsWorldMapUsable(string AbilityCommand)
	{
		PlayerAbilityLock.EnterReadLock();
		try
		{
			ActivatedAbilityEntry value;
			return !PlayerAbilitiesByCommand.TryGetValue(AbilityCommand, out value) || value.IsWorldMapUsable;
		}
		finally
		{
			PlayerAbilityLock.ExitReadLock();
		}
	}

	public static void UpdateFavorites()
	{
		RefreshPlayerAbilities();
	}

	private static void BuildNodes(List<AbilityNode> NodeList, XRL.World.GameObject GO)
	{
		NodeList.Clear();
		ActivatedAbilities activatedAbilities = GO.ActivatedAbilities;
		if (activatedAbilities == null)
		{
			return;
		}
		if (sortMode == AbilityManagerScreen.SortMode.Class)
		{
			foreach (var (_, activatedAbilityEntry2) in activatedAbilities.AbilityByGuid)
			{
				int index = -1;
				AbilityNode abilityNode = null;
				for (int num = NodeList.Count - 1; num >= 0; num--)
				{
					AbilityNode abilityNode2 = NodeList[num];
					if (abilityNode2.ParentNode != null)
					{
						if (abilityNode2.ParentNode.Category == activatedAbilityEntry2.Class)
						{
							index = num + 1;
							abilityNode = abilityNode2.ParentNode;
							break;
						}
					}
					else if (abilityNode2.Category == activatedAbilityEntry2.Class)
					{
						index = num + 1;
						abilityNode = abilityNode2;
						break;
					}
				}
				if (abilityNode == null)
				{
					abilityNode = new AbilityNode(null, activatedAbilityEntry2.Class);
					NodeList.Add(abilityNode);
					index = NodeList.Count;
				}
				NodeList.Insert(index, new AbilityNode(activatedAbilityEntry2, "", abilityNode));
			}
			return;
		}
		foreach (ActivatedAbilityEntry item in activatedAbilities.GetAbilityListOrderedByPreference())
		{
			NodeList.Add(new AbilityNode(item));
		}
	}

	public static string Show(XRL.World.GameObject GO)
	{
		if (!BeforeAbilityManagerOpenEvent.Check(The.Player))
		{
			return null;
		}
		if (Options.ModernUI)
		{
			AbilityManagerScreen.Result result = default;
			try
			{
				result = AbilityManagerScreen.OpenAbilityManager(GO).Result;
			}
			catch (Exception x)
			{
				MetricsManager.LogException("AbilityManagerScreen.OpenAbilityManager", x);
			}
			return result.ability?.Command;
		}
		GameManager.Instance.PushGameView("AbilityManager");
		TextConsole.LoadScrapBuffers();
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		List<AbilityNode> list = new List<AbilityNode>();
		int Index = 0;
		int Index2 = 0;
		while (true)
		{
			list.Clear();
			BuildNodes(list, GO);
			List<UnityEngine.KeyCode> hotkeySpread = ControlManager.GetHotkeySpread(new List<string> { "Menus", "UINav" });
			while (true)
			{
				Dictionary<char, string> dictionary;
				Dictionary<char, ActivatedAbilityEntry> dictionary2;
				Keys keys;
				char key;
				ActivatedAbilityEntry activatedAbilityEntry;
				if (!flag)
				{
					while (true)
					{
						XRL.World.Event.ResetPool();
						int num = 0;
						dictionary = new Dictionary<char, string>();
						dictionary2 = new Dictionary<char, ActivatedAbilityEntry>();
						scrapBuffer.Clear();
						scrapBuffer.SingleBox(0, 0, 79, 24, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
						scrapBuffer.Goto(3, 0);
						scrapBuffer.Write("[ {{W|Manage Abilities}} ]");
						scrapBuffer.X += 2;
						scrapBuffer.Write(" " + Markup.Color("W", ControlManager.getCommandInputDescription("Toggle", mapGlyphs: false)) + "-" + AbilityManagerScreen.sortModeDescription + " ");
						scrapBuffer.Goto(65, 0);
						scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Cancel", mapGlyphs: false) + "}}-exit ");
						int num2 = 2;
						int num3 = 0;
						ConsoleTreeNode<AbilityNode>.NextVisible(list, ref Index);
						if (Index2 < Index)
						{
							Index = Index2;
						}
						int i = Index;
						bool flag4 = GO.IsRealityDistortionUsable();
						while (num2 <= 21 && i <= list.Count)
						{
							for (; i < list.Count && list[i].ParentNode != null && !list[i].ParentNode.Expand; i++)
							{
							}
							if (i < list.Count)
							{
								if (list[i].Category != "")
								{
									scrapBuffer.Goto(4, num2);
									if (list[i].Expand)
									{
										scrapBuffer.Write("[-] ");
									}
									else
									{
										scrapBuffer.Write("[+] ");
									}
									scrapBuffer.Write("{{W|" + list[i].Category + "}}");
								}
								else
								{
									scrapBuffer.Goto(3, num2);
									StringBuilder stringBuilder = new StringBuilder();
									ActivatedAbilityEntry ability = list[i].Ability;
									scrapBuffer.Write(ability.GetUITile());
									dictionary[getHotkeyChar(num)] = ability.Command;
									dictionary2[getHotkeyChar(num)] = ability;
									if (!ability.Enabled)
									{
										stringBuilder.Append("  {{K|" + getHotkeyDisplay(num) + ") " + ability.DisplayName + (ability.IsAttack ? " [attack]" : "") + " [disabled]}}");
									}
									else if (ability.Cooldown <= 0)
									{
										if (ability.IsRealityDistortionBased && !flag4)
										{
											stringBuilder.Append("  {{K|" + getHotkeyDisplay(num) + ") " + ability.DisplayName + (ability.IsAttack ? " [attack]" : "") + " [astrally tethered]}}");
										}
										else
										{
											stringBuilder.Append("  " + getHotkeyDisplay(num) + ") " + ability.DisplayName + (ability.IsAttack ? " [{{W|attack}}]" : ""));
										}
									}
									else if (ability.IsRealityDistortionBased && !flag4)
									{
										stringBuilder.Append("  {{K|" + getHotkeyDisplay(num) + "}}) " + ability.DisplayName + " [{{C|" + ability.CooldownRounds + "}} turn cooldown, astrally tethered]");
									}
									else
									{
										stringBuilder.Append("  {{K|" + getHotkeyDisplay(num) + "}}) " + ability.DisplayName + " [{{C|" + ability.CooldownRounds + "}} turn cooldown]");
									}
									if (ability.Toggleable)
									{
										if (ability.ToggleState)
										{
											stringBuilder.Append(" {{K|[{{g|Toggled on}}]}}");
										}
										else
										{
											stringBuilder.Append(" {{K|[{{y|Toggled off}}]}}");
										}
									}
									string commandInputDescription = ControlManager.getCommandInputDescription(ability.Command);
									if (commandInputDescription != null && !string.IsNullOrEmpty(commandInputDescription))
									{
										stringBuilder.Append(" {{Y|<{{w|" + commandInputDescription + "}}>}}");
									}
									num++;
									scrapBuffer.Write(stringBuilder.ClipExceptFormatting(40, "..."));
								}
								num3 = i;
							}
							if (i == Index2)
							{
								scrapBuffer.Goto(2, num2);
								scrapBuffer.Write("{{Y|>}}");
							}
							num2++;
							i++;
						}
						if (list.Count == 0)
						{
							Popup.Show("You have no abilities to manage!");
							GameManager.Instance.PopGameView();
							return "";
						}
						if (flag3)
						{
							Index2 = num3;
							flag3 = false;
							continue;
						}
						if (list[Index2].Ability != null)
						{
							TextBlock textBlock = new TextBlock(list[Index2].Ability.Description, 30, 20);
							for (int j = 0; j < textBlock.Lines.Count; j++)
							{
								scrapBuffer.Goto(45, j + 2);
								scrapBuffer.Write(textBlock.Lines[j]);
							}
						}
						scrapBuffer.Goto(2, 24);
						scrapBuffer.Write("[ ");
						scrapBuffer.Write(Markup.Color("W", ControlManager.getCommandInputDescription("Accept")) + "-Use Ability");
						if (list[Index2].Ability != null)
						{
							scrapBuffer.Write(" " + Markup.Color("W", ControlManager.getCommandInputDescription("CmdInsert")) + "-Map key");
							scrapBuffer.Write(" " + Markup.Color("W", ControlManager.getCommandInputDescription("CmdDelete")) + "-unbind");
						}
						if (sortMode == AbilityManagerScreen.SortMode.Custom)
						{
							scrapBuffer.Write(" " + Markup.Color("W", ControlManager.getCommandInputDescription("V Negative")) + "/" + Markup.Color("W", ControlManager.getCommandInputDescription("V Positive")) + "-Change Order");
						}
						scrapBuffer.Write(" ]");
						if (Index != 0)
						{
							scrapBuffer.Goto(4, 1);
							scrapBuffer.Write("{{W|<More...>}}");
						}
						if (i < list.Count)
						{
							scrapBuffer.Goto(4, 22);
							scrapBuffer.Write("{{W|<More...>}}");
						}
						if (Index2 > num3 && Index < list.Count)
						{
							Index++;
							continue;
						}
						if (flag2)
						{
							Index2 = Index;
							flag2 = false;
							continue;
						}
						Popup._TextConsole.DrawBuffer(scrapBuffer);
						keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
						key = ("" + (char)Keyboard.Char + " ").ToLower()[0];
						if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && Keyboard.RawCode != Keys.Prior && Keyboard.RawCode != Keys.Next))
						{
							flag = true;
						}
						if (keys == Keys.NumPad2 && Index2 < list.Count - 1)
						{
							ConsoleTreeNode<AbilityNode>.NextVisible(list, ref Index2, 1);
						}
						if (keys == Keys.NumPad8 && Index2 > 0)
						{
							ConsoleTreeNode<AbilityNode>.PrevVisible(list, ref Index2, -1);
						}
						if (keys == Keys.NumPad4)
						{
							if (list[Index2].Category != "")
							{
								list[Index2].Expand = false;
							}
							else
							{
								if (list[Index2].ParentNode != null)
								{
									list[Index2].ParentNode.Expand = false;
								}
								ConsoleTreeNode<AbilityNode>.PrevVisible(list, ref Index2);
							}
						}
						if (keys == Keys.NumPad6)
						{
							list[Index2].Expand = true;
						}
						if (keys == Keys.Next)
						{
							if (num3 == Index2 && i < list.Count)
							{
								ConsoleTreeNode<AbilityNode>.NextVisible(list, ref Index2, 1);
								Index = Math.Min(Index2, list.Count - 20);
								flag3 = true;
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
								ConsoleTreeNode<AbilityNode>.PrevVisible(list, ref Index2, -1);
								Index = Math.Max(Index2 - 21, 0);
								flag2 = true;
							}
							else
							{
								Index2 = Index;
							}
						}
						if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:Toggle")
						{
							break;
						}
						if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Negative")
						{
							if (sortMode == AbilityManagerScreen.SortMode.Class)
							{
								list.ForEach((AbilityNode n) =>
								{
									n.Expand = false;
								});
								ConsoleTreeNode<AbilityNode>.PrevVisible(list, ref Index2);
								continue;
							}
							if (Index2 > 0)
							{
								goto IL_0a2f;
							}
						}
						if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Positive")
						{
							if (sortMode == AbilityManagerScreen.SortMode.Class)
							{
								list.ForEach((AbilityNode n) =>
								{
									n.Expand = true;
								});
								continue;
							}
							if (Index2 < list.Count - 1)
							{
								goto IL_0aed;
							}
						}
						_ = 191;
						activatedAbilityEntry = null;
						if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdInsert" && list[Index2].Ability != null)
						{
							AbilityManagerScreen.HandleRebindAsync(list[Index2].Ability, "Menu").Wait();
							continue;
						}
						goto IL_0ba3;
					}
					if (sortMode == AbilityManagerScreen.SortMode.Class)
					{
						sortMode = AbilityManagerScreen.SortMode.Custom;
					}
					else
					{
						sortMode = AbilityManagerScreen.SortMode.Class;
					}
					break;
				}
				GameManager.Instance.PopGameView();
				return null;
				IL_0ba3:
				if (keys >= Keys.A && keys <= Keys.Z && dictionary.ContainsKey(key))
				{
					activatedAbilityEntry = dictionary2[key];
				}
				if (Keyboard.RawCode == Keys.Space)
				{
					if (list[Index2].Ability != null)
					{
						activatedAbilityEntry = list[Index2].Ability;
					}
					else
					{
						list[Index2].Expand = !list[Index2].Expand;
					}
				}
				if (activatedAbilityEntry != null)
				{
					string notUsableDescription = activatedAbilityEntry.NotUsableDescription;
					if (string.IsNullOrEmpty(notUsableDescription))
					{
						return activatedAbilityEntry.Command;
					}
					if (Options.GetOption("OptionAbilityCooldownWarningAsMessage").ToUpper() == "YES")
					{
						MessageQueue.AddPlayerMessage("You must wait {{C|" + notUsableDescription + "}} to use that ability again.");
					}
					else
					{
						Popup.Show(notUsableDescription);
					}
				}
				if (keys == Keys.Escape || keys == Keys.NumPad5)
				{
					flag = true;
				}
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")
				{
					flag = true;
				}
				continue;
				IL_0aed:
				List<string> list2 = list.Select((AbilityNode n) => n.Ability?.Command).ToList();
				string item = list2[Index2];
				list2.RemoveAt(Index2);
				list2.Insert(Index2 + 1, item);
				ActivatedAbilities.PreferenceOrder = list2;
				Index2++;
				break;
				IL_0a2f:
				List<string> list3 = list.Select((AbilityNode n) => n.Ability?.Command).ToList();
				string item2 = list3[Index2];
				list3.RemoveAt(Index2);
				list3.Insert(Index2 - 1, item2);
				ActivatedAbilities.PreferenceOrder = list3;
				Index2--;
				break;
			}
			continue;
			char getHotkeyChar(int n)
			{
				if (hotkeySpread.Count <= n)
				{
					return '\0';
				}
				return Keyboard.ConvertKeycodeToLowercaseChar(hotkeySpread[n]);
			}
			string getHotkeyDisplay(int n)
			{
				if (hotkeySpread.Count <= n)
				{
					return " ";
				}
				return Keyboard.ConvertKeycodeToLowercaseChar(hotkeySpread[n]).ToString() ?? "";
			}
			char getHotkeyChar(int n)
			{
				if (hotkeySpread.Count <= n)
				{
					return '\0';
				}
				return Keyboard.ConvertKeycodeToLowercaseChar(hotkeySpread[n]);
			}
			string getHotkeyDisplay(int n)
			{
				if (hotkeySpread.Count <= n)
				{
					return " ";
				}
				return Keyboard.ConvertKeycodeToLowercaseChar(hotkeySpread[n]).ToString() ?? "";
			}
		}
	}
}
