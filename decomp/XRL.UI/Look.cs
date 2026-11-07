using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConsoleLib.Console;
using Genkit;
using ModelShark;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Text;

namespace XRL.UI;

[UIView("Looker", false, false, false, "Looker", "PickTargetFrame", false, 1, false)]
public class Look : IWantsTextConsoleInit
{
	public struct TooltipInformation
	{
		public string DisplayName;

		public string FeelingText;

		public string DifficultyText;

		public string WoundLevel;

		public string LongDescription;

		public IRenderable IconRenderable;

		public string SubHeader
		{
			get
			{
				if (string.IsNullOrEmpty(FeelingText))
				{
					return DifficultyText;
				}
				if (string.IsNullOrEmpty(DifficultyText))
				{
					return FeelingText;
				}
				return FeelingText + ", " + DifficultyText;
			}
		}
	}

	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	[NonSerialized]
	private static StringBuilder LookSB = new StringBuilder();

	[NonSerialized]
	private static ScreenBuffer Buffer = ScreenBuffer.create(80, 25);

	[NonSerialized]
	private static ScreenBuffer OldBuffer = ScreenBuffer.create(80, 25);

	private static LookSorter lookSorter = new LookSorter();

	public static XRL.World.GameObject lookingAt;

	private static ScrollRect _lookerScrollRect = null;

	public static bool bLocked
	{
		get
		{
			return Options.LookLocked;
		}
		set
		{
			Options.LookLocked = value;
		}
	}

	private static ScrollRect lookerScrollRect => _lookerScrollRect ?? (_lookerScrollRect = GameManager.Instance.lookerTooltip.Tooltip?.GameObject?.GetComponentInChildren<ScrollRect>());

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public static XRL.World.GameObject GetTargetAt(int X, int Y, Zone Z, Predicate<XRL.World.GameObject> ObjectTest = null, Predicate<XRL.World.GameObject> ExtraVisibility = null)
	{
		if (X < 0)
		{
			return null;
		}
		if (X > Z.Width - 1)
		{
			return null;
		}
		if (Y < 0)
		{
			return null;
		}
		if (Y > Z.Height - 1)
		{
			return null;
		}
		Cell cell = Z.GetCell(X, Y);
		XRL.World.GameObject result = null;
		if (Z.GetVisibility(X, Y))
		{
			result = ((ExtraVisibility == null) ? cell.GetFirstObjectWithPart("Brain", ObjectTest, GameObjectIsVisible) : cell.GetFirstObjectWithPart("Brain", ObjectTest, (XRL.World.GameObject o) => GameObjectIsVisible(o) || ExtraVisibility(o)));
		}
		else if (ExtraVisibility != null)
		{
			result = cell.GetFirstObjectWithPart("Brain", ObjectTest, ExtraVisibility);
		}
		return result;
	}

	private static bool GameObjectIsVisible(XRL.World.GameObject obj)
	{
		return obj.IsVisible();
	}

