using System;

namespace XRL.World.Parts;

[Serializable]
public class SoundOnDestroy : ISoundPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (ID != BeforeDestroyObjectEvent.ID)
		{
			return base.WantEvent(ID, cascade);
		}
		return true;
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		Trigger();
		return base.HandleEvent(E);
	}
}
