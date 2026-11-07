namespace XRL.World;

[GameEvent(Cache = Cache.Singleton)]
public class BeforePlayMusicEvent : SingletonEvent<BeforePlayMusicEvent>
{
	public SoundRequest Request;

	public string Track
	{
		get
		{
			return Request.Clip;
		}
		set
		{
			Request.Clip = value;
		}
	}

	public string Channel
	{
		get
		{
			return Request.Channel;
		}
		set
		{
			Request.Channel = value;
		}
	}

	public float VolumeAttenuation
	{
		get
		{
			return Request.Volume;
		}
		set
		{
			Request.Volume = value;
		}
	}

	public float CrossfadeDuration
	{
		get
		{
			return Request.CrossfadeDuration;
		}
		set
		{
			Request.CrossfadeDuration = value;
		}
	}

	public bool Crossfade
	{
		get
		{
			return Request.Crossfade;
		}
		set
		{
			Request.Crossfade = value;
		}
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Request = null;
	}

	public static bool Check(SoundRequest Request)
	{
		XRLGame game = The.Game;
		if (game == null)
		{
			return true;
		}
		SingletonEvent<BeforePlayMusicEvent>.Instance.Request = Request;
		return game.HandleEvent(SingletonEvent<BeforePlayMusicEvent>.Instance);
	}
}
