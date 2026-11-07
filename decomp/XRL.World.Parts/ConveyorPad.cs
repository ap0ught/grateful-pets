using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ConveyorPad : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 30;

	public string Direction = "N";

	public string Connections = "";

	public int nFrameOffset;

	[NonSerialized]
	private long lastImpuse;

	[NonSerialized]
	private long tileTickTimer;

	[NonSerialized]
	private int nTile;

	public ConveyorPad()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		CheckConnections();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		_ = ParentObject.Physics;
		_ = ParentObject.Render;
		int num = (XRLCore.CurrentFrame + nFrameOffset) % 60;
		string renderString = "º";
		if (Connections == "S")
		{
			renderString = "º";
		}
		if (Connections == "E")
		{
			renderString = "Í";
		}
		if (Connections == "W")
		{
			renderString = "Í";
		}
		if (Connections == "EW")
		{
			renderString = "Í";
		}
		if (Connections == "WE")
		{
			renderString = "Í";
		}
		if (Connections == "NW" || Connections == "WN")
		{
			renderString = "¼";
		}
		if (Connections == "NE" || Connections == "EN")
		{
			renderString = "È";
		}
		if (Connections == "SW" || Connections == "WS")
		{
			renderString = "»";
		}
		if (Connections == "SE" || Connections == "ES")
		{
			renderString = "É";
		}
		if (Connections == "NWE")
		{
			renderString = "Ê";
		}
		if (Connections == "SWE")
		{
			renderString = "Ë";
		}
		if (Connections == "NSE")
		{
			renderString = "Ê";
		}
		if (Connections == "NSW")
		{
			renderString = "¹";
		}
		if (Connections == "NSEW")
		{
			renderString = "Î";
		}
		string text = "Tiles/sw_conveyor_w_";
		if (Connections == "S")
		{
			text = "Tiles/sw_conveyor_w_";
		}
		if (Connections == "E")
		{
			text = "Tiles/sw_conveyor_n_";
		}
		if (Connections == "W")
		{
			text = "Tiles/sw_conveyor_n_";
		}
		if (Connections == "EW")
		{
			text = "Tiles/sw_conveyor_n_";
		}
		if (Connections == "WE")
		{
			text = "Tiles/sw_conveyor_n_";
		}
		if (Connections == "NW" || Connections == "WN")
		{
			text = "Tiles/sw_conveyor_se_";
		}
		if (Connections == "NE" || Connections == "EN")
		{
			text = "Tiles/sw_conveyor_sw_";
		}
		if (Connections == "SW" || Connections == "WS")
		{
			text = "Tiles/sw_conveyor_ne_";
		}
		if (Connections == "SE" || Connections == "ES")
		{
			text = "Tiles/sw_conveyor_nw_";
		}
		if (Connections == "NWE")
		{
			text = "Tiles/sw_conveyor_ne_";
		}
		if (Connections == "SWE")
		{
			text = "Tiles/sw_conveyor_se_";
		}
		if (Connections == "NSE")
		{
			text = "Tiles/sw_conveyor_sw_";
		}
		if (Connections == "NSW")
		{
			text = "Tiles/sw_conveyor_se_";
		}
		if (Connections == "NSEW")
		{
			text = "Tiles/sw_conveyor_n_";
		}
		if (tileTickTimer == 0L)
		{
			tileTickTimer = IComponent<GameObject>.wallTime;
		}
		int num2 = 1;
		if (Connections == "NS" && Direction == "S")
		{
			num2 = -1;
		}
		if (Connections == "SN" && Direction == "S")
		{
			num2 = -1;
		}
		if (Connections == "WS" && Direction == "W")
		{
			num2 = -1;
		}
		if (Connections == "SW" && Direction == "W")
		{
			num2 = -1;
		}
		if (Connections == "NW" && Direction == "N")
		{
			num2 = -1;
		}
		if (Connections == "WN" && Direction == "N")
		{
			num2 = -1;
		}
		if (Connections == "EN" && Direction == "E")
		{
			num2 = -1;
		}
		if (Connections == "NE" && Direction == "E")
		{
			num2 = -1;
		}
		if (Connections == "SE" && Direction == "S")
		{
			num2 = -1;
		}
		if (Connections == "ES" && Direction == "S")
		{
			num2 = -1;
		}
		if (Connections == "WE" && Direction == "W")
		{
			num2 = -1;
		}
		if (Connections == "EW" && Direction == "W")
		{
			num2 = -1;
		}
		if ((lastImpuse <= 0 || lastImpuse >= The.Game.Turns - 2) && IComponent<GameObject>.wallTime - tileTickTimer >= 150)
		{
			nTile += num2;
			tileTickTimer = IComponent<GameObject>.wallTime;
		}
		if (nTile > 4)
		{
			nTile = 1;
		}
		else if (nTile < 1)
		{
			nTile = 4;
		}
		E.Tile = text + nTile + ".bmp";
		string text2 = null;
		if (Globals.RenderMode == RenderModeType.Tiles)
		{
			text2 = "&y";
		}
		else if (num < 15)
		{
			E.RenderString = renderString;
			text2 = "&K";
		}
		else if (num < 30)
		{
			E.RenderString = renderString;
			text2 = "&y";
		}
		else if (num < 45)
		{
			E.RenderString = renderString;
			text2 = "&Y";
		}
		else
		{
			E.RenderString = renderString;
			text2 = "&y";
		}
		if (!text2.IsNullOrEmpty())
		{
			E.ApplyColors(text2, ICON_COLOR_PRIORITY);
		}
		return base.Render(E);
	}

	public bool CanBeMovedByConveyor(GameObject obj)
	{
		if (obj == ParentObject)
		{
			return false;
		}
		if (!obj.IsReal)
		{
			return false;
		}
		if (obj.HasTag("NoConveyor"))
		{
			return false;
		}
		if (obj.HasPart<Door>())
		{
			return false;
		}
		if (obj.HasPart<LiquidVolume>())
		{
			return false;
		}
		if (obj.GetMatterPhase() >= 3)
		{
			return false;
		}
		if (obj.HasPart<Forcefield>() || obj.HasPart<CryoZone>() || obj.HasPart<PyroZone>())
		{
			return false;
		}
		if (obj.HasTagOrProperty("ExcavatoryTerrainFeature"))
		{
			return false;
		}
		if (!obj.CanBeInvoluntarilyMoved())
		{
			return false;
		}
		if (!obj.PhaseAndFlightMatches(ParentObject))
		{
			return false;
		}
		return true;
	}

	public bool ConveyorImpulse(int Timing, List<GameObject> Pads)
	{
		if (Pads.Contains(ParentObject))
		{
			return true;
		}
		if (IsEMPed() || IsBroken() || IsRusted())
		{
			return true;
		}
		lastImpuse = The.Game.Turns;
		CheckConnections();
		Pads.Add(ParentObject);
		nFrameOffset = Timing;
		Timing -= 10;
		if (Timing < 0)
		{
			Timing = 60;
		}
		if (ParentObject.CurrentCell != null)
		{
			Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection(Direction, !ParentObject.IsVisible());
			if (cellFromDirection != null)
			{
				foreach (GameObject item in cellFromDirection.GetObjectsWithPartReadonly("ConveyorPad"))
				{
					item.GetPart<ConveyorPad>().ConveyorImpulse(Timing, Pads);
				}
			}
			foreach (GameObject item2 in ParentObject.CurrentCell.GetObjectsWithPartReadonly("Physics"))
			{
				if (CanBeMovedByConveyor(item2))
				{
					item2.Move(Direction, Forced: true);
				}
			}
		}
		return true;
	}

	private void CheckConnections()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			Connections = Direction;
			Cell cellFromDirection = cell.GetCellFromDirection("N");
			if (cellFromDirection != null && cellFromDirection.HasObjectWithPart("ConveyorPad") && cellFromDirection.GetFirstObjectWithPart("ConveyorPad").GetPart<ConveyorPad>().Direction == "S")
			{
				Connections = Direction + "N";
			}
			cellFromDirection = cell.GetCellFromDirection("S");
			if (cellFromDirection != null && cellFromDirection.HasObjectWithPart("ConveyorPad") && cellFromDirection.GetFirstObjectWithPart("ConveyorPad").GetPart<ConveyorPad>().Direction == "N")
			{
				Connections = Direction + "S";
			}
			cellFromDirection = cell.GetCellFromDirection("E");
			if (cellFromDirection != null && cellFromDirection.HasObjectWithPart("ConveyorPad") && cellFromDirection.GetFirstObjectWithPart("ConveyorPad").GetPart<ConveyorPad>().Direction == "W")
			{
				Connections = Direction + "E";
			}
			cellFromDirection = cell.GetCellFromDirection("W");
			if (cellFromDirection != null && cellFromDirection.HasObjectWithPart("ConveyorPad") && cellFromDirection.GetFirstObjectWithPart("ConveyorPad").GetPart<ConveyorPad>().Direction == "E")
			{
				Connections = Direction + "W";
			}
		}
	}
}
