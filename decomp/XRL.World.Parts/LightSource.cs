using System;

namespace XRL.World.Parts;

[Serializable]
public class LightSource : IPart, ILightSource
{
	public bool Darkvision;

	public bool OnlyIfCharged;

	public bool Lit = true;

	public int Radius = 5;

	public override bool SameAs(IPart p)
	{
		LightSource lightSource = p as LightSource;
		if (lightSource.Darkvision != Darkvision)
		{
			return false;
		}
		if (lightSource.OnlyIfCharged != OnlyIfCharged)
		{
			return false;
		}
		if (lightSource.Lit != Lit)
		{
			return false;
		}
		if (lightSource.Radius != Radius)
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
		RadiateLight();
		return base.HandleEvent(E);
	}

	public bool RadiateLight()
	{
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return false;
		}
		if (OnlyIfCharged && (IsBroken() || IsRusted() || !ParentObject.TestCharge(1, LiveOnly: false, 0L)))
		{
			return false;
		}
		if (Darkvision)
		{
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null && equipped.IsPlayer() && Lit)
			{
				if (Radius == 9999)
				{
					cell.ParentZone.LightAll(LightLevel.Darkvision);
				}
				else
				{
					cell.ParentZone.AddLight(cell.X, cell.Y, Radius, LightLevel.Darkvision);
				}
			}
		}
		else if (Lit)
		{
			if (Radius == 9999)
			{
				cell.ParentZone.LightAll();
			}
			else
			{
				cell.ParentZone.AddLight(cell.X, cell.Y, Radius);
			}
		}
		return true;
	}

	int ILightSource.GetRadius()
	{
		return Radius;
	}

	bool ILightSource.IsActive()
	{
		return Lit;
	}

	bool ILightSource.IsDarkvision()
	{
		return Darkvision;
	}
}
