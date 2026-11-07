using System;

namespace XRL.World.Parts;

[Serializable]
public class ApplyZoneBuilder : IPart
{
	public string ZoneBuilder;

	public bool Executed;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ExecuteBuilder();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}

	private void ExecuteBuilder()
	{
		if (!Executed)
		{
			Executed = true;
			ZoneManager.ApplyBuilderToZone(ZoneBuilder, ParentObject.CurrentZone);
		}
	}
}
