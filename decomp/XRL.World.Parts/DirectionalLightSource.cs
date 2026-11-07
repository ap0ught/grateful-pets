using System;
using Genkit;

namespace XRL.World.Parts;

[Serializable]
public class DirectionalLightSource : IPart
{
	public bool Darkvision;

	public bool OnlyIfCharged;

	public bool Lit = true;

	public string Directions = "NSEW";

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
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return true;
		}
		if (OnlyIfCharged && (IsBroken() || IsRusted() || !ParentObject.TestCharge(1, LiveOnly: false, 0L)))
		{
			return true;
		}
		LightLevel level = (Darkvision ? LightLevel.Darkvision : LightLevel.Light);
		for (int i = 0; i < Directions.Length; i++)
		{
			Location2D location2D = cell.Location;
			while (location2D != null && cell.ParentZone?.GetCell(location2D) != null)
			{
				cell.ParentZone.MixLight(cell.X, cell.Y, location2D.X, location2D.Y, level);
				if (Directions[i] == 'N')
				{
					location2D = location2D.FromDirection("N");
				}
				if (Directions[i] == 'S')
				{
					location2D = location2D.FromDirection("S");
				}
				if (Directions[i] == 'E')
				{
					location2D = location2D.FromDirection("E");
				}
				if (Directions[i] == 'W')
				{
					location2D = location2D.FromDirection("W");
				}
			}
		}
		return base.HandleEvent(E);
	}
}
