using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Ecstasy : IPart
{
	public string LeaderBlueprint = "Mechanimist Priest 1";

	public override bool SameAs(IPart p)
	{
		if ((p as Ecstasy).LeaderBlueprint != LeaderBlueprint)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && cell.ParentZone != null)
			{
				foreach (GameObject item in cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 6, "Combat", ParentObject))
				{
					if (!(item.Blueprint == LeaderBlueprint))
					{
						continue;
					}
					if (!ParentObject.HasEffect<Ecstatic>())
					{
						ParentObject.ApplyEffect(new Ecstatic());
					}
					goto IL_00bf;
				}
			}
			ParentObject.RemoveEffect<Ecstatic>();
		}
		goto IL_00bf;
		IL_00bf:
		return base.FireEvent(E);
	}
}