	public static void GetObjectListCone(int StartX, int StartY, List<XRL.World.GameObject> ObjectList, string Direction, Predicate<XRL.World.GameObject> ObjectTest = null, Predicate<XRL.World.GameObject> ExtraVisibility = null)
	{
		Zone currentZone = The.Player.CurrentZone;
		ObjectList.Clear();
		int num = 1;
		int num2 = 1;
		int num3 = 1;
		int num4 = 1;
		int num5 = 1;
		int num6 = 1;
		switch (Direction)
		{
		case "nw":
			num = -1;
			num2 = -1;
			num3 = 1;
			num4 = 0;
			num5 = 0;
			num6 = 1;
			break;
		case "n":
			num = 0;
			num2 = -1;
			num3 = -1;
			num4 = 0;
			num5 = 1;
			num6 = 0;
			break;
		case "ne":
			num = 1;
			num2 = -1;
			num3 = -1;
			num4 = 0;
			num5 = 0;
			num6 = 1;
			break;
		case "e":
			num = 1;
			num2 = 0;
			num3 = 0;
			num4 = 1;
			num5 = 0;
			num6 = -1;
			break;
		case "se":
			num = 1;
			num2 = 1;
			num3 = 0;
			num4 = -1;
			num5 = -1;
			num6 = 0;
			break;
		case "s":
			num = 0;
			num2 = 1;
			num3 = 1;
			num4 = 0;
			num5 = -1;
			num6 = 0;
			break;
		case "sw":
			num = -1;
			num2 = 1;
			num3 = 0;
			num4 = -1;
			num5 = 1;
			num6 = 0;
			break;
		case "w":
			num = -1;
			num2 = 0;
			num3 = 0;
			num4 = -1;
			num5 = 0;
			num6 = 1;
			break;
		}
		int num7 = StartX;
		int num8 = StartY;
		int num9 = 0;
		while (num7 >= 0 && num7 < currentZone.Width && num8 >= 0 && num8 < currentZone.Height)
		{
			XRL.World.GameObject targetAt = GetTargetAt(num7, num8, currentZone, ObjectTest, ExtraVisibility);
			if (targetAt != null && targetAt.Render != null && targetAt.Render.Visible && !targetAt.HasProperty("HideCon"))
			{
				ObjectList.Add(targetAt);
			}
			for (int i = 0; i <= num9; i++)
			{
				targetAt = GetTargetAt(num7 + num3 * i, num8 + num4 * i, currentZone, ObjectTest, ExtraVisibility);
				if (targetAt != null && targetAt.Render != null && targetAt.Render.Visible && !targetAt.HasProperty("HideCon"))
				{
					ObjectList.Add(targetAt);
				}
				targetAt = GetTargetAt(num7 + num5 * i, num8 + num6 * i, currentZone, ObjectTest, ExtraVisibility);
				if (targetAt != null && targetAt.Render != null && targetAt.Render.Visible && !targetAt.HasProperty("HideCon"))
				{
					ObjectList.Add(targetAt);
				}
			}
			num9++;
			num7 += num;
			num8 += num2;
		}
	}

	/// <summary>
	///     Generate the text only block of information used for the text UI and other
	///     lookers.
	/// </summary>
	public static string GenerateTooltipContent(XRL.World.GameObject O)
	{
		TooltipInformation tooltipInformation = GenerateTooltipInformation(O);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(tooltipInformation.DisplayName);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(tooltipInformation.LongDescription);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(tooltipInformation.SubHeader);
		stringBuilder.AppendLine(tooltipInformation.WoundLevel);
		return Markup.Transform(stringBuilder.ToString());
	}

