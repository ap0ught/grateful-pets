using System;

namespace XRL.World.Parts;

[Serializable]
public class Flaming : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		E.AddAdjective("{{fiery|flaming}}");
		return base.HandleEvent(E);
	}
}
