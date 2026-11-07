using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Genkit;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

[UIView("PickTarget", true, false, false, "Targeting", "PickTargetFrame", false, 1, false)]
public class PickTarget : IWantsTextConsoleInit
{
	public enum PickStyle
	{
		Cone,
		Line,
		Burst,
		Circle,
		EmptyCell
	}

	public static TextConsole _TextConsole;

	public static ScreenBuffer _ScreenBuffer;

	public static ScreenBuffer OldBuffer = ScreenBuffer.create(80, 25);

	public static ScreenBuffer Buffer = ScreenBuffer.create(80, 25);

	public static bool bLocked
	{
		get
		{
			return Options.PickTargetLocked;
		}
		set
		{
			Options.PickTargetLocked = value;
		}
	}

	public void Init(TextConsole TextConsole_, ScreenBuffer ScreenBuffer_)
	{
		_TextConsole = TextConsole_;
		_ScreenBuffer = ScreenBuffer_;
	}

	public static XRL.World.GameObject GetCombatObjectAt(int X, int Y, Zone Z)
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
		if (!cell.IsExplored() || !cell.IsLit())
		{
			return null;
		}
		return cell.GetFirstObjectWithPart("Combat");
	}

	public static void GetObjectListCone(int StartX, int StartY, List<XRL.World.GameObject> ObjectList, string Direction, Predicate<XRL.World.GameObject> ObjectTest = null, Predicate<XRL.World.GameObject> ExtraVisibility = null)
	{
		Look.GetObjectListCone(StartX, StartY, ObjectList, Direction, ObjectTest, ExtraVisibility);
	}

	public static List<Cell> ShowFieldPicker(int Range, int Size, int StartX, int StartY, string What = "Wall", bool StartAdjacent = false, bool ReturnNullForAbort = false, bool AllowDiagonals = false, bool AllowDiagonalStart = true, bool RequireVisibility = false)
	{
		PickTargetWindow.currentMode = PickTargetWindow.TargetMode.PickField;
		GameManager.Instance.PushGameView("PickTarget");
		OldBuffer.Copy(TextConsole.CurrentBuffer);
		Buffer.Copy(TextConsole.CurrentBuffer);
		List<Cell> list = new List<Cell>();
		XRL.World.Parts.Physics physics = The.Player.Physics;
		int num = Range;
		bool flag = false;
		bool flag2 = true;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		if (physics != null)
		{
			Cell currentCell = physics.CurrentCell;
			int num2 = StartX;
			int num3 = StartY;
			int num4 = num2;
			int num5 = num3;
			while (!flag3)
			{
				XRL.World.Event.ResetPool(resetMinEventPools: false);
				XRLCore.Core.RenderMapToBuffer(Buffer);
				XRLCore.ParticleManager.Frame();
				XRLCore.ParticleManager.Render(Buffer);
				string text = ((flag || !StartAdjacent || num2 != StartX || num3 != StartY) ? ("{{W|space}} {{W|5}}-" + (flag ? "End " : "Start") + " " + What + " {{C|" + num + "}}") : (num.ToString() ?? "")) + " " + ((num == 1) ? "square" : "squares") + " left  {{W|Escape}}-" + (flag ? "Clear" : "Cancel");
				if (num2 != num4 || num3 != num5)
				{
					num4 = num2;
					num5 = num3;
				}
				if (Options.ModernUI)
				{
					PickTargetWindow.currentText = text;
				}
				else
				{
					if (num2 < 40)
					{
						Buffer.Goto(79 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text), 0);
					}
					else
					{
						Buffer.Goto(1, 0);
					}
					Buffer.Write(text);
				}
				foreach (Cell item in list)
				{
					Buffer.Goto(item.X, item.Y);
					if (Options.UseImposters)
					{
						Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerSelected");
					}
					else
					{
						Buffer.Write("{{G|#}}");
					}
				}
				Buffer.Goto(num2, num3);
				if (Options.UseImposters)
				{
					Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerBright");
					Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetReticle");
				}
				else if (XRLCore.CurrentFrame % 32 < 16)
				{
					Buffer.Write("{{W|X}}");
				}
				if (!flag5)
				{
					Buffer.focusPosition = new Point2D(num2, num3);
				}
				_TextConsole.DrawBuffer(Buffer);
				if (!Keyboard.kbhit())
				{
					continue;
				}
				Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
				if (keys == Keys.Escape)
				{
					if (flag)
					{
						flag = false;
						list = new List<Cell>();
						num = Range;
						if (StartAdjacent)
						{
							num2 = StartX;
							num3 = StartY;
						}
					}
					else
					{
						flag3 = true;
						if (ReturnNullForAbort)
						{
							flag4 = true;
						}
					}
				}
				if (keys == Keys.U)
				{
					bLocked = false;
				}
				if (keys == Keys.L)
				{
					bLocked = true;
				}
				int num6 = num2;
				int num7 = num3;
				if (keys == Keys.MouseEvent)
				{
					Keyboard.MouseEvent currentMouseEvent = Keyboard.CurrentMouseEvent;
					if (currentMouseEvent.Event == "PointerOver")
					{
						flag5 = true;
						if (flag2)
						{
							flag2 = false;
						}
						else
						{
							num2 = currentMouseEvent.x;
							num3 = currentMouseEvent.y;
						}
					}
					else if (currentMouseEvent.Event == "RightClick")
					{
						flag3 = true;
					}
				}
				if (keys >= Keys.NumPad1 && keys <= Keys.NumPad9)
				{
					flag5 = false;
				}
				if (keys == Keys.NumPad1)
				{
					num2--;
					num3++;
				}
				if (keys == Keys.NumPad2)
				{
					num3++;
				}
				if (keys == Keys.NumPad3)
				{
					num2++;
					num3++;
				}
				if (keys == Keys.NumPad4)
				{
					num2--;
				}
				if (keys == Keys.NumPad6)
				{
					num2++;
				}
				if (keys == Keys.NumPad7)
				{
					num2--;
					num3--;
				}
				if (keys == Keys.NumPad8)
				{
					num3--;
				}
				if (keys == Keys.NumPad9)
				{
					num2++;
					num3--;
				}
				if (num2 < 0)
				{
					num2 = 0;
				}
				if (num2 >= currentCell.ParentZone.Width)
				{
					num2 = currentCell.ParentZone.Width - 1;
				}
				if (num3 < 0)
				{
					num3 = 0;
				}
				if (num3 >= currentCell.ParentZone.Height)
				{
					num3 = currentCell.ParentZone.Height - 1;
				}
				if (flag)
				{
					if ((!AllowDiagonals && num2 != num6 && num3 != num7) || Math.Abs(num6 - num2) > 1 || Math.Abs(num7 - num3) > 1)
					{
						num2 = num6;
						num3 = num7;
					}
				}
				else if (StartAdjacent)
				{
					if (num2 > StartX + 1)
					{
						num2 = StartX + 1;
					}
					else if (num2 < StartX - 1)
					{
						num2 = StartX - 1;
					}
					if (num3 > StartY + 1)
					{
						num3 = StartY + 1;
					}
					else if (num3 < StartY - 1)
					{
						num3 = StartY - 1;
					}
					if (!AllowDiagonalStart && num2 != StartX && num3 != StartY)
					{
						if (num6 == StartX)
						{
							num2 = num6;
						}
						else if (num7 == StartY)
						{
							num3 = num7;
						}
						else
						{
							num2 = num6;
							num3 = num7;
						}
					}
				}
				if (RequireVisibility && (num2 != num6 || num3 != num7) && !currentCell.ParentZone.GetCell(num2, num3).IsVisible())
				{
					num2 = num6;
					num3 = num7;
				}
				if (keys == Keys.NumPad5 || keys == Keys.F || keys == Keys.Space || keys == Keys.Enter || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick") || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdFire") || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdTargetSelf"))
				{
					if (!flag)
					{
						if (!StartAdjacent || num2 != StartX || num3 != StartY)
						{
							Cell cell = currentCell.ParentZone.GetCell(num2, num3);
							list.Add(cell);
							num--;
							flag = true;
						}
					}
					else
					{
						flag3 = true;
					}
				}
				if (!((num2 != num6 || num3 != num7) & flag))
				{
					continue;
				}
				Cell cell2 = currentCell.ParentZone.GetCell(num2, num3);
				if (list.CleanContains(cell2))
				{
					if (list.Count > 1 && cell2 == list[list.Count - 2])
					{
						list.Remove(currentCell.ParentZone.GetCell(num6, num7));
						num++;
					}
					else
					{
						num2 = num6;
						num3 = num7;
					}
				}
				else if (num <= 0)
				{
					num2 = num6;
					num3 = num7;
				}
				else
				{
					list.Add(currentCell.ParentZone.GetCell(num2, num3));
					num--;
				}
			}
		}
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(OldBuffer);
		if ((ReturnNullForAbort & flag4) && list.Count == 0)
		{
			return null;
		}
		return list;
	}

	public static Cell ShowPicker(PickStyle Style = PickStyle.Burst, int Radius = 0, int Range = 9999, int StartX = 40, int StartY = 12, bool Locked = true, AllowVis VisLevel = AllowVis.OnlyVisible, Predicate<XRL.World.GameObject> ExtraVisibility = null, Predicate<XRL.World.GameObject> ObjectTest = null, XRL.World.GameObject UsePassability = null, Point2D? Origin = null, string Label = null, bool EnforceRange = false, bool UseTarget = true)
	{
		PickTargetWindow.currentMode = PickTargetWindow.TargetMode.PickCells;
		GameManager.Instance.PushGameView("PickTarget");
		bool flag = Locked;
		OldBuffer.Copy(TextConsole.CurrentBuffer);
		Buffer.Copy(TextConsole.CurrentBuffer);
		Cell cell = The.Player?.GetCurrentCell();
		Cell result = null;
		if (cell != null)
		{
			bool flag2 = false;
			Point2D point2D = Origin ?? cell.Pos2D;
			if (UseTarget && Sidebar.CurrentTarget != null && !The.Player.IsConfused && (ObjectTest == null || ObjectTest(Sidebar.CurrentTarget)))
			{
				Cell currentCell = Sidebar.CurrentTarget.CurrentCell;
				if (currentCell != null && The.Player.InSameZone(currentCell))
				{
					StartX = currentCell.X;
					StartY = currentCell.Y;
				}
			}
			int x = StartX;
			int y = StartY;
			bool flag3 = true;
			List<Location2D> list = new List<Location2D>();
			List<Point> list2 = new List<Point>(8);
			bool flag4 = true;
			int num = x;
			int num2 = y;
			while (!flag2)
			{
				XRL.World.Event.ResetStringbuilderPool();
				XRL.World.Event.ResetGameObjectListPool();
				XRLCore.Core.RenderMapToBuffer(Buffer);
				XRLCore.ParticleManager.Frame();
				XRLCore.ParticleManager.Render(Buffer);
				Zone.Line(point2D.x, point2D.y, x, y, list2);
				string text = "&W";
				if (x != num || y != num2)
				{
					num = x;
					num2 = y;
				}
				switch (Style)
				{
				case PickStyle.EmptyCell:
				{
					if (list2.Count == 0)
					{
						Buffer.Goto(x, y);
						if (Options.UseImposters)
						{
							Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerBright");
							Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetReticle");
						}
						else
						{
							Buffer.Write("{{W|X}}");
						}
						break;
					}
					Buffer.Goto(x, y);
					int num5 = Math.Max(Math.Abs(point2D.x - x), Math.Abs(point2D.y - y));
					Cell cell3 = cell.ParentZone.GetCell(x, y);
					if (num5 >= list2.Count - 1 && XRLCore.CurrentFrame % 32 >= 16 && !Options.UseImposters)
					{
						break;
					}
					if (num5 > Range)
					{
						if (Options.UseImposters)
						{
							Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerInvalid");
							Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetReticle");
						}
						else
						{
							Buffer.Write("&K" + list2[num5].DisplayChar);
						}
					}
					else if (((!cell3.IsVisible() || !cell3.IsLit()) && (ExtraVisibility == null || !cell3.HasObject(ExtraVisibility))) || The.Player.IsConfused)
					{
						if (Options.UseImposters)
						{
							Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerBright");
							Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetReticle");
						}
						else
						{
							Buffer.Write("&Y" + list2[num5].DisplayChar);
						}
					}
					else if (((cell3.IsVisible() && cell3.IsLit()) || (ExtraVisibility != null && cell3.HasObject(ExtraVisibility))) && (cell3.IsEmpty() || (UsePassability != null && cell3.IsPassable(UsePassability))))
					{
						if (Options.UseImposters)
						{
							Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerSelected");
							Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetReticle");
						}
						else
						{
							text = "&G";
							Buffer.Write("X");
						}
					}
					else if (Options.UseImposters)
					{
						Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerError");
						Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetReticle");
					}
					else
					{
						text = "&R";
						Buffer.Write("X");
					}
					break;
				}
				case PickStyle.Cone:
				{
					XRL.Rules.Geometry.GetCone(Location2D.Get(point2D.x, point2D.y), Location2D.Get(x, y), Range, Radius, list);
					for (int k = 0; k < list.Count; k++)
					{
						Buffer.Goto(list[k].X, list[k].Y);
						Cell cell4 = cell.ParentZone.GetCell(list[k].X, list[k].Y);
						if (k >= list.Count)
						{
							continue;
						}
						if (list[k].Distance(Location2D.Get(StartX, StartY)) > Range)
						{
							if (Options.UseImposters)
							{
								Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerInvalid");
							}
							else
							{
								Buffer.Write("&KX");
							}
						}
						else if ((!cell4.IsVisible() || !cell4.IsLit()) && (ExtraVisibility == null || !cell4.HasObject(ExtraVisibility)))
						{
							if (Options.UseImposters)
							{
								Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerInvalid");
							}
							else
							{
								Buffer.Write("&KX");
							}
						}
						else if (((cell4.IsVisible() && cell4.IsLit()) || (ExtraVisibility != null && cell4.HasObject(ExtraVisibility))) && cell4.HasVisibleCombatObject() && !The.Player.IsConfused)
						{
							if (Options.UseImposters)
							{
								Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerError");
								continue;
							}
							text = "&R";
							ConsoleChar currentChar2 = Buffer.CurrentChar;
							Color background = (currentChar2.Detail = The.Color.Red);
							currentChar2.Background = background;
							currentChar2.Char = currentChar2.BackupChar;
							currentChar2._Tile = null;
						}
						else if (Options.UseImposters)
						{
							Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerDim");
						}
						else
						{
							text = "&yX";
							Buffer.Write(text);
						}
					}
					if (Options.UseImposters)
					{
						Buffer.Goto(x, y);
						Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerBright");
						Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetReticle");
					}
					else if (XRLCore.CurrentFrame % 32 < 16)
					{
						Buffer.Goto(x, y);
						Buffer.Write("{{W|X}}");
					}
					break;
				}
				case PickStyle.Line:
				{
					if (list2.Count == 0)
					{
						Buffer.Goto(x, y);
						Buffer.Write("{{W|X}}");
						break;
					}
					bool isConfused = The.Player.IsConfused;
					int num3 = ((!Origin.HasValue) ? 1 : 0);
					int count = list2.Count;
					while (num3 < count)
					{
						Buffer.Goto(list2[num3].X, list2[num3].Y);
						Cell cell2 = cell.ParentZone.GetCell(list2[num3].X, list2[num3].Y);
						string text2 = "&y";
						Color color = The.Color.Gray;
						Color detail = The.Color.Black;
						int num4;
						if (!isConfused)
						{
							num4 = (cell2.HasVisibleCombatObject() ? 1 : 0);
							if (num4 != 0)
							{
								if (XRLCore.CurrentFrameLong < 500)
								{
									text2 = "&R";
									color = The.Color.Red;
									detail = The.Color.DarkRed;
								}
								else
								{
									text2 = "&r";
									color = The.Color.DarkRed;
									detail = The.Color.Red;
								}
								goto IL_08c4;
							}
						}
						else
						{
							num4 = 0;
						}
						if (num3 > Radius)
						{
							text2 = "&K";
							color = The.Color.Black;
							detail = The.Color.Gray;
						}
						else if ((!cell2.IsVisible() || !cell2.IsLit()) && (ExtraVisibility == null || !cell2.HasObject(ExtraVisibility)))
						{
							text2 = "&K";
							color = The.Color.Black;
							detail = The.Color.Gray;
						}
						else
						{
							text2 = "&W";
						}
						goto IL_08c4;
						IL_08c4:
						if (num4 != 0 || num3 == count - 1)
						{
							ConsoleChar currentChar = Buffer.CurrentChar;
							if (currentChar._Tile != null)
							{
								currentChar._TileForeground = (currentChar._Foreground = color);
								currentChar._Detail = detail;
							}
							else
							{
								currentChar.Background = color;
								currentChar.Char = currentChar.BackupChar;
							}
						}
						else
						{
							Buffer.Write(text2 + list2[num3].DisplayChar);
						}
						num3++;
					}
					Buffer.Buffer[list2.Last().X, list2.Last().Y].imposterExtra.Add("Prefabs/Imposters/TargetReticle");
					break;
				}
				case PickStyle.Circle:
				{
					int x4 = x - Radius;
					int x5 = x + Radius;
					int y4 = y - Radius;
					int y5 = y + Radius;
					cell.ParentZone.Constrain(ref x4, ref y4, ref x5, ref y5);
					for (int l = y4; l <= y5; l++)
					{
						for (int m = x4; m <= x5; m++)
						{
							if (!(Math.Sqrt((m - x) * (m - x) + (l - y) * (l - y)) <= (double)Radius))
							{
								continue;
							}
							Buffer.Goto(m, l);
							if (m == x && l == y)
							{
								if (Options.UseImposters)
								{
									Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerBright");
									Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetReticle");
								}
								else
								{
									Buffer.Write("{{W|X}}");
								}
							}
							else if (Options.UseImposters)
							{
								Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerDim");
							}
							else
							{
								Buffer.Write("{{w|X}}");
							}
						}
					}
					break;
				}
				case PickStyle.Burst:
				{
					int x2 = x - Radius;
					int x3 = x + Radius;
					int y2 = y - Radius;
					int y3 = y + Radius;
					cell.ParentZone.Constrain(ref x2, ref y2, ref x3, ref y3);
					bool flag5 = false;
					string s = "&WX";
					string s2 = "&wX";
					if (point2D.ManhattanDistance(new Point2D(x, y)) > Range)
					{
						flag5 = true;
						s = "&KX";
						s2 = "&KX";
					}
					for (int i = y2; i <= y3; i++)
					{
						for (int j = x2; j <= x3; j++)
						{
							Buffer.Goto(j, i);
							if (Options.UseImposters)
							{
								if (j == x && i == y)
								{
									if (flag5)
									{
										Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerInvalid");
									}
									else
									{
										Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerBright");
									}
									Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetReticle");
								}
								else if (flag5)
								{
									Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerInvalid");
								}
								else
								{
									Buffer.CurrentChar.imposterExtra.Add("Prefabs/Imposters/TargetPickerDim");
								}
							}
							else if (j == x && i == y)
							{
								Buffer.Write(s);
							}
							else
							{
								Buffer.Write(s2);
							}
						}
					}
					break;
				}
				}
				if (Options.ModernUI)
				{
					PickTargetWindow.currentText = Label + " | {{W|" + ControlManager.getCommandInputDescription("Accept") + "}}-select" + (CapabilityManager.AllowKeyboardHotkeys ? (" | unlock (" + ControlManager.getCommandInputFormatted("CmdLockUnlock") + "))") : "");
				}
				else
				{
					if (x < 40)
					{
						if (Label.IsNullOrEmpty())
						{
							Buffer.Goto(54, 0);
						}
						else
						{
							Buffer.Goto(52 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(Label), 0);
						}
					}
					else
					{
						Buffer.Goto(1, 0);
					}
					if (!Label.IsNullOrEmpty())
					{
						Buffer.Write(Label);
						Buffer.Write("  ");
					}
					if (flag)
					{
						Buffer.Write("{{W|" + ControlManager.getCommandInputDescription("Accept") + "}}-select" + (CapabilityManager.AllowKeyboardHotkeys ? (" | unlock (" + ControlManager.getCommandInputFormatted("CmdLockUnlock") + "))") : ""));
					}
					else
					{
						Buffer.Write("{{W|" + ControlManager.getCommandInputDescription("Accept") + "}}-select" + (CapabilityManager.AllowKeyboardHotkeys ? (" | lock (" + ControlManager.getCommandInputFormatted("CmdLockUnlock") + ")") : ""));
					}
				}
				if (!flag4)
				{
					Buffer.focusPosition = new Point2D(x, y);
				}
				_TextConsole.DrawBuffer(Buffer);
				if (!Keyboard.kbhit())
				{
					continue;
				}
				Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
				if (keys == Keys.MouseEvent)
				{
					if (Keyboard.CurrentMouseEvent.Event == "PointerOver" && !flag3)
					{
						flag4 = true;
						x = Keyboard.CurrentMouseEvent.x;
						y = Keyboard.CurrentMouseEvent.y;
					}
					if (Keyboard.CurrentMouseEvent.Event == "RightClick")
					{
						flag2 = true;
					}
					if (Keyboard.CurrentMouseEvent.Event == "PointerOver")
					{
						flag3 = false;
					}
				}
				if (keys == Keys.NumPad5 || keys == Keys.Escape || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdTargetSelf"))
				{
					flag2 = true;
				}
				if (keys == Keys.F1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdLockUnlock"))
				{
					flag = !flag;
				}
				if (keys >= Keys.NumPad1 && keys <= Keys.NumPad9)
				{
					flag4 = false;
				}
				if (flag && !The.Player.IsConfused)
				{
					List<XRL.World.GameObject> list3 = new List<XRL.World.GameObject>();
					if (keys == Keys.NumPad1)
					{
						GetObjectListCone(x - 1, y + 1, list3, "sw", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad2)
					{
						GetObjectListCone(x, y + 1, list3, "s", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad3)
					{
						GetObjectListCone(x + 1, y + 1, list3, "se", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad4)
					{
						GetObjectListCone(x - 1, y, list3, "w", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad6)
					{
						GetObjectListCone(x + 1, y, list3, "e", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad7)
					{
						GetObjectListCone(x - 1, y - 1, list3, "nw", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad8)
					{
						GetObjectListCone(x, y - 1, list3, "n", ObjectTest, ExtraVisibility);
					}
					if (keys == Keys.NumPad9)
					{
						GetObjectListCone(x + 1, y - 1, list3, "ne", ObjectTest, ExtraVisibility);
					}
					bool flag6 = true;
					if (list3.Count > 0)
					{
						Cell currentCell2 = list3[0].CurrentCell;
						if (Math.Abs(currentCell2.X - point2D.x) <= Range && Math.Abs(currentCell2.Y - point2D.y) <= Range)
						{
							x = currentCell2.X;
							y = currentCell2.Y;
							flag6 = false;
						}
					}
					if (flag6)
					{
						if (keys == Keys.NumPad1)
						{
							x--;
							y++;
						}
						if (keys == Keys.NumPad2)
						{
							y++;
						}
						if (keys == Keys.NumPad3)
						{
							x++;
							y++;
						}
						if (keys == Keys.NumPad4)
						{
							x--;
						}
						if (keys == Keys.NumPad6)
						{
							x++;
						}
						if (keys == Keys.NumPad7)
						{
							x--;
							y--;
						}
						if (keys == Keys.NumPad8)
						{
							y--;
						}
						if (keys == Keys.NumPad9)
						{
							x++;
							y--;
						}
					}
				}
				else
				{
					if (keys == Keys.NumPad1)
					{
						x--;
						y++;
					}
					if (keys == Keys.NumPad2)
					{
						y++;
					}
					if (keys == Keys.NumPad3)
					{
						x++;
						y++;
					}
					if (keys == Keys.NumPad4)
					{
						x--;
					}
					if (keys == Keys.NumPad6)
					{
						x++;
					}
					if (keys == Keys.NumPad7)
					{
						x--;
						y--;
					}
					if (keys == Keys.NumPad8)
					{
						y--;
					}
					if (keys == Keys.NumPad9)
					{
						x++;
						y--;
					}
				}
				if (EnforceRange)
				{
					if (x < StartX - Range)
					{
						x = StartX - Range;
					}
					else if (x > StartX + Range)
					{
						x = StartX + Range;
					}
					if (y < StartY - Range)
					{
						y = StartY - Range;
					}
					else if (y > StartY + Range)
					{
						y = StartY + Range;
					}
				}
				if (keys == Keys.F || keys == Keys.Space || keys == Keys.Enter || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdFire") || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick"))
				{
					if (point2D.ManhattanDistance(new Point2D(x, y)) > Range && Style == PickStyle.Burst)
					{
						Popup.ShowFail("You must select a location within " + Range + " tiles!");
					}
					else if (VisLevel == AllowVis.OnlyVisible && !cell.ParentZone.GetCell(x, y).IsVisible() && (ExtraVisibility == null || !cell.ParentZone.GetCell(x, y).HasObject(ExtraVisibility)))
					{
						Popup.ShowFail("You may only select a visible square!");
					}
					else if (VisLevel == AllowVis.OnlyExplored && !cell.ParentZone.GetCell(x, y).IsExplored())
					{
						Popup.ShowFail("You may only select an explored square!");
					}
					else
					{
						flag2 = true;
						result = cell.ParentZone.GetCell(x, y);
					}
				}
				cell.ParentZone.Constrain(ref x, ref y);
			}
		}
		GameManager.Instance.PopGameView(bHard: true);
		_TextConsole.DrawBuffer(OldBuffer);
		return result;
	}
}
