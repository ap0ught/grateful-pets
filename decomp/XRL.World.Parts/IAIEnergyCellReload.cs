using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public abstract class IAIEnergyCellReload : IPart
{
	public void CheckEnergyCellReload(IAICommandListEvent E, int ToleranceFactor = 2, int RequirePercent = 110)
	{
		if (ParentObject.Equipped != E.Actor)
		{
			return;
		}
		Inventory inventory = E.Actor.Inventory;
		if (inventory == null || E.Actor.Stat("Intelligence") < 7)
		{
			return;
		}
		EnergyCellSocket part = ParentObject.GetPart<EnergyCellSocket>();
		if (part == null)
		{
			return;
		}
		int num = QueryDrawEvent.GetFor(ParentObject);
		if (num <= 0)
		{
			return;
		}
		int num2 = ParentObject.QueryCharge(LiveOnly: false, 0L);
		if (num2 >= num * ToleranceFactor)
		{
			return;
		}
		List<GameObject> list = Event.NewGameObjectList();
		inventory.GetObjects(list, part.CompatibleCell);
		if (list.Count == 0)
		{
			return;
		}
		int num3 = num2 * RequirePercent / 100;
		if (RequirePercent > 100)
		{
			if (num3 <= num2)
			{
				num3 = num2 + 1;
			}
		}
		else if (RequirePercent < 100 && num3 >= num2)
		{
			num3 = num2 - 1;
		}
		GameObject gameObject = null;
		int num4 = 0;
		foreach (GameObject item in list)
		{
			int num5 = item.QueryCharge(LiveOnly: false, 0L);
			if (num5 >= num3 && (gameObject == null || num5 > num4))
			{
				gameObject = item;
				num4 = num5;
			}
		}
		if (gameObject != null)
		{
			E.Add(EnergyCellSocket.REPLACE_CELL_INTERACTION, 1, ParentObject, Inv: true, Self: false, gameObject);
		}
	}
}
