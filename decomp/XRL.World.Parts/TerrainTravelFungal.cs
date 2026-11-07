using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class TerrainTravelFungal : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CheckLostChance");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CheckLostChance")
		{
			if (The.Core.IDKFA)
			{
				return false;
			}
			Cell cell = The.Player.CurrentCell;
			string zoneWorld = cell.ParentZone.GetZoneWorld();
			int x = cell.X;
			int y = cell.Y;
			int zoneX = Stat.Random(0, 2);
			int zoneY = Stat.Random(0, 2);
			int zoneZ = 10;
			if (x == FungalJungle.UpperLeft.x)
			{
				zoneX = 0;
			}
			if (y == FungalJungle.UpperLeft.y)
			{
				zoneY = 0;
			}
			if (x == FungalJungle.LowerRight.x)
			{
				zoneX = 2;
			}
			if (y == FungalJungle.LowerRight.y)
			{
				zoneY = 2;
			}
			string text = ZoneID.Assemble(zoneWorld, x, y, zoneX, zoneY, zoneZ);
			Lost e = new Lost(9999, text, zoneWorld);
			if (The.Player.ApplyEffect(e))
			{
				Zone zone = The.ZoneManager.GetZone(text);
				Zone zone2 = The.ZoneManager.SetActiveZone(zone);
				zone2.CheckWeather();
				The.Player.SystemMoveTo(zone2.GetPullDownLocation(The.Player));
				Popup.ShowBlock("You lose your way beneath a dense canopy of spores.");
				The.ZoneManager.ProcessGoToPartyLeader();
				The.Player.FireEvent(Event.New("AfterLost", "FromCell", cell));
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
