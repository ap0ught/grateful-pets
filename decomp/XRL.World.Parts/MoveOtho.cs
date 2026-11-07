using System;

namespace XRL.World.Parts;

[Serializable]
public class MoveOtho : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(TakenEvent E)
	{
		if (E.Actor != null && E.Actor.IsPlayer())
		{
			Zone zone = The.ZoneManager.GetZone("JoppaWorld.22.14.1.0.13");
			for (int i = 0; i < 80; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					Cell cell = zone.GetCell(i, j);
					foreach (GameObject item in cell.GetObjectsWithPart("Combat"))
					{
						if (item.Blueprint == "Otho")
						{
							cell.RemoveObject(item);
							zone.GetCell(39, 13).AddObject(item);
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}
}
