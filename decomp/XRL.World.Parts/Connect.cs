using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class Connect : IPart
{
	public string Direction = "";

	public string Type = "";

	public string Object = "";

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
		Cell cell = ParentObject.CurrentCell;
		XRLCore.Core.Game.ZoneManager.AddZoneConnection(cell.ParentZone.ZoneID, Direction, cell.X, cell.Y, Type, Object);
		return base.HandleEvent(E);
	}
}
