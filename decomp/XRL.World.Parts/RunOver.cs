using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class RunOver : IPart
{
	public static readonly string COMMAND_NAME = "CommandRunOver";

	public string Damage = "40-60";

	public string BreakSolidAV = "30";

	public int MaxTargetDistance = 6;

	public int KnockdownSaveTarget = 35;

	public string KnockdownSaveStat = "Strength";

	public string KnockdownSaveVs = "RunOver Knockdown";

	public int DazeSaveTarget = 35;

	public string DazeSaveStat = "Toughness";

	public string DazeSaveVs = "RunOver Daze";

	public int Charging;

	public Guid ActivatedAbilityID;

	[NonSerialized]
	private List<Cell> ChargeCells;

	public override void Initialize()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Run Over", COMMAND_NAME, "Maneuvers", null, "\u00af");
		base.Initialize();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != LeftCellEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Damage", Damage);
		stats.Set("Range", MaxTargetDistance);
		stats.Set("Knockdown", $"{KnockdownSaveStat} {KnockdownSaveTarget}");
		stats.Set("Daze", $"{DazeSaveStat} {DazeSaveTarget}");
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell == null || cell.OnWorldMap())
			{
				return E.Actor.Fail("You can't do that here.");
			}
			PickChargeTarget();
			if (ChargeCells == null || ChargeCells.Count <= 0)
			{
				return false;
			}
			Charging = GetTurnsToCharge();
			ParentObject.UseEnergy(1000, "Physical Ability RunOver");
			PlayWorldSound("sfx_ability_longBeam_attack_chargeUp");
			DidX("stare", null, null, null, null, ParentObject);
			if (Visible() && AutoAct.IsInterruptable())
			{
				AutoAct.Interrupt(null, null, ParentObject, IsThreat: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (Charging <= 0 && E.Distance <= MaxTargetDistance && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && E.Actor.PhaseMatches(E.Target) && E.Actor.FlightCanReach(E.Target) && !E.Target.IsInStasis() && E.Actor.HasLOSTo(E.Target))
		{
			E.Add(COMMAND_NAME, 10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Charging > 0)
		{
			if (ChargeCells == null || ChargeCells.Count == 0)
			{
				Charging = 0;
			}
			else
			{
				Charging--;
				ParentObject.UseEnergy(1000, "Physical Ability RunOver");
				if (Charging > 0)
				{
					if (ParentObject.IsPlayer())
					{
						The.Core.RenderDelay(500);
					}
					return false;
				}
				PerformCharge(ChargeCells);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		ChargeCells = null;
		Charging = 0;
		return base.HandleEvent(E);
	}

	private bool ValidChargeTarget(GameObject obj)
	{
		return obj?.IsCombatObject() ?? false;
	}

	public void PickChargeTarget()
	{
		if (Charging > 0)
		{
			return;
		}
		ChargeCells = PickLine(MaxTargetDistance, AllowVis.OnlyVisible, ValidChargeTarget, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, BlackoutStops: false, ParentObject, null, "Run Over");
		if (ChargeCells == null)
		{
			return;
		}
		ChargeCells = new List<Cell>(ChargeCells);
		ChargeCells.Insert(0, ParentObject.CurrentCell);
		if (ChargeCells.Count <= 1 || ChargeCells[0].ParentZone != ChargeCells[1].ParentZone)
		{
			return;
		}
		while (ChargeCells.Count <= MaxTargetDistance)
		{
			for (int i = 0; i < ChargeCells.Count - 1; i++)
			{
				if (ChargeCells.Count > MaxTargetDistance)
				{
					break;
				}
				if (ChargeCells[i].ParentZone != ChargeCells[i + 1].ParentZone)
				{
					break;
				}
				string directionFromCell = ChargeCells[i].GetDirectionFromCell(ChargeCells[i + 1]);
				ChargeCells.Add(ChargeCells[ChargeCells.Count - 1].GetCellFromDirection(directionFromCell, BuiltOnly: false));
			}
		}
	}

	public string PathDirectionAtStep(int n, List<Cell> path)
	{
		if (path.Count <= 1)
		{
			return ".";
		}
		n %= path.Count;
		return path[n].GetDirectionFromCell(path[(n + 1) % path.Count]);
	}

	public Cell CellAtStep(int n, List<Cell> path)
	{
		if (path.Count <= 1)
		{
			return path[0];
		}
		if (n < path.Count)
		{
			return path[n];
		}
		Cell cell = path[path.Count - 1];
		int num = n % path.Count;
		n %= path.Count;
		for (int i = 0; i <= n; i++)
		{
			cell = cell.GetCellFromDirection(PathDirectionAtStep(num, path), BuiltOnly: false);
			num++;
		}
		return cell;
	}

	public void PerformCharge(List<Cell> ChargePath, bool bDoEffect = true)
	{
		Charging = 0;
		DidX("charge", null, "!");
		bool flag = ParentObject.HasPart<Robot>();
		int i = 1;
		for (int num = ChargePath.Count; i < num; i++)
		{
			Cell cell = CellAtStep(i, ChargePath);
			if (cell == null)
			{
				break;
			}
			if (flag)
			{
				GameObject firstObjectWithPropertyOrTag = cell.GetFirstObjectWithPropertyOrTag("RobotStop");
				if (firstObjectWithPropertyOrTag != null)
				{
					DidXToY("are", "stopped in " + ParentObject.its + " tracks by", firstObjectWithPropertyOrTag, null, "!", null, null, null, ParentObject);
					break;
				}
			}
			foreach (GameObject item in Event.NewGameObjectList(cell.Objects))
			{
				if (item == ParentObject)
				{
					continue;
				}
				bool flag2 = item.IsCombatObject();
				if (flag2)
				{
					num = Math.Max(num, i + 2);
				}
				if (flag2 && ParentObject.PhaseMatches(item) && ParentObject.FlightCanReach(item))
				{
					DidXToY("run", "over", item, null, null, null, null, ParentObject);
					if (!item.MakeSave(KnockdownSaveStat, KnockdownSaveTarget, null, null, KnockdownSaveVs))
					{
						item.ApplyEffect(new Prone());
					}
					if (!item.MakeSave(DazeSaveStat, DazeSaveTarget, null, null, DazeSaveVs))
					{
						item.ApplyEffect(new Dazed());
					}
					int num2 = Damage.RollCached();
					if (num2 > 0)
					{
						item.TakeDamage(num2, "being run over by %O.", null, null, null, null, ParentObject);
					}
				}
				else if (item.ConsiderSolidFor(ParentObject))
				{
					if (BreakSolidAV.RollCached() < Stats.GetCombatAV(item))
					{
						DidXToY("are", "stopped in " + ParentObject.its + " tracks by", item, null, "!", null, null, null, ParentObject);
						break;
					}
					if (IComponent<GameObject>.Visible(item))
					{
						CombatJuice._cameraShake(0.25f);
						item.DustPuff();
					}
					int j = 0;
					for (int count = item.Count; j < count; j++)
					{
						item.Destroy();
					}
				}
			}
			ParentObject.DirectMoveTo(cell, 0, Forced: false, IgnoreCombat: true, IgnoreGravity: true);
			if (cell.IsVisible())
			{
				The.Core.RenderDelay(10, Interruptible: false);
			}
		}
		if (ParentObject.ShouldShunt())
		{
			Cell cell2 = ParentObject.CurrentCell?.GetFirstEmptyAdjacentCell();
			if (cell2 != null)
			{
				ParentObject.DirectMoveTo(cell2, 0, Forced: false, IgnoreCombat: true, IgnoreGravity: true);
			}
		}
		ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_charge_mega");
		ParentObject.Gravitate();
	}

	public int GetTurnsToCharge()
	{
		return 1;
	}

	public override bool Render(RenderEvent E)
	{
		if (ChargeCells != null && ChargeCells.Count > 0 && ParentObject.CurrentCell != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				E.ColorString = "&r^R";
			}
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (ChargeCells != null)
		{
			int num = 1000;
			int val = num / Math.Max(ChargeCells.Count, 1);
			int num2 = (int)(IComponent<GameObject>.frameTimerMS % num / Math.Max(val, 1));
			if (num2 > 0 && num2 < ChargeCells.Count && ChargeCells[num2].ParentZone == ParentObject.CurrentZone && ChargeCells[num2].IsVisible())
			{
				buffer.Goto(ChargeCells[num2].X, ChargeCells[num2].Y);
				buffer.Write(ParentObject.Render.RenderString);
				buffer.Buffer[ChargeCells[num2].X, ChargeCells[num2].Y].Tile = ParentObject.Render.Tile;
				buffer.Buffer[ChargeCells[num2].X, ChargeCells[num2].Y].TileForeground = The.Color.DarkRed;
				buffer.Buffer[ChargeCells[num2].X, ChargeCells[num2].Y].Detail = The.Color.Red;
				buffer.Buffer[ChargeCells[num2].X, ChargeCells[num2].Y].SetForeground('r');
			}
			base.OnPaint(buffer);
		}
	}
}
