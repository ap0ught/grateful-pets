using System;

namespace XRL.World.Parts;

[Serializable]
public class MissileStatusColor : IPart
{
	public string Color;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == PooledEvent<GetMissileStatusColorEvent>.ID)
			{
				return !string.IsNullOrEmpty(Color);
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetMissileStatusColorEvent E)
	{
		if (!string.IsNullOrEmpty(Color) && ParentObject.Understood())
		{
			E.Color = Color;
		}
		return base.HandleEvent(E);
	}
}
