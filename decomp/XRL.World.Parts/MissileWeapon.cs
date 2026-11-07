using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
[UIView("FireMissileWeapon", true, false, false, "Targeting", "PickTargetFrame", false, 1, false)]
public class MissileWeapon : IPart
{
	public int AnimationDelay = 10;

	public int ShotsPerAction = 1;

	public int AmmoPerAction = 1;

	public int ShotsPerAnimation = 1;

	public int AimVarianceBonus;

	public int WeaponAccuracy;

	public int MaxRange = 999;

	public string VariableMaxRange;

	public string AmmoChar = "ù";

	public bool NoWildfire;

	public bool ShowShotsPerAction = true;

	public bool FiresManually = true;

	public string ProjectilePenetrationStat;

	public string SlotType = "Missile Weapon";

	public int EnergyCost = 1000;

	public int RangeIncrement = 3;

	public string Modifier = "Agility";

	public string Skill = "Rifle";

	[NonSerialized]
	private static Event eModifyAimVariance = new Event("ModifyAimVariance", "Amount", 0);

	[NonSerialized]
	private static Event eModifyIncomingAimVariance = new Event("ModifyIncomingAimVariance", "Amount", 0);

	[NonSerialized]
	private static DieRoll VarianceDieRoll = new DieRoll("2d20");

	[NonSerialized]
	private static bool LockActive = true;

	[NonSerialized]
	private static List<GameObject> LockObjectList = new List<GameObject>();

	[NonSerialized]
	private static List<(string, string)> MenuOptions = new List<(string, string)>();

	[NonSerialized]
	private static MissilePath PlayerMissilePath = new MissilePath();

	[NonSerialized]
	private static MissilePath CalculatedMissilePath = new MissilePath();

	[NonSerialized]
	private static bool CalculatedMissilePathInUse;

	[NonSerialized]
	private static MissilePath SecondCalculatedMissilePath = new MissilePath();

	[NonSerialized]
	private static bool SecondCalculatedMissilePathInUse;

	public static List<Pair> ListOfVisitedSquares(int x1, int y1, int x2, int y2)
	{
		List<Pair> list = new List<Pair>(Math.Abs(x2 - x1) + Math.Abs(y2 - y1));
		int num = y1;
		int num2 = x1;
		int num3 = x2 - x1;
		int num4 = y2 - y1;
		list.Add(new Pair(x1, y1));
		int num5;
		if (num4 < 0)
		{
			num5 = -1;
			num4 = -num4;
		}
		else
		{
			num5 = 1;
		}
		int num6;
		if (num3 < 0)
		{
			num6 = -1;
			num3 = -num3;
		}
		else
		{
			num6 = 1;
		}
		int num7 = 2 * num4;
		int num8 = 2 * num3;
		if (num8 >= num7)
		{
			int num10;
			int num9 = (num10 = num3);
			for (int i = 0; i < num3; i++)
			{
				num2 += num6;
				num10 += num7;
				if (num10 > num8)
				{
					num += num5;
					num10 -= num8;
					if (num10 + num9 < num8)
					{
						list.Add(new Pair(num2, num - num5));
					}
					else if (num10 + num9 > num8)
					{
						list.Add(new Pair(num2 - num6, num));
					}
				}
				list.Add(new Pair(num2, num));
				num9 = num10;
			}
		}
		else
		{
			int num10;
			int num9 = (num10 = num4);
			for (int i = 0; i < num4; i++)
			{
				num += num5;
				num10 += num8;
				if (num10 > num7)
				{
					num2 += num6;
					num10 -= num7;
					if (num10 + num9 < num7)
					{
						list.Add(new Pair(num2 - num6, num));
					}
					else if (num10 + num9 > num7)
					{
						list.Add(new Pair(num2, num - num5));
					}
				}
				list.Add(new Pair(num2, num));
				num9 = num10;
			}
		}
		return list;
	}

	public static void CalculateMissilePath(MissilePath Path, Zone Z, int X0, int Y0, int X1, int Y1, bool IncludeStart = false, bool IncludeCover = false, bool MapCalculated = false, GameObject Actor = null)
	{
		Path.Reset();
		Path.X0 = X0 * 3;
		Path.Y0 = Y0 * 3;
		Path.X1 = X1 * 3 + 1;
		Path.Y1 = Y1 * 3 + 1;
		Path.Angle = (float)Math.Atan2(X1 - X0, Y1 - Y0);
		if (IncludeCover)
		{
			if (Path.Cover == null)
			{
				Path.Cover = new List<float>();
			}
		}
		bool flag = false;
		if (!MapCalculated & IncludeCover)
		{
			Z.CalculateMissileMap(Actor ?? The.Player);
		}
		if (IncludeStart)
		{
			Path.Path.Add(Z.GetCell(X0, Y0));
			if (IncludeCover)
			{
				Path.Cover.Add(Z.GetCoverAt(X0, Y0));
			}
		}
		if (X0 == X1 && Y0 == Y1)
		{
			Path.Path.Add(Z.GetCell(X0, Y0));
			if (IncludeCover)
			{
				Path.Cover.Add(Z.GetCoverAt(X0, Y0));
			}
		}
		else
		{
			bool flag2 = Math.Abs(Y1 - Y0) > Math.Abs(X1 - X0);
			if (flag2)
			{
				int num = X0;
				X0 = Y0;
				Y0 = num;
				int num2 = X1;
				X1 = Y1;
				Y1 = num2;
			}
			if (X0 > X1)
			{
				flag = true;
				int num3 = X1;
				X1 = X0;
				X0 = num3;
				int num4 = Y1;
				Y1 = Y0;
				Y0 = num4;
			}
			double num5 = X1 - X0;
			double num6 = Math.Abs(Y1 - Y0);
			double num7 = 0.0;
			double num8 = num6 / num5;
			int num9 = 0;
			int num10 = Y0;
			num9 = ((Y0 < Y1) ? 1 : (-1));
			int num11 = 0;
			for (int i = X0; i <= X1; i++)
			{
				num11++;
				Cell cell = (flag2 ? Z.GetCell(num10, i) : Z.GetCell(i, num10));
				Path.Path.Add(cell);
				if (IncludeCover)
				{
					float cover = cell.GetCover();
					Path.Cover.Add(cover);
				}
				num7 += num8;
				if (num7 >= 0.5)
				{
					num10 += num9;
					num7--;
				}
			}
		}
		if (flag)
		{
			Path.Path.Reverse();
			Path.Cover?.Reverse();
		}
		if (IncludeCover)
		{
			float num12 = 0f;
			int j = 0;
			for (int count = Path.Cover.Count; j < count; j++)
			{
				num12 += Path.Cover[j];
				Path.Cover[j] = num12;
			}
		}
	}

	public static MissilePath CalculateMissilePath(Zone Z, int X0, int Y0, int X1, int Y1, bool IncludeStart = false, bool IncludeCover = true, bool MapCalculated = false, GameObject Actor = null)
	{
		MissilePath missilePath = new MissilePath();
		CalculateMissilePath(missilePath, Z, X0, Y0, X1, Y1, IncludeStart, IncludeCover, MapCalculated, Actor);
		return missilePath;
	}

	public static void GetObjectListCone(int StartX, int StartY, List<GameObject> ObjectList, string Direction)
	{
		Look.GetObjectListCone(StartX, StartY, ObjectList, Direction);
	}

	public static string GetRoundCooldown(int nCooldown)
	{
		int num = Math.Max((int)Math.Ceiling((double)nCooldown / 10.0), 1);
		if (num == 1)
		{
			return "({{C|1}} turn)";
		}
		return "({{C|" + num + "}} turns)";
	}

