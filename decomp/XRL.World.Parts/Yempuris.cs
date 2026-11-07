using System;
using System.Collections.Generic;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class Yempuris : IPart
{
	public string ClusterSize = "1";

	public string Damage = "1d12";

	public string[] D = new string[8] { "NW", "SE", "SW", "NE", "N", "S", "E", "W" };

	public static long LastSplitTurn;

	public static long Growth;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectEnteredCell");
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (ParentObject.Physics.IsFrozen())
			{
				return true;
			}
			if (LastSplitTurn == XRLCore.Core.Game.Turns)
			{
				return true;
			}
			if (ParentObject.Physics.CurrentCell != null)
			{
				int num = 0;
				List<Cell> localAdjacentCells = ParentObject.Physics.CurrentCell.GetLocalAdjacentCells();
				for (int i = 0; i < localAdjacentCells.Count; i++)
				{
					if (localAdjacentCells[i].HasObjectWithBlueprint("Yempuris"))
					{
						num++;
					}
				}
				if (num > 6)
				{
					ParentObject.Destroy();
					return true;
				}
				string[] d = D;
				foreach (string direction in d)
				{
					Cell localCellFromDirection = ParentObject.Physics.CurrentCell.GetLocalCellFromDirection(direction);
					if (localCellFromDirection != null && !localCellFromDirection.HasObjectWithBlueprint("Yempuris"))
					{
						localCellFromDirection.AddObject("Yempuris");
						LastSplitTurn = XRLCore.Core.Game.Turns;
						return true;
					}
					if (50.in100())
					{
						return true;
					}
				}
			}
		}
		else if (E.ID == "ObjectEnteredCell")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && gameObjectParameter.IsCombat && ParentObject.IsHostileTowards(gameObjectParameter) && gameObjectParameter.PhaseAndFlightMatches(ParentObject) && gameObjectParameter.TakeDamage(Damage.RollCached(), "from %t impalement.", "Thorns", null, null, null, ParentObject))
			{
				if (gameObjectParameter.Energy != null)
				{
					gameObjectParameter.Energy.BaseValue -= 500;
				}
				gameObjectParameter.Bloodsplatter();
			}
		}
		return base.FireEvent(E);
	}
}
