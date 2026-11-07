using System;

namespace XRL.World.Parts;

[Serializable]
public class EngulfingSedentary : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeforeTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeTakeActionEvent E)
	{
		Engulfing part = ParentObject.GetPart<Engulfing>();
		if (part?.Engulfed != null && part.CheckEngulfed())
		{
			ParentObject.UseEnergy(1000, "Digestion");
			return false;
		}
		return base.HandleEvent(E);
	}
}
