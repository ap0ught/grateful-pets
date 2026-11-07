using System;

namespace XRL.World.Parts;

[Serializable]
public class RandomTileOnMove : RandomTile
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
		RandomizeTile();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		RandomizeTile();
		return base.HandleEvent(E);
	}
}
