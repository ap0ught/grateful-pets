using System;

namespace XRL.World.Parts;

[Serializable]
public class Icy : IPart
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
		E.AddAdjective("{{icy|icy}}");
		return base.HandleEvent(E);
	}
}
