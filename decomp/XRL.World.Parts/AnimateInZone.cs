using System;

namespace XRL.World.Parts;

[Serializable]
public class AnimateInZone : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	private void Animate(GameObject Object)
	{
		AnimateObject.Animate(Object);
		GameObjectFactory.ApplyBuilders(Object, ParentObject.GetBlueprint());
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		try
		{
			foreach (Cell item in E.Cell.IterateAdjacent(20, IncludeSelf: false, LocalOnly: true))
			{
				foreach (GameObject @object in item.Objects)
				{
					if (@object.Brain == null && AnimateObject.CanBeAnimated(@object))
					{
						Animate(@object);
						return base.HandleEvent(E);
					}
				}
			}
			GameObject gameObject = E.Cell.AddObject(PopulationManager.RollOneFrom("DynamicObjectsTable:AnimatableFurniture").Blueprint);
			Animate(gameObject);
		}
		finally
		{
			ParentObject.Destroy(null, Silent: true);
		}
		return base.HandleEvent(E);
	}
}
