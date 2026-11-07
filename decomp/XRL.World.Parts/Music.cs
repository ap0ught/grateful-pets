using System;

namespace XRL.World.Parts;

[Serializable]
public class Music : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<AfterGameLoadedEvent>.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		if (ParentObject.CurrentZone == IComponent<GameObject>.TheGame.ZoneManager.ActiveZone)
		{
			TryMusic();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		TryMusic();
		return true;
	}

	public void TryMusic()
	{
		SoundManager.PlayMusic(ParentObject.GetStringProperty("Track"), "music", !ParentObject.HasTag("NoCrossfade"));
	}
}
