using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class InteriorGround
{
	public bool BuildZone(Zone Z)
	{
		Z.ClearReachableMap();
		Z.ForeachCell((Cell c) =>
		{
			c.AddObject("InteriorVoid");
			Rocky.Paint(c);
		});
		return true;
	}
}
