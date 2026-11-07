using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsElectromagneticSensor : IPart
{
	[NonSerialized]
	public List<GameObject> HeardObjects = new List<GameObject>();

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		GameObject implantee = ParentObject.Implantee;
		if (implantee == null || !implantee.IsPlayer())
		{
			return;
		}
		List<GameObject> heardObjects = HeardObjects;
		Cell cell = implantee.CurrentCell;
		if (cell == null)
		{
			return;
		}
		int radius = GetRadius(implantee);
		HeardObjects = cell.ParentZone.FastSquareSearch(cell.X, cell.Y, radius, "Combat");
		foreach (GameObject item in heardObjects)
		{
			if (!HeardObjects.Contains(item))
			{
				item.RemoveEffect(typeof(SenseRobotEffect), IsOurs);
			}
		}
		int i = 0;
		for (int count = HeardObjects.Count; i < count; i++)
		{
			GameObject gameObject = HeardObjects[i];
			if (!gameObject.HasEffect(typeof(SenseRobotEffect), IsOurs) && WillSense(gameObject))
			{
				gameObject.ApplyEffect(new SenseRobotEffect(radius, implantee, ParentObject));
			}
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<ExtraHostilePerceptionEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ExtraHostilePerceptionEvent E)
	{
		GameObject implantee = ParentObject.Implantee;
		if (implantee != null && E.Actor == implantee && implantee.IsPlayer())
		{
			Cell cell = implantee.CurrentCell;
			if (cell != null)
			{
				List<GameObject> list = cell.ParentZone.FastSquareSearch(cell.X, cell.Y, GetRadius(), "Combat");
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					GameObject gameObject = list[i];
					if (gameObject.GetEffect(typeof(SenseRobotEffect), IsOurs) is SenseRobotEffect senseRobotEffect && senseRobotEffect.Listener == implantee && senseRobotEffect.Identified && implantee.IsRelevantHostile(gameObject))
					{
						E.Hostile = gameObject;
						E.PerceiveVerb = "detect";
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's range.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		foreach (GameObject heardObject in HeardObjects)
		{
			heardObject.RemoveEffect(typeof(SenseRobotEffect), IsOurs);
		}
		HeardObjects.Clear();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool WillSense(GameObject obj)
	{
		if (obj.HasPart<Robot>())
		{
			return true;
		}
		if (obj.HasTag("Robot"))
		{
			return true;
		}
		return false;
	}

	public bool IsOurs(Effect FX)
	{
		if (!(FX is SenseRobotEffect senseRobotEffect))
		{
			return false;
		}
		return senseRobotEffect.Device == ParentObject;
	}

	public int GetRadius(GameObject who)
	{
		return GetAvailableComputePowerEvent.AdjustUp(who, 9);
	}

	public int GetRadius()
	{
		return GetRadius(ParentObject.Implantee);
	}
}
