using System;

namespace XRL.World.Parts;

[Serializable]
public class RandomColorsOnMove : RandomColors
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		RandomizeColors();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		RandomizeColors();
		return base.HandleEvent(E);
	}

	public override bool KeepPartAround()
	{
		return true;
	}
}
