namespace XRL.World;

[GameEvent(Cache = Cache.Singleton)]
public class GetAmbientLightEvent : SingletonEvent<GetAmbientLightEvent>
{
	public object Source;

	public string Type;

	public LightLevel Light;

	public int Radius;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Source = null;
		Type = null;
		Light = LightLevel.Blackout;
		Radius = 0;
	}

	public static void Send(object Source, string Type, ref LightLevel Light, ref int Radius)
	{
		SingletonEvent<GetAmbientLightEvent>.Instance.Source = Source;
		SingletonEvent<GetAmbientLightEvent>.Instance.Type = Type;
		SingletonEvent<GetAmbientLightEvent>.Instance.Light = Light;
		SingletonEvent<GetAmbientLightEvent>.Instance.Radius = Radius;
		The.Game.HandleEvent(SingletonEvent<GetAmbientLightEvent>.Instance);
		Light = SingletonEvent<GetAmbientLightEvent>.Instance.Light;
		Radius = SingletonEvent<GetAmbientLightEvent>.Instance.Radius;
		SingletonEvent<GetAmbientLightEvent>.Instance.Reset();
	}
}
