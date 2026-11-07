using System;
using System.Linq;
using ConsoleLib.Console;

namespace XRL.World.Parts;

[Serializable]
public class ChasmMaterial : IPart
{
	public bool Lazy = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ZoneActivatedEvent.ID && ID != EnteredCellEvent.ID)
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (Lazy)
		{
			Lazy = false;
			FireEvent(Event.New("FirmPitEdges"));
			PaintChasm();
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckLazy();
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckLazy();
		return true;
	}

	public bool PaintChasm()
	{
		Cell cellFromDirection = ParentObject.Physics.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
		if (cellFromDirection == null)
		{
			return true;
		}
		if (cellFromDirection.ParentZone.ZoneID == ParentObject.Physics.CurrentCell.ParentZone.ZoneID)
		{
			if (cellFromDirection.HasObjectWithTag("PitDetailColor"))
			{
				ParentObject.Render.DetailColor = cellFromDirection.GetObjectsWithTag("PitDetailColor").FirstOrDefault().GetTag("PitDetailColor");
			}
			else
			{
				ParentObject.Render.DetailColor = "k";
			}
			ParentObject.Render.RenderString = " ";
			ParentObject.Render.TileColor = "&K";
			ParentObject.Render.ColorString = "&K";
			ParentObject.Render.Tile = null;
			return true;
		}
		RenderEvent renderEvent = cellFromDirection.Render();
		ParentObject.Render.RenderString = renderEvent.RenderString;
		ParentObject.Render.DetailColor = "k";
		ParentObject.Render.TileColor = "&K";
		ParentObject.Render.ColorString = "&K";
		ParentObject.Render.Tile = renderEvent.Tile;
		return true;
	}

	public bool CheckLazy()
	{
		if (Lazy && ParentObject.CurrentZone != null && !ParentObject.CurrentZone.IsActive())
		{
			string text = " ";
			ParentObject.Render.RenderString = ColorUtility.StripFormatting(text);
			ParentObject.Render.DetailColor = "k";
			ParentObject.Render.TileColor = "&K";
			ParentObject.Render.ColorString = "&K";
			ParentObject.Render.Tile = null;
			return true;
		}
		return PaintChasm();
	}
}
