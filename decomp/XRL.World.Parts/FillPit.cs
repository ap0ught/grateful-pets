using System;

namespace XRL.World.Parts;

[Serializable]
public class FillPit : IPart
{
	public bool DestroySelf;

	public string ReplaceWith;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforePullDownEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforePullDownEvent E)
	{
		if (ReplaceWith.IsNullOrEmpty())
		{
			E.Pit.Obliterate();
		}
		else
		{
			E.Pit.ReplaceWith(ReplaceWith);
		}
		if (DestroySelf)
		{
			ParentObject.Destroy();
		}
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
