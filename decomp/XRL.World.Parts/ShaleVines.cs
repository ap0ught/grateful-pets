using System;

namespace XRL.World.Parts;

[Serializable]
public class ShaleVines : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public void GrowVines()
	{
		if (ParentObject.CurrentCell != null && ParentObject.CurrentCell.AnyAdjacentCell((Cell c) => c.HasObjectWithPart("LiquidVolume") && c.HasOpenLiquidVolume() && c.GetOpenLiquidVolume().IsWaterPuddle() && c.HasWadingDepthLiquid()))
		{
			ParentObject.SetStringProperty("PaintedWall", "vineshale1");
			ParentObject.SetStringProperty("PaintedWallExtension", ".png");
			ParentObject.RemoveStringProperty("paintedWallSubstring");
			ParentObject.Render.ColorString = "&r^g";
			ParentObject.Render.TileColor = "&r";
			ParentObject.Render.DetailColor = "g";
		}
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		GrowVines();
		return base.HandleEvent(E);
	}
}
