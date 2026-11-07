namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class BeforeAbilityManagerOpenEvent : SingletonEvent<BeforeAbilityManagerOpenEvent>
{
	public new static readonly int CascadeLevel = 15;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public static bool Check(GameObject Object)
	{
		return Object.HandleEvent(SingletonEvent<BeforeAbilityManagerOpenEvent>.Instance);
	}
}
