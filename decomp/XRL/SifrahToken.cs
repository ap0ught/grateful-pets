using System;
using XRL.World;

namespace XRL;

/// This class is not used in the base game.
[Serializable]
public class SifrahToken : SifrahRenderable
{
	public string Description;

	public int EliminatedAt = -1;

	public bool UsabilityCheckedThisTurn;

	public bool DisabledThisTurn;

	public bool Eliminated => EliminatedAt != -1;

	public virtual string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		return Description;
	}

	public virtual bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		return DisabledThisTurn;
	}

	public virtual bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		return true;
	}

	public virtual void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
	}

	public virtual int GetPowerup(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		return 0;
	}
}
