using System;

namespace XRL.World.Parts;

[Serializable]
public class RootKnotStarter : IPart
{
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
		The.Game.RequireSystem<RootKnotSystem>().PlaceSummoner(ParentObject.CurrentZone);
		ParentObject.Destroy();
		return base.HandleEvent(E);
	}
}
