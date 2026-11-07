using System;

namespace XRL.World.Parts;

[Serializable]
public class Daylight : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		Cell playerCell = The.PlayerCell;
		if (playerCell == null || playerCell.ParentZone.IsInside())
		{
			return base.HandleEvent(E);
		}
		int num = Calendar.CurrentDaySegment / 500;
		int num2 = (int)((float)(Calendar.CurrentDaySegment - 500 * num) / 8.33333f);
		int num3 = 5;
		if (num < 5)
		{
			num3 = 0;
		}
		else if (num >= 5 && (num < 18 || (num == 18 && num2 < 15)))
		{
			num3 = (Calendar.CurrentDaySegment - 2500) / 10;
		}
		else
		{
			num3 = 80 - (Calendar.CurrentDaySegment - 9124) / 10;
			if (num3 < 0)
			{
				num3 = 0;
			}
		}
		LightLevel Light = LightLevel.Light;
		GetAmbientLightEvent.Send(this, "Daylight", ref Light, ref num3);
		if (num3 > playerCell.ParentZone.Width)
		{
			playerCell.ParentZone.AddLight(Light);
		}
		else if (num3 > 0)
		{
			playerCell.ParentZone.AddLight(playerCell.X, playerCell.Y, num3, Light);
		}
		if (num3 < 3)
		{
			playerCell.ParentZone.AddExplored(playerCell.X, playerCell.Y, 3);
		}
		return base.HandleEvent(E);
	}
}
