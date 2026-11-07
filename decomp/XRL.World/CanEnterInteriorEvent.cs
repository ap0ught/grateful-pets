using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class CanEnterInteriorEvent : SingletonEvent<CanEnterInteriorEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Actor;

	public GameObject Object;

	public Interior Interior;

	public int Status;

	public bool Action;

	public bool ShowMessage;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Object = null;
		Interior = null;
		Status = 0;
		Action = false;
		ShowMessage = false;
	}

	public static bool Check(GameObject Actor, GameObject Object, Interior Interior, ref int Status, ref bool Action, ref bool ShowMessage)
	{
		SingletonEvent<CanEnterInteriorEvent>.Instance.Actor = Actor;
		SingletonEvent<CanEnterInteriorEvent>.Instance.Object = Object;
		SingletonEvent<CanEnterInteriorEvent>.Instance.Interior = Interior;
		SingletonEvent<CanEnterInteriorEvent>.Instance.Status = Status;
		SingletonEvent<CanEnterInteriorEvent>.Instance.Action = Action;
		SingletonEvent<CanEnterInteriorEvent>.Instance.ShowMessage = ShowMessage;
		Actor.HandleEvent(SingletonEvent<CanEnterInteriorEvent>.Instance);
		Object.HandleEvent(SingletonEvent<CanEnterInteriorEvent>.Instance);
		Status = SingletonEvent<CanEnterInteriorEvent>.Instance.Status;
		Action = SingletonEvent<CanEnterInteriorEvent>.Instance.Action;
		ShowMessage = SingletonEvent<CanEnterInteriorEvent>.Instance.ShowMessage;
		SingletonEvent<CanEnterInteriorEvent>.Instance.Reset();
		return Status == 0;
	}
}
