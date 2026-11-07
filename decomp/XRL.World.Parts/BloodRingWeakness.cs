using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class BloodRingWeakness : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeforeBeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && ParentObject.Brain != null && !ParentObject.HasEffect<Terrified>())
		{
			foreach (GameObject item in cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 5, "Body", ParentObject))
			{
				if (item.IsPlayer() && ParentObject.IsHostileTowards(item) && item.HasEquippedItem("Blood-stained neck-ring"))
				{
					ParentObject.ApplyEffect(new Terrified(2, item, Psionic: false, Silent: true));
					break;
				}
			}
		}
		return base.HandleEvent(E);
	}
}