	public static MissilePath ShowPicker(int StartX, int StartY, bool Snap, AllowVis VisLevel, int Range, bool BowOrRifle, GameObject Projectile, ref FireType FireType, int MidRange = -1)
	{
		PickTargetWindow.currentMode = PickTargetWindow.TargetMode.PickCells;
		GameManager.Instance.PushGameView("FireMissileWeapon");
		GameObject gameObject = null;
		if (Snap && !The.Player.IsConfused)
		{
			gameObject = Sidebar.CurrentTarget ?? The.Player.GetNearestVisibleObject(Hostile: true, "Combat");
		}
		TextConsole textConsole = Popup._TextConsole;
		TextConsole.LoadScrapBuffers();
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		bool flag = false;
		bool flag2 = true;
		if (gameObject != null)
		{
			Cell cell = gameObject.CurrentCell;
			if (cell != null && The.Player.InSameZone(cell))
			{
				StartX = cell.X;
				StartY = cell.Y;
			}
		}
		Cell cell2 = The.Player.CurrentCell;
		if (cell2 != null)
		{
			PlayerMissilePath.Reset();
			cell2.ParentZone.CalculateMissileMap(The.Player);
			int num = StartX;
			int num2 = StartY;
			bool flag3 = true;
			while (!flag)
			{
				Event.ResetStringbuilderPool();
				Event.ResetGameObjectListPool();
				bool flag4 = false;
				bool flag5 = false;
				bool flag6 = false;
				bool flag7 = false;
				bool flag8 = false;
				bool flag9 = false;
				bool flag10 = false;
				bool flag11 = false;
				The.Core.RenderMapToBuffer(scrapBuffer);
				List<Point> list = Zone.Line(cell2.X, cell2.Y, num, num2);
				CalculateMissilePath(PlayerMissilePath, cell2.ParentZone, cell2.X, cell2.Y, num, num2, IncludeStart: false, IncludeCover: true, MapCalculated: true, The.Player);
				scrapBuffer.Goto(0, 2);
				Cell cell3 = cell2.ParentZone.GetCell(num, num2);
				if (!flag3)
				{
					scrapBuffer.focusPosition = cell3.Pos2D;
				}
				if (list.Count == 0)
				{
					scrapBuffer.Goto(num, num2);
					scrapBuffer.Buffer[num, num2].imposterExtra.Add("Prefabs/Imposters/TargetReticle");
				}
				else
				{
					bool isConfused = The.Player.IsConfused;
					int num3 = 1;
					int count = list.Count;
					while (num3 < count)
					{
						scrapBuffer.Goto(list[num3].X, list[num3].Y);
						Cell cell4 = cell2.ParentZone.GetCell(list[num3].X, list[num3].Y);
						string text = "&y";
						Color gray = The.Color.Gray;
						Color black = The.Color.Black;
						int num4;
						if (!isConfused)
						{
							num4 = (cell4.HasVisibleCombatObject() ? 1 : 0);
							if (num4 != 0)
							{
								if (XRLCore.CurrentFrameLong < 500)
								{
									text = "&R";
									gray = The.Color.Red;
									black = The.Color.DarkRed;
								}
								else
								{
									text = "&r";
									gray = The.Color.DarkRed;
									black = The.Color.Red;
								}
								goto IL_044a;
							}
						}
						else
						{
							num4 = 0;
						}
						if (num3 > Range)
						{
							text = "&K";
							gray = The.Color.Black;
							black = The.Color.Gray;
						}
						else if (MidRange >= 0 && num3 > MidRange)
						{
							text = "&W";
							gray = The.Color.Yellow;
							black = The.Color.Brown;
						}
						else if (!cell4.IsVisible() || !cell4.IsLit())
						{
							text = "&K";
							gray = The.Color.Black;
							black = The.Color.Gray;
						}
						else
						{
							float num5;
							if (isConfused)
							{
								num5 = 0f;
							}
							else if (PlayerMissilePath.Cover == null)
							{
								num5 = 0f;
							}
							else
							{
								num5 = ((num3 >= PlayerMissilePath.Cover.Count) ? PlayerMissilePath.Cover.Last() : PlayerMissilePath.Cover[num3]);
							}
							if (num5 >= 1f)
							{
								text = "&R";
								gray = The.Color.Red;
								black = The.Color.DarkRed;
							}
							else if ((double)num5 >= 0.8)
							{
								text = "&r";
								gray = The.Color.DarkRed;
								black = The.Color.Red;
							}
							else if ((double)num5 >= 0.5)
							{
								text = "&w";
								gray = The.Color.Brown;
								black = The.Color.Yellow;
							}
							else if ((double)num5 >= 0.2)
							{
								text = "&g";
								gray = The.Color.DarkGreen;
								black = The.Color.Green;
							}
							else
							{
								text = "&G";
								gray = The.Color.Green;
								black = The.Color.DarkGreen;
							}
						}
						goto IL_044a;
						IL_044a:
						if (num4 != 0 || num3 == count - 1)
						{
							ConsoleChar currentChar = scrapBuffer.CurrentChar;
							if (currentChar._Tile != null)
							{
								currentChar._TileForeground = (currentChar._Foreground = gray);
								currentChar._Detail = black;
							}
							else
							{
								currentChar.Background = gray;
								currentChar.Char = currentChar.BackupChar;
							}
						}
						else
						{
							scrapBuffer.Write(text + list[num3].DisplayChar);
						}
						num3++;
					}
					scrapBuffer.Buffer[list.Last().X, list.Last().Y].imposterExtra.Add("Prefabs/Imposters/TargetReticle");
				}
				int x = ((num >= 40) ? 1 : 43);
				scrapBuffer.Goto(x, 0);
				string text2 = "";
				text2 = ((!LockActive) ? ("{{W|" + ControlManager.getCommandInputFormatted("Accept", Options.ModernUI) + "}}-select | lock (" + ControlManager.getCommandInputFormatted("CmdLockUnlock", Options.ModernUI) + ")") : ("{{W|" + ControlManager.getCommandInputFormatted("Accept", Options.ModernUI) + "}}-select | unlock (" + ControlManager.getCommandInputFormatted("CmdLockUnlock", Options.ModernUI) + ")"));
				if (The.Player.IsConfused)
				{
					BowOrRifle = false;
				}
				bool flag12 = false;
				GameObject combatTarget = cell3.GetCombatTarget(The.Player, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, Projectile, null, null, null, null, AllowInanimate: false);
				int num6 = 1;
				MenuOptions.Clear();
				if (BowOrRifle && The.Player.HasSkill("Rifle_DrawABead"))
				{
					Rifle_DrawABead part = The.Player.GetPart<Rifle_DrawABead>();
					RifleMark rifleMark = combatTarget?.GetEffect<RifleMark>();
					scrapBuffer.Goto(x, num6);
					num6++;
					if (rifleMark != null && rifleMark.Marker.IsPlayer())
					{
						if (Options.ModernUI)
						{
							text2 = "{{G|marked target}} " + text2;
						}
						else
						{
							scrapBuffer.Write("{{G|marked target}}");
						}
						flag12 = true;
					}
					else if (part != null)
					{
						flag4 = true;
						if (Options.ModernUI)
						{
							text2 = text2 + " " + ControlManager.getCommandInputFormatted("CmdMarkTarget", Options.ModernUI) + " - mark target";
						}
						else
						{
							scrapBuffer.Write(" " + ControlManager.getCommandInputFormatted("CmdMarkTarget", Options.ModernUI) + " - mark target");
						}
						MenuOptions.Add(("mark target", "MarkTarget"));
					}
				}
				if (BowOrRifle && The.Player.HasSkill("Rifle_DrawABead") && combatTarget != null)
				{
					int num7 = 0;
					if (The.Player.HasSkill("Rifle_SuppressiveFire"))
					{
						scrapBuffer.Goto(x, num6);
						num6++;
						if (The.Player.HasSkill("Rifle_FlatteningFire"))
						{
							flag6 = Rifle_FlatteningFire.MeetsCriteria(combatTarget);
						}
						if (!flag12)
						{
							if (flag6)
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Flattening Fire (not marked)}}");
								}
								else
								{
									text2 = text2 + " {{K||" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Flattening Fire (not marked)}}";
								}
							}
							else if (!Options.ModernUI)
							{
								scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Suppressive Fire (not marked)}}");
							}
							else
							{
								text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Suppressive Fire (not marked)}}";
							}
						}
						else if (num7 <= 0)
						{
							flag5 = true;
							if (flag6)
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + "}} - {{W|Flattening Fire}}");
								}
								else
								{
									text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + "}} - {{W|Flattening Fire}}";
								}
								MenuOptions.Add(("Flattening Fire", "SupressiveFire"));
							}
							else
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + "}} - Suppressive Fire");
								}
								else
								{
									text2 = text2 + "{{W|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + "}} - Suppressive Fire";
								}
								MenuOptions.Add(("Suppressive Fire", "SupressiveFire"));
							}
						}
						else if (!Options.ModernUI)
						{
							scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Suppressive Fire " + GetRoundCooldown(num7) + "}}");
						}
						else
						{
							text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire1", Options.ModernUI) + " - Suppressive Fire " + GetRoundCooldown(num7) + "}}";
						}
					}
					if (The.Player.HasSkill("Rifle_WoundingFire"))
					{
						scrapBuffer.Goto(x, num6);
						num6++;
						if (The.Player.HasSkill("Rifle_DisorientingFire"))
						{
							flag8 = Rifle_DisorientingFire.MeetsCriteria(combatTarget);
						}
						if (!flag12)
						{
							if (flag8)
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Disorienting Fire (not marked)}}");
								}
								else
								{
									text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Disorienting Fire (not marked)}}";
								}
							}
							else if (!Options.ModernUI)
							{
								scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Wounding Fire (not marked)}}");
							}
							else
							{
								text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Wounding Fire (not marked)}}";
							}
						}
						else if (num7 <= 0)
						{
							flag7 = true;
							if (flag8)
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + "}} - {{W|Disorienting Fire}}");
								}
								else
								{
									text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + "}} - {{W|Disorienting Fire}}";
								}
								MenuOptions.Add(("Disorienting Fire", "WoundingFire"));
							}
							else
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + "}} - Wounding Fire");
								}
								else
								{
									text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + "}} - Wounding Fire";
								}
								MenuOptions.Add(("Wounding Fire", "WoundingFire"));
							}
						}
						else if (!Options.ModernUI)
						{
							scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Wounding Fire " + GetRoundCooldown(num7) + "}}");
						}
						else
						{
							text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire2", Options.ModernUI) + " - Wounding Fire " + GetRoundCooldown(num7) + "}}";
						}
					}
					if (The.Player.HasSkill("Rifle_SureFire"))
					{
						scrapBuffer.Goto(x, num6);
						num6++;
						if (The.Player.HasSkill("Rifle_BeaconFire"))
						{
							flag10 = Rifle_BeaconFire.MeetsCriteria(combatTarget);
						}
						if (!flag12)
						{
							if (flag10)
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Beacon Fire (not marked)}}");
								}
								else
								{
									text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Beacon Fire (not marked)}}";
								}
							}
							else if (!Options.ModernUI)
							{
								scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Sure Fire (not marked)}}");
							}
							else
							{
								text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Sure Fire (not marked)}}";
							}
						}
						else if (num7 <= 0)
						{
							flag9 = true;
							if (The.Player.HasSkill("Rifle_BeaconFire"))
							{
								flag10 = Rifle_BeaconFire.MeetsCriteria(combatTarget);
							}
							if (flag10)
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + "}} - {{W|Beacon Fire}}");
								}
								else
								{
									text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + "}} - {{W|Beacon Fire}}";
								}
								MenuOptions.Add(("Beacon Fire", "SureFire"));
							}
							else
							{
								if (!Options.ModernUI)
								{
									scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + "}} - Sure Fire");
								}
								else
								{
									text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + "}} - Sure Fire";
								}
								MenuOptions.Add(("Sure Fire", "SureFire"));
							}
						}
						else if (!Options.ModernUI)
						{
							scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Sure Fire " + GetRoundCooldown(num7) + "}}");
						}
						else
						{
							text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire3", Options.ModernUI) + " - Sure Fire " + GetRoundCooldown(num7) + "}}";
						}
					}
					if (The.Player.HasSkill("Rifle_OneShot"))
					{
						scrapBuffer.Goto(x, num6);
						num6++;
						Rifle_OneShot part2 = The.Player.GetPart<Rifle_OneShot>();
						if (!flag12)
						{
							if (!Options.ModernUI)
							{
								scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + " - Ultra Fire (not marked)}}");
							}
							else
							{
								text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + " - Ultra Fire (not marked)}}";
							}
						}
						else if (part2.Cooldown <= 0)
						{
							flag11 = true;
							if (!Options.ModernUI)
							{
								scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + "}} - Ultra Fire");
							}
							else
							{
								text2 = text2 + " {{W|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + "}} - Ultra Fire";
							}
							MenuOptions.Add(("Ultra Fire", "UltraFire"));
						}
						else if (!Options.ModernUI)
						{
							scrapBuffer.Write("{{K|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + " - Ultra Fire " + GetRoundCooldown(part2.Cooldown) + "}}");
						}
						else
						{
							text2 = text2 + " {{K|" + ControlManager.getCommandInputDescription("CmdAltFire4", Options.ModernUI) + " - Ultra Fire " + GetRoundCooldown(part2.Cooldown) + "}}";
						}
					}
				}
				if (MenuOptions.Count > 0)
				{
					scrapBuffer.Goto(x, num6);
					if (Options.ModernUI)
					{
						text2 = text2 + " [{{W|" + ControlManager.getCommandInputDescription("CmdMissileWeaponMenu") + "}}] Menu";
					}
					else
					{
						scrapBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("CmdMissileWeaponMenu") + "}}] Menu");
					}
				}
				if (Options.ModernUI)
				{
					PickTargetWindow.currentText = (text2.IsNullOrEmpty() ? "" : (text2 + " | ")) + "Fire Missile Weapon";
				}
				else if (!text2.IsNullOrEmpty())
				{
					scrapBuffer.WriteAt(Math.Max(80 - ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text2), 0), 0, text2);
				}
				if (!flag3)
				{
					scrapBuffer.focusPosition = new Point2D(num, num2);
				}
				textConsole.DrawBuffer(scrapBuffer);
				if (!Keyboard.kbhit())
				{
					continue;
				}
				Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
				string text3 = null;
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdMissileWeaponMenu")
				{
					int num8 = Popup.PickOption("Select Fire Mode", null, "", "Sounds/UI/ui_notification", MenuOptions.Select(((string, string) m) => m.Item1).ToArray(), null, null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
					if (num8 >= 0)
					{
						text3 = MenuOptions[num8].Item2;
					}
					else
					{
						keys = Keys.None;
					}
				}
				MenuOptions.Clear();
				if (text3 == "MarkTarget")
				{
					keys = Keys.M;
				}
				if (text3 == "SupressiveFire")
				{
					keys = Keys.D1;
				}
				if (text3 == "WoundingFire")
				{
					keys = Keys.D2;
				}
				if (text3 == "SureFire")
				{
					keys = Keys.D3;
				}
				if (text3 == "UltraFire")
				{
					keys = Keys.D4;
				}
				if (keys == Keys.MouseEvent)
				{
					if (Keyboard.CurrentMouseEvent.Event == "PointerOver" && !flag2)
					{
						num = Keyboard.CurrentMouseEvent.x;
						num2 = Keyboard.CurrentMouseEvent.y;
						flag3 = true;
					}
					if (Keyboard.CurrentMouseEvent.Event == "PointerOver")
					{
						flag2 = false;
					}
				}
				if (keys >= Keys.NumPad1 && keys <= Keys.NumPad9)
				{
					flag3 = false;
				}
				if (keys == Keys.NumPad5 || keys == Keys.Escape || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick"))
				{
					flag = true;
					GameManager.Instance.PopGameView();
					return null;
				}
				if (keys == Keys.U || keys == Keys.L || keys == Keys.F1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdLockUnlock"))
				{
					LockActive = !LockActive;
				}
				if (LockActive)
				{
					LockObjectList.Clear();
					if (!The.Player.IsConfused)
					{
						if (keys == Keys.NumPad1)
						{
							GetObjectListCone(num - 1, num2 + 1, LockObjectList, "sw");
						}
						if (keys == Keys.NumPad2)
						{
							GetObjectListCone(num, num2 + 1, LockObjectList, "s");
						}
						if (keys == Keys.NumPad3)
						{
							GetObjectListCone(num + 1, num2 + 1, LockObjectList, "se");
						}
						if (keys == Keys.NumPad4)
						{
							GetObjectListCone(num - 1, num2, LockObjectList, "w");
						}
						if (keys == Keys.NumPad6)
						{
							GetObjectListCone(num + 1, num2, LockObjectList, "e");
						}
						if (keys == Keys.NumPad7)
						{
							GetObjectListCone(num - 1, num2 - 1, LockObjectList, "nw");
						}
						if (keys == Keys.NumPad8)
						{
							GetObjectListCone(num, num2 - 1, LockObjectList, "n");
						}
						if (keys == Keys.NumPad9)
						{
							GetObjectListCone(num + 1, num2 - 1, LockObjectList, "ne");
						}
					}
					if (LockObjectList.Count > 0)
					{
						Cell cell5 = LockObjectList[0].CurrentCell;
						LockObjectList.Clear();
						if (Math.Abs(cell5.X - cell2.X) <= Range && Math.Abs(cell5.Y - cell2.Y) <= Range)
						{
							num = cell5.X;
							num2 = cell5.Y;
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
				if ((keys == Keys.Oemplus || keys == Keys.M || keys == Keys.Add || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdMarkTarget")) && flag4)
				{
					FireType = FireType.Mark;
					GameManager.Instance.PopGameView();
					return PlayerMissilePath;
				}
				if (flag5 && (keys == Keys.Oem1 || keys == Keys.D1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdAltFire1")))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowFail("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							The.Player.GetPart<Rifle_DrawABead>().ClearMark();
							FireType = FireType.SuppressingFire;
							if (flag6)
							{
								FireType = FireType.FlatteningFire;
							}
							GameManager.Instance.PopGameView();
							return PlayerMissilePath;
						}
						Popup.ShowFail("You may only select an explored square!");
					}
				}
				if (flag7 && (keys == Keys.OemQuestion || keys == Keys.D2 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdAltFire2")))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowFail("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							The.Player.GetPart<Rifle_DrawABead>().ClearMark();
							FireType = FireType.WoundingFire;
							if (flag8)
							{
								FireType = FireType.DisorientingFire;
							}
							GameManager.Instance.PopGameView();
							return PlayerMissilePath;
						}
						Popup.ShowFail("You may only select an explored square!");
					}
				}
				if (flag9 && (keys == Keys.Oemtilde || keys == Keys.D3 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdAltFire3")))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowFail("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							The.Player.GetPart<Rifle_DrawABead>().ClearMark();
							FireType = FireType.SureFire;
							if (flag10)
							{
								FireType = FireType.BeaconFire;
							}
							GameManager.Instance.PopGameView();
							return PlayerMissilePath;
						}
						Popup.ShowFail("You may only select an explored square!");
					}
				}
				if (flag11 && (keys == Keys.Oem4 || keys == Keys.D4 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdAltFire4")))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowFail("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							Rifle_OneShot part3 = The.Player.GetPart<Rifle_OneShot>();
							Event obj = Event.New("BeforeCooldownActivatedAbility", "AbilityEntry", null, "Turns", 1010, "Tags", "Agility");
							if (The.Player.FireEvent(obj) && obj.GetIntParameter("Turns") != 0 && !The.Core.cool)
							{
								int num9 = 0;
								if (The.Player.HasStat("Willpower"))
								{
									num9 = Math.Min(80, (The.Player.Stat("Willpower") - 16) * 5);
								}
								part3.Cooldown = 1000 * (100 - num9) / 100 + 10;
							}
							FireType = FireType.OneShot;
							The.Player.GetPart<Rifle_DrawABead>().ClearMark();
							GameManager.Instance.PopGameView();
							return PlayerMissilePath;
						}
						Popup.ShowFail("You may only select an explored square!");
					}
				}
				if (keys == Keys.F || keys == Keys.Space || keys == Keys.Enter || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:CmdFire") || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick"))
				{
					if (VisLevel == AllowVis.OnlyVisible && !cell2.ParentZone.GetCell(num, num2).IsVisible())
					{
						Popup.ShowFail("You may only select a visible square!");
					}
					else
					{
						if (VisLevel != AllowVis.OnlyExplored || cell2.ParentZone.GetCell(num, num2).IsExplored())
						{
							GameManager.Instance.PopGameView();
							return PlayerMissilePath;
						}
						Popup.ShowFail("You may only select an explored square!");
					}
				}
				if (num < 0)
				{
					num = 0;
				}
				if (num >= cell2.ParentZone.Width)
				{
					num = cell2.ParentZone.Width - 1;
				}
				if (num2 < 0)
				{
					num2 = 0;
				}
				if (num2 >= cell2.ParentZone.Height)
				{
					num2 = cell2.ParentZone.Height - 1;
				}
			}
		}
		GameManager.Instance.PopGameView();
		return null;
	}

	public void CheckHeavyWeaponMovementPenalty(GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = ParentObject.Equipped ?? ParentObject.Implantee;
			if (Subject == null)
			{
				return;
			}
		}
		Hampered effect = Subject.GetEffect<Hampered>();
		if (effect != null)
		{
			effect.CheckApplicable(Immediate: true);
		}
		else if (Hampered.Applicable(Subject))
		{
			Subject.ForceApplyEffect(new Hampered());
		}
	}

	private bool ExamineFailure(IExamineEvent E, int Chance)
	{
		if (E.Pass == 1 && GlobalConfig.GetBoolSetting("ContextualExamineFailures") && Chance.in100() && CheckLoadAmmoEvent.Check(ParentObject, E.Actor, ActivePartsIgnoreSubject: true))
		{
			Cell cell = ParentObject.CurrentCell ?? E.Actor?.CurrentCell;
			Cell cell2 = cell?.GetRandomLocalAdjacentCell(4);
			if (cell2 != null)
			{
				Event obj = Event.New("CommandFireMissile");
				obj.SetParameter("Actor", E.Actor);
				obj.SetParameter("StartCell", cell);
				obj.SetParameter("TargetCell", cell2);
				obj.SetFlag("IncludeStart", E.Actor.CurrentCell == cell && Chance.in100());
				obj.SetFlag("ShowEmitMessage", State: true);
				obj.SetFlag("ActivePartsIgnoreSubject", State: true);
				obj.SetFlag("UsePopups", State: true);
				if (FireEvent(obj))
				{
					E.Identify = true;
				}
			}
			return true;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterAddSkillEvent.ID && ID != PooledEvent<AfterRemoveSkillEvent>.ID && ID != EquippedEvent.ID && ID != ExamineCriticalFailureEvent.ID && ID != ExamineFailureEvent.ID && ID != PooledEvent<GenericQueryEvent>.ID && (ID != AdjustTotalWeightEvent.ID || !(Skill == "HeavyWeapons")) && (ID != SingletonEvent<GetEnergyCostEvent>.ID || !(Skill == "HeavyWeapons")) && ID != GetShortDescriptionEvent.ID && ID != QueryEquippableListEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ExamineFailureEvent E)
	{
		if (ExamineFailure(E, 25))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ExamineCriticalFailureEvent E)
	{
		if (ExamineFailure(E, 50))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterAddSkillEvent E)
	{
		if (E.Skill.Name == "HeavyWeapons_Tank")
		{
			CheckHeavyWeaponMovementPenalty();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterRemoveSkillEvent E)
	{
		if (E.Skill.Name == "HeavyWeapons_Tank")
		{
			CheckHeavyWeaponMovementPenalty();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CheckHeavyWeaponMovementPenalty(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		CheckHeavyWeaponMovementPenalty(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "PhaseHarmonicEligible" && ModPhaseHarmonic.IsProjectileCompatible(GetProjectileBlueprintEvent.GetFor(ParentObject)))
		{
			E.Result = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (E.Item == ParentObject && !E.List.Contains(E.Item) && ValidSlotType(E.SlotType))
		{
			if (!E.RequirePossible || E.SlotType == "Floating Nearby")
			{
				E.List.Add(E.Item);
			}
			else
			{
				string usesSlots = E.Item.UsesSlots;
				if (!usesSlots.IsNullOrEmpty() && (E.SlotType != "Thrown Weapon" || usesSlots.Contains("Thrown Weapon")) && (E.SlotType != "Hand" || usesSlots.Contains("Hand")))
				{
					if (E.Actor.IsGiganticCreature)
					{
						if (E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
						{
							E.List.Add(E.Item);
						}
					}
					else if (E.SlotType == "Hand" || E.SlotType == "Missile Weapon" || !E.Item.IsGiganticEquipment || !E.Item.IsNatural())
					{
						E.List.Add(E.Item);
					}
				}
				else if (!E.Actor.IsGiganticCreature || E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
				{
					int slotsRequiredFor = E.Item.GetSlotsRequiredFor(E.Actor, SlotType, FloorAtOne: false);
					if (slotsRequiredFor > 0 && slotsRequiredFor <= E.Actor.GetBodyPartCount(E.SlotType))
					{
						E.List.Add(E.Item);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.Append(GetDetailedStats());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustTotalWeightEvent E)
	{
		if (Skill == "HeavyWeapons")
		{
			GameObject gameObject = ParentObject.Equipped ?? ParentObject.InInventory;
			if (gameObject != null && gameObject.HasSkill("HeavyWeapons_StrappingShoulders"))
			{
				E.AdjustWeight(0.5);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandFireMissile");
		base.Register(Object, Registrar);
	}

	public string GetDetailedStats()
	{
		if (The.Player == null)
		{
			return "";
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("{{rules|");
		if (Skill == "Pistol")
		{
			stringBuilder.Append("\nWeapon Class: Pistol");
		}
		else if (Skill == "Rifle")
		{
			stringBuilder.Append("\nWeapon Class: Bows && Rifles");
		}
		else if (Skill == "HeavyWeapons")
		{
			stringBuilder.Append("\nWeapon Class: Heavy Weapon");
		}
		else if (!Skill.IsNullOrEmpty())
		{
			stringBuilder.Append("\nWeapon Class: ").Append(Skill);
		}
		if (WeaponAccuracy <= 0)
		{
			stringBuilder.Append("\nAccuracy: Very High");
		}
		else if (WeaponAccuracy < 5)
		{
			stringBuilder.Append("\nAccuracy: High");
		}
		else if (WeaponAccuracy < 10)
		{
			stringBuilder.Append("\nAccuracy: Medium");
		}
		else if (WeaponAccuracy < 25)
		{
			stringBuilder.Append("\nAccuracy: Low");
		}
		else
		{
			stringBuilder.Append("\nAccuracy: Very Low");
		}
		if (AmmoPerAction > 1)
		{
			stringBuilder.Append("\nMultiple ammo used per shot: " + AmmoPerAction);
		}
		if (ShotsPerAction > 1 && ShowShotsPerAction)
		{
			stringBuilder.Append("\nMultiple projectiles per shot: " + ShotsPerAction);
		}
		if (NoWildfire)
		{
			stringBuilder.Append("\nSpray fire: This item can be fired while adjacent to multiple enemies without risk of the shot going wild.");
		}
		if (Skill == "HeavyWeapons")
		{
			stringBuilder.Append("\n-25 move speed");
		}
		if (!ProjectilePenetrationStat.IsNullOrEmpty())
		{
			stringBuilder.Append("\nProjectiles fired with this weapon receive bonus penetration based on the wielder's ").Append(ProjectilePenetrationStat).Append('.');
		}
		stringBuilder.Append("}}");
		return stringBuilder.ToString();
	}

	private static int toCoord(float pos)
	{
		return (int)Math.Floor(pos / 3f);
	}

	public static List<Point> CalculateBulletTrajectory(out bool PlayerInvolved, out bool CameNearPlayer, out Cell NearPlayerCell, MissilePath Path, GameObject Projectile = null, GameObject Weapon = null, GameObject Owner = null, Zone Zone = null, string AimVariance = null, int FlatVariance = 0, int WeaponVariance = 0, bool IntendedPathOnly = false)
	{
		PlayerInvolved = false;
		CameNearPlayer = false;
		NearPlayerCell = null;
		double num = Math.Atan2((double)Path.X1 - (double)Path.X0, (double)Path.Y1 - (double)Path.Y0).normalizeRadians();
		List<Pair> list = new List<Pair>(32);
		int num2 = (int)(num * 57.32484076433121);
		Path.Angle = num2;
		int num3 = WeaponVariance + FlatVariance + ((!AimVariance.IsNullOrEmpty()) ? AimVariance.RollCached() : 0);
		if (Weapon != null && Weapon.HasRegisteredEvent("ModifyMissileWeaponAngle"))
		{
			Event obj = Event.New("ModifyMissileWeaponAngle", "Attacker", Owner, "Projectile", Projectile, "Angle", num, "Mod", num3);
			Weapon.FireEvent(obj);
			num = (double)obj.GetParameter("Angle");
			num3 = obj.GetIntParameter("Mod");
		}
		num += (double)num3 * 0.0174532925;
		double num4 = Math.Sin(num);
		double num5 = Math.Cos(num);
		double num6 = Path.X0;
		double num7 = Path.Y0;
		while (Math.Floor(num6) >= 0.0 && Math.Floor(num6) <= 237.0 && Math.Floor(num7) >= 0.0 && Math.Floor(num7) <= 72.0)
		{
			num6 += num4;
			num7 += num5;
		}
		list.AddRange(ListOfVisitedSquares((int)Path.X0, (int)Path.Y0, (int)num6, (int)num7));
		if (Zone != null && Projectile != null && !IntendedPathOnly)
		{
			Cell cell = null;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				int x = toCoord(list[i].x);
				int y = toCoord(list[i].y);
				Cell cell2 = Zone.GetCell(x, y);
				if (cell2 == null || cell2 == cell)
				{
					continue;
				}
				cell = cell2;
				if (i == 0 || ((!cell2.HasObjectWithRegisteredEvent("RefractLight") || !Projectile.HasTagOrProperty("Light")) && !cell2.HasObjectWithRegisteredEvent("ReflectProjectile")))
				{
					continue;
				}
				bool flag = true;
				GameObject Object = null;
				string clip = null;
				int num8 = -1;
				string verb = null;
				if (cell2.HasObjectWithRegisteredEvent("RefractLight") && Projectile.HasTagOrProperty("Light"))
				{
					Event obj2 = Event.New("RefractLight");
					obj2.SetParameter("Projectile", Projectile);
					obj2.SetParameter("Attacker", Owner);
					obj2.SetParameter("Cell", cell2);
					obj2.SetParameter("Angle", Path.Angle);
					obj2.SetParameter("Direction", Stat.Random(0, 359));
					obj2.SetParameter("Verb", null);
					obj2.SetParameter("Sound", "sfx_light_refract");
					obj2.SetParameter("By", (object)null);
					flag = cell2.FireEvent(obj2);
					if (!flag)
					{
						Object = obj2.GetGameObjectParameter("By");
						clip = obj2.GetParameter<string>("Sound");
						verb = obj2.GetStringParameter("Verb") ?? "refract";
						num8 = obj2.GetIntParameter("Direction").normalizeDegrees();
					}
				}
				if (flag && cell2.HasObjectWithRegisteredEvent("ReflectProjectile"))
				{
					Event obj3 = Event.New("ReflectProjectile");
					obj3.SetParameter("Projectile", Projectile);
					obj3.SetParameter("Attacker", Owner);
					obj3.SetParameter("Cell", cell2);
					obj3.SetParameter("Angle", Path.Angle);
					obj3.SetParameter("Direction", Stat.Random(0, 359));
					obj3.SetParameter("Verb", null);
					obj3.SetParameter("Sound", "sfx_light_refract");
					obj3.SetParameter("By", (object)null);
					flag = cell2.FireEvent(obj3);
					if (!flag)
					{
						Object = obj3.GetGameObjectParameter("By");
						clip = obj3.GetStringParameter("Sound");
						verb = obj3.GetStringParameter("Verb") ?? "reflect";
						num8 = obj3.GetIntParameter("Direction").normalizeDegrees();
					}
				}
				if (flag || !GameObject.Validate(ref Object))
				{
					continue;
				}
				if (Object.IsPlayer())
				{
					PlayerInvolved = true;
				}
				else
				{
					GameObject objectContext = Object.GetObjectContext();
					if (objectContext != null && objectContext.IsPlayer())
					{
						PlayerInvolved = true;
					}
				}
				Object?.Physics?.PlayWorldSound(clip, 0.5f, 0f, Combat: true);
				IComponent<GameObject>.XDidYToZ(Object, verb, Projectile, null, "!", null, null, Object);
				float num9 = list[i].x;
				float num10 = list[i].y;
				float num11 = num9;
				float num12 = num10;
				float num13 = (float)Math.Sin((float)num8 * (MathF.PI / 180f));
				float num14 = (float)Math.Cos((float)num8 * (MathF.PI / 180f));
				list.RemoveRange(i, list.Count - i);
				count = list.Count;
				Cell cell3 = cell2;
				do
				{
					num11 += num13;
					num12 += num14;
					Cell cell4 = Zone.GetCell(toCoord(num11), toCoord(num12));
					if (cell4 == null)
					{
						break;
					}
					if (cell4 == cell2)
					{
						continue;
					}
					list.Add(new Pair((int)num11, (int)num12));
					if (cell4 != cell3)
					{
						if (cell4.GetCombatTarget(Owner, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Projectile, null, null, null, null, AllowInanimate: false) != null || cell4.HasSolidObjectForMissile(Owner, Projectile))
						{
							break;
						}
						cell3 = cell4;
					}
				}
				while (num11 > 0f && num11 < 237f && num12 > 0f && num12 < 72f);
			}
		}
		List<Point> list2 = new List<Point>(list.Count / 2);
		int num15 = int.MinValue;
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			Pair pair = list[j];
			int num16 = toCoord(pair.x) + toCoord(pair.y) * 1000;
			if (num16 != num15)
			{
				list2.Add(new Point(toCoord(pair.x), toCoord(pair.y)));
				num15 = num16;
			}
		}
		if (The.Player != null && Zone != null && list2.Count > 0)
		{
			Cell cell5 = The.Player.GetCurrentCell();
			if (cell5 != null)
			{
				Cell cell6 = Zone.GetCell(list2[0]);
				if (cell6 != null && cell6.PathDistanceTo(cell5) >= 2)
				{
					bool flag2 = false;
					int k = 1;
					for (int count3 = list2.Count; k < count3; k++)
					{
						Cell cell7 = Zone.GetCell(list2[k]);
						if (cell7 == cell5)
						{
							break;
						}
						if (flag2)
						{
							if (cell7.PathDistanceTo(cell5) >= 2)
							{
								CameNearPlayer = true;
								break;
							}
						}
						else if (cell7.PathDistanceTo(cell5) <= 1)
						{
							flag2 = true;
							NearPlayerCell = cell7;
						}
					}
				}
			}
		}
		return list2;
	}

	public static List<Point> CalculateBulletTrajectory(MissilePath Path, GameObject projectile = null, GameObject weapon = null, GameObject owner = null, Zone zone = null, string AimVariance = null, int FlatVariance = 0, int WeaponVariance = 0, bool IntendedPathOnly = false)
	{
		bool PlayerInvolved;
		bool CameNearPlayer;
		Cell NearPlayerCell;
		return CalculateBulletTrajectory(out PlayerInvolved, out CameNearPlayer, out NearPlayerCell, Path, projectile, weapon, owner, zone, AimVariance, FlatVariance, WeaponVariance, IntendedPathOnly);
	}

	public string GetSlotType(bool Cybernetic = false)
	{
		string text = ((!Cybernetic) ? ParentObject.UsesSlots : ParentObject.GetPart<CyberneticsBaseItem>()?.Slots);
		if (text.IsNullOrEmpty())
		{
			text = SlotType;
		}
		if (text.IndexOf(',') != -1)
		{
			return text.CachedCommaExpansion()[0];
		}
		return text;
	}

	public bool ValidSlotType(string Type, bool Cybernetic = false)
	{
		string text = ((!Cybernetic) ? ParentObject.UsesSlots : ParentObject.GetPart<CyberneticsBaseItem>()?.Slots);
		if (text.IsNullOrEmpty())
		{
			text = SlotType;
		}
		if (text.IndexOf(',') != -1)
		{
			List<string> list = text.CachedCommaExpansion();
			if (!list.Contains(Type))
			{
				return list.Contains("*");
			}
			return true;
		}
		if (!(text == Type))
		{
			return text == "*";
		}
		return true;
	}

	public static void SetupProjectile(GameObject Projectile, GameObject Attacker, GameObject Launcher = null, Projectile pProjectile = null)
	{
		Projectile.SetIntProperty("Primed", 1);
		if (pProjectile != null)
		{
			pProjectile.Launcher = Launcher;
		}
		if (Attacker.HasEffect<Phased>() && !Projectile.HasTagOrProperty("IndependentPhaseProjectile") && Projectile.FireEvent("CanApplyPhased") && Projectile.ForceApplyEffect(new Phased(9999)))
		{
			Projectile.ModIntProperty("ProjectilePhaseAdded", 1);
		}
		if (Attacker.HasEffect<Omniphase>() && !Projectile.HasTagOrProperty("IndependentOmniphaseProjectile") && Projectile.FireEvent("CanApplyOmniphase") && Projectile.ForceApplyEffect(new Omniphase(9999)))
		{
			Projectile.ModIntProperty("ProjectileOmniphaseAdded", 1);
		}
		if (Launcher != null && Launcher.HasRegisteredEvent("ProjectileSetup"))
		{
			Launcher.FireEvent(Event.New("ProjectileSetup", "Attacker", Attacker, "Launcher", Launcher, "Projectile", Projectile));
		}
	}

	public static void CleanupProjectile(GameObject Projectile)
	{
		if (!GameObject.Validate(ref Projectile))
		{
			return;
		}
		if (Projectile.Physics.IsReal)
		{
			if (Projectile.GetIntProperty("ProjectilePhaseAdded") > 0)
			{
				Projectile.RemoveEffect<Phased>();
				Projectile.ModIntProperty("ProjectilePhaseAdded", -1, RemoveIfZero: true);
			}
			if (Projectile.GetIntProperty("ProjectileOmniphaseAdded") > 0)
			{
				Projectile.RemoveEffect<Omniphase>();
				Projectile.ModIntProperty("ProjectileOmniphaseAdded", -1, RemoveIfZero: true);
			}
		}
		else
		{
			Projectile.Obliterate();
		}
	}

	private void MissileHit(GameObject Attacker, GameObject Defender, GameObject Owner, GameObject Projectile, Projectile pProjectile, GameObject AimedAt, GameObject ApparentTarget, MissilePath MPath, Cell ImpactCell, FireType FireType, int AimLevel, int NaturalHitResult, int HitResult, bool PathInvolvesPlayer, GameObject MessageAsFrom, bool UsePopups, ref bool Done, ref bool PenetrateCreatures, ref bool PenetrateWalls, bool TargetWasInitiallyUnset, bool ShowUninvolved)
	{
		try
		{
			bool flag = false;
			if (!DefenderMissileHitEvent.Check(ParentObject, Attacker, Defender, Owner, Projectile, pProjectile, AimedAt, ApparentTarget, MPath, FireType, AimLevel, NaturalHitResult, HitResult, PathInvolvesPlayer, MessageAsFrom, ref Done, ref PenetrateCreatures, ref PenetrateWalls))
			{
				return;
			}
			bool flag2 = Defender != ApparentTarget;
			string text = null;
			if (MessageAsFrom != null && MessageAsFrom != Owner)
			{
				if (MessageAsFrom.IsPlayer())
				{
					text = "You";
				}
				else if (MessageAsFrom.HasProperName)
				{
					text = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(MessageAsFrom.ShortDisplayName);
				}
				else if (MessageAsFrom.Equipped == Owner)
				{
					text = Owner.Poss(MessageAsFrom);
				}
				else
				{
					text = ((MessageAsFrom.Equipped == null) ? ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(MessageAsFrom.T()) : ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(MessageAsFrom.Equipped.T()) + " " + MessageAsFrom.ShortDisplayName));
				}
			}
			if (Defender == AimedAt && (FireType == FireType.BeaconFire || (FireType == FireType.OneShot && Owner.HasSkill("Rifle_BeaconFire") && Rifle_BeaconFire.MeetsCriteria(Defender))))
			{
				if (Owner.IsPlayer())
				{
					if (text != null)
					{
						EmitMessage(text + MessageAsFrom.GetVerb("hit") + " " + ((Defender == MessageAsFrom) ? MessageAsFrom.itself : Defender.t()) + " in a vital area.", ' ', FromDialog: false, UsePopups);
					}
					else
					{
						EmitMessage("You hit " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " in a vital area.", ' ', FromDialog: false, UsePopups);
					}
				}
				Defender.BloodsplatterCone(SelfSplatter: true, MPath.Angle, 45);
				flag = true;
			}
			if (!flag)
			{
				int num = GetCriticalThresholdEvent.GetFor(Attacker, Defender, ParentObject, Projectile, Skill);
				int num2 = GetSpecialEffectChanceEvent.GetFor(Attacker, ParentObject, "Missile Critical", 5, Defender, Projectile);
				if (num2 != 5)
				{
					num -= (num2 - 5) / 5;
				}
				if (NaturalHitResult >= num)
				{
					flag = true;
				}
			}
			int num3 = pProjectile.BasePenetration;
			int num4 = pProjectile.BasePenetration + pProjectile.StrengthPenetration;
			if (flag)
			{
				BaseSkill genericSkill = Skills.GetGenericSkill(Skill, Attacker);
				if (genericSkill != null)
				{
					int weaponCriticalModifier = genericSkill.GetWeaponCriticalModifier(Attacker, Defender, ParentObject);
					if (weaponCriticalModifier != 0)
					{
						num3 += weaponCriticalModifier;
						num4 += weaponCriticalModifier;
					}
				}
			}
			if (!ProjectilePenetrationStat.IsNullOrEmpty() && Attacker != null)
			{
				num3 += Attacker.StatMod(ProjectilePenetrationStat);
			}
			Event obj = Event.New("WeaponMissileWeaponHit");
			obj.SetParameter("Attacker", Attacker);
			obj.SetParameter("Defender", Defender);
			obj.SetParameter("Weapon", ParentObject);
			obj.SetParameter("Penetrations", num3);
			obj.SetParameter("PenetrationCap", num4);
			obj.SetParameter("MessageAsFrom", MessageAsFrom);
			obj.SetFlag("Critical", flag);
			ParentObject.FireEvent(obj);
			num3 = obj.GetIntParameter("Penetrations");
			num4 = obj.GetIntParameter("PenetrationCap");
			flag = obj.HasFlag("Critical");
			obj.ID = "AttackerMissileWeaponHit";
			Attacker?.FireEvent(obj);
			obj.ID = "DefenderMissileWeaponHit";
			Defender?.FireEvent(obj);
			if (flag)
			{
				obj.ID = "MissileAttackerCriticalHit";
				Attacker.FireEvent(obj);
			}
			bool defenderIsCreature = Defender.HasTag("Creature");
			string blueprint = Defender.Blueprint;
			WeaponUsageTracking.TrackMissileWeaponHit(Owner, ParentObject, Projectile, defenderIsCreature, blueprint, flag2);
			GetMissileWeaponPerformanceEvent getMissileWeaponPerformanceEvent = GetMissileWeaponPerformanceEvent.GetFor(Owner, ParentObject, Projectile, num3, num4, pProjectile.BaseDamage, null, null, pProjectile.PenetrateCreatures, pProjectile.PenetrateWalls, pProjectile.Quiet, null, null, Active: true);
			if (getMissileWeaponPerformanceEvent.PenetrateCreatures)
			{
				PenetrateCreatures = true;
			}
			if (getMissileWeaponPerformanceEvent.PenetrateWalls)
			{
				PenetrateWalls = true;
			}
			Damage damage = new Damage(0);
			damage.AddAttributes(getMissileWeaponPerformanceEvent.Attributes);
			int num5 = 0;
			if (getMissileWeaponPerformanceEvent.Attributes.Contains("Mental"))
			{
				if (Defender.Brain == null && (Defender.IsCreature ? getMissileWeaponPerformanceEvent.PenetrateCreatures : PenetrateWalls))
				{
					return;
				}
				num5 = Stats.GetCombatMA(Defender);
			}
			else
			{
				num5 = Stats.GetCombatAV(Defender);
			}
			int num6 = 0;
			num6 = (getMissileWeaponPerformanceEvent.Attributes.Contains("NonPenetrating") ? 1 : ((!getMissileWeaponPerformanceEvent.Attributes.Contains("Vorpal")) ? getMissileWeaponPerformanceEvent.RollDamagePenetrations(num5) : (Stat.RollDamagePenetrations(0, 0, 0) + getMissileWeaponPerformanceEvent.PenetrationBonus)));
			string OutcomeMessageFragment = null;
			MissilePenetrateEvent.Process(ParentObject, Attacker, Defender, Owner, Projectile, pProjectile, AimedAt, ApparentTarget, MPath, FireType, AimLevel, NaturalHitResult, PathInvolvesPlayer, MessageAsFrom, ref num6, ref OutcomeMessageFragment);
			if (Skill == "Pistol" && Owner.HasSkill("Pistol_DisarmingShot") && Owner.StatMod("Agility").in100())
			{
				Disarming.Disarm(Defender, Attacker, 100, "Strength", "Agility", null, ParentObject);
			}
			if (num6 == 0)
			{
				Defender.ParticleBlip("&K\a", 10, 0L);
				if (Owner.IsPlayer())
				{
					if (text != null)
					{
						EmitMessage(text + MessageAsFrom.GetVerb("fail") + " to penetrate " + Defender.poss("armor") + " with " + MessageAsFrom.its_(Projectile) + OutcomeMessageFragment + "!", 'r', FromDialog: false, UsePopups);
					}
					else
					{
						EmitMessage(Owner.Poss(Projectile) + Projectile.GetVerb("fail") + " to penetrate " + Defender.poss("armor") + OutcomeMessageFragment + "!", 'r', FromDialog: false, UsePopups);
					}
				}
				else if (Defender.IsPlayer())
				{
					if (text != null)
					{
						EmitMessage(text + MessageAsFrom.GetVerb("fail") + " to penetrate your armor with " + MessageAsFrom.its_(Projectile) + OutcomeMessageFragment + "!", 'g', FromDialog: false, UsePopups);
					}
					else
					{
						EmitMessage(Owner.Poss(Projectile) + Projectile.GetVerb("fail") + " to penetrate your armor" + OutcomeMessageFragment + "!", 'g', FromDialog: false, UsePopups);
					}
				}
				Done = true;
				if (Projectile.IsValid())
				{
					ImpactCell?.AddObject(Projectile, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit");
				}
				Event obj2 = Event.New("ProjectileHit");
				obj2.SetParameter("Attacker", Attacker);
				obj2.SetParameter("Defender", Defender);
				obj2.SetParameter("Skill", Skill);
				obj2.SetParameter("Damage", damage);
				obj2.SetParameter("AimLevel", AimLevel);
				obj2.SetParameter("Owner", Attacker);
				obj2.SetParameter("Launcher", ParentObject);
				obj2.SetParameter("Projectile", Projectile);
				obj2.SetParameter("Path", MPath);
				obj2.SetParameter("Penetrations", 0);
				obj2.SetParameter("ApparentTarget", ApparentTarget);
				obj2.SetParameter("AimedAt", AimedAt);
				obj2.SetParameter("ImpactCell", ImpactCell);
				obj2.SetFlag("Critical", flag);
				Projectile.FireEvent(obj2);
				obj2.ID = "DefenderProjectileHit";
				Defender.FireEvent(obj2);
				obj2.ID = "LauncherProjectileHit";
				ParentObject.FireEvent(obj2);
				return;
			}
			if (Defender == AimedAt && Defender.IsCombatObject())
			{
				if (FireType == FireType.SuppressingFire || FireType == FireType.FlatteningFire || (FireType == FireType.OneShot && Attacker.HasSkill("Rifle_SuppressiveFire")))
				{
					if (Defender.ApplyEffect(new Suppressed(Stat.Random(3, 5))))
					{
						if (text != null)
						{
							IComponent<GameObject>.EmitMessage(MessageAsFrom, Grammar.MakePossessive(text) + " suppressive fire locks " + Defender.t() + " in place.");
						}
						else
						{
							IComponent<GameObject>.EmitMessage(Attacker, Attacker.Poss("suppressive fire locks ") + Defender.t() + " in place.");
						}
					}
					if (Attacker.HasSkill("Rifle_FlatteningFire") && Rifle_FlatteningFire.MeetsCriteria(Defender))
					{
						if (Defender.ApplyEffect(new Prone()))
						{
							if (text != null)
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, Grammar.MakePossessive(text) + " flattening fire drops " + Defender.t() + " to the ground!");
							}
							else
							{
								IComponent<GameObject>.EmitMessage(Attacker, Attacker.Poss("flattening fire drops ") + Defender.t() + " to the ground!");
							}
						}
						Disarming.Disarm(Defender, Attacker, 100, "Strength", "Agility", null, ParentObject);
					}
				}
				if (FireType == FireType.WoundingFire || FireType == FireType.DisorientingFire || (FireType == FireType.OneShot && Attacker.HasSkill("Rifle_WoundingFire")))
				{
					string text2 = (Attacker.IsPlayer() ? "You" : Attacker.T());
					if (Defender.ApplyEffect(new Bleeding(num6.ToString(), 20 + getMissileWeaponPerformanceEvent.BaseDamage.RollMaxCached(), Attacker, Stack: false)))
					{
						if (text != null)
						{
							IComponent<GameObject>.EmitMessage(MessageAsFrom, text + MessageAsFrom.GetVerb("wound") + " " + Defender.t() + ".");
						}
						else
						{
							IComponent<GameObject>.EmitMessage(Attacker, text2 + Attacker.GetVerb("wound") + " " + Defender.t() + ".");
						}
						Defender.BloodsplatterCone(SelfSplatter: true, MPath.Angle, 45);
					}
					if (Attacker.HasSkill("Rifle_DisorientingFire") && Rifle_DisorientingFire.MeetsCriteria(Defender) && Defender.ApplyEffect(new Disoriented(Stat.Random(5, 7), 4)))
					{
						if (text != null)
						{
							IComponent<GameObject>.EmitMessage(MessageAsFrom, text + MessageAsFrom.GetVerb("disorient") + " " + Defender.t() + ".");
						}
						else
						{
							IComponent<GameObject>.EmitMessage(Attacker, text2 + Attacker.GetVerb("disorient") + " " + Defender.t() + ".");
						}
					}
				}
			}
			if (Options.ShowMonsterHPHearts)
			{
				Defender.ParticleBlip(Stat.GetResultColor(num6) + "\u0003", 10, 0L);
			}
			bool flag3 = getMissileWeaponPerformanceEvent.BaseDamage != "0";
			if (getMissileWeaponPerformanceEvent.Attributes.Contains("Mental") && Defender.Brain == null)
			{
				flag3 = false;
				if (Attacker.IsPlayer())
				{
					OutcomeMessageFragment = ", but your mental attack has no effect" + (OutcomeMessageFragment.IsNullOrEmpty() ? "" : OutcomeMessageFragment);
				}
			}
			string adverb = (flag ? "critically" : null);
			string text3 = (flag ? " critically" : "");
			if (flag3)
			{
				DieRoll possiblyCachedDamageRoll = getMissileWeaponPerformanceEvent.GetPossiblyCachedDamageRoll();
				int num7 = 0;
				for (int i = 0; i < num6; i++)
				{
					num7 += possiblyCachedDamageRoll.Resolve();
				}
				damage.Amount = num7;
				if (flag2)
				{
					damage.AddAttribute("Accidental");
				}
				int phase = Projectile.GetPhase();
				if ((damage.Amount > 0) & flag)
				{
					Defender.ParticleText("*critical hit*", IComponent<GameObject>.ConsequentialColorChar(null, Defender));
					Defender.PlayWorldSound("Sounds/Damage/sfx_damage_critical", 0.5f, 0f, Combat: true);
				}
				if (damage.Amount > 0)
				{
					if (num6 < 2)
					{
						Defender?.PlayWorldSound("Sounds/Damage/sfx_damage_penetrate_low");
					}
					else if (num6 < 4)
					{
						Defender?.PlayWorldSound("Sounds/Damage/sfx_damage_penetrate_med");
					}
					else
					{
						Defender?.PlayWorldSound("Sounds/Damage/sfx_damage_penetrate_high");
					}
					Event obj3 = Event.New("DealingMissileDamage");
					obj3.SetParameter("Attacker", Attacker);
					obj3.SetParameter("Defender", Defender);
					obj3.SetParameter("Skill", Skill);
					obj3.SetParameter("Damage", damage);
					obj3.SetParameter("AimLevel", AimLevel);
					obj3.SetParameter("Phase", phase);
					obj3.SetFlag("Critical", flag);
					if (!Attacker.FireEvent(obj3))
					{
						damage.Amount = 0;
					}
					if (obj3.HasFlag("RecheckPhase"))
					{
						phase = Projectile.GetPhase();
					}
				}
				if (damage.Amount > 0)
				{
					Event obj4 = Event.New("WeaponDealingMissileDamage");
					obj4.SetParameter("Attacker", Attacker);
					obj4.SetParameter("Defender", Defender);
					obj4.SetParameter("Skill", Skill);
					obj4.SetParameter("Damage", damage);
					obj4.SetParameter("AimLevel", AimLevel);
					obj4.SetParameter("Phase", phase);
					obj4.SetFlag("Critical", flag);
					if (!ParentObject.FireEvent(obj4))
					{
						damage.Amount = 0;
					}
					if (obj4.HasFlag("RecheckPhase"))
					{
						phase = Projectile.GetPhase();
					}
				}
				bool flag4 = false;
				if (damage.Amount > 0)
				{
					Defender.WillCheckHP(true);
					flag4 = true;
					Event obj5 = Event.New("TakeDamage");
					obj5.SetParameter("Damage", damage);
					obj5.SetParameter("Owner", Attacker);
					obj5.SetParameter("Attacker", Attacker);
					obj5.SetParameter("Weapon", ParentObject);
					obj5.SetParameter("Projectile", Projectile);
					obj5.SetParameter("Phase", phase);
					obj5.SetParameter("OutcomeMessageFragment", OutcomeMessageFragment);
					obj5.SetFlag("WillUseOutcomeMessageFragment", State: true);
					if (ParentObject.HasTagOrProperty("NoMissileSetTarget"))
					{
						obj5.SetFlag("NoSetTarget", State: true);
					}
					if (!Defender.FireEvent(obj5))
					{
						damage.Amount = 0;
					}
					OutcomeMessageFragment = obj5.GetStringParameter("OutcomeMessageFragment");
				}
				WeaponUsageTracking.TrackMissileWeaponDamage(Owner, ParentObject, Projectile, defenderIsCreature, blueprint, flag2, damage);
				if (damage.Amount > 0)
				{
					if (Options.ShowMonsterHPHearts)
					{
						Defender.ParticleBlip(Defender.GetHPColor() + "\u0003", 10, 0L);
					}
				}
				else if (flag4)
				{
					Defender.WillCheckHP(false);
					flag4 = false;
				}
				if (Owner.IsPlayer())
				{
					if (OutcomeMessageFragment != null)
					{
						if (Defender.IsVisible())
						{
							if (text != null)
							{
								if (MessageAsFrom.IsVisible())
								{
									IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
								else
								{
									IComponent<GameObject>.EmitMessage(Owner, "Something hits " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
							}
							else
							{
								IComponent<GameObject>.EmitMessage(Owner, "You" + text3 + " hit " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
					}
					else if (damage.Amount > 0 || !damage.SuppressionMessageDone)
					{
						if (text != null)
						{
							if (Defender.IsVisible())
							{
								if (MessageAsFrom.IsVisible())
								{
									IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") with " + Projectile.an() + " for " + damage.Amount + " damage!", Stat.GetResultColorChar(num6), FromDialog: false, UsePopups);
								}
								else
								{
									IComponent<GameObject>.EmitMessage(Owner, "Something hits " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") with " + Projectile.an() + " for " + damage.Amount + " damage!", Stat.GetResultColorChar(num6), FromDialog: false, UsePopups);
								}
							}
							else if (Defender.IsAudible(The.Player, 80))
							{
								IComponent<GameObject>.EmitMessage(Owner, text + MessageAsFrom.GetVerb("hit") + " something " + Owner.DescribeDirectionToward(Defender) + "!", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (Defender.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, "You" + text3 + " hit " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") with " + Projectile.an() + " for " + damage.Amount + " damage!", Stat.GetResultColorChar(num6), FromDialog: false, UsePopups);
						}
						else if (Defender.IsAudible(The.Player, 80))
						{
							IComponent<GameObject>.EmitMessage(Owner, "You hit something " + Owner.DescribeDirectionToward(Defender) + "!", ' ', FromDialog: false, UsePopups);
						}
					}
				}
				else if (Defender.IsPlayer())
				{
					if (OutcomeMessageFragment != null)
					{
						if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " you with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(Owner, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " you" + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (Owner.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb) + " you with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " you " + Defender.DescribeDirectionFrom(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
						}
					}
					else if (text != null)
					{
						if (MessageAsFrom.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " you (x" + num6 + ") with " + Projectile.an() + " for " + damage.Amount + " damage!", 'r', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " you (x" + num6 + ") " + Defender.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", 'r', FromDialog: false, UsePopups);
						}
					}
					else if (Owner.IsVisible())
					{
						IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb) + " you (x" + num6 + ") with " + Projectile.an() + " for " + damage.Amount + " damage!", 'r', FromDialog: false, UsePopups);
					}
					else
					{
						IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " you (x" + num6 + ") " + Defender.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", 'r', FromDialog: false, UsePopups);
					}
				}
				else if ((PathInvolvesPlayer | ShowUninvolved) && Defender.IsVisible())
				{
					if (OutcomeMessageFragment != null)
					{
						if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " " + The.Player.DescribeDirectionFrom(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (Owner.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " " + The.Player.DescribeDirectionFrom(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
						}
					}
					else if (text != null)
					{
						if (MessageAsFrom.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") with " + Projectile.an() + " for " + damage.Amount + " damage!", ' ', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") " + The.Player.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", ' ', FromDialog: false, UsePopups);
						}
					}
					else if (Owner.IsVisible())
					{
						IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") with " + Projectile.an() + " for " + damage.Amount + " damage!", ' ', FromDialog: false, UsePopups);
					}
					else
					{
						IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") " + The.Player.DescribeDirectionFrom(Owner) + " for " + damage.Amount + " damage!", ' ', FromDialog: false, UsePopups);
					}
				}
				if (flag4)
				{
					Defender.CheckHP(null, null, null, Preregistered: true);
				}
			}
			else if (!getMissileWeaponPerformanceEvent.Quiet)
			{
				if (Owner.IsPlayer())
				{
					if (OutcomeMessageFragment != null)
					{
						if (Defender.IsVisible())
						{
							if (text != null)
							{
								if (MessageAsFrom.IsVisible())
								{
									IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
								else
								{
									IComponent<GameObject>.EmitMessage(Owner, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " " + Defender.DescribeDirectionFrom(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
								}
							}
							else
							{
								IComponent<GameObject>.EmitMessage(Owner, "You" + text3 + " hit " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
					}
					else if (text != null)
					{
						if (Defender.IsVisible())
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") with " + Projectile.an() + "!", Stat.GetResultColorChar(num6), FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(Owner, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + Projectile.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") " + Defender.DescribeDirectionFrom(Owner) + "!", Stat.GetResultColorChar(num6), FromDialog: false, UsePopups);
							}
						}
						else if (MessageAsFrom.IsVisible() && Defender.IsAudible(The.Player, 80))
						{
							IComponent<GameObject>.EmitMessage(MessageAsFrom, text + MessageAsFrom.GetVerb("hit") + " something " + Owner.DescribeDirectionToward(Defender) + "!", ' ', FromDialog: false, UsePopups);
						}
					}
					else if (Defender.IsVisible())
					{
						IComponent<GameObject>.EmitMessage(Owner, "You" + text3 + " hit " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") with " + Projectile.an() + "!", Stat.GetResultColorChar(num6), FromDialog: false, UsePopups);
					}
					else if (Defender.IsAudible(The.Player, 80))
					{
						IComponent<GameObject>.EmitMessage(Owner, "You hit something " + Owner.DescribeDirectionToward(Defender) + "!", ' ', FromDialog: false, UsePopups);
					}
				}
				else if (Defender.IsPlayer())
				{
					if (OutcomeMessageFragment != null)
					{
						if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " you with " + Projectile.an() + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " you " + Defender.DescribeDirectionToward(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (Owner.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb) + " you" + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " you " + Defender.DescribeDirectionToward(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
						}
					}
					else if (text != null)
					{
						if (MessageAsFrom.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + " you with " + Projectile.an() + "! (x" + num6 + ")", 'r', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " you " + Defender.DescribeDirectionToward(Owner) + "! (x" + num6 + ")", 'r', FromDialog: false, UsePopups);
						}
					}
					else if (Owner.IsVisible())
					{
						IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb) + " you! (x" + num6 + ")", 'r', FromDialog: false, UsePopups);
					}
					else
					{
						IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " you " + Defender.DescribeDirectionToward(Owner) + "! (x" + num6 + ")", 'r', FromDialog: false, UsePopups);
					}
				}
				else if ((PathInvolvesPlayer | ShowUninvolved) && Defender.IsVisible())
				{
					if (OutcomeMessageFragment != null)
					{
						if (text != null)
						{
							if (MessageAsFrom.IsVisible())
							{
								IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
							else
							{
								IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " " + The.Player.DescribeDirectionToward(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
							}
						}
						else if (Owner.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " " + The.Player.DescribeDirectionToward(Owner) + OutcomeMessageFragment + ".", ' ', FromDialog: false, UsePopups);
						}
					}
					else if (text != null)
					{
						if (MessageAsFrom.IsVisible())
						{
							IComponent<GameObject>.EmitMessage(MessageAsFrom, text + text3 + MessageAsFrom.GetVerb("hit") + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + "! (x" + num6 + ")", ' ', FromDialog: false, UsePopups);
						}
						else
						{
							IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") " + The.Player.DescribeDirectionToward(Owner) + "!", ' ', FromDialog: false, UsePopups);
						}
					}
					else if (Owner.IsVisible())
					{
						IComponent<GameObject>.EmitMessage(Owner, Owner.Does("hit", int.MaxValue, null, null, adverb) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + "! (x" + num6 + ")", ' ', FromDialog: false, UsePopups);
					}
					else
					{
						IComponent<GameObject>.EmitMessage(The.Player, Projectile.Does("hit", int.MaxValue, null, null, adverb, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " " + ((Defender == Owner) ? Owner.itself : Defender.t()) + " (x" + num6 + ") " + The.Player.DescribeDirectionToward(Owner) + "!", ' ', FromDialog: false, UsePopups);
					}
				}
			}
			if ((Owner.IsPlayer() && !ParentObject.HasTagOrProperty("NoMissileSetTarget") && Sidebar.CurrentTarget == null && Defender != Owner && Defender.IsCreature && Defender.IsHostileTowards(Owner) && Defender.IsVisible()) & TargetWasInitiallyUnset)
			{
				Sidebar.CurrentTarget = Defender;
			}
			if (Projectile.IsValid())
			{
				ImpactCell?.AddObject(Projectile, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit");
			}
			Event obj6 = Event.New("ProjectileHit");
			obj6.SetParameter("Attacker", Attacker);
			obj6.SetParameter("Defender", Defender);
			obj6.SetParameter("Projectile", Projectile);
			obj6.SetParameter("Skill", Skill);
			obj6.SetParameter("Damage", damage);
			obj6.SetParameter("AimLevel", AimLevel);
			obj6.SetParameter("Owner", Attacker);
			obj6.SetParameter("Launcher", ParentObject);
			obj6.SetParameter("Path", MPath);
			obj6.SetParameter("Penetrations", num6);
			obj6.SetParameter("ImpactCell", ImpactCell);
			obj6.SetParameter("ApparentTarget", ApparentTarget);
			obj6.SetParameter("AimedAt", AimedAt);
			obj6.SetFlag("critical", flag);
			Projectile.FireEvent(obj6);
			obj6.ID = "DefenderProjectileHit";
			Defender.FireEvent(obj6);
			obj6.ID = "LauncherProjectileHit";
			ParentObject.FireEvent(obj6);
			if (!getMissileWeaponPerformanceEvent.PenetrateCreatures)
			{
				Done = true;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("MissileWeapon:MissileHit", x);
		}
		finally
		{
			if (GameObject.Validate(ref Defender))
			{
				Defender.WillCheckHP(false);
			}
		}
	}

	public bool IsSkilled(GameObject Actor)
	{
		if (Actor != null)
		{
			if (Skill == "Pistol")
			{
				return Actor.HasSkill("Pistol_SteadyHands");
			}
			if (Skill == "Rifle" || Skill == "Bow")
			{
				return Actor.HasSkill("Rifle_SteadyHands");
			}
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandFireMissile")
		{
			GameObject gameObject = E.GetGameObjectParameter("Actor") ?? E.GetGameObjectParameter("Owner");
			if (gameObject == null)
			{
				return false;
			}
			Cell cell = E.GetParameter("TargetCell") as Cell;
			Cell cell2 = E.GetParameter("StartCell") as Cell;
			GameObject Attacker = gameObject;
			FireType fireType = FireType.Normal;
			if (E.HasParameter("FireType"))
			{
				fireType = (FireType)E.GetParameter("FireType");
			}
			if (cell2 == null)
			{
				cell2 = gameObject.CurrentCell;
			}
			if (cell2 == null)
			{
				return false;
			}
			Zone parentZone = cell2.ParentZone;
			if (parentZone == null)
			{
				return false;
			}
			bool flag = parentZone.IsActive();
			bool activePartsIgnoreSubject = E.HasFlag("ActivePartsIgnoreSubject");
			bool flag2 = E.HasFlag("UsePopups");
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
			ScreenBuffer screenBuffer = E.GetParameter("ScreenBuffer") as ScreenBuffer;
			if (flag)
			{
				if (screenBuffer != null)
				{
					scrapBuffer.Copy(screenBuffer);
				}
				else
				{
					XRLCore.Core.RenderBaseToBuffer(scrapBuffer);
				}
			}
			GameObject gameObject2 = null;
			int intParameter = E.GetIntParameter("AimLevel");
			MissilePath missilePath = E.GetParameter("Path") as MissilePath;
			if (missilePath == null)
			{
				if (CalculatedMissilePathInUse)
				{
					if (SecondCalculatedMissilePathInUse)
					{
						missilePath = new MissilePath();
					}
					else
					{
						missilePath = SecondCalculatedMissilePath;
						SecondCalculatedMissilePathInUse = true;
					}
				}
				else
				{
					missilePath = CalculatedMissilePath;
					CalculatedMissilePathInUse = true;
				}
				CalculateMissilePath(missilePath, parentZone, cell2.X, cell2.Y, cell.X, cell.Y, E.HasFlag("IncludeStart"), IncludeCover: false, MapCalculated: false, gameObject);
			}
			try
			{
				int intParameter2 = E.GetIntParameter("FlatVariance");
				if (!Attacker.FireEvent("BeginMissileAttack"))
				{
					return false;
				}
				bool flag3 = false;
				if (!NoWildfire)
				{
					int num = 0;
					foreach (Cell adjacentCell in Attacker.CurrentCell.GetAdjacentCells())
					{
						foreach (GameObject item2 in adjacentCell.LoopObjectsWithPart("Combat"))
						{
							if (!item2.IsHostileTowards(Attacker) || !item2.PhaseAndFlightMatches(Attacker) || !item2.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
							{
								continue;
							}
							num++;
							if (num > 1)
							{
								if (50.in100())
								{
									flag3 = true;
								}
								goto end_IL_025b;
							}
						}
						continue;
						end_IL_025b:
						break;
					}
				}
				float num2 = missilePath.X1 - missilePath.X0;
				float num3 = missilePath.Y1 - missilePath.Y0;
				float num4 = 0f;
				string text = "-";
				num4 = ((num2 != 0f) ? (Math.Abs(num3) / Math.Abs(num2)) : 9999f);
				if (num4 >= 2f)
				{
					text = "|";
				}
				else if ((double)num4 >= 0.5)
				{
					if (num2 < 0f)
					{
						text = ((!(num3 > 0f)) ? "\\" : "/");
					}
					else
					{
						text = ((!(num3 > 0f)) ? "/" : "\\");
					}
				}
				else
				{
					text = "-";
				}
				ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer2();
				if (!CheckLoadAmmoEvent.Check(ParentObject, gameObject, out var Message, activePartsIgnoreSubject))
				{
					if (!Message.IsNullOrEmpty() && gameObject.IsPlayer())
					{
						EmitMessage(Message, 'r', FromDialog: false, flag2);
					}
					if (Attacker != null && Attacker.Brain != null && ParentObject.FireEvent("ReloadPossible"))
					{
						Attacker.Brain.NeedToReload = true;
					}
					return false;
				}
				int num5 = 0;
				List<GameObject> list = new List<GameObject>(ShotsPerAction);
				List<Projectile> list2 = new List<Projectile>(ShotsPerAction);
				GameObject gameObject3 = null;
				for (int i = 0; i < AmmoPerAction; i++)
				{
					if (!LoadAmmoEvent.Check(ParentObject, gameObject, out var Projectile, out var LoadedAmmo, out var Message2, activePartsIgnoreSubject))
					{
						if (!Message2.IsNullOrEmpty() && gameObject.IsPlayer())
						{
							EmitMessage(Message2, 'r', FromDialog: false, flag2);
						}
						break;
					}
					if (GameObject.Validate(ref LoadedAmmo))
					{
						gameObject2 = LoadedAmmo;
					}
					num5++;
					if (GameObject.Validate(ref Projectile))
					{
						list.Add(Projectile);
						list2.Add(Projectile.GetPart<Projectile>());
						if (gameObject3 == null)
						{
							gameObject3 = Projectile;
						}
					}
				}
				for (int j = AmmoPerAction; j < ShotsPerAction; j++)
				{
					int num6 = j - AmmoPerAction;
					if (list.Count < num6)
					{
						num6 = 0;
					}
					if (list.Count > num6)
					{
						GameObject Object = list[num6].DeepCopy();
						if (GameObject.Validate(ref Object))
						{
							list.Add(Object);
							list2.Add(Object.GetPart<Projectile>());
						}
					}
				}
				for (int num7 = list.Count - 1; num7 >= 0; num7--)
				{
					SetupProjectile(list[num7], Attacker, ParentObject, list2[num7]);
				}
				if (E.HasFlag("ShowEmitMessage") && list.Count > 0)
				{
					GameObject gameObject4 = ((ParentObject.CurrentCell == null) ? gameObject : ParentObject);
					if (IComponent<GameObject>.Visible(gameObject4))
					{
						if (list.Count == 1)
						{
							GameObject gameObject5 = list[0];
							GameObject useVisibilityOf = gameObject4;
							DidXToY("emit", gameObject5, null, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, flag2, useVisibilityOf);
						}
						else
						{
							string[] array = new string[list.Count];
							bool flag4 = true;
							int k = 0;
							for (int count = list.Count; k < count; k++)
							{
								array[k] = list[k].ShortDisplayName;
								if (flag4 && k > 0 && array[k] != array[0])
								{
									flag4 = false;
								}
							}
							if (flag4)
							{
								string extra = list.Count.Things(array[0]);
								GameObject useVisibilityOf = gameObject4;
								DidX("emit", extra, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, flag2, useVisibilityOf);
							}
							else
							{
								int l = 0;
								for (int num8 = array.Length; l < num8; l++)
								{
									array[l] = Grammar.A(array[l]);
								}
								string extra2 = Grammar.MakeAndList(array);
								GameObject useVisibilityOf = gameObject4;
								DidX("emit", extra2, "!", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, flag2, useVisibilityOf);
							}
						}
					}
				}
				if (num5 == 0)
				{
					if (Attacker != null && Attacker.Brain != null && ParentObject.FireEvent("ReloadPossible"))
					{
						Attacker.Brain.NeedToReload = true;
					}
					return false;
				}
				if (flag3)
				{
					if (Attacker.IsPlayer())
					{
						EmitMessage("Your shot goes wild!", 'R', FromDialog: false, flag2);
					}
					else if (IComponent<GameObject>.Visible(Attacker))
					{
						EmitMessage(Attacker.Poss("shot") + " goes wild!", ColorCoding.ConsequentialColorChar(null, Attacker), FromDialog: false, flag2);
					}
				}
				int num9 = 0;
				num9 = ((num5 < AmmoPerAction) ? ((int)Math.Ceiling((float)ShotsPerAction * ((float)num5 / (float)AmmoPerAction))) : ShotsPerAction);
				if (num9 > 0)
				{
					ParentObject.FireEvent("WeaponMissleWeaponFiring");
				}
				string value = ParentObject?.GetTag("MissileFireSound");
				Event obj = Event.New("QueryMissileFireSound");
				obj.SetParameter("Weapon", ParentObject);
				obj.SetParameter("Sound", value);
				obj.SetParameter("Ammo", gameObject2);
				obj.SetParameter("Attacker", Attacker);
				ParentObject?.FireEvent(obj);
				gameObject2?.FireEvent(obj);
				value = obj.GetStringParameter("Sound");
				gameObject?.PlayWorldSound(value, 0.5f, 0f, Combat: true);
				switch (fireType)
				{
				case FireType.SuppressingFire:
					gameObject?.PlayWorldSound("sfx_ability_weaponSkill_suppressiveFire", 0.5f, 0f, Combat: true);
					break;
				case FireType.FlatteningFire:
					gameObject?.PlayWorldSound("sfx_ability_weaponSkill_suppressiveFire_upgraded", 0.5f, 0f, Combat: true);
					break;
				case FireType.SureFire:
					gameObject?.PlayWorldSound("sfx_ability_weaponSkill_sureFire", 0.5f, 0f, Combat: true);
					break;
				case FireType.BeaconFire:
					gameObject?.PlayWorldSound("sfx_ability_weaponSkill_sureFire_upgraded", 0.5f, 0f, Combat: true);
					break;
				case FireType.WoundingFire:
					gameObject?.PlayWorldSound("sfx_ability_weaponSkill_woundingFire", 0.5f, 0f, Combat: true);
					break;
				case FireType.DisorientingFire:
					gameObject?.PlayWorldSound("sfx_ability_weaponSkill_woundingFire_upgraded", 0.5f, 0f, Combat: true);
					break;
				case FireType.OneShot:
					gameObject?.PlayWorldSound("sfx_ability_weaponSkill_ultraFire", 0.5f, 0f, Combat: true);
					break;
				}
				GameObject Object2 = E.GetGameObjectParameter("AimedAt") ?? cell.GetCombatTarget(Attacker, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, gameObject3, null, null, gameObject, null, AllowInanimate: true, InanimateSolidOnly: true);
				if (GameObject.Validate(ref Object2))
				{
					if (Object2.IsInGraveyard())
					{
						Object2 = null;
					}
					else if (Object2.IsPlayer())
					{
						if (AutoAct.IsActive() && IComponent<GameObject>.Visible(Attacker))
						{
							AutoAct.Interrupt("something is shooting at you", null, Attacker, IsThreat: true);
						}
					}
					else if (!Object2.IsHostileTowards(gameObject))
					{
						Object2.AddOpinion<OpinionFriendlyFire>(gameObject);
					}
				}
				else
				{
					Object2 = null;
				}
				GameObject gameObject6 = Object2;
				if (gameObject.IsPlayer())
				{
					MissilePath path = missilePath;
					GameObject useVisibilityOf = Attacker;
					foreach (Point item3 in CalculateBulletTrajectory(path, gameObject3, null, useVisibilityOf, null, null, 0, 0, IntendedPathOnly: true))
					{
						Cell cell3 = parentZone.GetCell(item3.X, item3.Y);
						if (cell3 == Attacker.CurrentCell)
						{
							continue;
						}
						GameObject combatTarget = cell3.GetCombatTarget(Attacker, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, gameObject3, null, null, null, null, AllowInanimate: true, InanimateSolidOnly: true);
						if (combatTarget == null || combatTarget == gameObject)
						{
							continue;
						}
						if (combatTarget != gameObject6 && !combatTarget.IsLedBy(gameObject))
						{
							if (gameObject6 == null)
							{
								gameObject6 = combatTarget;
							}
							if (combatTarget.Brain != null && (gameObject.PhaseMatches(combatTarget) || (gameObject3 != null && gameObject3.PhaseMatches(combatTarget))) && !combatTarget.IsHostileTowards(gameObject) && combatTarget.Brain.FriendlyFireIncident(gameObject))
							{
								combatTarget.AddOpinion<OpinionFriendlyFire>(gameObject);
							}
						}
						break;
					}
				}
				List<List<Point>> list3 = new List<List<Point>>();
				int num10 = 0;
				int num11 = -gameObject.StatMod(Modifier);
				if (IsSkilled(Attacker))
				{
					num11 -= 2;
				}
				bool flag5 = E.HasFlag("TargetUnset");
				if (gameObject.IsPlayer() && !ParentObject.HasTagOrProperty("NoMissileSetTarget"))
				{
					if (Object2 != null && Sidebar.CurrentTarget != Object2)
					{
						if (!flag5 && !Object2.IsCreature)
						{
							GameObject currentTarget = Sidebar.CurrentTarget;
							if (currentTarget != null && currentTarget.IsCreature)
							{
								goto IL_0bdc;
							}
						}
						if (Object2.IsVisible())
						{
							Sidebar.CurrentTarget = Object2;
							goto IL_0c04;
						}
					}
					goto IL_0bdc;
				}
				goto IL_0c04;
				IL_0c04:
				if (Object2 != null)
				{
					num11 += Object2.GetIntProperty("IncomingAimModifier");
					if (Rifle_DrawABead.IsCompatibleMissileWeapon(Skill) && Object2.HasEffect((RifleMark fx) => fx.Marker == Attacker))
					{
						num11--;
					}
				}
				num11 -= intParameter;
				num11 -= AimVarianceBonus;
				num11 -= Attacker.GetIntProperty("MissileWeaponAccuracyBonus");
				num11 -= ParentObject.GetIntProperty("MissileWeaponAccuracyBonus");
				eModifyAimVariance.SetParameter("Amount", 0);
				Attacker.FireEvent(eModifyAimVariance);
				ParentObject.FireEvent(eModifyAimVariance);
				num11 += eModifyAimVariance.GetIntParameter("Amount");
				if (Object2 != null && Object2.HasRegisteredEvent("ModifyIncomingAimVariance"))
				{
					eModifyIncomingAimVariance.SetParameter("Amount", 0);
					Object2.FireEvent(eModifyIncomingAimVariance);
					num11 += eModifyIncomingAimVariance.GetIntParameter("Amount");
				}
				int num12 = VarianceDieRoll.Resolve();
				num10 = Math.Abs(num12 - 21) + num11;
				if (fireType == FireType.SureFire || fireType == FireType.BeaconFire || (fireType == FireType.OneShot && Attacker.HasSkill("Rifle_SureFire")))
				{
					num10 = 0;
				}
				if (num10 < 0)
				{
					num10 = 0;
				}
				if (num12 < 25)
				{
					num10 = -num10;
				}
				num10 += intParameter2;
				if (Attacker.HasEffect<Running>() && (Skill != "Pistol" || !Attacker.HasSkill("Pistol_SlingAndRun")) && !Attacker.HasProperty("EnhancedSprint"))
				{
					num10 += Stat.Random(-23, 23);
				}
				if (flag3)
				{
					num10 += Stat.Random(-23, 23);
				}
				bool flag6 = false;
				int Spread = 0;
				int num13 = 0;
				if (num9 > 1)
				{
					flag6 = GetFixedMissileSpreadEvent.GetFor(ParentObject, out Spread);
					if (flag6)
					{
						num13 = Stat.Random(-WeaponAccuracy, WeaponAccuracy);
					}
				}
				List<bool> list4 = new List<bool>(num9);
				List<bool> list5 = new List<bool>(num9);
				List<bool> list6 = new List<bool>(num9);
				List<Cell> list7 = new List<Cell>(num9);
				for (int num14 = 0; num14 < num9; num14++)
				{
					int num15 = intParameter2;
					int num16 = num10;
					int value2;
					if (flag6)
					{
						value2 = num13;
						int num17 = -Spread / 2 + Spread * num14 / (num9 - 1);
						num15 += num17;
						num16 += num17;
					}
					else
					{
						value2 = Stat.Random(-WeaponAccuracy, WeaponAccuracy);
					}
					Event obj2 = Event.New("WeaponMissileWeaponShot");
					obj2.SetParameter("AimVariance", num16);
					obj2.SetParameter("FlatVariance", num15);
					obj2.SetParameter("WeaponAccuracy", value2);
					ParentObject.FireEvent(obj2);
					GameObject gameObject7 = ((list.Count > num14) ? list[num14] : null);
					if (gameObject7 == null)
					{
						MetricsManager.LogError("had no projectile for shot " + num14 + " from " + ParentObject.DebugName);
						continue;
					}
					List<Point> item = CalculateBulletTrajectory(out var PlayerInvolved, out var CameNearPlayer, out var NearPlayerCell, missilePath, gameObject7, ParentObject, Attacker, Attacker.CurrentZone, obj2.GetIntParameter("AimVariance").ToString(), obj2.GetIntParameter("FlatVariance"), obj2.GetIntParameter("WeaponAccuracy"));
					list3.Add(item);
					list4.Add(item: false);
					list5.Add(PlayerInvolved);
					list6.Add(CameNearPlayer);
					list7.Add(NearPlayerCell);
				}
				if (flag)
				{
					scrapBuffer2.Copy(scrapBuffer);
				}
				int num18 = Math.Min(num9, ShotsPerAnimation);
				The.Player.GetCurrentCell();
				Event obj3 = Event.New("ProjectileEntering", "Attacker", Attacker);
				Event obj4 = Event.New("ProjectileEnteringCell", "Attacker", Attacker);
				List<GameObject> objectsThatWantEvent = cell.ParentZone.GetObjectsThatWantEvent(PooledEvent<ProjectileMovingEvent>.ID, ProjectileMovingEvent.CascadeLevel);
				ProjectileMovingEvent projectileMovingEvent = null;
				if (objectsThatWantEvent.Count > 0)
				{
					projectileMovingEvent = PooledEvent<ProjectileMovingEvent>.FromPool();
					projectileMovingEvent.Attacker = Attacker;
					projectileMovingEvent.Launcher = ParentObject;
					projectileMovingEvent.TargetCell = cell;
					projectileMovingEvent.ApparentTarget = gameObject6;
					projectileMovingEvent.ScreenBuffer = scrapBuffer2;
				}
				GameObject gameObjectParameter = E.GetGameObjectParameter("MessageAsFrom");
				MissileWeaponVFXConfiguration missileWeaponVFXConfiguration = null;
				for (int num19 = 0; num19 < list.Count; num19 += num18)
				{
					int num20 = 0;
					bool flag7 = false;
					for (int num21 = num19; num21 < num19 + num18 && num21 < list3.Count; num21++)
					{
						if (list3[num21].Count > num20)
						{
							num20 = list3[num21].Count;
						}
					}
					int num22 = AnimationDelay - num20 / 5;
					if (num22 > 0 && E.HasParameter("AnimationDelayMultiplier"))
					{
						num22 = (int)((float)num22 * E.GetParameter<float>("AnimationDelayMultiplier"));
					}
					int num23 = cell2.X - cell.X;
					int num24 = cell2.Y - cell.Y;
					_ = (int)Math.Sqrt(num23 * num23 + num24 * num24) / RangeIncrement;
					int num25 = ((VariableMaxRange != null) ? Math.Min(VariableMaxRange.RollCached(), MaxRange) : MaxRange);
					bool flag8 = false;
					for (int num26 = num19; num26 < list.Count && num26 < list.Count + num18; num26++)
					{
						GameObjectBlueprint blueprint = list[num26].GetBlueprint();
						bool flag9 = false;
						Dictionary<string, string> value3;
						if (list[num26].HasStringProperty("ProjectileVFX") || blueprint.HasTag("ProjectileVFX"))
						{
							if (missileWeaponVFXConfiguration == null)
							{
								missileWeaponVFXConfiguration = MissileWeaponVFXConfiguration.next();
								CombatJuiceManager.startDelay();
							}
							missileWeaponVFXConfiguration.addStep(num26, list3[num26][0].location);
							missileWeaponVFXConfiguration.setPathProjectileVFX(num26, list[num26].GetPropertyOrTag("ProjectileVFX"), list[num26].GetPropertyOrTag("ProjectileVFXConfiguration"));
							flag9 = true;
						}
						else if (blueprint.xTags != null && blueprint.xTags.TryGetValue("ProjectileVFX", out value3))
						{
							if (missileWeaponVFXConfiguration == null)
							{
								missileWeaponVFXConfiguration = MissileWeaponVFXConfiguration.next();
								CombatJuiceManager.startDelay();
							}
							missileWeaponVFXConfiguration.addStep(num26, list3[num26][0].location);
							missileWeaponVFXConfiguration.setPathProjectileVFX(num26, value3);
							missileWeaponVFXConfiguration.SetPathProjectileRender(num26, list[num26]);
							flag9 = true;
						}
						if (flag9)
						{
							ConfigureMissileVisualEffectEvent.Send(missileWeaponVFXConfiguration, missileWeaponVFXConfiguration.GetPath(num26), Attacker, ParentObject, list[num26]);
						}
					}
					for (int num27 = 1; num27 < num20 && num27 <= num25; num27++)
					{
						if (flag && AmmoChar != "f" && AmmoChar != "m" && AmmoChar != "e")
						{
							scrapBuffer2.Copy(scrapBuffer);
						}
						bool flag10 = true;
						for (int num28 = num19; num28 < num19 + num18 && num28 < list4.Count; num28++)
						{
							if (!list4[num28])
							{
								flag10 = false;
								break;
							}
						}
						if (flag10)
						{
							break;
						}
						for (int num29 = num19; num29 < num19 + num18 && num29 < list3.Count; num29++)
						{
							List<Point> list8 = list3[num29];
							if (num27 >= list8.Count)
							{
								list4[num29] = true;
							}
							if (list4[num29])
							{
								continue;
							}
							Projectile projectile = list2[num29];
							GameObject Object3 = list[num29];
							Cell cell4 = parentZone.GetCell(list8[num27 - 1].X, list8[num27 - 1].Y);
							Cell cell5 = parentZone.GetCell(list8[num27].X, list8[num27].Y);
							if (cell5 != null)
							{
								missileWeaponVFXConfiguration?.addStep(num29, list8[num27].location);
							}
							if (flag && cell5 != null && cell5.IsVisible() && missileWeaponVFXConfiguration == null)
							{
								flag7 = true;
							}
							if (flag7)
							{
								string text2 = projectile.RenderChar ?? AmmoChar;
								scrapBuffer2.Goto(list8[num27].X, list8[num27].Y);
								if (text2 == "sm")
								{
									scrapBuffer2.Goto(list8[num27].X, list8[num27].Y);
									int num30 = Stat.Random(1, 3);
									string s = "+";
									if (num30 == 1)
									{
										s = "&R*";
									}
									if (num30 == 2)
									{
										s = "&W*";
									}
									if (num30 == 3)
									{
										s = "&Y*";
									}
									scrapBuffer2.Write(s);
								}
								else if (text2 == "e")
								{
									float num31 = 0f;
									float num32 = 0f;
									float num33 = (float)Stat.Random(85, 185) / 58f;
									num31 = (float)Math.Sin(num33) / 6f;
									num32 = (float)Math.Cos(num33) / 6f;
									int num34 = Stat.Random(1, 3);
									string text3 = "";
									text3 = ((char)Stat.Random(191, 198)).ToString() ?? "";
									if (num34 == 1)
									{
										text3 = "&Y" + text3;
									}
									if (num34 == 2)
									{
										text3 = "&W*" + text3;
									}
									if (num34 == 3)
									{
										text3 = "&C*" + text3;
									}
									XRLCore.ParticleManager.Add(text3, (float)list8[num27].X + num31 * 2f, (float)list8[num27].Y + num32 * 2f, num31, num32, 2);
									XRLCore.ParticleManager.Frame();
									XRLCore.ParticleManager.Render(scrapBuffer2);
									scrapBuffer2.Goto(list8[num27].X, list8[num27].Y);
									if (num34 == 1)
									{
										text3 = "&Y" + text3;
									}
									if (num34 == 2)
									{
										text3 = "&W*" + text3;
									}
									if (num34 == 3)
									{
										text3 = "&C*" + text3;
									}
									scrapBuffer2.Write(text3);
								}
								else if (text2.Contains("-"))
								{
									scrapBuffer2.Write(text2.Replace("-", text) ?? "");
								}
								else if (text2 == "m")
								{
									float num35 = 0f;
									float num36 = 0f;
									float num37 = (float)Stat.Random(85, 185) / 58f;
									num35 = (float)Math.Sin(num37) / 6f;
									num36 = (float)Math.Cos(num37) / 6f;
									int num38 = Stat.Random(1, 3);
									string text4 = "";
									switch (num38)
									{
									case 1:
										text4 = "°";
										break;
									case 2:
										text4 = "±";
										break;
									case 3:
										text4 = "²";
										break;
									}
									XRLCore.ParticleManager.Add(text4, list8[num27].X, list8[num27].Y, num35, num36);
									XRLCore.ParticleManager.Frame();
									XRLCore.ParticleManager.Render(scrapBuffer2);
									scrapBuffer2.Goto(list8[num27].X, list8[num27].Y);
									switch (num38)
									{
									case 1:
										text4 = "&R*";
										break;
									case 2:
										text4 = "&W*";
										break;
									case 3:
										text4 = "&Y*";
										break;
									}
									scrapBuffer2.Write(text4);
								}
								else if (text2.StartsWith("GG"))
								{
									string text5 = null;
									string text6 = text2.Substring(2);
									int num39 = 1;
									for (int num40 = Math.Min(list8.Count - 1, num27 - 1); num39 <= num40; num39++)
									{
										int x = list8[num39].X;
										int y = list8[num39].Y;
										if (num39 == num40)
										{
											text5 = "X";
										}
										else
										{
											int x2 = list8[num39 - 1].X;
											int y2 = list8[num39 - 1].Y;
											int x3 = list8[num39 + 1].X;
											int y3 = list8[num39 + 1].Y;
											if (y == y3 && y == y2)
											{
												text5 = "-";
											}
											else if (x == x3 && x == x2)
											{
												text5 = "|";
											}
											else if ((x == x3 && x != x2 && y != y3 && y == x2) || (x != x3 && x == x2 && y == y3 && y != x2))
											{
												text5 = null;
											}
											else if (y3 > y2)
											{
												text5 = ((x3 > x2) ? "\\" : "/");
											}
											else if (y3 < y2)
											{
												text5 = ((x3 > x2) ? "/" : "\\");
											}
										}
										if (!text5.IsNullOrEmpty())
										{
											if (!text6.IsNullOrEmpty())
											{
												text5 = "{{" + text6 + "|" + text5 + "}}";
											}
											scrapBuffer2.WriteAt(x, y, text5);
											scrapBuffer2.Draw();
											XRLCore.ParticleManager.Frame();
										}
									}
								}
								else
								{
									switch (text2)
									{
									case "HR":
									{
										for (int num43 = 1; num43 < list8.Count && num43 < num27; num43++)
										{
											scrapBuffer2.Goto(list8[num43].X, list8[num43].Y);
											string text8 = "&b";
											switch (Stat.Random(1, 3))
											{
											case 1:
												text8 = "&r";
												break;
											case 2:
												text8 = "&b";
												break;
											case 3:
												text8 = "&r";
												break;
											}
											switch (Stat.Random(1, 3))
											{
											case 1:
												text8 += "^b";
												break;
											case 2:
												text8 += "^r";
												break;
											case 3:
												text8 += "^b";
												break;
											}
											int num44 = Stat.Random(1, 3);
											scrapBuffer2.Write(text8 + " ");
										}
										break;
									}
									case "FR":
									{
										for (int num41 = 1; num41 < list8.Count && num41 < num27; num41++)
										{
											scrapBuffer2.Goto(list8[num41].X, list8[num41].Y);
											string text7 = "&C";
											switch (Stat.Random(1, 3))
											{
											case 1:
												text7 = "&C";
												break;
											case 2:
												text7 = "&B";
												break;
											case 3:
												text7 = "&Y";
												break;
											}
											switch (Stat.Random(1, 3))
											{
											case 1:
												text7 += "^C";
												break;
											case 2:
												text7 += "^B";
												break;
											case 3:
												text7 += "^Y";
												break;
											}
											int num42 = Stat.Random(1, 3);
											scrapBuffer2.Write(text7 + (char)(219 + Stat.Random(0, 4)));
										}
										break;
									}
									case "f":
									{
										for (int num45 = 1; num45 < list8.Count && num45 < num27; num45++)
										{
											Cell cell6 = parentZone?.GetCell(list8[num45].X, list8[num45].Y);
											if (cell6 != null)
											{
												cell6.Flameburst();
												continue;
											}
											scrapBuffer2.Goto(list8[num45].X, list8[num45].Y);
											string text9 = "&R";
											switch (Stat.Random(1, 3))
											{
											case 1:
												text9 = "&R";
												break;
											case 2:
												text9 = "&W";
												break;
											case 3:
												text9 = "&Y";
												break;
											}
											switch (Stat.Random(1, 3))
											{
											case 1:
												text9 += "^R";
												break;
											case 2:
												text9 += "^W";
												break;
											case 3:
												text9 += "^Y";
												break;
											}
											int num46 = Stat.Random(1, 3);
											scrapBuffer2.Write(text9 + (char)(219 + Stat.Random(0, 4)));
										}
										break;
									}
									default:
										scrapBuffer2.Write(text2 ?? "");
										break;
									}
								}
							}
							GetMissileWeaponPerformanceEvent getMissileWeaponPerformanceEvent = GetMissileWeaponPerformanceEvent.GetFor(gameObject, ParentObject, Object3);
							int num47 = 0;
							GameObject SolidObject;
							bool IsSolid;
							bool IsCover;
							GameObject gameObject8;
							while (true)
							{
								cell5.FindSolidObjectForMissile(Attacker, Projectile: Object3, Launcher: ParentObject, SolidObject: out SolidObject, IsSolid: out IsSolid, IsCover: out IsCover, RecheckHit: out var RecheckHit, RecheckPhase: out var _, PenetrateCreatures: getMissileWeaponPerformanceEvent.PenetrateCreatures, PenetrateWalls: getMissileWeaponPerformanceEvent.PenetrateWalls, ApparentTarget: gameObject6);
								if (RecheckHit && ++num47 < 100)
								{
									continue;
								}
								gameObject8 = cell5.GetCombatTarget(Attacker, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, Launcher: ParentObject, Projectile: Object3, CheckPhaseAgainst: Object3, Skip: null, SkipList: null, AllowInanimate: true, InanimateSolidOnly: true);
								if (gameObject8 == null)
								{
									break;
								}
								GameObject projectile2 = Object3;
								GameObject useVisibilityOf = ParentObject;
								GameObject attacker = Attacker;
								GameObject gameObject9 = gameObject8;
								GameObject apparentTarget = gameObject6;
								bool flag11 = BeforeProjectileHitEvent.Check(projectile2, attacker, gameObject9, out var Recheck, out var _, getMissileWeaponPerformanceEvent.PenetrateCreatures, getMissileWeaponPerformanceEvent.PenetrateWalls, useVisibilityOf, apparentTarget);
								if (!Recheck || ++num47 >= 100)
								{
									if (!flag11)
									{
										gameObject8 = null;
									}
									break;
								}
							}
							bool Done = false;
							bool showUninvolved = false;
							if (!IsSolid)
							{
								cell5.WakeCreaturesInArea();
								obj3.SetParameter("Cell", cell5);
								obj3.SetParameter("Path", missilePath);
								obj3.SetParameter("p", num27);
								Object3.FireEvent(obj3);
								obj4.SetParameter("Projectile", Object3);
								obj4.SetParameter("Cell", cell5);
								obj4.SetParameter("Path", missilePath);
								obj4.SetParameter("p", num27);
								if (!cell5.FireEvent(obj4))
								{
									Done = true;
								}
								else if (!MissileTraversingCellEvent.Check(Object3, cell5, cell4.GetDirectionFromCell(cell5), Attacker))
								{
									Done = true;
								}
								else if (projectileMovingEvent != null)
								{
									projectileMovingEvent.Projectile = Object3;
									projectileMovingEvent.Defender = gameObject8;
									projectileMovingEvent.Cell = cell5;
									projectileMovingEvent.Path = list8;
									projectileMovingEvent.PathIndex = num27;
									foreach (GameObject item4 in objectsThatWantEvent)
									{
										bool flag12 = item4.HandleEvent(projectileMovingEvent);
										if (projectileMovingEvent.HitOverride != null)
										{
											gameObject8 = projectileMovingEvent.HitOverride;
											projectileMovingEvent.HitOverride = null;
										}
										if (projectileMovingEvent.ActivateShowUninvolved)
										{
											showUninvolved = true;
										}
										if (!flag12)
										{
											Done = true;
											break;
										}
									}
								}
							}
							if (!Done && !GameObject.Validate(ref Object3))
							{
								Done = true;
							}
							bool flag13 = false;
							if (gameObject8 != null && (!flag3 || cell5.DistanceTo(gameObject) >= 2))
							{
								if (AutoAct.IsActive())
								{
									if (gameObject8.IsPlayerControlledAndPerceptible() && !gameObject8.IsTrifling && !Attacker.IsPlayerControlled())
									{
										AutoAct.Interrupt("you " + gameObject8.GetPerceptionVerb() + " something shooting at " + gameObject8.t() + (gameObject8.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(gameObject8))), null, gameObject8, IsThreat: true);
									}
									else if (gameObject8.DistanceTo(The.Player) <= 1 && Attacker.IsHostileTowards(The.Player))
									{
										AutoAct.Interrupt("something is shooting at you or " + gameObject8.t(), null, gameObject8, IsThreat: true);
									}
								}
								int num48 = Stat.Random(1, 20);
								int num49 = GetToHitModifierEvent.GetFor(Attacker, gameObject8, ParentObject, 0, Object3, null, Skill, null, Prospective: false, Missile: true);
								int num50 = num48 + num49;
								int combatDV = Stats.GetCombatDV(gameObject8);
								Event obj5 = Event.New("WeaponGetDefenderDV");
								obj5.SetParameter("Weapon", ParentObject);
								obj5.SetParameter("Defender", gameObject8);
								obj5.SetParameter("NaturalHitResult", num48);
								obj5.SetParameter("Result", num50);
								obj5.SetParameter("Skill", Skill);
								obj5.SetParameter("DV", combatDV);
								gameObject8?.FireEvent(obj5);
								obj5.ID = "ProjectileGetDefenderDV";
								projectile?.FireEvent(obj5);
								combatDV = obj5.GetIntParameter("DV");
								if (!gameObject8.HasSkill("Acrobatics_SwiftReflexes"))
								{
									combatDV -= 5;
								}
								if (!gameObject8.IsMobile())
								{
									combatDV = -100;
								}
								if (num50 > combatDV)
								{
									if (Object3.HasTagOrProperty("NoDodging"))
									{
										if (gameObject8.IsPlayer())
										{
											if (gameObject8.IsCombat && gameObject8.CanChangeMovementMode("Dodging"))
											{
												IComponent<GameObject>.XDidYToZ(gameObject8, "attempt", "to flinch away, but", Object3, "is too wide", "!", null, null, null, gameObject8);
											}
										}
										else if (gameObject8.IsVisible() && gameObject8.IsCombat && gameObject8.CanChangeMovementMode("Dodging"))
										{
											IComponent<GameObject>.XDidYToZ(gameObject8, "attempt", "to flinch out of the way of", Object3, ", but it's too wide", "!", null, null, null, gameObject8);
										}
									}
									if (IComponent<GameObject>.Visible(gameObject8))
									{
										flag8 = true;
									}
									flag13 = true;
									bool PenetrateCreatures = false;
									bool PenetrateWalls = false;
									MissileHit(Attacker, gameObject8, gameObject, Object3, projectile, Object2, gameObject6, missilePath, cell5, fireType, intParameter, num48, num50, list5[num29], gameObjectParameter, flag2, ref Done, ref PenetrateCreatures, ref PenetrateWalls, flag5, showUninvolved);
								}
								else if (combatDV != -100 && gameObject8.InActiveZone() && !Object3.HasTagOrProperty("NoDodging"))
								{
									string passByVerb = projectile.PassByVerb;
									gameObject8.ParticleBlip("&K\t", 10, 0L);
									if (gameObject8.IsPlayer())
									{
										gameObject8.PlayWorldSound("sfx_missile_generic_flinched");
										if (!passByVerb.IsNullOrEmpty())
										{
											if (gameObject8.IsCombat && gameObject8.CanChangeMovementMode("Dodging"))
											{
												IComponent<GameObject>.XDidYToZ(gameObject8, "flinch", "away as", Object3, Object3.GetVerb(passByVerb, PrependSpace: false) + " past " + The.Player.DescribeDirectionFrom(gameObject), "!", null, null, gameObject8, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
											}
											else
											{
												IComponent<GameObject>.XDidYToZ(Object3, passByVerb, "past " + The.Player.DescribeDirectionFrom(gameObject), gameObject8, null, "!", null, null, gameObject8, null, UseFullNames: false, IndefiniteSubject: true);
											}
										}
									}
									else if (gameObject8.IsVisible())
									{
										if (gameObject8.IsCombat && gameObject8.CanChangeMovementMode("Dodging"))
										{
											IComponent<GameObject>.XDidYToZ(gameObject8, "flinch", "out of the way of", Object3, gameObject.IsPlayerControlled() ? null : The.Player.DescribeDirectionFrom(gameObject), "!", null, null, gameObject8, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
										}
										else if (!passByVerb.IsNullOrEmpty())
										{
											IComponent<GameObject>.XDidYToZ(Object3, passByVerb, "past", gameObject8, gameObject.IsPlayerControlled() ? null : The.Player.DescribeDirectionFrom(gameObject), "!", null, null, gameObject8, null, UseFullNames: false, IndefiniteSubject: true);
										}
									}
								}
								gameObject8.StopMoving();
							}
							if (!(IsSolid | Done))
							{
								continue;
							}
							bool PenetrateCreatures2 = false;
							bool PenetrateWalls2 = false;
							if (IsSolid | flag13)
							{
								cell5?.PlayWorldSound(Object3?.GetSoundTag("ImpactSound"));
							}
							if (IsSolid && !flag13)
							{
								if (Object3.IsValid())
								{
									cell5?.AddObject(Object3, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit");
								}
								if (SolidObject != null)
								{
									int num51 = Stat.Random(1, 20) + Math.Max(0, Attacker.StatMod(Modifier));
									if (IComponent<GameObject>.Visible(SolidObject))
									{
										flag8 = true;
									}
									MissileHit(Attacker, SolidObject, gameObject, Object3, projectile, Object2, gameObject6, missilePath, cell5, fireType, intParameter, num51, num51, list5[num29], gameObjectParameter, flag2, ref Done, ref PenetrateCreatures2, ref PenetrateWalls2, flag5, showUninvolved);
								}
								else
								{
									Event obj6 = Event.New("ProjectileHit");
									obj6.SetParameter("Attacker", Attacker);
									obj6.SetParameter("Defender", (object)null);
									obj6.SetParameter("Skill", Skill);
									obj6.SetParameter("Damage", (object)null);
									obj6.SetParameter("AimLevel", intParameter);
									obj6.SetParameter("Owner", Attacker);
									obj6.SetParameter("Launcher", ParentObject);
									obj6.SetParameter("Path", missilePath);
									obj6.SetParameter("Penetrations", 0);
									obj6.SetParameter("ApparentTarget", gameObject6);
									obj6.SetParameter("AimedAt", Object2);
									obj6.SetFlag("Critical", State: false);
									Object3.FireEvent(obj6);
									obj6.ID = "DefenderProjectileHit";
									gameObject8?.FireEvent(obj6);
									obj6.ID = "LauncherProjectileHit";
									ParentObject.FireEvent(obj6);
								}
							}
							bool flag14 = !Done && ((IsSolid & PenetrateWalls2) || ((IsCover & PenetrateCreatures2) && (SolidObject?.IsOrganic ?? false)));
							if (!flag14)
							{
								list4[num29] = true;
							}
							if (IsSolid && !flag14)
							{
								if (Object3.IsValid())
								{
									cell4?.AddObject(Object3, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit", null, null, null, null, null, E);
								}
							}
							else if (Done && Object3.IsValid())
							{
								cell5?.AddObject(Object3, Forced: false, System: false, IgnoreGravity: true, NoStack: true, Silent: false, Repaint: true, FlushTransient: true, null, "Missile Transit", null, null, null, null, null, E);
							}
							if (!flag14 && Object3 != null && Object3.IsValid())
							{
								Object3.WasThrown(Attacker, gameObject6);
								CleanupProjectile(Object3);
							}
						}
						if (flag7)
						{
							XRLCore._Console.DrawBuffer(scrapBuffer2);
							if (num22 > 0)
							{
								Thread.Sleep(num22);
							}
						}
					}
					if (!flag8 && list6.Count > num19 && list6[num19] && list5.Count > num19 && !list5[num19])
					{
						GameObject Object4 = list[num19];
						Projectile projectile3 = list2[num19];
						if (GameObject.Validate(ref Object4) && !projectile3.PassByVerb.IsNullOrEmpty())
						{
							IComponent<GameObject>.EmitMessage(The.Player, Object4.Does(projectile3.PassByVerb, int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: true) + " past " + The.Player.DescribeDirectionFrom(gameObject) + ".");
						}
						if (!Attacker.IsPlayerLed())
						{
							AutoAct.Interrupt(null, list7[num19], null, IsThreat: true);
						}
					}
				}
				for (int num52 = list.Count - 1; num52 >= 0; num52--)
				{
					GameObject Object5 = list[num52];
					if (GameObject.Validate(ref Object5))
					{
						Object5.Obliterate();
					}
				}
				CombatJuiceManager.endDelay();
				if (missileWeaponVFXConfiguration != null && Attacker?.CurrentZone != null && Attacker.CurrentZone.IsActive())
				{
					CombatJuice.missileWeaponVFX(missileWeaponVFXConfiguration);
				}
				float num53 = 1f;
				if (E.HasParameter("EnergyMultiplier"))
				{
					num53 = E.GetParameter<float>("EnergyMultiplier");
				}
				if (Skill == "Pistol" && num53 > 0f)
				{
					if (Attacker.HasEffect<EmptyTheClips>())
					{
						num53 *= 0.5f;
					}
					if (Attacker.HasSkill("Pistol_FastestGun"))
					{
						num53 *= 0.75f;
					}
					int intProperty = Attacker.GetIntProperty("PistolEnergyModifier");
					if (intProperty != 0)
					{
						num53 *= (100f - (float)intProperty) / 100f;
					}
				}
				ShotCompleteEvent.Send(ParentObject, gameObject, gameObject2);
				int num54 = (int)((float)EnergyCost * num53);
				if (num54 > 0)
				{
					gameObject.UseEnergy(num54, "Combat Missile " + Skill);
				}
				goto end_IL_018a;
				IL_0bdc:
				if (flag5 && gameObject6 != null && gameObject6 != Object2 && gameObject6.IsHostileTowards(gameObject) && gameObject6.IsVisible())
				{
					Sidebar.CurrentTarget = gameObject6;
				}
				goto IL_0c04;
				end_IL_018a:;
			}
			finally
			{
				if (CalculatedMissilePathInUse && missilePath == CalculatedMissilePath)
				{
					CalculatedMissilePath.Reset();
					CalculatedMissilePathInUse = false;
				}
				else if (SecondCalculatedMissilePathInUse && missilePath == SecondCalculatedMissilePath)
				{
					SecondCalculatedMissilePath.Reset();
					SecondCalculatedMissilePathInUse = false;
				}
				CombatJuiceManager.endDelay();
			}
		}
		return base.FireEvent(E);
	}

	public bool ReadyToFire()
	{
		return CheckReadyToFireEvent.Check(ParentObject);
	}

	public string GetNotReadyToFireMessage()
	{
		return GetNotReadyToFireMessageEvent.GetFor(ParentObject);
	}

	public string Status(MissileWeaponArea.MissileWeaponAreaWeaponStatus modernUIStatus = null)
	{
		if (modernUIStatus != null)
		{
			modernUIStatus.renderable = ParentObject.RenderForUI();
			modernUIStatus.text = "";
			modernUIStatus.display = "";
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GetMissileWeaponStatusEvent.Send(ParentObject, stringBuilder, modernUIStatus);
		string text = null;
		if (stringBuilder.Length > 0)
		{
			text = stringBuilder.ToString();
		}
		int num = 23;
		if (text != null)
		{
			num -= ConsoleLib.Console.ColorUtility.LengthExceptFormatting(text);
		}
		string text2;
		if (num > 0)
		{
			text2 = ParentObject.ShortDisplayNameStripped;
			if (text2.Length > num)
			{
				text2 = text2.Substring(0, num).Trim();
			}
			if (text != null)
			{
				text2 += text;
			}
		}
		else
		{
			text2 = text ?? "";
		}
		return text2;
	}

	public static bool IsVorpal(GameObject Object)
	{
		if (Object.TryGetPart<MissilePerformance>(out var Part) && !Part.AddAttributes.IsNullOrEmpty() && Part.AddAttributes.HasDelimitedSubstring(',', "Vorpal"))
		{
			return true;
		}
		if (Object.TryGetPart<PoweredMissilePerformance>(out var Part2) && !Part2.AddAttributes.IsNullOrEmpty() && Part2.AddAttributes.HasDelimitedSubstring(',', "Vorpal"))
		{
			return true;
		}
		GameObject Projectile = null;
		string Blueprint = null;
		GetMissileWeaponProjectileEvent.GetFor(Object, ref Projectile, ref Blueprint);
		if (Projectile != null)
		{
			Projectile part = Projectile.GetPart<Projectile>();
			if (part != null && part.HasAttribute("Vorpal"))
			{
				return true;
			}
		}
		if (Blueprint != null)
		{
			GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(Blueprint);
			if (blueprintIfExists != null && blueprintIfExists.GetPartParameter("Projectile", "Attributes", "").HasDelimitedSubstring(',', "Vorpal"))
			{
				return true;
			}
		}
		return false;
	}
}
