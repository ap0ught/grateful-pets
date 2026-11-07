using System;

namespace XRL.World.Parts;

[Serializable]
public class RandomSubterraneanColors : RandomColors
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		RandomizeColors();
		return base.HandleEvent(E);
	}

	public override void RandomizeColors()
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null)
		{
			if (currentZone.IsInside())
			{
				base.RandomizeColors();
			}
			else if (!KeepPartAround())
			{
				ParentObject.RemovePart(this);
			}
		}
	}
}
