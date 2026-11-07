using System;

namespace XRL.World.Parts;

[Serializable]
public class ExcludeMods : IPart
{
	public string Exclude;

	public override bool SameAs(IPart p)
	{
		if ((p as ExcludeMods).Exclude != Exclude)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == CanBeModdedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeModdedEvent E)
	{
		if (Exclude == "*")
		{
			return false;
		}
		if (!Exclude.IsNullOrEmpty() && !E.ModName.IsNullOrEmpty() && Exclude.HasDelimitedSubstring(',', E.ModName))
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
