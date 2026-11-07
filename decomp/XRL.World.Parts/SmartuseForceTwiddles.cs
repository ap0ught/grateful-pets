using System;

namespace XRL.World.Parts;

[Serializable]
public class SmartuseForceTwiddles : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID)
		{
			return ID == CommandSmartUseEarlyEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		ParentObject.Twiddle();
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