	/// <summary>
	///     Generates a list of text fields and a renderable to use for the looker tooltip.
	/// </summary>
	public static TooltipInformation GenerateTooltipInformation(XRL.World.GameObject go)
	{
		Description part = go.GetPart<Description>();
		StringBuilder stringBuilder = new StringBuilder();
		part.GetLongDescription(stringBuilder);
		return new TooltipInformation
		{
			DisplayName = go.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, go.HasProperName),
			FeelingText = part.GetFeelingDescription(),
			DifficultyText = part.GetDifficultyDescription(),
			WoundLevel = Strings.WoundLevel(go),
			LongDescription = stringBuilder.ToString(),
			IconRenderable = go.RenderForUI("Look,Tooltip")
		};
	}

	public static void QueueItemTooltip(Vector3 screenPos, XRL.World.GameObject O, bool stayOpen, UnityEngine.GameObject tooltipTrigger = null)
	{
		ShowItemTooltipAsync(screenPos, O, stayOpen, tooltipTrigger);
	}

	public static async void ShowItemTooltipAsync(Vector3 screenPos, XRL.World.GameObject O, bool stayOpen, UnityEngine.GameObject tooltipTrigger = null)
	{
		TooltipTrigger tooltip = GameManager.Instance.generalTooltip;
		if (tooltip == null || O == null)
		{
			return;
		}
		string contents = await GameManager.Instance.gameQueue.executeAsync(() => GenerateTooltipContent(O));
		await The.UiContext;
		if (tooltip.IsDisplayed())
		{
			if (tooltip.remoteTrigger == tooltipTrigger)
			{
				return;
			}
			tooltip.ForceHideTooltip();
		}
		tooltip.SetText("BodyText", RTF.FormatToRTF(contents.ToString()));
		tooltip.staysOpen = stayOpen;
		tooltip.ShowManually(bForceDisplay: true, screenPos, usePosOverride: false, stayOpen);
		tooltip.remoteTrigger = tooltipTrigger;
		tooltip.onHideAction = () =>
		{
			GameManager.Instance.gameQueue.queueTask(() =>
			{
				if (XRL.World.GameObject.Validate(ref O))
				{
					O.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
					The.Player?.FireEvent(XRL.World.Event.New("LookedAt", "Object", O));
				}
			});
		};
	}

	public static async Task SetupItemTooltipAsync(XRL.World.GameObject item, TooltipTrigger trigger)
	{
		if (!XRL.World.GameObject.Validate(item))
		{
			return;
		}
		trigger.SetText("BodyText", Sidebar.FormatToRTF(await GameManager.Instance.gameQueue.executeAsync(() => GenerateTooltipContent(item))));
		trigger.onHideAction = () =>
		{
			GameManager.Instance.gameQueue.queueTask(() =>
			{
				if (XRL.World.GameObject.Validate(ref item))
				{
					item.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
					The.Player?.FireEvent(XRL.World.Event.New("LookedAt", "Object", item));
				}
			});
		};
	}

	public static void QueueLookerTooltip(int x, int y, string mode = "rightclick", int pickObject = 0)
	{
		if (The.Player == null || !The.Player.InACell())
		{
			return;
		}
		Cell cell = The.Player.CurrentZone.GetCell(x, y);
		bool flag = cell.IsSolid();
		XRL.World.GameObject O = null;
		int num = 0;
		List<XRL.World.GameObject> list = new List<XRL.World.GameObject>(cell.Objects);
		list.Sort(lookSorter);
		foreach (XRL.World.GameObject item in list)
		{
			if (item.HasPart<Description>() && (flag ? item.CanInteractInCellWithSolid(The.Player) : item.IsVisible()))
			{
				O = item;
				num++;
				if (num > pickObject)
				{
					break;
				}
			}
		}
		if (O == null)
		{
			return;
		}
		TooltipInformation contents = GenerateTooltipInformation(O);
		GameManager.Instance.uiQueue.queueTask(() =>
		{
			TooltipTrigger tooltipTrigger = ((mode == "looker") ? GameManager.Instance.lookerTooltip : GameManager.Instance.tileTooltip);
			foreach (ParameterizedTextField parameterizedTextField2 in tooltipTrigger.parameterizedTextFields)
			{
				ParameterizedTextField current2;
				ParameterizedTextField parameterizedTextField = (current2 = parameterizedTextField2);
				current2.value = RTF.FormatToRTF(Markup.Color("y", parameterizedTextField.name switch
				{
					"DisplayName" => contents.DisplayName, 
					"ConText" => contents.SubHeader, 
					"WoundLevel" => contents.WoundLevel, 
					"LongDescription" => contents.LongDescription.Trim(), 
					_ => "", 
				}), "FF", 60) ?? "";
			}
			tooltipTrigger.AdditionalData = contents.IconRenderable;
			if (mode == "looker")
			{
				if (!tooltipTrigger.transform.parent.gameObject.activeInHierarchy)
				{
					tooltipTrigger.transform.parent.gameObject.SetActive(value: true);
				}
				GameManager.Instance.SetPlayerCell(new Point2D(x, y), updateCamera: true);
				Vector3 cellCenter = GameManager.Instance.GetCellCenter(x, y);
				tooltipTrigger.ShowManually(bForceDisplay: true, Camera.main.WorldToScreenPoint(cellCenter), usePosOverride: true);
			}
			else
			{
				tooltipTrigger.ShowManually(bForceDisplay: true);
			}
			if (O != null)
			{
				tooltipTrigger.onHideAction = () =>
				{
					GameManager.Instance.gameQueue.queueSingletonTask("LookedAt" + O.GetHashCode(), () =>
					{
						O.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
						The.Player?.FireEvent(XRL.World.Event.New("LookedAt", "Object", O));
					});
				};
			}
		});
	}

	public static Cell ShowLooker(int Range, int StartX, int StartY)
	{
		PickTargetWindow.currentMode = PickTargetWindow.TargetMode.PickCells;
		GameManager.Instance.PushGameView("Looker");
		Buffer.Copy(TextConsole.CurrentBuffer);
		OldBuffer.Copy(TextConsole.CurrentBuffer);
		Cell currentCell = The.Player.CurrentCell;
		Zone zone = currentCell?.ParentZone;
		bool flag = false;
		Cell result = null;
		if (zone != null)
		{
			int num = StartX;
			int num2 = StartY;
			int num3 = 0;
			XRL.World.GameObject gameObject = null;
			IRenderable renderable = null;
			TextBlock textBlock = null;
			int num4 = 0;
			int num5 = 0;
			string s = "";
			int num6 = 3;
			int num7 = 3;
			int num8 = 0;
			bool flag2 = true;
			Cell cell = null;
			while (!flag)
			{
				XRL.World.Event.ResetPool(resetMinEventPools: false);
				XRLCore.Core.RenderMapToBuffer(Buffer);
				XRLCore.ParticleManager.Frame();
				XRLCore.ParticleManager.Render(Buffer);
				List<XRL.World.GameObject> list = XRL.World.Event.NewGameObjectList();
				List<Point> list2 = Zone.Line(currentCell.X, currentCell.Y, num, num2, ReadOnly: true);
				Cell cell2 = null;
				if (list2.Count == 0 || list2.Count == 1)
				{
					Buffer.Goto(num, num2);
					Buffer.Buffer[num, num2].imposterExtra.Add("Prefabs/Imposters/TargetReticle");
					cell2 = zone.GetCell(num, num2);
				}
				else
				{
					for (int i = 1; i < list2.Count; i++)
					{
						Buffer.Goto(list2[i].X, list2[i].Y);
						Cell cell3 = zone.GetCell(list2[i].X, list2[i].Y);
						if (i == list2.Count - 1)
						{
							_ = Buffer.CurrentChar;
							Buffer.Buffer[num, num2].imposterExtra.Add("Prefabs/Imposters/TargetReticle");
							cell2 = cell3;
						}
					}
				}
				if (cell2 != cell)
				{
					num8 = 0;
					cell = cell2;
				}
				List<XRL.World.GameObject> objectsInCell = cell2.GetObjectsInCell();
				objectsInCell.Sort(lookSorter);
				num8 = Math.Max(0, Math.Min((objectsInCell?.Count ?? 0) - 1, num8));
				XRL.World.GameObject gameObject2 = null;
				Description description = null;
				int num9 = 0;
				int num10 = 0;
				foreach (XRL.World.GameObject item in objectsInCell)
				{
					Description part = item.GetPart<Description>();
					if (part != null && item.IsVisible())
					{
						if (num9 <= num8)
						{
							gameObject2 = item;
							description = part;
							num9++;
						}
						num10++;
					}
				}
				using TextBuilder textBuilder = TextBuilder.Get();
				if (Options.ModernUI)
				{
					if (num10 > 1)
					{
						textBuilder.Append($"Look [{{{{C|{num9 - num10 + 2}}}}}/{{{{C|{objectsInCell.Count}}}}}] | ");
					}
					else
					{
						textBuilder.Append("Look | ");
					}
				}
				if (bLocked)
				{
					textBuilder.Append("{{W|" + ControlManager.getCommandInputFormatted("Cancel") + "}} | {{hotkey|(" + ControlManager.getCommandInputFormatted("CmdLockUnlock") + ")}} {{W|u}}nlock");
				}
				else
				{
					textBuilder.Append("{{W|" + ControlManager.getCommandInputFormatted("Cancel") + "}} | {{hotkey|(" + ControlManager.getCommandInputFormatted("CmdLockUnlock") + ")}} {{W|l}}ock");
				}
				if (num10 > 1)
				{
					textBuilder.Append(" | {{hotkey|" + ControlManager.getCommandInputFormatted("V Positive") + "/" + ControlManager.getCommandInputFormatted("V Negative") + "}} change selection");
				}
				if (gameObject2 != null)
				{
					textBuilder.Append(" | {{hotkey|" + ControlManager.getCommandInputFormatted("Accept") + "}} interact");
				}
				textBuilder.Append(" | {{hotkey|" + ControlManager.getCommandInputFormatted("CmdWalk") + "}} walk");
				if (Options.DebugInternals)
				{
					textBuilder.Append(" | {{hotkey|n}} show navweight");
				}
				string text = textBuilder.ToString();
				int x = ((num >= 40) ? 1 : (79 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text)));
				if (Options.ModernUI)
				{
					PickTargetWindow.currentText = textBuilder.ToString();
				}
				else if (!Options.ModernUI)
				{
					Buffer.WriteAt(x, 0, text);
					if (gameObject2 != null && description != null)
					{
						if (gameObject != gameObject2)
						{
							if (gameObject != null)
							{
								gameObject.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
								if (The.Player != null)
								{
									The.Player.FireEvent(XRL.World.Event.New("LookedAt", "Object", gameObject));
								}
							}
							num6 = num;
							num7 = num2;
							gameObject = gameObject2;
							renderable = gameObject2.RenderForUI("Look");
							int adjustFirstLine = ((renderable != null) ? (-2) : 0);
							s = Strings.WoundLevel(gameObject2);
							string displayName = gameObject2.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: false, BaseOnly: false, WithIndefiniteArticle: false, gameObject2.HasProperName);
							LookSB.Clear().Append(" {{Y|").Append(displayName)
								.Append("}} \n\n");
							description.GetLongDescription(LookSB);
							int num11 = ConsoleLib.Console.ColorUtility.LengthExceptFormatting(displayName) + 2;
							if (renderable != null)
							{
								num11 += 4;
							}
							num4 = Math.Max(35, num11);
							int num12 = ((num < 40) ? (80 - num - 3) : (num - 3));
							if (num4 > num12)
							{
								num4 = num12;
							}
							textBlock = new TextBlock(LookSB, num4, 100, ReverseBlocks: false, adjustFirstLine);
							while (textBlock.Lines.Count > 22 && num4 < num12)
							{
								num4 += Math.Max(2, (num4 - num12) / 2);
								if (num4 > num12)
								{
									num4 = num12;
								}
								textBlock = new TextBlock(LookSB, num4, 100, ReverseBlocks: false, adjustFirstLine);
							}
							num5 = Math.Min(textBlock.Lines.Count, 22);
							num6 = ((num < 40) ? (num6 + 1) : (num - num4 - 2));
							num7 = ((num7 < 12) ? (num7 + 1) : (num7 - (num5 + 2)));
							if (num7 < 0)
							{
								num7 = 0;
							}
							if (num7 + num5 > 24)
							{
								num7 -= num7 + num5 - 23;
							}
							num3 = 0;
						}
						Buffer.Fill(num6, num7, num6 + num4 + 1, num7 + num5 + 1, 32, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
						Buffer.SingleBox(num6, num7, num6 + num4 + 1, num7 + num5 + 1, ConsoleLib.Console.ColorUtility.MakeColor(TextColor.Cyan, TextColor.Black));
						for (int j = 0; j < num5 && j + num3 < textBlock.Lines.Count; j++)
						{
							if (j == 1 && num3 != 0)
							{
								Buffer.Goto(num6 + 1, num7 + j);
								Buffer.Write("<MORE - use {{W|PgUp}} to scroll up>");
							}
							else
							{
								Buffer.Goto(num6 + 1, num7 + j);
								Buffer.Write(textBlock.Lines[j + ((j != 0) ? num3 : 0)]);
							}
							if (j == 0 && num3 == 0 && renderable != null)
							{
								Buffer.Goto(num6 + num4 - 2, num7 + j);
								Buffer.Write("   ");
								Buffer.Goto(num6 + num4 - 1, num7 + j);
								Buffer.Write(renderable);
							}
						}
						LookSB.Length = 0;
						string difficultyDescription = description.GetDifficultyDescription();
						if (!string.IsNullOrEmpty(difficultyDescription))
						{
							LookSB.Append(difficultyDescription);
						}
						string feelingDescription = description.GetFeelingDescription();
						if (!string.IsNullOrEmpty(feelingDescription))
						{
							LookSB.Compound(feelingDescription, ", ");
						}
						int num13 = num7 + num5 + 1;
						if (num13 > 24)
						{
							num13 = 24;
						}
						if (LookSB.Length > 0)
						{
							Buffer.Goto(num6 + num4 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(LookSB), num13);
							Buffer.Write(LookSB);
						}
						Buffer.Goto(num6 + 1, num13);
						Buffer.Write(s);
						if (textBlock != null && num3 + num5 < textBlock.Lines.Count)
						{
							Buffer.Goto(num6 + 1, num7 + num5);
							Buffer.Write("<MORE - use {{W|PgDown}} to scroll down>");
						}
					}
					else if (gameObject != null)
					{
						gameObject.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
						if (The.Player != null)
						{
							The.Player.FireEvent(XRL.World.Event.New("LookedAt", "Object", gameObject));
						}
						gameObject = null;
					}
				}
				Buffer.focusPosition = new Point2D(num, num2);
				if (Options.ModernUI)
				{
					if (gameObject != gameObject2)
					{
						flag2 = true;
					}
					if (flag2)
					{
						GameManager.Instance.uiQueue.awaitTask(() =>
						{
							GameManager.Instance.lookerTooltip.ForceHideTooltip();
						});
						GameManager.Instance.gameQueue.executeTasks();
						if (gameObject2 != null && gameObject2.Physics != null)
						{
							QueueLookerTooltip(gameObject2.CurrentCell.X, gameObject2.CurrentCell.Y, "looker", num8);
						}
						flag2 = false;
						gameObject = gameObject2;
					}
				}
				lookingAt = gameObject;
				_TextConsole.DrawBuffer(Buffer);
				if (Keyboard.kbhit())
				{
					ScreenBuffer.ClearImposterSuppression();
					Keys keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
					if (keys == Keys.MouseEvent)
					{
						if (Keyboard.CurrentMouseEvent.Event == "RightClick")
						{
							flag = true;
						}
						if (Keyboard.CurrentMouseEvent.Event == "LeftClick")
						{
							num = Keyboard.CurrentMouseEvent.x;
							num2 = Keyboard.CurrentMouseEvent.y;
						}
						flag2 = true;
					}
					if ((keys == Keys.Space || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Interact") || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:Interact")) && gameObject2 != null)
					{
						HideTooltips();
						gameObject = null;
						if (gameObject2.Twiddle())
						{
							flag = true;
						}
					}
					if (keys == Keys.Enter)
					{
						flag = true;
					}
					if (Keyboard.IsCommandKey("V Positive"))
					{
						num8++;
						flag2 = true;
					}
					if (Keyboard.IsCommandKey("V Negative"))
					{
						num8--;
						flag2 = true;
					}
					if (Keyboard.IsCommandKey("Page Up") || keys == Keys.Prior)
					{
						if (GameManager.Instance.ModernUI)
						{
							GameManager.Instance.uiQueue.queueTask(() =>
							{
								lookerScrollRect.verticalNormalizedPosition += lookerScrollRect.viewport.rect.height / lookerScrollRect.content.rect.height / 2f;
							});
						}
						if (num3 > 0)
						{
							num3--;
						}
					}
					if (Keyboard.IsCommandKey("Page Down") || keys == Keys.Next)
					{
						if (GameManager.Instance.ModernUI)
						{
							GameManager.Instance.uiQueue.queueTask(() =>
							{
								lookerScrollRect.verticalNormalizedPosition -= lookerScrollRect.viewport.rect.height / lookerScrollRect.content.rect.height / 2f;
							});
						}
						if (textBlock != null && num3 + num5 < textBlock.Lines.Count)
						{
							num3++;
						}
					}
					if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdWalk" && TutorialManager.currentStep == null)
					{
						AutoAct.Setting = "M" + num + "," + num2;
						The.ActionManager.SkipPlayerTurn = true;
						flag = true;
					}
					if (keys == Keys.N && Options.DebugInternals)
					{
						Popup.Show(cell.X + ", " + cell.Y + ": " + cell.GetNavigationWeightFor(The.Player));
					}
					if (keys == Keys.NumPad5 || keys == Keys.Escape)
					{
						flag = true;
					}
					if (keys == Keys.U || keys == Keys.L || keys == Keys.F1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdLockUnlock"))
					{
						bLocked = !bLocked;
					}
					if (bLocked && !The.Player.OnWorldMap())
					{
						list.Clear();
						if (keys == Keys.NumPad1)
						{
							GetObjectListCone(num - 1, num2 + 1, list, "sw");
						}
						if (keys == Keys.NumPad2)
						{
							GetObjectListCone(num, num2 + 1, list, "s");
						}
						if (keys == Keys.NumPad3)
						{
							GetObjectListCone(num + 1, num2 + 1, list, "se");
						}
						if (keys == Keys.NumPad4)
						{
							GetObjectListCone(num - 1, num2, list, "w");
						}
						if (keys == Keys.NumPad6)
						{
							GetObjectListCone(num + 1, num2, list, "e");
						}
						if (keys == Keys.NumPad7)
						{
							GetObjectListCone(num - 1, num2 - 1, list, "nw");
						}
						if (keys == Keys.NumPad8)
						{
							GetObjectListCone(num, num2 - 1, list, "n");
						}
						if (keys == Keys.NumPad9)
						{
							GetObjectListCone(num + 1, num2 - 1, list, "ne");
						}
						if (list.Count > 0)
						{
							Cell currentCell2 = list[0].CurrentCell;
							num = currentCell2.X;
							num2 = currentCell2.Y;
						}
						else
						{
							if (keys == Keys.NumPad1)
							{
								num--;
								num2++;
							}
							if (keys == Keys.NumPad2)
							{
								num2++;
							}
							if (keys == Keys.NumPad3)
							{
								num++;
								num2++;
							}
							if (keys == Keys.NumPad4)
							{
								num--;
							}
							if (keys == Keys.NumPad6)
							{
								num++;
							}
							if (keys == Keys.NumPad7)
							{
								num--;
								num2--;
							}
							if (keys == Keys.NumPad8)
							{
								num2--;
							}
							if (keys == Keys.NumPad9)
							{
								num++;
								num2--;
							}
						}
					}
					else
					{
						if (keys == Keys.NumPad1)
						{
							num--;
							num2++;
						}
						if (keys == Keys.NumPad2)
						{
							num2++;
						}
						if (keys == Keys.NumPad3)
						{
							num++;
							num2++;
						}
						if (keys == Keys.NumPad4)
						{
							num--;
						}
						if (keys == Keys.NumPad6)
						{
							num++;
						}
						if (keys == Keys.NumPad7)
						{
							num--;
							num2--;
						}
						if (keys == Keys.NumPad8)
						{
							num2--;
						}
						if (keys == Keys.NumPad9)
						{
							num++;
							num2--;
						}
					}
					if (keys == Keys.F || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdFire"))
					{
						flag = true;
						result = zone.GetCell(num, num2);
					}
					if (num < 0)
					{
						num = 0;
					}
					if (num >= zone.Width)
					{
						num = zone.Width - 1;
					}
					if (num2 < 0)
					{
						num2 = 0;
					}
					if (num2 >= zone.Height)
					{
						num2 = zone.Height - 1;
					}
				}
				else
				{
					Keyboard.IdleWait();
				}
			}
			if (flag)
			{
				if (Options.ModernUI)
				{
					HideTooltips();
				}
				else if (gameObject != null)
				{
					gameObject.FireEvent(XRL.World.Event.New("AfterLookedAt", "Looker", The.Player));
					if (The.Player != null)
					{
						The.Player.FireEvent(XRL.World.Event.New("LookedAt", "Object", gameObject));
					}
				}
			}
		}
		HideTooltips();
		GameManager.Instance.PopGameView();
		_TextConsole.DrawBuffer(OldBuffer, null, bSkipIfOverlay: true);
		lookingAt = null;
		return result;
	}

	public static void HideTooltips()
	{
		if (Options.ModernUI)
		{
			GameManager.Instance.uiQueue.awaitTask(() =>
			{
				GameManager.Instance.lookerTooltip.ForceHideTooltip();
			});
			GameManager.Instance.gameQueue.executeTasks();
		}
	}
}
