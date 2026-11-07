using System;

namespace XRL.World.Parts;

[Serializable]
public class CatacombsExitTeleporter : IPoweredPart
{
	public string TargetZones = "JoppaWorld.53.3.2.0.11,JoppaWorld.53.3.2.2.11";

	public CatacombsExitTeleporter()
	{
		ChargeUse = 0;
		WorksOnCellContents = true;
		NameForStatus = "MatterRecompositionSystem";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetAdjacentNavigationWeightEvent.ID && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		E.MinWeight(60);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object != ParentObject && IsObjectActivePartSubject(E.Object) && E.Object.IsCombatObject())
		{
			string randomElement = TargetZones.CachedCommaExpansion().GetRandomElement();
			Cell cell = The.ZoneManager.GetZone(randomElement).GetFirstObjectWithPart("StairsUp")?.CurrentCell?.GetConnectedSpawnLocation();
			Cell cell2 = E.Object.CurrentCell;
			if (cell != null)
			{
				E.Object.TeleportTo(cell, 0);
				if (E.Object.IsPlayer() && E.Object.CurrentCell != cell2)
				{
					cell.ParentZone.SetActive();
					IComponent<GameObject>.AddPlayerMessage("You are teleported to an exit.");
				}
				E.Object.TeleportSwirl();
			}
		}
		return base.HandleEvent(E);
	}
}
