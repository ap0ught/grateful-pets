using XRL.Collections;

namespace XRL.Sound;

public class MusicSourceCollection : StringMap<MusicSource>
{
	public void Update()
	{
		for (int i = 0; i < Length; i++)
		{
			if (Slots[i].Key != null)
			{
				Slots[i].Value.Update();
			}
		}
	}

	public void SetMusicBackground(bool State)
	{
		for (int i = 0; i < Length; i++)
		{
			if (Slots[i].Key != null)
			{
				Slots[i].Value.SetMusicBackground(State);
			}
		}
	}

	public async void StopFadeAsync()
	{
		await The.UiContext;
		StopFade();
	}

	public void StopFade()
	{
		for (int i = 0; i < Length; i++)
		{
			if (Slots[i].Key != null)
			{
				Slots[i].Value.StopFade();
			}
		}
	}
}
