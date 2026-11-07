using System;

namespace XRL.World.Parts;

[Serializable]
public class StarOrchidTempleWidget : IPart
{
	public class Handler : IEventHandler
	{
		public bool HandleEvent(GetZoneEvent E)
		{
			if (E.ZoneID.StartsWith(MapCell, StringComparison.Ordinal))
			{
				ZoneID.Parse(E.ZoneID, out var World, out var ParasangX, out var ParasangY, out var ZoneX, out var ZoneY, out var ZoneZ);
				if (ZoneX != 1 || ZoneY != 1)
				{
					E.Result = The.ZoneManager.GetZone(ZoneID.Assemble(World, ParasangX, ParasangY, 1, 1, ZoneZ));
					return false;
				}
			}
			return true;
		}
	}

	public static string MapCell;

	public static Handler HandlerInstance;

	public override bool WantEvent(int ID, int cascade)
	{
		return ID == ZoneActivatedEvent.ID;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		MapCell = $"{E.Zone.ZoneWorld}.{E.Zone.wX}.{E.Zone.wY}";
		The.Game.RegisterEvent(HandlerInstance ?? (HandlerInstance = new Handler()), PooledEvent<GetZoneEvent>.ID);
		return base.HandleEvent(E);
	}
}
