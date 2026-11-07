using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainTeleport_MassTeleportOther_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they teleport all creatures surrounding @them.";
	}

	public override void Apply(GameObject Subject)
	{
		if (Subject.OnWorldMap())
		{
			return;
		}
		GameObject gameObject = null;
		foreach (Cell localAdjacentCell in Subject.CurrentCell.GetLocalAdjacentCells())
		{
			int num = 0;
			while (++num < 10)
			{
				GameObject combatObject = localAdjacentCell.GetCombatObject();
				if (combatObject == null || combatObject == gameObject)
				{
					break;
				}
				combatObject.RandomTeleport(Swirl: true);
				gameObject = combatObject;
			}
		}
	}
}
