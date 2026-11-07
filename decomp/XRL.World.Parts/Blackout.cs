using System;

namespace XRL.World.Parts;

[Serializable]
public class Blackout : IPart
{
	public bool OnlyIfCharged;

	public bool Lit = true;

	public bool ConfusionSuspends = true;

	public int Radius = 5;

	public override bool SameAs(IPart p)
	{
		Blackout blackout = p as Blackout;
		if (blackout.OnlyIfCharged != OnlyIfCharged)
		{
			return false;
		}
		if (blackout.Lit != Lit)
		{
			return false;
		}
		if (blackout.ConfusionSuspends != ConfusionSuspends)
		{
			return false;
		}
		if (blackout.Radius != Radius)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (E.Pass == 1)
		{
			E.AfterHandlers.Add(this);
		}
		else if (E.Pass == 2)
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null && IsActive())
			{
				if (Radius == 999)
				{
					cell.ParentZone.BlackoutAll();
				}
				else
				{
					cell.ParentZone.RemoveLight(cell.X, cell.Y, Radius, LightLevel.Blackout);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public bool IsActive()
	{
		if (!Lit)
		{
			return false;
		}
		if (OnlyIfCharged && !ParentObject.TestCharge(1, LiveOnly: false, 0L))
		{
			return false;
		}
		if (ConfusionSuspends && ParentObject.IsConfused)
		{
			return false;
		}
		return true;
	}
}
