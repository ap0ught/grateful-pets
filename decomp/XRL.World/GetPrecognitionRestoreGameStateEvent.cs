using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetPrecognitionRestoreGameStateEvent : PooledEvent<GetPrecognitionRestoreGameStateEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public Dictionary<string, object> GameState;

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
		Object = null;
		GameState = null;
	}

	public void Set(string Key, object Value)
	{
		if (GameState == null)
		{
			GameState = new Dictionary<string, object>();
		}
		GameState[Key] = Value;
	}

	public static Dictionary<string, object> GetFor(GameObject Object)
	{
		bool flag = true;
		Dictionary<string, object> dictionary = null;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetPrecognitionRestoreGameState"))
		{
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, object>();
			}
			Event obj = Event.New("GetPrecognitionRestoreGameState");
			obj.SetParameter("Object", Object);
			obj.SetParameter("GameState", dictionary);
			flag = Object.FireEvent(obj);
			dictionary = obj.GetParameter("GameState") as Dictionary<string, object>;
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetPrecognitionRestoreGameStateEvent>.ID, CascadeLevel))
		{
			GetPrecognitionRestoreGameStateEvent getPrecognitionRestoreGameStateEvent = PooledEvent<GetPrecognitionRestoreGameStateEvent>.FromPool();
			getPrecognitionRestoreGameStateEvent.Object = Object;
			getPrecognitionRestoreGameStateEvent.GameState = dictionary;
			flag = Object.HandleEvent(getPrecognitionRestoreGameStateEvent);
			dictionary = getPrecognitionRestoreGameStateEvent.GameState;
		}
		return dictionary;
	}
}
