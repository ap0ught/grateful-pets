using System;

namespace XRL.World.Parts;

[Serializable]
public class DisplayNameColor : IPart
{
	public string Color;

	public int ColorPriority = 20;

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
		if (!Color.IsNullOrEmpty())
		{
			E.AddColor(Color, ColorPriority);
		}
		return base.HandleEvent(E);
	}

	public void SetColor(string Color)
	{
		this.Color = Color;
	}

	public void SetColorAndPriority(string Color, int Priority)
	{
		this.Color = Color;
		ColorPriority = Priority;
	}

	public void SetColorByPriority(string Color, int Priority)
	{
		if (this.Color.IsNullOrEmpty() || ColorPriority < Priority)
		{
			this.Color = Color;
			ColorPriority = Priority;
		}
	}
}
